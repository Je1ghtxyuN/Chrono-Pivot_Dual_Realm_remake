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
    static bool waterOnly;
    static bool acousticOnly;
    static bool orientationOnly;
    static int SceneCount=>orientationOnly?5:acousticOnly?1:waterOnly?4:15-firstIndex;
    static string ReportPath=>orientationOnly?"Documentation/OrientationReview/input-playtest.txt":acousticOnly?"Documentation/AcousticReview/input-playtest.txt":waterOnly?"Documentation/WaterReview/input-playtest.txt":"Documentation/Verification/natural-all-playtest.txt";
    public static void OrientationOnly(){orientationOnly=true;firstIndex=3;Run();}
    public static void AcousticOnly(){acousticOnly=true;firstIndex=2;Run();}
    public static void WaterOnly(){waterOnly=true;firstIndex=1;Run();}
    public static void Remaining(){firstIndex=4;Run();}
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
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
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
            results.Insert(0,"PASS: scenes "+(orientationOnly?"3,8,9,13,14":acousticOnly?"2":waterOnly?"1,5,9,10":firstIndex+"..14")+" / "+(SceneCount*3)+" natural tasks. Virtual XR grip/drag/release; actual collisions and measurement updates; calibrated ballistic releases; status cannot bypass; reset and physical exits. First scene is covered by NaturalFirstPlayCheck. Physical headset and SteamVR streaming are not tested.");
            File.WriteAllLines(ReportPath,results);Debug.Log((orientationOnly?"NATURAL_ORIENTATION_PLAYTEST_OK":acousticOnly?"NATURAL_ACOUSTIC_PLAYTEST_OK":waterOnly?"NATURAL_WATER_PLAYTEST_OK":"NATURAL_ALL_PLAYTEST_OK")+" scenes="+SceneCount+" tasks="+(SceneCount*3));finishing=true;EditorApplication.isPlaying=false;
        }
        catch(Exception e)
        {
            failed=finishing=true;Application.logMessageReceived-=OnLog;Debug.LogError(e);File.WriteAllLines(ReportPath,new[]{"FAIL",e.ToString()}.Concat(results));EditorApplication.isPlaying=false;
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
    {
        var measure=One<ReportMirrorMeasure>();int before=level.Step;
        foreach(var t in Turn(measure.mirror,60,1.2f))yield return t;Require(measure.Reading==0 && level.Step==before,"Incorrect reflection passed");
        var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="Incident light obstruction";
        blocker.transform.position=Vector3.Lerp(measure.emitter.position,measure.mirror.transform.position,.5f);blocker.transform.localScale=Vector3.one*.12f;
        foreach(var t in Turn(measure.mirror,135,1.2f))yield return t;Require(measure.Reading==0 && level.Step==before,"Incident obstruction bypassed optics task");
        Object.Destroy(blocker);yield return .2f;
        if(measure.secondMirror!=null)
        {
            Require(measure.Reading==0 && level.Step==before,"First reflection alone completed the two-mirror task");
            foreach(var t in Turn(measure.secondMirror,60,1.2f))yield return t;Require(measure.Reading==0,"Incorrect second reflection passed");
            foreach(var t in Turn(measure.secondMirror,135,1.2f))yield return t;
        }
        else yield return 1.2f;
        Require(measure.Reading==1,"Actual reflected ray missed receiver after removing blocker and aligning mirrors");
    }
    static IEnumerable<float> Power(ReportElectromagnetMeasure power)
    {
        foreach(var t in Move(power.core,power.coreSeat.position))yield return t;
        foreach(var t in Turn(power.current,2.5f,1.4f))yield return t;
        Require(power.ArmatureEngaged && power.Field>2,"Core / actual armature did not engage");
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
            if(waterOnly && index!=1 && index!=5 && index!=9 && index!=10)continue;
            if(acousticOnly && index!=2)continue;
            if(orientationOnly && index!=3 && index!=8 && index!=9 && index!=13 && index!=14)continue;
            if(SceneManager.GetActiveScene().buildIndex!=index)SceneManager.LoadScene(index);yield return .6f;
            origin=One<XROrigin>();level=One<DemoLevel>();var gameUI=origin.GetComponent<ReportGameUI>();if(gameUI!=null)gameUI.enabled=false;
            Grip(0);Pose(origin.transform.position+Vector3.up*1.5f);yield return .15f;
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
                    var wrong=GameObject.CreatePrimitive(PrimitiveType.Cube);wrong.transform.localScale=Vector3.one*.08f;wrong.transform.position=bell.transform.position+bell.transform.right*.5f;
                    var wrongBody=wrong.AddComponent<Rigidbody>();wrongBody.useGravity=false;wrongBody.velocity=-bell.transform.right*2;yield return .6f;
                    Require(bell.Strikes==0 && level.Step==0,"An unrelated object counted as a mallet strike");Object.Destroy(wrong);
                    var malletBody=bell.mallet.GetComponent<Rigidbody>();bell.mallet.transform.position=bell.transform.position+bell.transform.right*.5f;malletBody.useGravity=false;malletBody.velocity=-bell.transform.right*2;yield return .6f;
                    Require(bell.Strikes==0 && level.Step==0,"Throwing an unheld mallet counted as intentional playing");malletBody.useGravity=true;bell.mallet.ResetPose();yield return .3f;
                    foreach(var t in Grab(bell.mallet))yield return t;
                    yield return .3f;Require(bell.Strikes==0,"Pickup or stationary holding counted as striking");
                    Pose(bell.transform.position+bell.transform.right*.45f);yield return .2f;Pose(bell.transform.position);yield return .18f;Grip(0);yield return .7f;
                    Require(bell.Strikes>0,"Real mallet collision did not ring bell");Step(1);
                    Require(Object.FindObjectsOfType<AudioSource>().Any(a=>a.isPlaying && a.clip!=null && a.clip.name.Contains("BellStrike")),"Accepted strike did not produce its bell sound");
                    var resonance=Object.FindObjectsOfType<ReportAcousticMeasure>().First(m=>m.experiment==ReportAcousticReading.Resonance);
                    foreach(var t in Turn(resonance.frequency,resonance.frequency.valueRange.x,1.2f))yield return t;Require(level.Step==1,"Far-off drive passed resonance");
                    foreach(var t in Turn(resonance.frequency,resonance.naturalFrequency,1.2f))yield return t;Step(2);
                    var beats=Object.FindObjectsOfType<ReportAcousticMeasure>().First(m=>m.experiment==ReportAcousticReading.Beats);
                    foreach(var t in Turn(beats.frequency,beats.naturalFrequency-1,1.8f))yield return t;Require(level.Step==2,"One-hertz beats passed tuning");
                    foreach(var t in Turn(beats.fineFrequency,1,1.8f))yield return t;Require(beats.Reading<.3f,"Fine tuning did not align the actual signal");break;
                }
                case 3:case 13:
                {
                    var compass=One<ReportCompassMeasure>();
                    float direction=compass.southBearing>280?-1:1;
                    foreach(var t in Turn(compass.needle,compass.southBearing+direction*8,2))yield return t;Require(level.Step==0 && compass.Operations==0,"Tiny compass deflection passed");
                    foreach(var t in Turn(compass.needle,compass.southBearing+direction*70,3.6f))yield return t;Require(compass.Reading<5,"Needle failed to return south");Step(1);
                    if(index==3)
                    {
                        var sight=One<ReportSightMeasure>();var sky=One<ReportStarDial>();var skyBefore=sky.skyRoot.rotation;
                        foreach(var t in Turn(sight.ring,0,1.2f))yield return t;Require(level.Step==1,"Off-target sight passed");
                        Require(Quaternion.Angle(skyBefore,sky.skyRoot.rotation)>40,"Actual star dial drag failed to rotate the sky and Big Dipper");
                        foreach(var t in Turn(sight.ring,45,1.2f))yield return t;Require(sight.Reading<=3,"Sight direction did not point at actual star");Step(2);foreach(var t in Mirror())yield return t;
                    }
                    else{foreach(var t in Power(One<ReportElectromagnetMeasure>()))yield return t;Step(2);foreach(var t in Throw(One<ReportThrowTarget>()))yield return t;}break;
                }
                case 4:
                {
                    var p=One<ReportElectromagnetMeasure>();foreach(var t in Turn(p.current,2.5f,.8f))yield return t;Require(!p.ArmatureEngaged && level.Step==0,"No-core power accepted");
                    foreach(var t in Turn(p.current,0,.1f))yield return t;
                    foreach(var t in Move(p.core,p.coreSeat.position,.8f))yield return t;Step(1);
                    foreach(var t in Turn(p.current,4,.7f))yield return t;Require(level.Step==1 && !p.Energized,"Overcurrent accepted");
                    foreach(var t in Turn(p.current,2.5f,1.4f))yield return t;Require(p.ArmatureEngaged,"Armature has not actually closed");Step(2);
                    foreach(var t in Turn(p.current,0,.8f))yield return t;Require(p.latch.Value==0 && !p.ArmatureEngaged,"Loss of power did not relock latch");
                    foreach(var t in Turn(p.current,2.5f,1.4f))yield return t;
                    foreach(var t in Turn(p.latch,1,.8f))yield return t;break;
                }
                case 5:
                {
                    var p=One<ReportHydraulicMeasure>();foreach(var t in Turn(p.inlet,1,.2f))yield return t;
                    foreach(var t in Move(p.seal,p.sealSeat.position,.6f))yield return t;Require(!p.Sealed && level.Step==0,"Pressurized seal installation accepted");
                    foreach(var t in Move(p.seal,p.sealSeat.position+p.transform.right*.4f,.2f))yield return t;
                    foreach(var t in Turn(p.inlet,0,.8f))yield return t;Step(1);
                    foreach(var t in Move(p.seal,p.sealSeat.position,.8f))yield return t;Step(2);
                    foreach(var t in Turn(p.plunger,.4f,.6f))yield return t;Require(p.Reading==0 && level.Step==2,"Closed discharge valve raised load");
                    foreach(var t in Turn(p.plunger,0,.1f))yield return t;foreach(var t in Turn(p.inlet,.5f,.2f))yield return t;
                    Require(Mathf.Abs(p.Pressure-p.inputForce/p.inputArea)<1,"Half-open valve reduced equilibrium pressure");
                    foreach(var t in Turn(p.plunger,.4f,.8f))yield return t;
                    Require(Mathf.Abs(p.Reading-.1f)<.005f,"Hydraulic volume ratio wrong");break;
                }
                case 6:
                {
                    var p=One<ReportPendulumMeasure>();foreach(var t in Turn(p.angle,5,4.5f))yield return t;Require(p.Period==0 && level.Step==0,"Tiny swing recorded a period");
                    foreach(var t in Turn(p.angle,20,6.5f))yield return t;Require(p.Period>2,"No actual pendulum crossing period");Step(1);
                    foreach(var t in Turn(p.length,1,.15f))yield return t;Require(p.Period==0,"Changed length retained old period");
                    foreach(var t in Turn(p.angle,20,6.5f))yield return t;Step(2);
                    var lc=One<ReportLCMeasure>();float frequency=1/p.Period,capacitance=1/(4*Mathf.PI*Mathf.PI*lc.inductance*frequency*frequency);
                    foreach(var t in Turn(lc.capacitance,lc.capacitance.valueRange.x,1))yield return t;Require(level.Step==2,"Mismatched LC passed");
                    foreach(var t in Turn(lc.capacitance,capacitance,1))yield return t;break;
                }
                case 7:
                {
                    var targets=Object.FindObjectsOfType<ReportThrowTarget>().OrderBy(t=>t.requiredStation.index).ToArray();
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
                    var dial=One<ReportDialMeasure>();float alignment=Vector3.SignedAngle(dial.transform.forward,dial.referenceLine.forward,Vector3.up);
                    if(alignment>90)alignment-=180;if(alignment<-90)alignment+=180;
                    foreach(var t in Turn(dial.dial,alignment,1.2f))yield return t;Require(dial.Reading<=3,"Actual dial line not aligned");Step(1);
                    var sight=One<ReportSightMeasure>();var ring=sight!=null?sight.ring:Object.FindObjectsOfType<ReportHandleMeasure>().First(m=>m.label=="观测环照准角").handle;
                    foreach(var t in Turn(ring,45,1.2f))yield return t;Step(2);
                    foreach(var t in Turn(One<ReportSunlightMeasure>().sunModel,225))yield return t;break;
                }
                case 12:
                {
                    var stack=One<ReportStackMeasure>();for(int i=0;i<stack.pieces.Length;i++)foreach(var t in Move(stack.pieces[i],stack.positions[i].position,.8f))yield return t;yield return 2.5f;
                    Require(stack.Reading==3,"Physical snow stack unstable "+stack.Reading+" positions="+string.Join(";",stack.pieces.Select(g=>g.transform.position.ToString())));Step(1);
                    var roll=One<ReportRollMeasure>();foreach(var t in Move(roll.ball,roll.rampStart.position,1.5f))yield return t;Require(roll.Reading==1,"Real ramp roll did not arrive");Step(2);
                    var heat=One<ReportHeatMeasure>();foreach(var t in Move(heat.sample,heat.warmZone.position,.3f))yield return t;
                    foreach(var t in Move(heat.insulation,heat.coverSeat.position,1.2f))yield return t;Require(heat.InsulatedSeconds>0 && level.Step==2,"Cover observation did not begin");
                    foreach(var t in Move(heat.insulation,heat.coverSeat.position+heat.transform.right*.5f,.3f))yield return t;Require(heat.InsulatedSeconds==0,"Removed cover retained observation");
                    foreach(var t in Move(heat.insulation,heat.coverSeat.position,5.8f))yield return t;break;
                }
                case 14:
                {
                    foreach(var t in Turn(One<ReportWheelLiftMeasure>().wheel,250))yield return t;Step(1);
                    var stack=One<ReportStackMeasure>();yield return 5;
                    BridgeSupplies(stack);Require(stack.Reading==0,"Unassembled bridge counted as supported");
                    for(int i=0;i<stack.pieces.Length;i++)foreach(var t in Move(stack.pieces[i],stack.positions[i].position,.7f))yield return t;yield return 2.5f;
                    Require(stack.Reading==3,"Bridge load support unstable "+stack.Reading+" positions="+string.Join(";",stack.pieces.Select(p=>p.transform.position.ToString())));Step(2);foreach(var t in Mirror())yield return t;break;
                }
            }
            yield return .8f;Require(level.Completed,"Natural experiments did not complete");yield return 2.7f;Require(level.ExitReady,"Physical exit did not open");
            results.Add("PASS scene="+index+" "+level.title+" all 3 actual experiment conditions; negative status check; exit opens");Debug.Log("NATURAL_SCENE_PLAY_OK index="+index);
            level.ResetLevel();yield return .6f;Require(level.Step==0 && !level.Completed && !level.ExitReady,"Reset did not relock goal/exit");
            if(index==3)
            {
                Require(Quaternion.Angle(One<ReportStarDial>().skyRoot.rotation,Quaternion.identity)<.01f,"Reset did not restore the demonstration sky");
                var optics=One<ReportMirrorMeasure>();Require(optics.secondMirror.Operations==0 && optics.Reading==0,"Reset retained second-mirror operation or an unlocked light path");
            }
            if(index==14){yield return 2;BridgeSupplies(One<ReportStackMeasure>());results.Add("PASS bridge supplies remain on the bench before assembly and after reset; gravity/contact remain active.");}
            foreach(var m in Object.FindObjectsOfType<ReportThrowTarget>())Require(m.Hits==0,"Throw hits retained on reset");
            foreach(var m in Object.FindObjectsOfType<ReportCompassMeasure>())Require(m.Operations==0,"Compass displacement retained on reset");
            foreach(var m in Object.FindObjectsOfType<ReportPendulumMeasure>())Require(m.Period==0,"Pendulum period retained on reset");
            foreach(var m in Object.FindObjectsOfType<ReportHeatMeasure>())Require(m.InsulatedSeconds==0,"Heat observation retained on reset");
            Pose(origin.transform.position+Vector3.up*1.5f);Grip(0);
        }
    }
    static void BridgeSupplies(ReportStackMeasure stack)
    {
        foreach(var piece in stack.pieces)
        {
            var body=piece.GetComponent<Rigidbody>();var bounds=piece.GetComponent<Collider>().bounds;
            Require(!body.isKinematic && body.useGravity && !piece.isSelected && body.velocity.magnitude<.12f && Mathf.Abs(bounds.min.y-stack.baseSupport.bounds.max.y)<.03f,
                "Bridge supply fell or failed to settle: "+piece.name+" position="+piece.transform.position+" velocity="+body.velocity);
        }
    }
}
