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
    static ReportPickupMeasure pickup;
    static ReportGrabbable[] samples;
    static Vector3[] sampleHomes;
    static ReportThrowTarget target;
    static ReportSunlightMeasure sun;
    static int phase,item;
    static double until,timeout,shotDeadline;
    static string error;
    static bool failed,oldOptionsEnabled,oldXRStart;
    static bool repeatedPickup,repeatedThrow;
    static int expectedHits;
    static Vector3 moveFrom,moveTo;
    static double moveStart,moveDuration;
    static int afterMove;
    static EnterPlayModeOptions oldOptions;
    static InputSettings.UpdateMode oldMode;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Next(int value,double delay=.25){phase=value;until=EditorApplication.timeSinceStartup+delay;}
    static void Grip(float value)=>InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("gripPressed"),value);
    static void MoveHand(Vector3 destination,int next)
    {
        moveFrom=origin.transform.Find("Camera Floor Offset/Right Controller").position;moveTo=destination;
        moveStart=EditorApplication.timeSinceStartup;moveDuration=Math.Max(.4,Vector3.Distance(moveFrom,moveTo)/1.5f);afterMove=next;Next(31,0);
    }
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
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);phase=0;item=0;failed=false;repeatedPickup=repeatedThrow=false;expectedHits=0;error=null;timeout=EditorApplication.timeSinceStartup+160;
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
                    var gameUI=origin.GetComponent<ReportGameUI>();if(gameUI!=null){gameUI.Resume();gameUI.enabled=false;}
                    pickup=UnityEngine.Object.FindObjectOfType<ReportPickupMeasure>();samples=pickup.samples;sampleHomes=samples.Select(s=>s.transform.position).ToArray();
                    target=UnityEngine.Object.FindObjectOfType<ReportThrowTarget>();sun=UnityEngine.Object.FindObjectOfType<ReportSunlightMeasure>();
                    level.Use(level.stations[0]);Require(level.Step==0,"Status bypasses real pickup");
                    Pose(samples[item].transform.position);Next(1,.4);break;
                case 1:Pose(samples[item].transform.position);Grip(1);Next(2,.3);break;
                case 2:
                    Require(samples[item].isSelected,"Could not grab hand-sized material "+item);
                    Require(pickup.Reading==item+1,"Distinct pickup count incorrect");
                    if(item==2){Next(3,.6);break;}
                    samples[item].RequestThrow(Vector3.zero);Grip(0);Next(4,.4);break;
                case 3:Require(level.Step==1,"Three pickups did not advance while holding third sample");samples[item].RequestThrow(Vector3.zero);Grip(0);Next(4,.3);break;
                case 4:
                    Debug.Log("PICKUP_RELEASE item="+item+" specimens="+string.Join(";",samples.Select(s=>s.transform.position+" v="+s.GetComponent<Rigidbody>().velocity)));
                    if(item==0 && !repeatedPickup){repeatedPickup=true;Pose(samples[0].transform.position);Next(21);break;}
                    item++;if(item<3){Pose(samples[item].transform.position);Next(1);}else{item=0;Pose(samples[0].transform.position);Next(5);}break;
                case 21:Grip(1);Next(22);break;
                case 22:
                    Require(samples[0].isSelected && pickup.Reading==1 && level.Step==0,"Repeated sample falsely counted as a new pickup");
                    Grip(0);level.ResetLevel();Next(23,.5);break;
                case 23:
                    Require(pickup.Reading==0 && target.Hits==0 && level.Step==0,"Reset retained experiment counters");
                    Pose(samples[0].transform.position);Next(1);break;
                case 5:
                    Require(level.Step==1,"Pickup phase did not advance");level.Use(level.stations[1]);Require(level.Step==1,"Status bypasses throw experiment");
                    Grip(1);Next(24);break;
                case 24:
                    Require(samples[0].isSelected,"Could not grab sample for invalid placement check");
                    MoveHand(samples[0].transform.position+Vector3.up*1.1f,29);break;
                case 29:MoveHand(target.transform.position+Vector3.up*1.3f,30);break;
                case 30:MoveHand(target.transform.position+Vector3.up*.18f,25);break;
                case 31:
                    float progress=Mathf.Clamp01((float)((EditorApplication.timeSinceStartup-moveStart)/moveDuration));
                    Pose(Vector3.Lerp(moveFrom,moveTo,progress));if(progress>=1)Next(afterMove,.3);break;
                case 25:samples[0].RequestThrow(Vector3.zero);Grip(0);Next(26,.7);break;
                case 26:
                    Require(target.Hits==0 && level.Step==1,"Carrying a sample into landing zone counted as a throw");
                    Pose(samples[0].transform.position);Next(6);break;
                case 6:Pose(samples[item].transform.position);Grip(1);Next(7);break;
                case 7:
                    Require(samples[item].isSelected,"Could not take sample for free throw item="+item+" position="+samples[item].transform.position+" velocity="+samples[item].GetComponent<Rigidbody>().velocity);
                    MoveHand(samples[item].transform.position+Vector3.up*1.1f,28);break;
                case 28:MoveHand(target.transform.position+Vector3.forward*2.3f+Vector3.up*1.3f,8);break;
                case 8:
                    // Deterministic release input; the actual Rigidbody flight and trigger detect landing.
                    float flight=.8f;var destination=target.transform.position+Vector3.up*.12f;
                    var velocity=(destination-samples[item].transform.position-Physics.gravity*.5f*flight*flight)/flight;
                    samples[item].RequestThrow(velocity);Grip(0);expectedHits++;shotDeadline=EditorApplication.timeSinceStartup+6;Next(13,.3);break;
                case 13:
                    if(target.Hits<expectedHits)
                    {
                        var track=samples[item].GetComponent<ReportThrowTrack>();
                        Require(EditorApplication.timeSinceStartup<shotDeadline,"Physical sample missed target; position="+track.transform.position+" release="+track.ReleasePosition+" velocity="+track.ReleaseVelocity+" angle="+track.LaunchAngle+" target="+target.transform.position);
                        Next(13,.15);break;
                    }
                    Require(target.DifferentItems==item+1,"Repeated sample inflated distinct landing count");
                    Debug.Log("NATURAL_SHOT_OK sample="+item+" distinct="+target.DifferentItems);
                    if(item==0 && !repeatedThrow){repeatedThrow=true;Next(27,.8);break;}
                    item++;if(item<3){Pose(samples[item].transform.position);Next(6);}else Next(14,.8);break;
                case 27:
                    var landedPosition=samples[0].transform.position;
                    var other=level.grabbables.First(g=>g is ReportMechanismHandle);float otherValue=((ReportMechanismHandle)other).Value;
                    for(int i=1;i<samples.Length;i++){samples[i].transform.position+=Vector3.right*8;var missedBody=samples[i].GetComponent<Rigidbody>();missedBody.position=samples[i].transform.position;missedBody.velocity=Vector3.zero;}
                    var retryUI=origin.GetComponent<ReportGameUI>();retryUI.enabled=true;retryUI.ShowPage(ReportUIPage.Tasks);
                    var retrieve=retryUI.GetComponentsInChildren<UnityEngine.UI.Button>().FirstOrDefault(b=>b.name=="UI_Retrieve");
                    Require(retrieve!=null,"First successful throw still has no way to retrieve remaining missed samples");retrieve.onClick.Invoke();
                    Require(target.Hits==1 && target.DifferentItems==1 && level.Step==1 && pickup.Reading==3,"Retrieval erased valid hits or pickup progress, or advanced the unfinished experiment");
                    Require(Vector3.Distance(samples[0].transform.position,landedPosition)<.01f,"Retrieval moved an already successful sample");
                    for(int i=1;i<samples.Length;i++)Require(Vector3.Distance(samples[i].transform.position,sampleHomes[i])<.04f && !samples[i].GetComponent<ReportThrowTrack>().HasThrown,"Retrieval did not restore an unhit sample to its authored home");
                    Require(Mathf.Abs(((ReportMechanismHandle)other).Value-otherValue)<.001f,"Retrieval changed another experiment's mechanism");
                    retryUI.enabled=false;Pose(samples[0].transform.position);Next(6);break;
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
                    var exit=UnityEngine.Object.FindObjectOfType<DemoExit>();var body=origin.GetComponent<CharacterController>();body.enabled=false;origin.transform.position=exit.transform.position;body.enabled=true;Physics.SyncTransforms();shotDeadline=EditorApplication.timeSinceStartup+15;Next(20,1);break;
                case 20:
                    if(SceneManager.GetActiveScene().buildIndex!=1 && EditorApplication.timeSinceStartup<shotDeadline){Next(20,.3);break;}
                    Require(SceneManager.GetActiveScene().buildIndex==1,"Physical exit did not advance");
                    File.WriteAllText("Documentation/Verification/natural-first-playtest.txt","PASS: virtual VR grip grabs 14cm samples; three distinct pickups advance without placement; duplicate pickup rejected; reset clears counters; carrying into landing zone does not count; actual release and Rigidbody flights hit target with three distinct samples; duplicate landing does not replace a missing sample; retrieval restores only unhit samples, preserves pickup and hit progress, leaves successful sample and other mechanism unchanged, and allows subsequent real throws to complete; status cannot bypass; original sun drag/light/exit and next-scene flow retained. Deterministic throw velocity supplied through player release API; physical headset not tested.\n");
                    Debug.Log("NATURAL_FIRST_PLAYTEST_OK");phase=99;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception ex)
        {
            failed=true;Application.logMessageReceived-=OnLog;Debug.LogError(ex.ToString());File.WriteAllText("Documentation/Verification/natural-first-playtest.txt","FAIL\n"+ex);phase=99;EditorApplication.isPlaying=false;
        }
    }
}
