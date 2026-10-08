using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Symbolic travel feedback. One persistent effect covers every authorized loading entry.
[DefaultExecutionOrder(500)]
public sealed class ReportSceneTransition : MonoBehaviour
{
    static ReportSceneTransition instance;
    public static bool IsTransitioning=>instance!=null && instance.busy;
    public static AsyncOperation LoadingOperation=>instance!=null?instance.operation:null;
    bool busy;
    AsyncOperation operation;
    Transform viewer;
    ParticleSystem particles;
    MeshRenderer fade;
    Material fadeMaterial;
    Mesh fadeMesh;
    Canvas[] hiddenUI=new Canvas[0];
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetState(){instance=null;}
    public static bool Begin(int index)
    {
        if(IsTransitioning)return false;
        if(instance==null)
        {
            var root=new GameObject("Scene travel particles");DontDestroyOnLoad(root);instance=root.AddComponent<ReportSceneTransition>();instance.CreateEffects();
        }
        instance.busy=true;instance.StartCoroutine(instance.Travel(index));return true;
    }
    void CreateEffects()
    {
        var veil=new GameObject("Travel fade");veil.transform.SetParent(transform,false);veil.transform.localPosition=Vector3.forward*.5f;veil.transform.localScale=Vector3.one*3;
        fadeMesh=new Mesh{name="Travel veil",vertices=new[]{new Vector3(-1,-1,0),new Vector3(-1,1,0),new Vector3(1,1,0),new Vector3(1,-1,0)},triangles=new[]{0,1,2,0,2,3}};fadeMesh.RecalculateBounds();
        veil.AddComponent<MeshFilter>().sharedMesh=fadeMesh;fade=veil.AddComponent<MeshRenderer>();fadeMaterial=new Material(Resources.Load<Material>("TransitionFade"));fade.sharedMaterial=fadeMaterial;fade.enabled=false;
        fade.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;fade.receiveShadows=false;
        var emitter=new GameObject("Travel grains");emitter.transform.SetParent(transform,false);particles=emitter.AddComponent<ParticleSystem>();
        var main=particles.main;main.playOnAwake=false;main.useUnscaledTime=true;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.startLifetime=.6f;main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.012f,.026f);main.maxParticles=64;
        var emission=particles.emission;emission.enabled=false;var shape=particles.shape;shape.enabled=false;
        var alpha=particles.colorOverLifetime;alpha.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.75f,.1f),new GradientAlphaKey(0,1)});alpha.color=gradient;
        var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("TransitionParticles");renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    IEnumerator Travel(int index)
    {
        var player=FindObjectOfType<DesktopPlayer>();var ui=player.GetComponent<ReportGameUI>();
        if(ui!=null && ui.enabled && ui.MenuOpen)ui.Resume();
        Block(player);Follow(player.head.transform);HideUI(ui);fade.enabled=true;SetFade(0);Burst(false);
        yield return FadeTo(1,.32f);
        operation=SceneManager.LoadSceneAsync(index);yield return operation;
        yield return null; // The incoming UI initializes before its canvases are hidden.
        player=FindObjectOfType<DesktopPlayer>();ui=player.GetComponent<ReportGameUI>();Block(player);Follow(player.head.transform);HideUI(ui);
        particles.Clear();Burst(true);yield return FadeTo(0,.38f);
        particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);fade.enabled=false;RestoreUI();busy=false;operation=null;
        player.SetUIBlocked(ui!=null && ui.enabled && ui.MenuOpen);player.ResumePointer();
    }
    static void Block(DesktopPlayer player)
    {
        player.SetUIBlocked(true);foreach(var grab in player.GetComponentsInChildren<ReportGrabInteractor>())grab.ReleaseUntilButtonUp();
    }
    void HideUI(ReportGameUI ui)
    {
        hiddenUI=ui!=null?ui.GetComponentsInChildren<Canvas>(true).Where(c=>c.enabled).ToArray():new Canvas[0];foreach(var canvas in hiddenUI)canvas.enabled=false;
    }
    void RestoreUI(){foreach(var canvas in hiddenUI)if(canvas!=null)canvas.enabled=true;hiddenUI=new Canvas[0];}
    void Follow(Transform head){viewer=head;transform.SetPositionAndRotation(viewer.position,viewer.rotation);}
    void LateUpdate()
    {
        if(!busy)return;if(viewer==null && Camera.main!=null)viewer=Camera.main.transform;
        if(viewer!=null)transform.SetPositionAndRotation(viewer.position,viewer.rotation);
    }
    void Burst(bool arriving)
    {
        particles.Play();
        for(int i=0;i<40;i++)
        {
            float angle=Random.Range(0,Mathf.PI*2),radius=arriving?Random.Range(.15f,.3f):Random.Range(.55f,.72f);
            var radial=new Vector3(Mathf.Cos(angle)*1.45f,Mathf.Sin(angle),0);var tangent=new Vector3(-radial.y,radial.x,0);
            var particle=new ParticleSystem.EmitParams{position=radial*radius+Vector3.forward,velocity=radial*(arriving?.7f:-.75f)+tangent*.15f,startColor=Color.Lerp(new Color(.72f,.56f,.30f),new Color(.30f,.66f,.60f),Random.value)};
            particles.Emit(particle,1);
        }
        particles.Pause();
    }
    IEnumerator FadeTo(float opacity,float duration)
    {
        float initial=fadeMaterial.color.a,elapsed=0;
        while(elapsed<duration)
        {
            // Keep both visuals together when a first-use shader or scene causes a long frame.
            float step=Mathf.Min(Time.unscaledDeltaTime,.05f);elapsed+=step;particles.Simulate(step,true,false,false);
            SetFade(Mathf.Lerp(initial,opacity,Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/duration))));yield return null;
        }
        SetFade(opacity);
    }
    void SetFade(float opacity){fadeMaterial.color=new Color(.018f,.04f,.045f,opacity);}
    void OnDestroy(){RestoreUI();if(fadeMaterial!=null)Destroy(fadeMaterial);if(fadeMesh!=null)Destroy(fadeMesh);if(instance==this)instance=null;}
}
