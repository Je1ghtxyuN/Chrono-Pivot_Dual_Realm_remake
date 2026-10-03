using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public static class NaturalFirstPlayCheck
{
    static XRHMD head;
    static XRController right;
    static XROrigin origin;
    static DemoLevel level;
    static ReportMechanicalLauncher launcher;
    static ReportThrowTarget target;
    static ReportSunlightMeasure sun;
    static int phase,item;
    static double until,timeout,shotDeadline;
    static string error;
    static bool failed,oldOptionsEnabled,oldXRStart;
    static EnterPlayModeOptions oldOptions;
    static InputSettings.UpdateMode oldMode;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static PuzzlePlacement Placement=>level.stations[0].puzzle.placements[item];
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Next(int value,double delay=.25){phase=value;until=EditorApplication.timeSinceStartup+delay;}
    static void Grip(float value)=>InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("gripPressed"),value);
    static void Pose(Vector3 position)
    {
        var offset=origin.CameraFloorOffsetObject.transform;
        InputSystem.QueueDeltaStateEvent(right.devicePosition,offset.InverseTransformPoint(position));
        InputSystem.QueueDeltaStateEvent(right.deviceRotation,Quaternion.Inverse(offset.rotation));
        InputSystem.QueueDeltaStateEvent(right.trackingState,3);
    }
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXRStart=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        oldMode=InputSystem.settings.updateMode;oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);phase=0;item=0;failed=false;error=null;timeout=EditorApplication.timeSinceStartup+160;
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
                foreach(var device in new InputDevice[]{head,right})if(device!=null && device.added)InputSystem.RemoveDevice(device);
                InputSystem.settings.updateMode=oldMode;InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
                AssetDatabase.SaveAssets();EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;EditorApplication.Exit(failed?1:0);return;
            }
            if(EditorApplication.timeSinceStartup>timeout)throw new Exception("Natural first test timeout phase="+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(error==null,"Runtime error: "+error);
            switch(phase)
            {
                case 0:
                    InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.RegisterLayout(@"{""name"":""ReportNaturalController"",""extend"":""XRController"",""controls"":[
                        {""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},
                        {""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},
                        {""name"":""trackingState"",""layout"":""Integer"",""offset"":28},
                        {""name"":""gripPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32}]}");
                    head=InputSystem.AddDevice<XRHMD>();right=(XRController)InputSystem.AddDevice("ReportNaturalController");InputSystem.SetDeviceUsage(right,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(head.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(head.trackingState,3);
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    launcher=UnityEngine.Object.FindObjectOfType<ReportMechanicalLauncher>();target=UnityEngine.Object.FindObjectOfType<ReportThrowTarget>();sun=UnityEngine.Object.FindObjectOfType<ReportSunlightMeasure>();
                    Pose(Placement.item.transform.position);Next(1,.4);break;
                case 1:Grip(1);Next(2,.3);break;
                case 2:
                    Require(Placement.item.isSelected,"Could not grab material "+item);Pose(Placement.socket.position+Vector3.up*.035f);Next(3,.4);break;
                case 3:
                    level.Use(level.stations[0]);Require(level.Step==0,"Held material counted as placed");Grip(0);Next(4,.7);break;
                case 4:
                    Require(Placement.IsReady,"Material did not seat "+item);item++;
                    if(item<3){Pose(Placement.item.transform.position);Next(1);}else{item=0;Next(5,.7);}break;
                case 5:
                    Require(level.Step==1,"Material placement did not auto-advance");level.Use(level.stations[1]);Require(level.Step==1,"Status button bypasses throw experiment");
                    Pose(launcher.samples[item].transform.position);Next(6);break;
                case 6:Grip(1);Next(7);break;
                case 7:
                    Require(launcher.samples[item].isSelected,"Could not take sample for launcher");Pose(launcher.cup.position+Vector3.up*.035f);Next(8,.4);break;
                case 8:Grip(0);Next(9,.4);break;
                case 9:
                    Require(Vector3.Distance(launcher.samples[item].transform.position,launcher.cup.position)<.12f,"Material did not load cup");
                    Pose(launcher.pull.transform.position);Next(10);break;
                case 10:Grip(1);Next(11);break;
                case 11:
                    Require(launcher.pull.isSelected,"Could not grab physical pull handle");Pose(launcher.pull.transform.position+launcher.pull.transform.TransformDirection(launcher.pull.localAxis)*.5f);Next(12,.35);break;
                case 12:
                    Require(launcher.pull.Value>=.75f,"Tracked hand did not pull launcher rail");Grip(0);shotDeadline=EditorApplication.timeSinceStartup+6;Next(13,.3);break;
                case 13:
                    if(target.DifferentItems<item+1)
                    {
                        var track=launcher.samples[item].GetComponent<ReportThrowTrack>();
                        Require(EditorApplication.timeSinceStartup<shotDeadline,"Physical sample missed target; position="+track.transform.position+" release="+track.ReleasePosition+" velocity="+track.ReleaseVelocity+" angle="+track.LaunchAngle+" target="+target.transform.position);
                        Next(13,.15);break;
                    }
                    Debug.Log("NATURAL_SHOT_OK sample="+item+" mass="+launcher.samples[item].GetComponent<Rigidbody>().mass);
                    item++;if(item<3){Pose(launcher.samples[item].transform.position);Next(6);}else Next(14,.8);break;
                case 14:
                    Require(level.Step==2,"Three real landings did not unlock shadow experiment");Pose(sun.sunModel.transform.TransformPoint(Vector3.forward*.55f));Next(15);break;
                case 15:Grip(1);Next(16);break;
                case 16:
                    Require(sun.sunModel.isSelected,"Could not grab actual sun model");Pose(sun.sunModel.transform.position+Vector3.right*.55f);Next(17,.35);break;
                case 17:
                    Require(sun.Reading>=80 && sun.Reading<=100,"Orbital drag did not move sun east; bearing="+sun.Reading);Grip(0);Next(18,.8);break;
                case 18:
                    Require(level.Completed,"Physical shadow condition did not complete level");Require(sun.sunlight.transform.forward.x<-.5f,"Actual light did not follow sun east");Next(19,2.8);break;
                case 19:
                    Require(level.ExitReady,"Exit remained locked after experiments");
                    var exit=UnityEngine.Object.FindObjectOfType<DemoExit>();var body=origin.GetComponent<CharacterController>();body.enabled=false;origin.transform.position=exit.transform.position;body.enabled=true;Physics.SyncTransforms();Next(20,1);break;
                case 20:
                    Require(SceneManager.GetActiveScene().buildIndex==1,"Physical exit did not advance");
                    File.WriteAllText("Documentation/Verification/natural-first-playtest.txt","PASS: actual virtual VR grip; material placement auto-progress; physical rail pull/release launches three distinct masses; Rigidbody gravity/target collisions record all three landings; no click bypass; sun model orbital dragging changes directional light; auto completion, physical exit and next scene. Physical headset not tested.\n");
                    Debug.Log("NATURAL_FIRST_PLAYTEST_OK");phase=99;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception ex)
        {
            failed=true;Application.logMessageReceived-=OnLog;Debug.LogError(ex.ToString());File.WriteAllText("Documentation/Verification/natural-first-playtest.txt","FAIL\n"+ex);phase=99;EditorApplication.isPlaying=false;
        }
    }
}
