using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;
using XRController=UnityEngine.InputSystem.XR.XRController;

public static class GrabPlayCheck
{
    static Keyboard keyboard;
    static Mouse mouse;
    static XRHMD head;
    static XRController left,right;
    static XROrigin origin;
    static DesktopPlayer player;
    static DemoLevel level;
    static ReportGrabbable prop;
    static ReportGrabInteractor desktopGrab,leftGrab,rightGrab;
    static Vector3 initial,held;
    static float originalHoldDistance,coarseDelta,handleBefore,pickupDistance;
    static ReportMechanismHandle fineHandle;
    static int phase;
    static double until,timeout;
    static string error;
    static bool failed,oldOptionsEnabled,oldXRStart;
    static EnterPlayModeOptions oldOptions;
    static InputSettings.UpdateMode oldInputMode;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Next(int value,double delay=.2){phase=value;until=EditorApplication.timeSinceStartup+delay;}
    static void Keys(params Key[] keys)=>InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));
    static void Grip(XRController device,float value)=>InputSystem.QueueDeltaStateEvent(device.GetChildControl<ButtonControl>("gripPressed"),value);
    static void Pose(XRController device,Vector3 position,Quaternion rotation)
    {
        var offset=origin.CameraFloorOffsetObject.transform;
        InputSystem.QueueDeltaStateEvent(device.devicePosition,offset.InverseTransformPoint(position));
        InputSystem.QueueDeltaStateEvent(device.deviceRotation,Quaternion.Inverse(offset.rotation)*rotation);
        InputSystem.QueueDeltaStateEvent(device.trackingState,3);
    }
    static void AimDesktop()
    {
        var body=origin.GetComponent<CharacterController>();body.enabled=false;
        var p=prop.transform.position+Vector3.forward*2;p.y=level.spawn.y;origin.transform.position=p;body.enabled=true;
        var look=Quaternion.LookRotation(prop.transform.position-origin.Camera.transform.position);
        origin.transform.rotation=Quaternion.Euler(0,look.eulerAngles.y,0);
        origin.Camera.transform.localRotation=Quaternion.Euler(Mathf.DeltaAngle(0,look.eulerAngles.x),0,0);Physics.SyncTransforms();
    }
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXRStart=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        oldInputMode=InputSystem.settings.updateMode;oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        phase=0;failed=false;error=null;timeout=EditorApplication.timeSinceStartup+100;
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
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).InitManagerOnStart=oldXRStart;
                foreach(var device in new InputDevice[]{keyboard,mouse,head,left,right})if(device!=null && device.added)InputSystem.RemoveDevice(device);
                InputSystem.settings.updateMode=oldInputMode;InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
                AssetDatabase.SaveAssets();EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;EditorApplication.Exit(failed?1:0);return;
            }
            if(EditorApplication.timeSinceStartup>timeout)throw new Exception("Grab test timeout phase="+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(error==null,"Runtime error: "+error);
            switch(phase)
            {
                case 0:
                    InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();player=origin.GetComponent<DesktopPlayer>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    // UI has a separate input/pause test; isolate this physical interaction check.
                    var gameUI=origin.GetComponent<ReportGameUI>();if(gameUI!=null){gameUI.Resume();gameUI.enabled=false;}
                    prop=GameObject.Find("HubMaterialCube_0").transform.Find("材料样品").GetComponent<ReportGrabbable>();initial=prop.transform.position;
                    if(prop.GetComponent<ReportThrowTrack>()==null)prop.gameObject.AddComponent<ReportThrowTrack>();
                    desktopGrab=origin.Camera.GetComponent<ReportGrabInteractor>();
                    leftGrab=origin.transform.Find("Camera Floor Offset/Left Controller").GetComponent<ReportGrabInteractor>();
                    rightGrab=origin.transform.Find("Camera Floor Offset/Right Controller").GetComponent<ReportGrabInteractor>();
                    Next(1,.4);break;
                case 1:
                    Require(player.IsDesktop,"No-headset test must enter desktop mode");
                    InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1,position=new Vector2(1000,20)});Next(2);break;
                case 2:
                    Require(player.Captured,"Desktop pointer did not capture");InputSystem.QueueStateEvent(mouse,new MouseState());AimDesktop();Next(3);break;
                case 3:
                    pickupDistance=Vector3.Distance(prop.transform.position,origin.Camera.transform.position);Keys(Key.E);Next(4,.4);break;
                case 4:
                    Require(prop.isSelected && desktopGrab.IsSelecting(prop),"E did not physically grab the specimen");
                    Require(Mathf.Abs(Vector3.Distance(prop.transform.position,origin.Camera.transform.position)-pickupDistance)<.12f,"Desktop pickup pulled the object abruptly to a fixed distance");
                    held=prop.transform.position;origin.transform.position+=Vector3.right*.8f;origin.Camera.transform.localRotation=Quaternion.Euler(-25,0,0);Next(5,.5);break;
                case 5:
                    Require(Vector3.Distance(held,prop.transform.position)>.5f,"Held specimen did not follow camera movement");
                    Require(Vector3.Distance(prop.transform.position,desktopGrab.attachTransform.position)<.12f,"Specimen did not reach configured desktop hold point");
                    originalHoldDistance=desktopGrab.desktopHoldDistance;InputSystem.QueueStateEvent(mouse,new MouseState{scroll=new Vector2(0,120)});Next(32);break;
                case 32:
                    Require(prop.isSelected && desktopGrab.desktopHoldDistance<originalHoldDistance-.1f,"Scroll up did not bring the held object closer");
                    InputSystem.QueueStateEvent(mouse,new MouseState{scroll=new Vector2(0,-120)});Next(33);break;
                case 33:
                    Require(Mathf.Abs(desktopGrab.desktopHoldDistance-originalHoldDistance)<.01f,"Scroll down did not move the object back out");
                    InputSystem.QueueStateEvent(mouse,new MouseState{scroll=new Vector2(0,2400)});Next(34);break;
                case 34:
                    Require(desktopGrab.desktopHoldDistance>=.45f && desktopGrab.desktopHoldDistance<originalHoldDistance,"Close placement distance was not limited");
                    InputSystem.QueueStateEvent(mouse,new MouseState{scroll=new Vector2(0,-4800)});Next(35);break;
                case 35:
                    Require(desktopGrab.desktopHoldDistance<=2.5f && prop.isSelected,"Placement distance exceeded reach or released the item");
                    desktopGrab.desktopHoldDistance=originalHoldDistance;held=prop.transform.position;Keys();Next(6,.35);break;
                case 6:
                    Require(!prop.isSelected && !prop.GetComponent<Rigidbody>().isKinematic,"E release did not return specimen to physics");
                    Require(prop.transform.position.y<held.y-.05f,"Released specimen did not fall under gravity; before="+held+" after="+prop.transform.position+" velocity="+prop.GetComponent<Rigidbody>().velocity+" gravity="+prop.GetComponent<Rigidbody>().useGravity);
                    level.ResetLevel();Next(7);break;
                case 7:
                    Require(Vector3.Distance(prop.transform.position,initial)<.04f,"Reset did not return dropped specimen to base");
                    AimDesktop();InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1});Next(8,.4);break;
                case 8:
                    Require(prop.isSelected,"Left mouse button did not grab");origin.Camera.transform.localRotation=Quaternion.Euler(-25,0,0);Keys(Key.F);Next(30,.6);break;
                case 30:
                    Require(prop.isSelected && desktopGrab.ThrowCharge01>.1f,"F did not charge a held object");Keys();Next(31,.15);break;
                case 31:
                    Require(!prop.isSelected && prop.GetComponent<ReportThrowTrack>().HasThrown,"Releasing F did not physically throw the object");
                    Require(prop.GetComponent<ReportThrowTrack>().ReleaseVelocity.magnitude>2 && Vector3.Dot(prop.GetComponent<ReportThrowTrack>().ReleaseVelocity,origin.Camera.transform.forward)>2,"Throw did not use aimed charged velocity");
                    Require(!desktopGrab.hasSelection,"Held grab button immediately recaptured thrown object");Keys(Key.Escape);Next(9);break;
                case 9:
                    Require(!prop.isSelected && !player.Captured,"Escape did not release held object and pointer");
                    Keys();InputSystem.QueueStateEvent(mouse,new MouseState());level.ResetLevel();
                    InputSystem.RegisterLayout(@"{""name"":""ReportGrabTestController"",""extend"":""XRController"",""controls"":[
                        {""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},
                        {""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},
                        {""name"":""trackingState"",""layout"":""Integer"",""offset"":28},
                        {""name"":""gripPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32},
                        {""name"":""grip"",""layout"":""Axis"",""offset"":36},
                        {""name"":""triggerPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":40}]}");
                    head=InputSystem.AddDevice<XRHMD>();left=(XRController)InputSystem.AddDevice("ReportGrabTestController");right=(XRController)InputSystem.AddDevice("ReportGrabTestController");
                    InputSystem.SetDeviceUsage(left,CommonUsages.LeftHand);InputSystem.SetDeviceUsage(right,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(head.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(head.trackingState,3);
                    Pose(left,initial+Vector3.left*2,Quaternion.identity);Pose(right,initial+Vector3.forward,Quaternion.LookRotation(Vector3.back));Next(10,.4);break;
                case 10:
                    Require(!player.IsDesktop,"Tracked virtual headset did not select VR");Grip(right,1);Next(11,.4);break;
                case 11:
                    Require(prop.isSelected && rightGrab.IsSelecting(prop),"Right grip ray did not grab specimen");
                    var avatar=origin.GetComponentInChildren<LowPolyPlayerAvatar>();
                    Require(Vector3.Distance(avatar.rightHand.position,rightGrab.transform.position)<.01f,"Avatar hand did not follow tracked controller");
                    held=prop.transform.position;Pose(right,rightGrab.transform.position+Vector3.up*.4f,Quaternion.LookRotation(Vector3.back));Next(12,.4);break;
                case 12:
                    Require(Vector3.Distance(prop.transform.position,held)>.25f,"VR specimen did not follow tracked hand");
                    level.ResetLevel();Next(13,.3);break;
                case 13:
                    Require(!prop.isSelected && Vector3.Distance(prop.transform.position,initial)<.04f,"Reset while holding did not cancel and restore grab");
                    Pose(right,initial,Quaternion.identity);Next(14);break;
                case 14:
                    Require(!prop.isSelected,"Held grip automatically regrabbed after reset");Grip(right,0);Pose(right,initial+Vector3.right*2,Quaternion.identity);
                    Pose(left,initial,Quaternion.LookRotation(Vector3.up));Next(15);break;
                case 15:InputSystem.QueueDeltaStateEvent(left.GetChildControl<AxisControl>("grip"),.9f);Next(16,.4);break;
                case 16:
                    Require(prop.isSelected && leftGrab.IsSelecting(prop),"Analog left grip near touch did not grab specimen");
                    Pose(right,prop.transform.position,Quaternion.identity);Grip(right,1);Next(17);break;
                case 17:
                    Require(prop.interactorsSelecting.Count==1 && leftGrab.IsSelecting(prop) && !rightGrab.hasSelection,"Both hands simultaneously selected one specimen");
                    InputSystem.QueueDeltaStateEvent(left.GetChildControl<AxisControl>("grip"),0f);Grip(right,0);Next(18);break;
                case 18:
                    Require(!prop.isSelected,"Releasing left grip did not release near grab");level.ResetLevel();Pose(right,initial,Quaternion.identity);Next(19);break;
                case 19:Grip(right,1);Next(20,.3);break;
                case 20:
                    Require(prop.isSelected && rightGrab.IsSelecting(prop),"Near right grip grab failed");
                    InputSystem.QueueDeltaStateEvent(right.trackingState,0);Next(21);break;
                case 21:
                    Require(!prop.isSelected,"Lost hand tracking did not release object");Pose(right,initial,Quaternion.identity);Next(22);break;
                case 22:
                    Require(!prop.isSelected,"Tracking recovery regrabbed with already held grip");Grip(right,0);level.ResetLevel();
                    InputSystem.QueueDeltaStateEvent(head.trackingState,0);Next(23);break;
                case 23:
                    Require(!player.IsDesktop,"Temporary HMD tracking loss switched to desktop and overwrote tracked poses");
                    Require(!origin.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider>().enabled,"Locomotion continued with untracked HMD");
                    InputSystem.QueueDeltaStateEvent(head.trackingState,3);Next(24);break;
                case 24:
                    Require(!player.IsDesktop && origin.GetComponent<UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider>().enabled,"HMD recovery failed to restore VR locomotion");
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("triggerPressed"),1f);Next(25);break;
                case 25:
                    Require(!origin.GetComponent<ReportSceneBrowser>().nextScene.action.IsPressed(),"Interaction trigger also requests a scene change");
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("triggerPressed"),0f);level.ResetLevel();Pose(right,initial,Quaternion.identity);Next(26);break;
                case 26:Grip(right,1);Next(27,.3);break;
                case 27:
                    Require(prop.isSelected,"Cannot grab before VR throwing check");
                    Pose(right,rightGrab.transform.position+Vector3.up*.4f+Vector3.forward*.15f,Quaternion.identity);Next(28,.08);break;
                case 28:Grip(right,0);Next(29,.12);break;
                case 29:
                    Require(!prop.isSelected && prop.GetComponent<ReportThrowTrack>().HasThrown,"Swing and grip release failed to register a physical VR throw");
                    Require(prop.GetComponent<ReportThrowTrack>().ReleaseVelocity.magnitude>.1f,"VR throw lost tracked hand velocity");
                    foreach(var device in new InputDevice[]{head,left,right})if(device!=null && device.added)InputSystem.RemoveDevice(device);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(2);Next(36,.6);break;
                case 36:
                    if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex!=2){Next(36);break;}
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();player=origin.GetComponent<DesktopPlayer>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    var ui=origin.GetComponent<ReportGameUI>();ui.Resume();ui.enabled=false;player.controlMode=ReportControlMode.Desktop;
                    fineHandle=UnityEngine.Object.FindObjectsOfType<ReportAcousticMeasure>().First(m=>m.experiment==ReportAcousticReading.Resonance).frequency;
                    prop=fineHandle;desktopGrab=origin.Camera.GetComponent<ReportGrabInteractor>();AimDesktop();Next(37,.4);break;
                case 37:player.ResumePointer();Keys();InputSystem.QueueStateEvent(mouse,new MouseState());Next(38);break;
                case 38:Keys(Key.E);Next(39,.4);break;
                case 39:
                    Require(fineHandle.isSelected,"Could not grab the real acoustic frequency wheel");handleBefore=fineHandle.Value;
                    InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(20,0)});Next(40);break;
                case 40:
                    coarseDelta=fineHandle.Value-handleBefore;Require(coarseDelta>.001f,"Mouse drag did not adjust the actual wheel");handleBefore=fineHandle.Value;
                    Keys(Key.E,Key.LeftShift);InputSystem.QueueStateEvent(mouse,new MouseState{delta=new Vector2(20,0)});Next(41);break;
                case 41:
                    Require(Mathf.Abs(fineHandle.Value-handleBefore-coarseDelta*.2f)<Mathf.Max(.01f,coarseDelta*.05f),"Shift did not provide precise adjustment: normal="+coarseDelta+" fine="+(fineHandle.Value-handleBefore));Keys();
                    Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/grab-playtest.txt","PASS: desktop E/mouse grab retaining pickup distance, follow, scroll placement distance and reach limits, release/gravity, charged throw, no recapture; VR grip/analog near/ray grabs, hand ownership, reset, tracking loss/recovery, swing-and-release throw; real acoustic wheel coarse drag and Shift fine adjustment. Physical headset and SteamVR streaming not tested.\n");
                    Debug.Log("REPORT_GRAB_PLAYTEST_OK");phase=99;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception ex)
        {
            failed=true;Application.logMessageReceived-=OnLog;Debug.LogError("phase="+phase+" "+ex);
            Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/grab-playtest.txt","FAIL phase="+phase+"\n"+ex);phase=99;EditorApplication.isPlaying=false;
        }
    }
}
