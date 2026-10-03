using System;
using System.Collections.Generic;
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
using Object=UnityEngine.Object;

// Actual virtual controller grabs/drags/releases; forces, collision and measurement scripts run in Play Mode.
public static class NaturalAllPlayCheck
{
    static XRHMD head;
    static XRController hand;
    static XROrigin origin;
    static DemoLevel level;
    static IEnumerator<float> routine;
    static double until,deadline;
    static string runtimeError;
    static bool finishing,failed,oldOptionsEnabled,oldXR;
    static EnterPlayModeOptions oldOptions;
    static InputSettings.UpdateMode oldMode;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static readonly List<string> results=new List<string>();
    static Vector3 lastHandPosition;
    static int firstIndex=1;
    public static void Remaining(){firstIndex=7;Run();}
    public static void LastThree(){firstIndex=12;Run();}
    public static void LastOne(){firstIndex=14;Run();}
    static void Require(bool condition,string message){if(!condition)throw new Exception("Scene "+SceneManager.GetActiveScene().buildIndex+" step "+(level!=null?level.Step:-1)+": "+message);}
    static void Grip(float value)=>InputSystem.QueueDeltaStateEvent(hand.GetChildControl<ButtonControl>("gripPressed"),value);
    static void Pose(Vector3 position,Quaternion rotation)
    {
        lastHandPosition=position;
        var offset=origin.CameraFloorOffsetObject.transform;
        InputSystem.QueueDeltaStateEvent(hand.devicePosition,offset.InverseTransformPoint(position));
        InputSystem.QueueDeltaStateEvent(hand.deviceRotation,Quaternion.Inverse(offset.rotation)*rotation);
        InputSystem.QueueDeltaStateEvent(hand.trackingState,3);
    }
    static void Pose(Vector3 position)=>Pose(position,Quaternion.identity);
    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXR=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        oldMode=InputSystem.settings.updateMode;oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[firstIndex].path);results.Clear();failed=finishing=false;runtimeError=null;routine=Exercises().GetEnumerator();until=0;deadline=EditorApplication.timeSinceStartup+500;
        Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void OnLog(string text,string stack,LogType kind){if(kind==LogType.Error || kind==LogType.Exception)runtimeError=text;}
    static void Tick()
    {
        try
        {
            if(finishing)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                EditorSettings.enterPlayModeOptions=oldOptions;EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).InitManagerOnStart=oldXR;
                foreach(var d in new InputDevice[]{head,hand})if(d!=null && d.added)InputSystem.RemoveDevice(d);
                InputSystem.settings.updateMode=oldMode;InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
                EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;AssetDatabase.SaveAssets();EditorApplication.Exit(failed?1:0);return;
            }
            if(EditorApplication.timeSinceStartup>deadline)throw new Exception("Natural experiments timed out");
            if(!EditorApplication.isPlaying || EditorApplication.timeSinceStartup<until)return;
            Require(runtimeError==null,"Runtime error: "+runtimeError);
            if(routine.MoveNext()){until=EditorApplication.timeSinceStartup+routine.Current;return;}
            results.Insert(0,"PASS: scenes "+firstIndex+"..14 / "+((15-firstIndex)*3)+" natural tasks. Virtual XR grip/drag/release; actual collisions and measurement updates; calibrated ballistic releases; status cannot bypass; reset and physical exits. First scene is covered by NaturalFirstPlayCheck. Physical headset and SteamVR streaming are not tested.");
            File.WriteAllLines("Documentation/Verification/natural-all-playtest.txt",results);Debug.Log("NATURAL_ALL_PLAYTEST_OK scenes="+(15-firstIndex)+" tasks="+((15-firstIndex)*3));finishing=true;EditorApplication.isPlaying=false;
        }
        catch(Exception e)
        {
            failed=finishing=true;Application.logMessageReceived-=OnLog;Debug.LogError(e);File.WriteAllLines("Documentation/Verification/natural-all-playtest.txt",new[]{"FAIL",e.ToString()}.Concat(results));EditorApplication.isPlaying=false;
        }
    }
    static IEnumerable<float> Grab(ReportGrabbable item)
    {
        Pose(item.transform.position);yield return .15f;Grip(1);yield return .2f;Require(item.isSelected,"Could not grab "+item.name+" position="+item.transform.position+" hand="+lastHandPosition+" velocity="+item.GetComponent<Rigidbody>().velocity+" nearby="+string.Join(";",Physics.OverlapSphere(lastHandPosition,.16f).Select(c=>c.name)));
    }
    static IEnumerable<float> Move(ReportGrabbable item,Vector3 destination,float settle=.45f)
    {
        foreach(var t in Grab(item))yield return t;
        foreach(var t in MoveHand(item.transform.position+Vector3.up*.45f))yield return t;
        foreach(var t in MoveHand(destination+Vector3.up*.4f))yield return t;
        foreach(var t in MoveHand(destination+Vector3.up*.025f))yield return t;
        yield return .35f;
        if(item.name.Contains("雪球层"))Debug.Log("NATURAL_STACK_HELD "+item.name+" dest="+destination+" actual="+item.transform.position+" selected="+item.isSelected+" kinematic="+item.GetComponent<Rigidbody>().isKinematic+" time="+Time.time);
        Grip(0);yield return settle;
        if(item.name.Contains("雪球层"))
        {
            Debug.Log("NATURAL_STACK_RELEASE "+item.name+" pos="+item.transform.position+" velocity="+item.GetComponent<Rigidbody>().velocity+" collider="+item.GetComponent<Collider>().bounds+" bond="+item.GetComponent<ReportSnowBond>().Bonded);
            foreach(var p in One<ReportStackMeasure>().pieces)Debug.Log("NATURAL_STACK_STATE "+p.name+" pos="+p.transform.position+" selected="+p.isSelected+" bodyKin="+p.GetComponent<Rigidbody>().isKinematic+" bodyV="+p.GetComponent<Rigidbody>().velocity+" bond="+p.GetComponent<ReportSnowBond>().Bonded);
        }
    }
    static IEnumerable<float> MoveHand(Vector3 destination)
    {
        var from=lastHandPosition;
        for(int i=1;i<=16;i++){Pose(Vector3.Lerp(from,destination,i/16f));yield return .025f;}
    }
    static IEnumerable<float> Turn(ReportMechanismHandle h,float destination,float settle=.65f)
    {
        var axis=h.transform.TransformDirection(h.localAxis).normalized;var direction=Vector3.ProjectOnPlane(Vector3.up,axis).normalized;if(direction.sqrMagnitude<.01f)direction=Vector3.ProjectOnPlane(Vector3.forward,axis).normalized;
        var center=h.transform.position;
        float radius=.35f;
        if(h.rotationFromPosition)
        {
            var collider=h.colliders.FirstOrDefault();
            if(collider!=null){var offset=Vector3.ProjectOnPlane(collider.bounds.center-center,axis);if(offset.magnitude>.15f){direction=offset.normalized;radius=offset.magnitude;}}
        }
        var rotation=Quaternion.FromToRotation(Vector3.up,direction);var start=h.rotationFromPosition?center+direction*radius:center;
        Pose(start,rotation);yield return .15f;Grip(1);yield return .2f;Require(h.isSelected,"Cannot grab handle "+h.name);
        float previous=h.Value,range=h.valueRange.y-h.valueRange.x;
        if(h.motion==ReportHandleMotion.Slide){Pose(start+axis*(destination-previous)/range*h.vrTravelForFullRange,rotation);yield return .25f;}
        else
        {
            float degrees=(destination-previous)/range*h.vrDegreesForFullRange;int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(degrees)/55));
            for(int i=1;i<=steps;i++){var q=Quaternion.AngleAxis(degrees*i/steps,axis);Pose(h.rotationFromPosition?center+q*direction*radius:center,q*rotation);yield return .12f;}
        }
        Require(Mathf.Abs(h.Value-destination)<Mathf.Max(.03f,range*.025f),"Handle gesture incorrect "+h.name+": "+h.Value+" wanted "+destination);
        Grip(0);yield return settle;
    }
    static void Step(int expected){Require(level.Step==expected,"Expected step "+expected+"; "+level.Feedback);}
    static T One<T>() where T:UnityEngine.Object=>Object.FindObjectOfType<T>();
    static IEnumerable<float> Mirror()
    {foreach(var t in Turn(One<ReportMirrorMeasure>().mirror,135))yield return t;Require(One<ReportMirrorMeasure>().Reading==1,"Actual reflected ray missed receiver");}
    static IEnumerable<float> Power(ReportElectromagnetMeasure power)
    {
        foreach(var t in Move(power.core,power.coreSeat.position))yield return t;
        foreach(var t in Turn(power.current,2.5f))yield return t;
        Require(power.Energized && power.Field>2,"Magnetic field / core did not energize");
    }
    static IEnumerable<float> Throw(ReportThrowTarget target)
    {
        var item=target.acceptedItems[0].GetComponent<ReportGrabbable>();foreach(var t in Grab(item))yield return t;
        var release=target.launchArea.position;Pose(release);yield return .25f;
        var landing=target.transform.position+Vector3.up*.14f;var delta=landing-release;float flight=.65f;
        item.RequestThrow(new Vector3(delta.x/flight,delta.y/flight+Physics.gravity.magnitude*flight*.5f,delta.z/flight));
        Grip(0);yield return 1.25f;var track=item.GetComponent<ReportThrowTrack>();Require(target.Hits>0,"Actual ballistic throw failed target "+target.name+" target="+target.transform.position+" release="+track.ReleasePosition+" velocity="+track.ReleaseVelocity+" item="+item.transform.position+" hit="+track.LastCollision);
    }
    static IEnumerable<float> Exercises()
    {
        yield return .3f;
        InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.RegisterLayout(@"{""name"":""ReportExperimentController"",""extend"":""XRController"",""controls"":[{""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},{""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},{""name"":""trackingState"",""layout"":""Integer"",""offset"":28},{""name"":""gripPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":32}]}");
        head=InputSystem.AddDevice<XRHMD>();hand=(XRController)InputSystem.AddDevice("ReportExperimentController");InputSystem.SetDeviceUsage(hand,CommonUsages.RightHand);
        InputSystem.QueueDeltaStateEvent(head.centerEyePosition,Vector3.up*1.7f);InputSystem.QueueDeltaStateEvent(head.trackingState,3);
        for(int index=firstIndex;index<15;index++)
        {
            if(SceneManager.GetActiveScene().buildIndex!=index)SceneManager.LoadScene(index);yield return .6f;
            origin=One<XROrigin>();level=One<DemoLevel>();Grip(0);Pose(origin.transform.position+Vector3.up*1.5f);yield return .15f;
            Require(level.Step==0,"Untouched scene advanced automatically");level.Use(level.stations[0]);Require(level.Step==0,"Status bypassed experiment");
            switch(index)
            {
                case 1:case 10:
                {
                    var boat=One<ReportBuoyancyMeasure>();foreach(var t in Move(boat.seats[0].item,boat.seats[0].transform.position,.8f))yield return t;Step(1);
                    foreach(var t in Move(boat.seats[1].item,boat.seats[1].transform.position,.8f))yield return t;Step(2);
                    foreach(var t in Move(boat.seats[2].item,boat.seats[2].transform.position,.5f))yield return t;Require(boat.Reading>1,"Copper did not overload boat");
                    foreach(var t in Turn(boat.gate,1,.6f))yield return t;Require(boat.Transit==0,"Overloaded boat transited gate");
                    foreach(var t in Move(boat.seats[2].item,boat.transform.position+boat.transform.forward*-.4f+Vector3.up*1.14f,.5f))yield return t;
                    foreach(var t in Turn(boat.gate,0,.2f))yield return t;foreach(var t in Turn(boat.gate,1,5))yield return t;
                    Require(boat.Transit>.98f,"Boat did not transit after safe reload");break;
                }
                case 2:
                {
                    var bell=Object.FindObjectsOfType<ReportAcousticMeasure>().First(m=>m.experiment==ReportAcousticReading.Strike);
                    foreach(var t in Grab(bell.mallet))yield return t;
                    Pose(bell.transform.position+bell.transform.right*.45f);yield return .2f;Pose(bell.transform.position);yield return .18f;Grip(0);yield return .7f;
                    Require(bell.Strikes>0,"Real mallet collision did not ring bell");Step(1);
                    foreach(var t in Turn(Object.FindObjectsOfType<ReportAcousticMeasure>().First(m=>m.experiment==ReportAcousticReading.Resonance).frequency,100,.8f))yield return t;Step(2);
                    foreach(var t in Turn(Object.FindObjectsOfType<ReportAcousticMeasure>().First(m=>m.experiment==ReportAcousticReading.Beats).frequency,100,.8f))yield return t;break;
                }
                case 3:case 13:
                {
                    var compass=One<ReportCompassMeasure>();foreach(var t in Turn(compass.needle,250,3.6f))yield return t;Require(compass.Reading<5,"Needle failed to return south");Step(1);
                    if(index==3){foreach(var t in Turn(Object.FindObjectsOfType<ReportHandleMeasure>().First(m=>m.label=="观测环照准角").handle,45))yield return t;Step(2);foreach(var t in Mirror())yield return t;}
                    else{foreach(var t in Power(One<ReportElectromagnetMeasure>()))yield return t;Step(2);foreach(var t in Throw(One<ReportThrowTarget>()))yield return t;}break;
                }
                case 4:
                {
                    var p=One<ReportElectromagnetMeasure>();foreach(var t in Move(p.core,p.coreSeat.position,.8f))yield return t;Step(1);
                    foreach(var t in Turn(p.current,4,.7f))yield return t;Require(level.Step==1 && !p.Energized,"Overcurrent accepted");
                    foreach(var t in Turn(p.current,2.5f,.8f))yield return t;Step(2);
                    foreach(var t in Turn(p.latch,1,.8f))yield return t;break;
                }
                case 5:
                {
                    var p=One<ReportHydraulicMeasure>();foreach(var t in Turn(p.inlet,0,.8f))yield return t;Step(1);
                    foreach(var t in Move(p.seal,p.sealSeat.position,.8f))yield return t;Step(2);
                    foreach(var t in Turn(p.inlet,1,.2f))yield return t;foreach(var t in Turn(p.plunger,.4f,.8f))yield return t;
                    Require(Mathf.Abs(p.Reading-.1f)<.005f,"Hydraulic volume ratio wrong");break;
                }
                case 6:
                {
                    var p=One<ReportPendulumMeasure>();foreach(var t in Turn(p.angle,20,6.5f))yield return t;Require(p.Period>2,"No actual pendulum crossing period");Step(1);
                    foreach(var t in Turn(p.length,1,.15f))yield return t;foreach(var t in Turn(p.angle,20,6.5f))yield return t;Step(2);
                    var lc=One<ReportLCMeasure>();float frequency=1/p.Period,capacitance=1/(4*Mathf.PI*Mathf.PI*lc.inductance*frequency*frequency);
                    foreach(var t in Turn(lc.capacitance,capacitance,1))yield return t;break;
                }
                case 7:
                {
                    var targets=Object.FindObjectsOfType<ReportThrowTarget>().OrderBy(t=>t.name).ToArray();
                    foreach(var t in Throw(targets[0]))yield return t;Step(1);foreach(var t in Throw(targets[1]))yield return t;Step(2);
                    var heat=One<ReportHeatMeasure>();foreach(var t in Move(heat.sample,heat.warmZone.position,2))yield return t;break;
                }
                case 8:
                {
                    foreach(var p in level.stations[0].puzzle.placements)foreach(var t in Move(p.item,p.socket.position,.4f))yield return t;yield return .7f;Step(1);
                    var domino=One<ReportDominoMeasure>();var ball=level.grabbables.First(g=>g.name.Contains("击牌球"));foreach(var t in Grab(ball))yield return t;
                    var first=domino.dominoes[0];Pose(first.position-first.transform.right*.25f);yield return .2f;Pose(first.position+first.transform.right*.22f);yield return .3f;Grip(0);yield return 2.8f;
                    Require(domino.Reading==6,"Real domino chain incomplete: "+domino.Reading);Step(2);foreach(var t in Mirror())yield return t;break;
                }
                case 9:
                {
                    foreach(var t in Power(One<ReportElectromagnetMeasure>()))yield return t;Step(1);foreach(var t in Mirror())yield return t;Step(2);
                    var p=One<ReportHydraulicMeasure>();foreach(var t in Turn(p.inlet,1,.2f))yield return t;foreach(var t in Turn(p.plunger,.4f,.8f))yield return t;break;
                }
                case 11:
                {
                    foreach(var t in Turn(Object.FindObjectsOfType<ReportHandleMeasure>().First(m=>m.label=="方位误差").handle,0))yield return t;Step(1);
                    foreach(var t in Turn(Object.FindObjectsOfType<ReportHandleMeasure>().First(m=>m.label=="观测环照准角").handle,45))yield return t;Step(2);
                    foreach(var t in Turn(One<ReportSunlightMeasure>().sunModel,225))yield return t;break;
                }
                case 12:
                {
                    var stack=One<ReportStackMeasure>();for(int i=0;i<stack.pieces.Length;i++)foreach(var t in Move(stack.pieces[i],stack.positions[i].position,.8f))yield return t;yield return 2.5f;
                    Require(stack.Reading==3,"Physical snow stack unstable "+stack.Reading+" positions="+string.Join(";",stack.pieces.Select(g=>g.transform.position.ToString())));Step(1);
                    var roll=One<ReportRollMeasure>();foreach(var t in Move(roll.ball,roll.rampStart.position,1.5f))yield return t;Require(roll.Reading==1,"Real ramp roll did not arrive");Step(2);
                    var heat=One<ReportHeatMeasure>();foreach(var t in Move(heat.sample,heat.warmZone.position,.3f))yield return t;foreach(var t in Move(heat.insulation,heat.coverSeat.position,5.8f))yield return t;break;
                }
                case 14:
                {
                    foreach(var t in Turn(One<ReportWheelLiftMeasure>().wheel,250))yield return t;Step(1);
                    var stack=One<ReportStackMeasure>();for(int i=0;i<stack.pieces.Length;i++)foreach(var t in Move(stack.pieces[i],stack.positions[i].position,.7f))yield return t;yield return 2.5f;
                    Require(stack.Reading==3,"Bridge load support unstable "+stack.Reading+" positions="+string.Join(";",stack.pieces.Select(p=>p.transform.position.ToString())));Step(2);foreach(var t in Mirror())yield return t;break;
                }
            }
            yield return .8f;Require(level.Completed,"Natural experiments did not complete");yield return 2.7f;Require(level.ExitReady,"Physical exit did not open");
            results.Add("PASS scene="+index+" "+level.title+" all 3 actual experiment conditions; negative status check; exit opens");Debug.Log("NATURAL_SCENE_PLAY_OK index="+index);
            level.ResetLevel();yield return .6f;Require(level.Step==0 && !level.Completed && !level.ExitReady,"Reset did not relock goal/exit");
            foreach(var m in Object.FindObjectsOfType<ReportThrowTarget>())Require(m.Hits==0,"Throw hits retained on reset");
            Pose(origin.transform.position+Vector3.up*1.5f);Grip(0);
        }
    }
}
