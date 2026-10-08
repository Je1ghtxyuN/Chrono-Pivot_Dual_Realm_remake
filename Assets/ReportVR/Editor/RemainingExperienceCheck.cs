using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class RemainingExperienceCheck
{
    static readonly List<string> results=new List<string>();
    static void Check(bool value,string message){results.Add((value?"PASS: ":"FAIL: ")+message);}
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(value,null);
    static void Scene(int index)=>EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);
    public static void UpgradeAndVerify(){ReportRemainingUpgrade.All();Verify();}
    public static void Verify()
    {
        results.Clear();Scene(4);var power=Object.FindObjectOfType<ReportElectromagnetMeasure>();Call(power.current,"Awake");Call(power,"Awake");
        power.core.transform.position=power.coreSeat.position+Vector3.right*.15f;
        Check(!power.CoreInstalled,"A core beside its socket is not installed");
        Check(typeof(ReportElectromagnetMeasure).GetProperty("ArmatureEngaged")!=null,"Latch depends on actual armature engagement");
        Scene(5);var hydraulic=Object.FindObjectOfType<ReportHydraulicMeasure>();Call(hydraulic.inlet,"Awake");
        hydraulic.seal.transform.position=hydraulic.sealSeat.position;hydraulic.inlet.SetValue(.5f);
        hydraulic.inlet.SetValue(0);var sealSocket=hydraulic.sealSeat.GetComponent<ReportPlacementSocket>();Call(sealSocket,"Awake");Call(sealSocket,"Update");hydraulic.inlet.SetValue(.5f);
        Check(Mathf.Abs(hydraulic.Pressure-hydraulic.inputForce/hydraulic.inputArea)<1,"An open valve does not scale equilibrium pressure by its opening");
        Scene(6);var pendulum=Object.FindObjectOfType<ReportPendulumMeasure>();Call(pendulum.angle,"Awake");Call(pendulum.length,"Awake");Call(pendulum,"FixedUpdate");
        typeof(ReportPendulumMeasure).GetField("<Period>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(pendulum,2.3f);
        pendulum.length.SetValue(1);Call(pendulum,"FixedUpdate");Check(pendulum.Period==0,"Changing pendulum length invalidates the previous period");
        Scene(7);foreach(var target in Object.FindObjectsOfType<ReportThrowTarget>())
        {
            Check(target.requiredStation!=null,"Throw target records only its active task");
            if(target.requiredStation.index==1)
            {
                var ball=target.acceptedItems[0];ball.transform.position=target.launchArea.position;ball.RecordRelease(Vector3.up*2);
                ball.transform.position=target.transform.position;target.SendMessage("OnTriggerEnter",ball.GetComponent<Collider>());
                Check(target.Hits==0,"A real release reaching the later target before its turn is ignored");
            }
        }
        Scene(8);var level=Object.FindObjectOfType<DemoLevel>();var placement=level.stations[0].puzzle.placements[0];var socket=placement.socket.GetComponent<ReportPlacementSocket>();
        placement.item.transform.SetPositionAndRotation(placement.socket.position,placement.socket.rotation*Quaternion.Euler(0,0,90));Call(socket,"Awake");Call(socket,"Update");
        Check(Quaternion.Angle(placement.item.transform.rotation,placement.socket.rotation)<.1f,"Guided assembly socket aligns the connector orientation");
        Scene(11);var dial=Object.FindObjectOfType<DemoLevel>().stations[0].puzzle.learningTask.requirements[0].measure;
        Check(dial.GetType().Name!="ReportHandleMeasure","Sundial alignment measures the physical north-south line rather than a fixed knob value");
        Scene(12);var heat=Object.FindObjectOfType<ReportHeatMeasure>();Call(heat,"Awake");
        typeof(ReportHeatMeasure).GetField("<InsulatedSeconds>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(heat,5f);
        heat.insulation.transform.position=heat.coverSeat.position+Vector3.up;Call(heat,"Update");Check(heat.InsulatedSeconds==0,"Removing the cover interrupts the continuous insulation observation");
        var stack=Object.FindObjectOfType<ReportStackMeasure>();
        for(int i=0;i<stack.pieces.Length;i++){var body=stack.pieces[i].GetComponent<Rigidbody>();body.isKinematic=false;body.velocity=body.angularVelocity=Vector3.zero;body.position=stack.positions[i].position+Vector3.up*.08f;}
        Physics.SyncTransforms();Check(stack.Reading<3,"Near-target pieces suspended above support do not count as stable stacking");
        var roll=Object.FindObjectOfType<ReportRollMeasure>();roll.ball.transform.position=roll.rampStart.position;roll.ball.GetComponent<ReportThrowTrack>().RecordRelease(Vector3.right*2);Call(roll,"Update");
        Check(!(bool)typeof(ReportRollMeasure).GetField("armed",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(roll),"A thrown ball passing the ramp start cannot arm the gravity-roll task");
        var progressProperty=typeof(ReportLearningTask).GetProperty("StabilityProgress");
        Check(progressProperty!=null,"Player can see continuous stable-hold progress");
        if(progressProperty!=null)
        {
            var taskObject=new GameObject("Temporary stability test");var task=taskObject.AddComponent<ReportLearningTask>();task.stableDuration=1;task.BeginGoal();task.Observe(true,.25f);
            Check(Mathf.Abs((float)progressProperty.GetValue(task)-.25f)<.001f,"Stability progress reflects elapsed valid observation");
            task.Observe(false,.1f);Check((float)progressProperty.GetValue(task)==0,"Invalid observation clears stability progress");Object.DestroyImmediate(taskObject);
        }
        Directory.CreateDirectory("Documentation/RemainingReview");File.WriteAllLines("Documentation/RemainingReview/physics.txt",results);
        foreach(var line in results)Debug.Log(line);
        if(results.Exists(v=>v.StartsWith("FAIL")))throw new InvalidOperationException("Remaining level regression checks failed");
        Debug.Log("REMAINING_PHYSICS_OK checks="+results.Count);
    }
}
