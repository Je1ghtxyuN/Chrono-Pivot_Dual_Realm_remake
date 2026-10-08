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

// Actual grip input grabs, carries, releases and validates the introductory puzzle.
public static class PuzzlePlayCheck
{
    static XRHMD head;
    static XRController right;
    static XROrigin origin;
    static DemoLevel level;
    static int phase,item;
    static double until,timeout;
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
    static void Pose(Vector3 position,Quaternion rotation)
    {
        var offset=origin.CameraFloorOffsetObject.transform;
        InputSystem.QueueDeltaStateEvent(right.devicePosition,offset.InverseTransformPoint(position));
        InputSystem.QueueDeltaStateEvent(right.deviceRotation,Quaternion.Inverse(offset.rotation)*rotation);
        InputSystem.QueueDeltaStateEvent(right.trackingState,3);
    }
    static void Aim(Transform panel,Vector3 point)=>Pose(point-panel.forward*.9f,Quaternion.LookRotation(panel.forward));
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXRStart=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        oldMode=InputSystem.settings.updateMode;oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);phase=0;item=0;failed=false;error=null;timeout=EditorApplication.timeSinceStartup+100;
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
            if(EditorApplication.timeSinceStartup>timeout)throw new Exception("Puzzle Play timeout phase="+phase);
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(error==null,"Runtime error: "+error);
            switch(phase)
            {
                case 0:
                    InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.RegisterLayout(@"{""name"":""ReportPuzzleTestController"",""extend"":""XRController"",""controls"":[
                        {""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},
                        {""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},
                        {""name"":""trackingState"",""layout"":""Integer"",""offset"":28},
                        {""name"":""gripPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32}]}");
                    head=InputSystem.AddDevice<XRHMD>();right=(XRController)InputSystem.AddDevice("ReportPuzzleTestController");InputSystem.SetDeviceUsage(right,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(head.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(head.trackingState,3);
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    var gameUI=origin.GetComponent<ReportGameUI>();if(gameUI!=null)gameUI.enabled=false;
                    Pose(Placement.item.transform.position,Quaternion.identity);Next(1,.4);break;
                case 1:Grip(1);Next(2,.4);break;
                case 2:
                    Require(Placement.item.isSelected,"Near grip did not grab material "+item);
                    Pose(Placement.socket.position+Vector3.up*.035f,Quaternion.identity);Next(3,.5);break;
                case 3:
                    Require(Vector3.Distance(Placement.item.transform.position,Placement.socket.position)<.12f,"Carried material failed to reach socket");
                    level.Use(level.stations[0]);Require(level.Step==0,"Held material counted as placed");Grip(0);Next(4,.7);break;
                case 4:
                    Debug.Log("PUZZLE_PLACEMENT item="+item+" socket="+Placement.socket.position+" state="+string.Join(";",level.stations[0].puzzle.placements.Select(p=>p.label+":"+p.item.transform.position+":"+p.item.GetComponent<Rigidbody>().velocity+":"+p.item.isSelected)));
                    Require(!Placement.item.isSelected && Placement.IsReady,"Released material did not settle in socket "+item);
                    item++;if(item<3){Pose(Placement.item.transform.position,Quaternion.identity);Next(1);}else{item=2;Pose(Placement.item.transform.position,Quaternion.identity);Next(40);}break;
                case 40:Grip(1);Next(41);break;
                case 41:
                    Require(Placement.item.isSelected && !Placement.IsReady && Placement.item.GetComponent<Rigidbody>().constraints!=RigidbodyConstraints.FreezeAll,"Seated piece could not be picked up again");
                    Grip(0);Next(42,.4);break;
                case 42:
                    Require(Placement.IsReady,"Re-released piece did not seat again");Aim(level.stations[0].transform,level.stations[0].indicator.bounds.center);Next(5);break;
                case 5:Grip(1);Next(6);break;
                case 6:
                    Require(level.Step==1,"Placed material validation ray did not advance; attempts="+level.stations[0].Attempts+" feedback="+level.Feedback+" placements="+string.Join(";",level.stations[0].puzzle.placements.Select(p=>p.label+":"+p.IsReady+":"+Vector3.Distance(p.item.transform.position,p.socket.position)))+" ray="+string.Join(";",Physics.RaycastAll(origin.transform.Find("Camera Floor Offset/Right Controller").position,origin.transform.Find("Camera Floor Offset/Right Controller").forward,4,~0,QueryTriggerInteraction.Collide).OrderBy(h=>h.distance).Select(h=>h.collider.name)));Grip(0);
                    Aim(level.stations[1].transform,level.stations[1].puzzle.buttons[0].indicator.bounds.center);Next(7);break;
                case 7:Grip(1);Next(8);break;
                case 8:
                    Require(level.stations[1].puzzle.SelectedChoice==0,"Choice card did not receive grip input");Grip(0);Aim(level.stations[1].transform,level.stations[1].indicator.bounds.center);Next(9);break;
                case 9:Grip(1);Next(10);break;
                case 10:
                    Require(level.Step==1,"Wrong choice advanced level");Grip(0);Aim(level.stations[1].transform,level.stations[1].puzzle.buttons[1].indicator.bounds.center);Next(11);break;
                case 11:Grip(1);Next(12);break;
                case 12:
                    Grip(0);Aim(level.stations[1].transform,level.stations[1].indicator.bounds.center);Next(13);break;
                case 13:Grip(1);Next(14);break;
                case 14:
                    Require(level.Step==2,"Correct choice failed to advance");Grip(0);Aim(level.stations[2].transform,level.stations[2].puzzle.buttons[2].indicator.bounds.center);Next(15);break;
                case 15:Grip(1);Next(16);break;
                case 16:
                    Grip(0);Aim(level.stations[2].transform,level.stations[2].indicator.bounds.center);Next(17);break;
                case 17:Grip(1);Next(18,.2);break;
                case 18:
                    Require(level.Completed && !level.ExitReady,"Completion/door timing incorrect");Grip(0);Next(19,2.8);break;
                case 19:
                    Require(level.ExitReady,"Door did not fully open");
                    var exit=UnityEngine.Object.FindObjectOfType<DemoExit>();var body=origin.GetComponent<CharacterController>();body.enabled=false;origin.transform.position=exit.transform.position;body.enabled=true;Physics.SyncTransforms();Next(20,1);break;
                case 20:
                    Require(SceneManager.GetActiveScene().buildIndex==1,"Physical exit did not load next level");
                    File.WriteAllText("Documentation/Verification/puzzle-playtest.txt","PASS: actual Play Mode virtual right grip; three materials grabbed, carried, released and settled into sockets; held-piece rejection; choice card input; wrong choice rejection; correct validation; opening delay; physical exit trigger loads next level. Real headset not tested.\n");
                    Debug.Log("PUZZLE_PLAYTEST_OK");phase=99;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception ex)
        {
            failed=true;Application.logMessageReceived-=OnLog;Debug.LogError(ex.ToString());File.WriteAllText("Documentation/Verification/puzzle-playtest.txt","FAIL\n"+ex);phase=99;EditorApplication.isPlaying=false;
        }
    }
}
