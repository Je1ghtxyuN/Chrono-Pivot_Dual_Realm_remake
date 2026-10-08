using System;
using System.IO;
using System.Linq;
using PhysicsTutorPrototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Sends a Chinese question through the real game UI to the local NF4 backend.
public static class PhysicsTutorRealTextCheck
{
    static ReportGameUI ui;
    static ReportPhysicsTutor tutor;
    static int phase,step;
    static double next,deadline;
    static bool failed,oldOptionsEnabled;
    static EnterPlayModeOptions oldOptions;
    const string Question="为什么船装入配重后会下沉一点？";
    static T Widget<T>(string name) where T:Component=>ui.GetComponentsInChildren<T>(true).First(c=>c.name==name);
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);
        phase=0;deadline=EditorApplication.timeSinceStartup+180;failed=false;
        EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void Tick()
    {
        try
        {
            if(phase==99)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                EditorSettings.enterPlayModeOptions=oldOptions;EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;
                EditorApplication.update-=Tick;EditorApplication.Exit(failed?1:0);return;
            }
            Require(EditorApplication.timeSinceStartup<deadline,"Real text test timed out at phase "+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<next)return;
            switch(phase)
            {
                case 0:
                    ui=UnityEngine.Object.FindObjectOfType<ReportGameUI>();ui.persistSettings=false;ui.saveProgress=false;
                    ui.ShowPage(ReportUIPage.Tutor);
                    tutor=ui.GetComponent<ReportPhysicsTutor>();
                    Require(tutor!=null && ui.MenuOpen,"Tutor page did not open");
                    step=tutor.level.Step;
                    Widget<InputField>("UI_TutorEndpoint").text="http://127.0.0.1:18080/api/tutor";
                    Widget<InputField>("UI_TutorQuestion").ActivateInputField();
                    phase=1;next=EditorApplication.timeSinceStartup+.5;break;
                case 1:
                    foreach(char character in Question)Widget<InputField>("UI_TutorQuestion").ProcessEvent(new Event{type=EventType.KeyDown,character=character});
                    Require(tutor.Question==Question,"Chinese text did not reach the game tutor");
                    Widget<Button>("UI_TutorSend").onClick.Invoke();
                    Require(tutor.Busy,"UI send did not start a real HTTP request");
                    phase=2;next=EditorApplication.timeSinceStartup+.2;break;
                case 2:
                    if(tutor.Busy){next=EditorApplication.timeSinceStartup+.2;break;}
                    Require(tutor.Status.Contains("回答已收到"),"Real model request failed: "+tutor.Status);
                    Require(!string.IsNullOrWhiteSpace(tutor.Output),"Model reply was not displayed");
                    Require(tutor.level.Step==step,"Asking tutor changed game progress");
                    var raw=File.ReadAllLines(tutor.Client.EvidenceLogPath).Last();
                    Require(raw.Contains("NF4") && raw.Contains("adapter_used"),"Evidence log does not identify real quantized adapter backend");
                    Capture();
                    Finish("PASS: Chinese text key events -> game input -> real localhost HTTP -> NF4 3B + physics LoRA reply displayed; game progress unchanged.\nQuestion: "+Question+"\nReply: "+tutor.Output+"\nStatus: "+tutor.Status+"\nEvidence log: "+tutor.Client.EvidenceLogPath+"\nNo fixture responses used. OS IME and microphone not covered.\n");break;
            }
        }
        catch(Exception error){failed=true;Finish("FAIL phase="+phase+"\n"+error);}
    }
    static void Finish(string result)
    {
        Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/physics-tutor-real-text.txt",result);
        Debug.Log(result);Time.timeScale=1;phase=99;EditorApplication.isPlaying=false;
    }
    static void Capture()
    {
        var camera=ui.player.head;var canvases=ui.GetComponentsInChildren<Canvas>().Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.3f;}Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(1600,1000,24);var pixels=new Texture2D(1600,1000,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1600,1000),0,0);pixels.Apply();
        Directory.CreateDirectory("Documentation/UIPreviews");File.WriteAllBytes("Documentation/UIPreviews/15_AI_Tutor_Real_NF4.png",pixels.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);
        foreach(var canvas in canvases)canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.ForceUpdateCanvases();
    }
}
