using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public static class SceneTransitionPlayCheck
{
    static int phase,instanceId;
    static double until,deadline;
    static bool failed,oldXR,oldOptionsEnabled;
    static EnterPlayModeOptions oldOptions;
    static string error;
    static Component effect;
    static ParticleSystem particles;
    static Type transition;
    static bool Busy=>transition!=null && (bool)transition.GetProperty("IsTransitioning").GetValue(null);
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static void Next(int value,double delay=.1){phase=value;until=EditorApplication.timeSinceStartup+delay;}
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXR=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);phase=0;failed=false;error=null;deadline=EditorApplication.timeSinceStartup+80;
        Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void OnLog(string message,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)error=message;}
    static void Tick()
    {
        try
        {
            if(phase==99)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                EditorSettings.enterPlayModeOptions=oldOptions;EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).InitManagerOnStart=oldXR;
                EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;AssetDatabase.SaveAssets();EditorApplication.Exit(failed?1:0);return;
            }
            Require(EditorApplication.timeSinceStartup<deadline,"Transition test timeout phase="+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;Require(error==null,"Runtime error: "+error);
            var browser=Object.FindObjectOfType<ReportSceneBrowser>();var ui=browser.GetComponent<ReportGameUI>();var player=browser.GetComponent<DesktopPlayer>();
            switch(phase)
            {
                case 0:
                    ui.saveProgress=ui.persistSettings=false;ui.Resume();browser.allowDebugNavigation=false;
                    typeof(ReportGameUI).GetProperty("HighestUnlocked").GetSetMethod(true).Invoke(ui,new object[]{0});
                    Require(!browser.TryLoadScene(-1) && !browser.TryLoadScene(1),"Invalid/locked requests must not transition");
                    browser.allowDebugNavigation=true;ui.ShowPage(ReportUIPage.Scenes);
                    ui.GetComponentsInChildren<Button>().Single(b=>b.name=="UI_Scene_1").onClick.Invoke();
                    transition=typeof(ReportSceneBrowser).Assembly.GetType("ReportSceneTransition");
                    effect=Object.FindObjectsOfType<MonoBehaviour>().FirstOrDefault(c=>c.GetType()==transition);
                    Require(effect!=null,"Actual scene selection has no particle transition");Require(Busy,"Scene selection must begin a guarded transition");
                    particles=effect.GetComponentInChildren<ParticleSystem>();Require(particles!=null && particles.particleCount>0,"Departure must emit a brief particle burst");instanceId=effect.GetInstanceID();
                    Require(player.UIBlocked && !browser.TryLoadScene(2),"Transition must block input and duplicate requests");
                    Time.timeScale=0;Next(1,.16);break;
                case 1:
                    Require(SceneManager.GetActiveScene().buildIndex==0,"Outgoing scene must remain visible during the departure fade");
                    var fade=effect.GetComponentsInChildren<MeshRenderer>().Single();Require(fade.sharedMaterial.color.a>.15f,"Fade must advance using unscaled time while simulation is paused");
                    Require(particles.particleCount>0,"Departure particles must remain visible through the fade");
                    Capture("departing");Next(2,.2);break;
                case 2:
                    if(SceneManager.GetActiveScene().buildIndex!=1 || Busy){Next(2,.1);break;}
                    Require(!player.UIBlocked && Time.timeScale==1 && !ui.MenuOpen,"Arrival must restore normal gameplay");
                    Require(particles.particleCount==0 && !effect.GetComponentsInChildren<MeshRenderer>().Single().enabled,"Completed transition must clear particles and fade overlay");
                    Require(effect.GetInstanceID()==instanceId && Object.FindObjectsOfType<MonoBehaviour>().Count(c=>c.GetType()==transition)==1,"Transition must persist once without duplicate effects");
                    Capture("arrived");ui.saveProgress=ui.persistSettings=false;
                    var level=Object.FindObjectOfType<DemoLevel>();foreach(var station in level.stations)level.ResolveStation(station);Next(3,.3);break;
                case 3:
                    Require(!ui.MenuOpen && !player.UIBlocked && Time.timeScale==1,"Completing experiments must leave gameplay active while the exit opens");
                    Next(4,2.6);break;
                case 4:
                    Require(Object.FindObjectOfType<DemoLevel>().ExitReady,"Completion fixture must open the physical exit");
                    var exit=Object.FindObjectOfType<DemoExit>();var body=player.GetComponent<CharacterController>();body.enabled=false;player.transform.position=exit.transform.position;body.enabled=true;Physics.SyncTransforms();Next(5,.12);break;
                case 5:
                    Require(Busy || SceneManager.GetActiveScene().buildIndex==2,"Actually entering the unlocked exit must trigger the same transition");Next(6,.1);break;
                case 6:
                    if(SceneManager.GetActiveScene().buildIndex!=2 || Busy){Next(6,.1);break;}
                    Require(!player.UIBlocked && particles.particleCount==0 && effect.GetInstanceID()==instanceId,"Exit transition must finish, restore controls and reuse the same effect");
                    Directory.CreateDirectory("Documentation/TransitionReview");File.WriteAllText("Documentation/TransitionReview/play.txt","PASS: invalid/locked requests, actual scene-selection button, departure particles, unscaled fade while paused, input/re-entry guard, actual scene loading, cleared overlay/particles, one persistent effect, physical unlocked exit, arrival gameplay restore.\n");Debug.Log("SCENE_TRANSITION_PLAY_OK");Finish(false);break;
            }
        }
        catch(Exception e){Directory.CreateDirectory("Documentation/TransitionReview");File.WriteAllText("Documentation/TransitionReview/play.txt","FAIL phase="+phase+" "+e);Debug.LogException(e);Finish(true);}
    }
    static void Capture(string name)
    {
        var camera=Camera.main;var target=new RenderTexture(1200,750,24);var pixels=new Texture2D(1200,750,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1200,750),0,0);pixels.Apply();
        Directory.CreateDirectory("Documentation/TransitionReview");File.WriteAllBytes("Documentation/TransitionReview/"+name+".png",pixels.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;target.Release();Object.Destroy(target);Object.Destroy(pixels);
    }
    static void Finish(bool failure){failed=failure;Time.timeScale=1;phase=99;EditorApplication.isPlaying=false;}
}
