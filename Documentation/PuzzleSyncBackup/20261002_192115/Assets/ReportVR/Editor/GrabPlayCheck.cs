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
                    prop=GameObject.Find("HubMaterialCube_0").transform.Find("材料样品").GetComponent<ReportGrabbable>();initial=prop.transform.position;
                    desktopGrab=origin.Camera.GetComponent<ReportGrabInteractor>();
                    leftGrab=origin.transform.Find("Camera Floor Offset/Left Controller").GetComponent<ReportGrabInteractor>();
                    rightGrab=origin.transform.Find("Camera Floor Offset/Right Controller").GetComponent<ReportGrabInteractor>();
                    Next(1,.4);break;
                case 1:
                    Require(player.IsDesktop,"No-headset test must enter desktop mode");
                    InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1,position=new Vector2(1000,20)});Next(2);break;
                case 2:
                    Require(player.Captured,"Desktop pointer did not capture");InputSystem.QueueStateEvent(mouse,new MouseState());AimDesktop();Next(3);break;
                case 3:Keys(Key.E);Next(4,.4);break;
                case 4:
                    Require(prop.isSelected && desktopGrab.IsSelecting(prop),"E did not physically grab the specimen");
                    held=prop.transform.position;origin.transform.position+=Vector3.right*.8f;origin.Camera.transform.localRotation=Quaternion.Euler(-25,0,0);Next(5,.5);break;
                case 5:
                    Require(Vector3.Distance(held,prop.transform.position)>.5f,"Held specimen did not follow camera movement");
                    Require(Vector3.Distance(prop.transform.position,desktopGrab.attachTransform.position)<.12f,"Specimen did not reach configured desktop hold point");
                    held=prop.transform.position;Keys();Next(6,.35);break;
                case 6:
                    Require(!prop.isSelected && !prop.GetComponent<Rigidbody>().isKinematic,"E release did not return specimen to physics");
                    Require(prop.transform.position.y<held.y-.05f,"Released specimen did not fall under gravity; before="+held+" after="+prop.transform.position+" velocity="+prop.GetComponent<Rigidbody>().velocity+" gravity="+prop.GetComponent<Rigidbody>().useGravity);
                    level.ResetLevel();Next(7);break;
                case 7:
                    Require(Vector3.Distance(prop.transform.position,initial)<.04f,"Reset did not return dropped specimen to base");
                    AimDesktop();InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1});Next(8,.4);break;
                case 8:
                    Require(prop.isSelected,"Left mouse button did not grab");Keys(Key.Escape);Next(9);break;
                case 9:
                    Require(!prop.isSelected && !player.Captured,"Escape did not release held object and pointer");
                    Keys();InputSystem.QueueStateEvent(mouse,new MouseState());level.ResetLevel();
                    InputSystem.RegisterLayout(@"{""name"":""ReportGrabTestController"",""extend"":""XRController"",""controls"":[
                        {""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},
                        {""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},
                        {""name"":""trackingState"",""layout"":""Integer"",""offset"":28},
                        {""name"":""gripPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32},
                        {""name"":""grip"",""layout"":""Axis"",""offset"":36}]}");
                    head=InputSystem.AddDevice<XRHMD>();left=(XRController)InputSystem.AddDevice("ReportGrabTestController");right=(XRController)InputSystem.AddDevice("ReportGrabTestController");
                    InputSystem.SetDeviceUsage(left,CommonUsages.LeftHand);InputSystem.SetDeviceUsage(right,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(head.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(head.trackingState,3);
                    Pose(left,initial+Vector3.left*2,Quaternion.identity);Pose(right,initial+Vector3.forward,Quaternion.LookRotation(Vector3.back));Next(10,.4);break;
                case 10:
                    Require(!player.IsDesktop,"Tracked virtual headset did not select VR");Grip(right,1);Next(11,.4);break;
                case 11:
                    Require(prop.isSelected && rightGrab.IsSelecting(prop),"Right grip ray did not grab specimen");
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
                    Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/grab-playtest.txt","PASS: actual Play Mode input; desktop E and mouse grab, follow, release and gravity, Escape release; right VR grip ray grab and tracked follow; analog left grip near grab; single-hand ownership; reset while held; release latch; tracking loss/recovery. Physical headset and SteamVR streaming not tested.\n");
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
