using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum ReportAudioSurface {Wood,Stone,Metal,Snow,Glass}
public sealed class ReportObjectAudio : MonoBehaviour
{
    [Header("材质与力度")]
    public ReportAudioSurface surface;
    [Range(0,1)] public float volume=1;
    [Min(0)] public float minimumImpactSpeed=.3f;
    [Min(.01f)] public float fullImpactSpeed=4;
    [Min(.02f)] public float impactInterval=.12f;
    [Header("机关与滚动")]
    [Range(.005f,.2f)] public float mechanismTickFraction=.025f;
    [Min(.03f)] public float mechanismInterval=.12f;
    public bool rollingSound;
    public bool dominoSound;
    ReportGrabbable grab;
    ReportMechanismHandle handle;
    Rigidbody body;
    AudioSource rolling;
    float nextImpact,nextTick,lastValue,lastContact;
    void Reset()=>InferSurface();
    void Awake(){grab=GetComponent<ReportGrabbable>();handle=grab as ReportMechanismHandle;body=GetComponent<Rigidbody>();}
    void Start(){if(handle!=null)lastValue=handle.Value;if(rollingSound)rolling=ReportAudio.CreateLoop(ReportSound.Roll,transform);}
    void OnEnable(){if(grab!=null){grab.selectEntered.AddListener(Grabbed);grab.selectExited.AddListener(Released);}}
    void OnDisable()
    {
        if(grab!=null){grab.selectEntered.RemoveListener(Grabbed);grab.selectExited.RemoveListener(Released);}
        if(rolling!=null)rolling.Stop();
    }
    void Grabbed(SelectEnterEventArgs args){ReportAudio.Play(ReportSound.Grab,transform.position,volume, false,SurfacePitch());if(handle!=null)lastValue=handle.Value;}
    void Released(SelectExitEventArgs args)=>ReportAudio.Play(ReportSound.Release,transform.position,volume,false,SurfacePitch());
    float SurfacePitch()=>surface==ReportAudioSurface.Stone?.8f:surface==ReportAudioSurface.Metal?1.15f:surface==ReportAudioSurface.Snow?.9f:1;
    void OnCollisionEnter(Collision contact)
    {
        lastContact=Time.time;
        // The accepted bell strike owns its ringing; avoid doubling it with a generic material hit.
        if(contact.collider.GetComponentInParent<ReportAcousticMeasure>()!=null)return;
        float speed=contact.relativeVelocity.magnitude;if(speed<minimumImpactSpeed || Time.unscaledTime<nextImpact)return;
        nextImpact=Time.unscaledTime+impactInterval;
        var cue=dominoSound?ReportSound.Domino:(ReportSound)((int)ReportSound.ImpactWood+(int)surface);
        ReportAudio.Play(cue,contact.GetContact(0).point,volume*Mathf.Sqrt(Mathf.Clamp01(speed/fullImpactSpeed)));
    }
    void OnCollisionStay(Collision contact)=>lastContact=Time.time;
    void Update()
    {
        if(handle!=null)
        {
            float delta=Mathf.Abs(handle.Value-lastValue)/Mathf.Max(.001f,handle.valueRange.y-handle.valueRange.x);
            if(!handle.isSelected){lastValue=handle.Value;return;}
            if(delta>=mechanismTickFraction && Time.unscaledTime>=nextTick)
            {
                var cue=handle.motion==ReportHandleMotion.Slide?ReportSound.HandleSlide:surface==ReportAudioSurface.Wood?ReportSound.HandleWood:surface==ReportAudioSurface.Stone?ReportSound.HandleStone:ReportSound.HandleMetal;
                ReportAudio.Play(cue,transform.position,volume);lastValue=handle.Value;nextTick=Time.unscaledTime+mechanismInterval;
            }
        }
        if(rolling!=null)ReportAudio.SetLoop(rolling,ReportSound.Roll,body!=null && !body.isKinematic && (grab==null || !grab.isSelected) && Time.time-lastContact<.15f?Mathf.Clamp01(body.velocity.magnitude/2):0);
    }
    public void InferSurface()
    {
        string names="";for(var t=transform;t!=null;t=t.parent)names+=" "+t.name;
        foreach(var text in GetComponentsInChildren<TextMesh>())names+=" "+text.text;
        if(names.Contains("HubMaterialCube_0") || names.Contains("木") || names.Contains("骨牌") || names.Contains("Domino"))surface=ReportAudioSurface.Wood;
        else if(names.Contains("HubMaterialCube_1") || names.Contains("石"))surface=ReportAudioSurface.Stone;
        else if(names.Contains("雪") || names.Contains("Snow") || names.Contains("冰"))surface=ReportAudioSurface.Snow;
        else if(names.Contains("玻璃") || names.Contains("Glass"))surface=ReportAudioSurface.Glass;
        else surface=ReportAudioSurface.Metal;
        rollingSound=GetComponent<SphereCollider>()!=null || names.Contains("雪球") || names.Contains("小球");
    }
}
