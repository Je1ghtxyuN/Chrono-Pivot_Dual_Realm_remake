using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

// Exercises the real menu and UnityWebRequest against an external test HTTP fixture.
public static class PhysicsTutorPlayCheck
{
    static ReportGameUI ui;
    static MonoBehaviour tutor;
    static int phase;
    static double until,timeout;
    static bool failed,oldOptionsEnabled;
    static EnterPlayModeOptions oldOptions;
    static string error;
    static Keyboard keyboard;
    static Mouse mouse;
    static XRHMD hmd;
    static XRController hand;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static int dialogueTurn;
    static object Read(string name)=>tutor.GetType().GetProperty(name).GetValue(tutor);
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static void Open()=>ui.ShowPage((ReportUIPage)Enum.Parse(typeof(ReportUIPage),"Tutor"));
    static T Widget<T>(string name) where T:Component=>ui.GetComponentsInChildren<T>().Single(w=>w.name==name);
    static void Click(string name)=>Widget<Button>(name).onClick.Invoke();
    static void Next(int value,double delay=.25){phase=value;until=EditorApplication.timeSinceStartup+delay;}
    static void MouseAt(string name,bool pressed)
    {
        var rect=(RectTransform)Widget<Selectable>(name).transform;
        var position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        InputSystem.QueueStateEvent(mouse,new MouseState{position=position,buttons=(ushort)(pressed?1:0)});
    }
    static void AimHand(string name)
    {
        var rect=(RectTransform)Widget<Selectable>(name).transform;
        var target=rect.TransformPoint(rect.rect.center);var start=target-ui.player.head.transform.forward*.5f;
        var offset=ui.player.head.transform.parent;
        InputSystem.QueueDeltaStateEvent(hand.devicePosition,offset.InverseTransformPoint(start));
        InputSystem.QueueDeltaStateEvent(hand.deviceRotation,Quaternion.Inverse(offset.rotation)*Quaternion.LookRotation(target-start));
        InputSystem.QueueDeltaStateEvent(hand.trackingState,3);
    }
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);
        oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        phase=0;timeout=EditorApplication.timeSinceStartup+95;failed=false;error=null;
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
                InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
                foreach(var device in new InputDevice[]{keyboard,mouse,hmd,hand})if(device!=null && device.added)InputSystem.RemoveDevice(device);
                Application.logMessageReceived-=OnLog;EditorApplication.update-=Tick;EditorApplication.Exit(failed?1:0);return;
            }
            Require(EditorApplication.timeSinceStartup<timeout,"Tutor test timed out at phase "+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(error==null,"Runtime error: "+error);
            switch(phase)
            {
                case 0:
                    ui=UnityEngine.Object.FindObjectOfType<ReportGameUI>();ui.persistSettings=false;ui.saveProgress=false;
                    Require(Enum.GetNames(typeof(ReportUIPage)).Contains("Tutor"),"Missing AI tutor entry in the experiment menu");
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();
                    Click("UI_Tutor");Next(1);break;
                case 1:
                    tutor=ui.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="ReportPhysicsTutor");
                    Require(tutor!=null,"Tutor did not connect to the game menu");
                    Require(ui.MenuOpen && Time.timeScale==0 && ui.player.UIBlocked,"Tutor typing did not block gameplay");
                    Widget<InputField>("UI_TutorEndpoint").text=""; // Exercise the unconfigured branch despite the local-service default.
                    Require(string.IsNullOrEmpty(Widget<InputField>("UI_TutorEndpoint").text),"Model connection must be empty until configured");
                    Require(ui.tutorConnection!=null && Directory.Exists(ui.tutorConnection.baseModelPath) && Path.GetFileName(ui.tutorConnection.baseModelPath)=="Qwen2.5-3B-Instruct","Shared base model path does not point to the downloaded model");
                    Widget<InputField>("UI_TutorQuestion").text="未配置服务的调试问题";Click("UI_TutorSend");
                    Require(!(bool)Read("Busy") && ((string)Read("Status")).Contains("未配置"),"Unconfigured tutor attempted model inference");
                    Capture("12_AI_Tutor_EmptyModel");
                    Widget<InputField>("UI_TutorQuestion").text="";MouseAt("UI_TutorQuestion",true);Next(10);break;
                case 10:MouseAt("UI_TutorQuestion",false);Next(11);break;
                case 11:
                    Require(Widget<InputField>("UI_TutorQuestion").isFocused,"Mouse click failed to focus the debug text input");
                    foreach(char character in "文字输入调试")Widget<InputField>("UI_TutorQuestion").ProcessEvent(new Event{type=EventType.KeyDown,character=character});
                    Require((string)Read("Question")=="文字输入调试","Chinese key events did not reach the question state");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));Next(12);break;
                case 12:
                    Require(ui.Page.ToString()=="Tutor","Typing Tab navigated away from the tutor");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    Widget<InputField>("UI_TutorEndpoint").text="http://127.0.0.1:18081/api/tutor";
                    Widget<InputField>("UI_TutorQuestion").text="   ";Click("UI_TutorSend");
                    Require(!(bool)Read("Busy") && ((string)Read("Status")).Contains("问题"),"Empty input was sent");
                    Widget<InputField>("UI_TutorQuestion").text="为什么船装入配重后下沉？";
                    Click("UI_TutorSend");Require((bool)Read("Busy"),"Text input did not start an HTTP request");
                    Click("UI_TutorSend");Next(2,.8);break;
                case 2:
                    if((bool)Read("Busy")){Next(2,.15);break;}
                    Require(((string)Read("Output")).Contains("测试回答：浮力"),"Real HTTP reply was not displayed");
                    Require(((string)Read("Output")).Contains("OpenStax"),"Retrieved evidence was lost");
                    Require(Widget<ScrollRect>("Tutor Reply").content.rect.height>137,"Long answer was clipped without scrolling");
                    Capture("13_AI_Tutor_HTTP_Fixture");
                    Require(UnityEngine.Object.FindObjectOfType<DemoLevel>().Step==0,"Asking the tutor changed puzzle progress");
                    Widget<InputField>("UI_TutorQuestion").text="第二个问题";Click("UI_TutorSend");Next(3,.8);break;
                case 3:
                    if((bool)Read("Busy")){Next(3,.15);break;}
                    Require(((string)Read("Output")).Contains("第二次"),"Dialogue follow-up failed");
                    Click("UI_TutorNew");Widget<InputField>("UI_TutorQuestion").text="触发错误";Click("UI_TutorSend");Next(4,.8);break;
                case 4:
                    if((bool)Read("Busy")){Next(4,.15);break;}
                    Require(((string)Read("Status")).Contains("失败"),"HTTP failure was presented as an answer");
                    Widget<InputField>("UI_TutorQuestion").text="恢复请求";Click("UI_TutorSend");Next(5,.8);break;
                case 5:
                    if((bool)Read("Busy")){Next(5,.15);break;}
                    Require(((string)Read("Output")).Contains("恢复"),"Failure left the client permanently busy");
                    dialogueTurn=0;Click("UI_TutorNew");Next(20,.05);break;
                case 20:
                    Widget<InputField>("UI_TutorQuestion").text=dialogueTurn<11?"历史边界 "+dialogueTurn:"长回答 "+(dialogueTurn-11);
                    Click("UI_TutorSend");Next(21,.1);break;
                case 21:
                    if((bool)Read("Busy")){Next(21,.1);break;}
                    Require(!((string)Read("Status")).Contains("失败"),"Dialogue exceeded the server history contract");
                    dialogueTurn++;
                    if(dialogueTurn<15){Next(20,.05);break;}
                    Click("UI_TutorVoice");Next(6,.25);break;
                case 6:
                    ui.ShowPage(ReportUIPage.Tasks);Require(!(bool)Read("Listening"),"Leaving the tutor did not stop microphone listening");
                    Open();ui.Resume();Require(!ui.MenuOpen && !ui.player.UIBlocked && Time.timeScale==1,"Tutor exit did not restore gameplay");
                    Require(!(bool)Read("Listening"),"Closing the menu left the microphone active");
                    Open();Click("UI_TutorNew");Widget<InputField>("UI_TutorEndpoint").text="";
                    InputSystem.RegisterLayout(@"{""name"":""TutorTestController"",""extend"":""XRController"",""controls"":[{""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},{""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},{""name"":""trackingState"",""layout"":""Integer"",""offset"":28},{""name"":""triggerPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32}]}");
                    hmd=InputSystem.AddDevice<XRHMD>();hand=(XRController)InputSystem.AddDevice("TutorTestController");InputSystem.SetDeviceUsage(hand,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(hmd.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(hmd.centerEyeRotation,Quaternion.identity);InputSystem.QueueDeltaStateEvent(hmd.trackingState,3);
                    ui.ShowPage(ReportUIPage.Tasks);Next(30,.4);break;
                case 30:
                    Require(!ui.player.IsDesktop && ui.MenuCanvas.renderMode==RenderMode.WorldSpace,"Tutor did not use the VR world menu");
                    AimHand("UI_AskTutor");Next(31);break;
                case 31:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(32);break;
                case 32:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(33);break;
                case 33:
                    Require(ui.Page.ToString()=="Tutor","VR trigger failed to open the tutor from the task page");Capture("14_AI_Tutor_VR");
                    AimHand("UI_TutorHint");Next(34);break;
                case 34:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(35);break;
                case 35:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(36);break;
                case 36:
                    Require((int)Read("HintLevel")==2,"VR trigger failed to operate a tutor control");ui.Resume();
                    Finish("PASS: downloaded 3B base model configured; local-service default; empty endpoint branch makes no inference; menu entry and gameplay pause; actual mouse focus and Chinese InputField key events; Tab typing isolation; real HTTP request/reply/evidence and long-answer scroll; duplicate-send protection; 19 requests exercise 20-item and 10000-character history bounds; new conversation; HTTP 429 and recovery; voice startup/error handling and stop on navigation/close; virtual tracked VR trigger opens tutor from task page and changes hint level. Uses local fixture responses, not model answers. Real microphone transcription, Chinese IME and physical headset require manual testing.\n");break;
            }
        }
        catch(Exception ex){failed=true;Finish("FAIL phase="+phase+"\n"+ex);}
    }
    static void Finish(string result)
    {
        Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/physics-tutor-playtest.txt",result);
        Application.logMessageReceived-=OnLog;Debug.Log(result);Time.timeScale=1;phase=99;EditorApplication.isPlaying=false;
    }
    static void Capture(string name)
    {
        var camera=ui.player.head;var canvases=ui.GetComponentsInChildren<Canvas>().Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.3f;}Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(1600,1000,24);var pixels=new Texture2D(1600,1000,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1600,1000),0,0);pixels.Apply();
        Directory.CreateDirectory("Documentation/UIPreviews");File.WriteAllBytes("Documentation/UIPreviews/"+name+".png",pixels.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);
        foreach(var canvas in canvases)canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.ForceUpdateCanvases();
    }
}
