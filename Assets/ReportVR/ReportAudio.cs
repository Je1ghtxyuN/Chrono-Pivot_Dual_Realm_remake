using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Mono world sounds use distance attenuation; interface sounds remain readable in VR menus.
public sealed class ReportAudio : MonoBehaviour
{
    public ReportSoundBank bank;
    static ReportAudio instance;
    AudioSource[] voices;
    readonly Dictionary<ReportSound,float> nextPlay=new Dictionary<ReportSound,float>();
    readonly List<AudioSource> loops=new List<AudioSource>();
    float suppressWorldUntil;
    bool paused;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Clear(){instance=null;SceneManager.sceneLoaded-=OnSceneLoaded;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        var library=Resources.Load<ReportSoundBank>("InteractionSoundBank");
        if(library==null){Debug.LogWarning("InteractionSoundBank missing; interaction sounds are disabled.");return;}
        var root=new GameObject("Interaction Audio");instance=root.AddComponent<ReportAudio>();instance.bank=library;
        DontDestroyOnLoad(root);instance.CreateVoices();SceneManager.sceneLoaded+=OnSceneLoaded;
    }
    void CreateVoices()
    {
        voices=new AudioSource[bank.maximumVoices];
        for(int i=0;i<voices.Length;i++)voices[i]=Source(new GameObject("Sound voice "+i).transform,false);
    }
    AudioSource Source(Transform parent,bool loop)
    {
        if(!loop)parent.SetParent(transform,false);
        var source=parent.gameObject.AddComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;
        source.spatialBlend=1;source.rolloffMode=AudioRolloffMode.Logarithmic;source.minDistance=bank.nearDistance;
        source.maxDistance=bank.farDistance;source.dopplerLevel=0;source.volume=0;return source;
    }
    static void OnSceneLoaded(Scene scene,LoadSceneMode mode)=>AttachScene();
    public static void AttachScene()
    {
        if(instance!=null)instance.loops.RemoveAll(source=>source==null);
        foreach(var grab in FindObjectsOfType<ReportGrabbable>())AttachObject(grab.gameObject);
        foreach(var domino in FindObjectsOfType<ReportDominoMeasure>())foreach(var body in domino.dominoes)if(body!=null){AttachObject(body.gameObject);body.GetComponent<ReportObjectAudio>().dominoSound=true;}
        foreach(var level in FindObjectsOfType<DemoLevel>())
        {
            if(level.GetComponent<ReportExperimentAudio>()==null)level.gameObject.AddComponent<ReportExperimentAudio>();
            if(level.player!=null && level.player.GetComponent<ReportFootstepAudio>()==null)level.player.gameObject.AddComponent<ReportFootstepAudio>();
        }
    }
    public static void AttachObject(GameObject go)
    {
        if(go.GetComponent<ReportObjectAudio>()==null){var sound=go.AddComponent<ReportObjectAudio>();sound.InferSurface();}
    }
    public static bool Play(ReportSound cue,Vector3 position,float strength=1,bool ui=false,float pitch=1)
    {
        if(instance==null || instance.bank==null || !ui && (Time.timeScale==0 || Time.unscaledTime<instance.suppressWorldUntil))return false;
        var entry=instance.bank.Find(cue);
        if(entry==null || entry.clip==null || entry.volume<=0 || instance.bank.masterVolume<=0 || strength<=0)return false;
        if(instance.nextPlay.TryGetValue(cue,out float next) && Time.unscaledTime<next)return false;
        AudioSource voice=null;
        foreach(var source in instance.voices)if(!source.isPlaying){voice=source;break;}
        if(voice==null)
        {
            foreach(var source in instance.voices)if(voice==null || source.priority>voice.priority)voice=source;
            if(voice.priority<entry.priority)return false;
        }
        voice.Stop();voice.transform.position=position;voice.clip=entry.clip;voice.loop=false;
        voice.spatialBlend=ui?0:1;voice.ignoreListenerPause=ui;voice.priority=entry.priority;
        voice.pitch=Mathf.Clamp(pitch*Random.Range(entry.pitchRange.x,entry.pitchRange.y),.25f,3);
        voice.volume=instance.bank.masterVolume*entry.volume*Mathf.Clamp01(strength);voice.Play();
        instance.nextPlay[cue]=Time.unscaledTime+entry.minimumInterval;return true;
    }
    public static AudioSource CreateLoop(ReportSound cue,Transform owner)
    {
        if(instance==null)return null;var entry=instance.bank.Find(cue);if(entry==null || entry.clip==null)return null;
        var node=new GameObject(cue+" loop").transform;node.SetParent(owner,false);
        var source=instance.Source(node,true);source.clip=entry.clip;source.pitch=1;source.priority=200;instance.loops.Add(source);return source;
    }
    public static void SetLoop(AudioSource source,ReportSound cue,float intensity)
    {
        if(source==null || instance==null)return;
        var entry=instance.bank.Find(cue);
        bool active=Time.timeScale>0 && Time.unscaledTime>=instance.suppressWorldUntil && intensity>.001f;
        float volume=active?entry.volume*instance.bank.masterVolume*Mathf.Clamp01(intensity):0;
        source.volume=Mathf.MoveTowards(source.volume,volume,Time.unscaledDeltaTime*1.5f);
        if(active && !source.isPlaying)source.Play();else if(!active && source.volume<.001f)source.Stop();
    }
    public static void SilenceWorld()
    {
        if(instance==null)return;instance.suppressWorldUntil=Time.unscaledTime+.2f;
        foreach(var source in instance.voices)if(source.spatialBlend>0)source.Stop();
        foreach(var source in instance.loops)if(source!=null){source.Stop();source.volume=0;}
    }
    void Update()
    {
        bool now=Time.timeScale==0;if(now==paused)return;paused=now;
        foreach(var source in voices)if(source.spatialBlend>0){if(paused)source.Pause();else source.UnPause();}
    }
    void OnDestroy(){SceneManager.sceneLoaded-=OnSceneLoaded;if(instance==this)instance=null;}
}
