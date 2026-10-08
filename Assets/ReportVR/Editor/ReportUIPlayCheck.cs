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
using Unity.XR.CoreUtils;

// Drive real uGUI widgets with mouse and virtual controller input.
public static class ReportUIPlayCheck
{
    static MonoBehaviour ui;
    static Keyboard keyboard;
    static Mouse mouse;
    static XRHMD hmd;
    static XRController hand;
    static XROrigin origin;
    static DemoLevel level;
    static int phase;
    static double until,timeout;
    static bool failed,oldOptionsEnabled;
    static EnterPlayModeOptions oldOptions;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static Vector3 initial;
    static ReportGrabbable retryItem;
    static Vector3 retryHome;
    static string error;
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static object Read(string name)=>ui.GetType().GetProperty(name).GetValue(ui);
    static void Call(string method)=>ui.GetType().GetMethod(method).Invoke(ui,null);
    static void Open(string page)
    {
        var method=ui.GetType().GetMethod("ShowPage");method.Invoke(ui,new[]{Enum.Parse(method.GetParameters()[0].ParameterType,page)});
    }
    static Selectable Widget(string name)=>ui.GetComponentsInChildren<Selectable>(true).Single(w=>w.name==name);
    static void MouseAt(string name,bool pressed,float fraction=.5f)
    {
        var rect=(RectTransform)Widget(name).transform;
        var local=new Vector3(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,fraction),rect.rect.center.y);
        var position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(local));
        InputSystem.QueueStateEvent(mouse,new MouseState{position=position,buttons=(ushort)(pressed?1:0)});
    }
    static void Next(int step,double delay=.18){phase=step;until=EditorApplication.timeSinceStartup+delay;}
    static void AimHand(string name,float fraction=.5f)
    {
        var rect=(RectTransform)Widget(name).transform;
        var target=rect.TransformPoint(new Vector3(Mathf.Lerp(rect.rect.xMin,rect.rect.xMax,fraction),rect.rect.center.y));
        var start=target-origin.Camera.transform.forward*.5f;var offset=origin.CameraFloorOffsetObject.transform;
        InputSystem.QueueDeltaStateEvent(hand.devicePosition,offset.InverseTransformPoint(start));
        InputSystem.QueueDeltaStateEvent(hand.deviceRotation,Quaternion.Inverse(offset.rotation)*Quaternion.LookRotation(target-start));
        InputSystem.QueueDeltaStateEvent(hand.trackingState,3);
    }
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        phase=0;failed=false;error=null;timeout=EditorApplication.timeSinceStartup+90;
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
            if(EditorApplication.timeSinceStartup>timeout)throw new Exception("UI timeout phase="+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(error==null,"Runtime error: "+error);
            switch(phase)
            {
                case 0:
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    ui=origin.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="ReportGameUI");
                    Require(ui!=null,"Missing playable desktop and VR UI on the rig");
                    ui.GetType().GetField("persistSettings").SetValue(ui,false);ui.GetType().GetField("saveProgress").SetValue(ui,false);
                    ui.GetType().GetProperty("HighestUnlocked").GetSetMethod(true).Invoke(ui,new object[]{0});origin.GetComponent<ReportSceneBrowser>().allowDebugNavigation=false;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();Next(1,.4);break;
                case 1:
                    Require((bool)Read("MenuOpen") && Time.timeScale==0,"Start menu must pause experiments");
                    Capture("01_Home");MouseAt("UI_Tasks",true);Next(2);break;
                case 2:MouseAt("UI_Tasks",false);Next(3);break;
                case 3:
                    Require(Read("Page").ToString()=="Tasks" && level.Step==0,"Mouse task tab failed or advanced puzzle");
                    Capture("02_Tasks");Next(5);break;
                case 5:
                    Require(!ui.GetComponentsInChildren<Selectable>(true).Any(w=>w.name=="UI_Hint") && level.Step==0,"Removed hint option remains or UI bypassed experiment");
                    Require(!ui.GetComponentsInChildren<Button>().Any(b=>b.name=="UI_Retrieve" && b.gameObject.activeInHierarchy),"Non-throw experiment offers unrelated retrieval");
                    level.ResolveStation(level.stations[0]);
                    retryItem=UnityEngine.Object.FindObjectOfType<ReportThrowTarget>().acceptedItems[0].GetComponent<ReportGrabbable>();retryHome=retryItem.transform.position;
                    retryItem.transform.position+=Vector3.right*8;retryItem.GetComponent<Rigidbody>().position=retryItem.transform.position;
                    Open("Tasks");Next(33);break;
                case 33:
                    Require(ui.GetComponentsInChildren<Button>().Any(b=>b.name=="UI_Retrieve" && b.gameObject.activeInHierarchy),"Throw task has no retrieval action; only restarting the entire level is available");
                    Capture("12_Throw_Retry");MouseAt("UI_Retrieve",true);Next(34);break;
                case 34:MouseAt("UI_Retrieve",false);Next(35);break;
                case 35:
                    Require(Vector3.Distance(retryItem.transform.position,retryHome)<.04f && level.Step==1,"Mouse retrieval failed to restore the missed prop while preserving task progress");
                    Require(!(bool)Read("MenuOpen") && Time.timeScale==1 && (int)Read("HighestUnlocked")==0,"Retrieval failed to resume gameplay or bypassed level completion");
                    Require(UnityEngine.Object.FindObjectOfType<ReportThrowTarget>().Hits==0,"Retrieval recorded a false target hit");
                    level.ResetLevel();Open("Scenes");Next(6);break;
                case 6:
                    Require(!Widget("UI_Scene_1").interactable,"Unfinished next level is selectable");Capture("03_Scenes");Open("Settings");Next(7);break;
                case 7:Capture("04_Settings");MouseAt("UI_Volume",true,.25f);Next(8);break;
                case 8:MouseAt("UI_Volume",false,.25f);Next(9);break;
                case 9:
                    Require(Mathf.Abs(AudioListener.volume-.25f)<.06f,"Volume slider does not adjust actual audio");
                    Open("Controls");Next(10);break;
                case 10:
                    Capture("05_Controls");Call("Resume");Next(20,.3);break;
                case 20:
                    Capture("06_Desktop_HUD");initial=origin.transform.position;
                    Require(!(bool)Read("MenuOpen") && Time.timeScale==1,"Resume failed to restore simulation");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));Next(11);break;
                case 11:
                    Require((bool)Read("MenuOpen"),"Escape did not open pause menu");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.E));Next(12,.3);break;
                case 12:
                    Require(Vector3.Distance(initial,origin.transform.position)<.02f,"Menu allowed locomotion");
                    Require(!origin.GetComponentsInChildren<ReportGrabInteractor>().Any(g=>g.hasSelection),"Menu allowed physical grabbing");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));Next(27);break;
                case 27:
                    Require(!(bool)Read("MenuOpen") && origin.GetComponent<DesktopPlayer>().Captured,"Closing menu with Escape did not restore mouse look");
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                    Open("Home");
                    InputSystem.RegisterLayout(@"{""name"":""ReportUITestController"",""extend"":""XRController"",""controls"": [{""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},{""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},{""name"":""trackingState"",""layout"":""Integer"",""offset"":28},{""name"":""triggerPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32}]}");
                    hmd=InputSystem.AddDevice<XRHMD>();hand=(XRController)InputSystem.AddDevice("ReportUITestController");InputSystem.SetDeviceUsage(hand,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(hmd.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(hmd.centerEyeRotation,Quaternion.identity);InputSystem.QueueDeltaStateEvent(hmd.trackingState,3);Next(13,.4);break;
                case 13:
                    Require(!origin.GetComponent<DesktopPlayer>().IsDesktop,"Virtual headset did not select VR UI");
                    AimHand("UI_Settings");Next(14);break;
                case 14:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(15);break;
                case 15:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(16);break;
                case 16:
                    Require(Read("Page").ToString()=="Settings","VR trigger failed to click world menu");
                    Capture("07_VR_Menu");AimHand("UI_Volume",.75f);Next(21);break;
                case 21:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(22);break;
                case 22:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(23);break;
                case 23:
                    Require(Mathf.Abs(AudioListener.volume-.75f)<.06f,"VR slider failed to adjust actual audio");
                    level.ResolveStation(level.stations[0]);retryHome=retryItem.transform.position;
                    retryItem.transform.position+=Vector3.right*8;retryItem.GetComponent<Rigidbody>().position=retryItem.transform.position;
                    Open("Tasks");Next(36);break;
                case 36:AimHand("UI_Retrieve");Next(37);break;
                case 37:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(38);break;
                case 38:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(39);break;
                case 39:
                    Require(Vector3.Distance(retryItem.transform.position,retryHome)<.04f && level.Step==1 && !(bool)Read("MenuOpen"),"VR trigger retrieval did not restore the missed prop and resume without losing progress");
                    Require(UnityEngine.Object.FindObjectOfType<ReportThrowTarget>().Hits==0,"VR retrieval recorded a false target hit");level.ResetLevel();
                    for(int i=0;i<level.stations.Length;i++)level.ResolveStation(level.stations[i]);Call("Resume");Next(17);break;
                case 17:
                    Require(!(bool)Read("MenuOpen") && Time.timeScale==1 && !origin.GetComponent<DesktopPlayer>().UIBlocked,"Completion interrupted exploration with a paused menu");
                    Require((int)Read("HighestUnlocked")>=1,"Completion failed to unlock the next level");Next(32,2.7);break;
                case 32:
                    Require(level.ExitReady,"Completion did not leave physics running so the exit could open");Open("Completed");
                    Require(Widget("UI_Next").interactable,"Completed level did not unlock next UI action");
                    Capture("08_Completed");
                    Open("ConfirmReset");Next(24);break;
                case 24:Capture("09_Reset");AimHand("UI_ConfirmReset");Next(25);break;
                case 25:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(26);break;
                case 26:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(18);break;
                case 18:
                    Require(level.Step==0 && !level.Completed && !(bool)Read("MenuOpen"),"Confirmed VR reset did not clear task progress and resume");
                    Open("ConfirmQuit");Capture("10_Quit");Open("Scenes");AimHand("UI_Scene_1");Next(28);break;
                case 28:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(29);break;
                case 29:InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("triggerPressed"),0f);Next(30,.6);break;
                case 30:
                    if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex!=1 || ReportSceneTransition.IsTransitioning){Next(30,.1);break;}
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();ui=origin.GetComponents<MonoBehaviour>().First(c=>c.GetType().Name=="ReportGameUI");
                    ui.GetType().GetField("persistSettings").SetValue(ui,false);ui.GetType().GetField("saveProgress").SetValue(ui,false);
                    Require(!(bool)Read("MenuOpen") && Time.timeScale==1 && !origin.GetComponent<DesktopPlayer>().UIBlocked,"Scene load left the new level paused");
                    InputSystem.RemoveDevice(hmd);Next(31,.4);break;
                case 31:
                    Require(origin.GetComponent<DesktopPlayer>().IsDesktop && origin.GetComponent<DesktopPlayer>().Captured,"Switching to desktop after scene load failed to restore mouse control");
                    Capture("11_Next_Level_HUD");AudioListener.volume=1;
                    Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/ui-playtest.txt","PASS: actual mouse/VR UI navigation and sliders; actual mouse and VR-trigger retrieval returns unhit throw props, preserves task progress, records no false hits and resumes gameplay; unrelated tasks hide retrieval; task page cannot bypass experiments; locked scenes; pause/resume and Escape pointer restore; completion keeps simulation and gameplay active, unlocks next level and lets the physical exit open; optional completion page and VR reset; unlocked scene button loads/resumes next level and desktop fallback. Physical headset and SteamVR UI ergonomics not tested.\n");
                    Debug.Log("REPORT_UI_PLAYTEST_OK");phase=99;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception ex){failed=true;Application.logMessageReceived-=OnLog;Debug.LogError("UI phase="+phase+" "+ex);Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/ui-playtest.txt","FAIL phase="+phase+"\n"+ex);Time.timeScale=1;phase=99;EditorApplication.isPlaying=false;}
    }
    static void Capture(string name)
    {
        var camera=origin.Camera;var canvases=ui.GetComponentsInChildren<Canvas>().Where(c=>c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.3f;}Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(1600,1000,24);var pixels=new Texture2D(1600,1000,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1600,1000),0,0);pixels.Apply();
        Directory.CreateDirectory("Documentation/UIPreviews");File.WriteAllBytes("Documentation/UIPreviews/"+name+".png",pixels.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);foreach(var canvas in canvases)canvas.renderMode=RenderMode.ScreenSpaceOverlay;Canvas.ForceUpdateCanvases();
    }
}
