using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class ReportMusicPlayCheck
{
    static IEnumerator<float> routine;
    static double until,deadline;
    static string error;
    static bool finishing,failed,oldXR,oldOptionsEnabled;
    static EnterPlayModeOptions oldOptions;
    static float oldVolume;
    static readonly List<string> results=new List<string>();
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        AssetDatabase.Refresh();oldVolume=AudioListener.volume;
        Require(Resources.Load("BackgroundMusicBank")==null,"Background music bank still included");
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXR=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);results.Clear();finishing=failed=false;error=null;
        routine=Exercise().GetEnumerator();until=0;deadline=EditorApplication.timeSinceStartup+300;
        Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void OnLog(string message,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)error=message;}
    static void Tick()
    {
        try
        {
            if(finishing)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                EditorSettings.enterPlayModeOptions=oldOptions;EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).InitManagerOnStart=oldXR;
                AudioListener.volume=oldVolume;Application.logMessageReceived-=OnLog;EditorApplication.update-=Tick;
                AssetDatabase.SaveAssets();EditorApplication.Exit(failed?1:0);return;
            }
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("No-music check timed out");
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(error==null,error);if(routine.MoveNext()){until=EditorApplication.timeSinceStartup+routine.Current;return;}
            File.WriteAllLines("Documentation/MusicReview/playtest.txt",results);Debug.Log("REPORT_NO_MUSIC_PLAY_OK scenes=15 quietFootsteps=3");
            finishing=true;EditorApplication.isPlaying=false;
        }
        catch(Exception e)
        {
            failed=finishing=true;Application.logMessageReceived-=OnLog;Debug.LogError(e);
            File.WriteAllLines("Documentation/MusicReview/playtest.txt",new[]{"FAIL",e.ToString()});EditorApplication.isPlaying=false;
        }
    }
    static IEnumerable<float> Exercise()
    {
        yield return 1.8f;
        for(int index=0;index<15;index++)
        {
            if(index>0)SceneManager.LoadScene(index);
            // LoadScene completes on a later frame; inspect the actual scene.
            double settleDeadline=EditorApplication.timeSinceStartup+12;
            while(SceneManager.GetActiveScene().buildIndex!=index)
            {Require(EditorApplication.timeSinceStartup<settleDeadline,"Scene transition stalled index="+index);yield return .05f;}
            yield return .15f;
            Require(GameObject.Find("Background Music")==null,"Background music object still boots");
            foreach(var source in Object.FindObjectsOfType<AudioSource>())
                Require(source.clip==null || !IsTheme(source.clip.name),"Background music clip still assigned");
            Require(Object.FindObjectOfType<ReportAudio>()!=null && Object.FindObjectOfType<ReportFootstepAudio>()!=null,"Interaction audio or footsteps missing in scene "+index);
            results.Add("PASS scene="+index+": no BGM object or theme clip; interaction audio and footsteps retained.");
            Debug.Log("REPORT_NO_MUSIC_SCENE_OK index="+index);
        }
        foreach(float wait in CheckFootsteps())yield return wait;
    }
    static bool IsTheme(string name)=>name=="Garden_Pentatonic" || name=="Laboratory_Pulse" || name=="Starlight_Shadow" || name=="Snow_Lantern";
    static IEnumerable<float> CheckFootsteps()
    {
        var audio=Object.FindObjectOfType<ReportAudio>();Require(audio!=null,"Interaction audio missing");
        var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.position=new Vector3(500,-.1f,500);floor.transform.localScale=new Vector3(20,.2f,20);
        var player=new GameObject("Quiet footstep check");player.transform.position=new Vector3(500,1,500);
        var steps=player.AddComponent<ReportFootstepAudio>();
        float savedScale=Time.timeScale;Time.timeScale=1;ReportAudio.SilenceWorld();yield return .5f;
        var cues=new[]{ReportSound.FootStone,ReportSound.FootWood,ReportSound.FootSnow};
        var names=new[]{"Stone floor","Bridge floor","Snow floor"};
        for(int i=0;i<cues.Length;i++)
        {
            var entry=audio.bank.Find(cues[i]);Require(entry!=null && entry.clip!=null,"Footstep asset missing");
            Require(entry.volume<=.1f && Mathf.Abs(steps.volume-.4f)<.001f,"Footstep settings too loud");
            var samples=new float[entry.clip.samples*entry.clip.channels];Require(entry.clip.GetData(samples,0),"Cannot read imported footstep");
            float peak=0;foreach(float sample in samples)peak=Mathf.Max(peak,Mathf.Abs(sample));Require(peak>.01f && peak<=.17f,"Footstep import normalized or silent");
            floor.name=names[i];player.transform.position+=Vector3.right*.8f;yield return .05f;
            AudioSource voice=null;foreach(var source in audio.GetComponentsInChildren<AudioSource>())if(source.clip==entry.clip && source.isPlaying)voice=source;
            Require(voice!=null,"Walking did not trigger "+cues[i]);
            Require(Mathf.Abs(voice.volume-.032f)<.001f && voice.spatialBlend==1,"Incorrect quiet spatial footstep volume");
            yield return .5f;
            foreach(var source in audio.GetComponentsInChildren<AudioSource>())Require(source.clip!=entry.clip || !source.isPlaying,"Stationary footstep repeated");
            Time.timeScale=0;player.transform.position+=Vector3.right*.8f;yield return .1f;
            foreach(var source in audio.GetComponentsInChildren<AudioSource>())Require(source.clip!=entry.clip || !source.isPlaying,"Paused walking triggered footstep");
            Time.timeScale=1;yield return .1f;
            results.Add("PASS quiet footstep="+cues[i]+" imported peak="+peak+" source volume=0.032; walking trigger; stationary and paused silent.");
        }
        Object.Destroy(player);Object.Destroy(floor);Time.timeScale=savedScale;
    }
}
