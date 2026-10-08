using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ParticleExperienceCheck
{
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(value,null);
    public static void Boats()
    {
        foreach(int index in new[]{1,10})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);
            var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();var feedback=boat.GetComponent("ReportBoatParticles");
            Require(feedback!=null,"Missing boat feedback in scene "+index);
            foreach(var seat in boat.seats){Call(seat,"Awake");seat.item.transform.position=seat.transform.position+Vector3.right*3;}
            Call(boat,"Awake");var display=Object.FindObjectOfType<ReportBoatDisplay>();if(display!=null)Call(display,"Awake");Call(feedback,"Awake");
            var systems=feedback.GetComponentsInChildren<ParticleSystem>();Require(systems.Length==(index==1?2:1),"Boat effects must include the linked canal boat only in level 2");
            Call(feedback,"LateUpdate");Require(systems.All(s=>s.particleCount==0),"Idle boat must not emit");
            var wood=boat.seats[0];wood.item.transform.position=wood.transform.position;Call(wood,"FixedUpdate");
            if(display!=null)Call(display,"LateUpdate");Call(feedback,"LateUpdate");
            Require(systems.All(s=>s.particleCount>0),"Actual loading must produce waterline splashes on both boats");
            var particles=new ParticleSystem.Particle[40];systems[0].GetParticles(particles);
            Require(Mathf.Abs(particles[0].position.y-boat.waterHeight)<.02f,"Tabletop splash must originate on the water surface");
            if(display!=null){systems[1].GetParticles(particles);float height=(float)feedback.GetType().GetField("displayWaterHeight").GetValue(feedback);Require(Mathf.Abs(particles[0].position.y-height)<.02f,"Canal splash must originate on its own water surface");}
            foreach(var s in systems)s.Clear();Call(feedback,"LateUpdate");Require(systems.All(s=>s.particleCount==0),"Constant load must not repeatedly splash");
            wood.item.transform.position=wood.transform.position+Vector3.up*2;Call(wood,"FixedUpdate");Call(feedback,"LateUpdate");Require(systems.All(s=>s.particleCount>0),"Actual unloading must splash");
            Object.FindObjectOfType<DemoLevel>().ResetLevel();Require(systems.All(s=>s.particleCount==0),"Reset must clear boat effects");
            Call(feedback,"LateUpdate");Require(systems.All(s=>s.particleCount==0),"Reset must not masquerade as unloading");
            boat.hull.position+=boat.transitTravel.normalized*.08f;if(display!=null)Call(display,"LateUpdate");Call(feedback,"LateUpdate");Require(systems.All(s=>s.particleCount>0),"Actual horizontal movement must produce sparse wake droplets");
            foreach(var s in systems)s.Clear();boat.hull.position+=Vector3.up*.01f;Call(feedback,"LateUpdate");Require(systems.All(s=>s.particleCount==0),"Vertical settling alone must not produce continuous wake");
            feedback.GetType().GetMethod("ResetFeedback").Invoke(feedback,null);feedback.gameObject.SetActive(false);Require(systems.All(s=>s.particleCount==0),"Disabling feedback must clear particles");
        }
        Directory.CreateDirectory("Documentation/ParticleReview");File.WriteAllText("Documentation/ParticleReview/boats.txt","PASS: idle, actual load/unload, linked large boat, waterline positions, horizontal wake, vertical settling, reset and disable.\n");Debug.Log("PARTICLE_BOATS_OK");
    }
    public static void SceneBudget()
    {
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            var systems=Object.FindObjectsOfType<ParticleSystem>().Where(s=>s.transform.parent!=null && s.transform.parent.name=="Physical particles").ToArray();
            Require(systems.Sum(s=>s.main.maxParticles)<=240,"New particle budget exceeded");
            foreach(var s in systems)
            {
                Require(!s.main.playOnAwake && !s.emission.enabled,"New particles must be event driven");
                Require(!s.collision.enabled && !s.lights.enabled,"New particles must not add physical collision or lights");
                Require(s.GetComponent<ParticleSystemRenderer>().sharedMaterial!=null,"Particle material missing");
            }
        }
        Debug.Log("PARTICLE_BUDGET_OK");
    }
    public static void Hydraulic()
    {
        foreach(int index in new[]{5,9})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);var experiment=Object.FindObjectOfType<ReportHydraulicMeasure>();
            var feedback=experiment.GetComponent<ReportHydraulicWaterFeedback>();Require(feedback!=null,"Hydraulic particle feedback missing");
            if(experiment.seal==null){experiment.inlet.SetValue(1);Call(feedback,"Update");Require(!feedback.leakage.emission.enabled,"Sealed hydraulic system must not spray");continue;}
            var socket=experiment.sealSeat.GetComponent<ReportPlacementSocket>();Call(socket,"Awake");
            experiment.seal.transform.position=experiment.sealSeat.position+Vector3.right;Call(socket,"Update");experiment.inlet.SetValue(1);Call(feedback,"Update");
            Require(feedback.leakage.emission.enabled && feedback.leakage.emission.rateOverTime.constant>0,"Open unsealed valve must leak");
            experiment.inlet.SetValue(0);Call(feedback,"Update");Require(!feedback.leakage.emission.enabled,"Closing valve must stop new leakage");
            experiment.seal.transform.position=experiment.sealSeat.position;Call(socket,"Update");Require(experiment.Sealed,"Released seal must seat while valve is closed");
            experiment.inlet.SetValue(1);Call(feedback,"Update");Require(!feedback.leakage.emission.enabled,"Sealed interface must stop leakage after reopening valve");
        }
        Directory.CreateDirectory("Documentation/ParticleReview");File.WriteAllText("Documentation/ParticleReview/hydraulic.txt","PASS: levels 6 and 10; actual closed-valve socket seating; open/unsealed leaks; closing/sealing stops new droplets.\n");Debug.Log("PARTICLE_HYDRAULIC_OK");
    }
    public static void ClassicBoat()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        var model=boat.hull.Find("Recognizable boat").GetComponentInChildren<MeshFilter>();var collider=model.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=model.sharedMesh;
        Physics.SyncTransforms();RaycastHit hit;var ray=new Ray(boat.hull.TransformPoint(new Vector3(.13f,.65f,.02f)),Vector3.down);
        Require(collider.Raycast(ray,out hit,1),"Classic boat must have an interior floor");
        Require(boat.hull.InverseTransformPoint(hit.point).y<.10f,"The small wooden boat must have an open interior between its benches, rather than a flat covered deck");
        Object.DestroyImmediate(collider);Debug.Log("CLASSIC_BOAT_OK");
    }
    public static void Presence()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);
        var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        foreach(var surface in Object.FindObjectsOfType<ReportWaterSurface>())Debug.Log("PARTICLE_SURFACE "+surface.name+" "+surface.GetComponent<Renderer>().bounds);
        var display=Object.FindObjectOfType<ReportBoatDisplay>();
        foreach(var r in display.hull.GetComponentsInChildren<MeshRenderer>())Debug.Log("PARTICLE_SHIP "+r.name+" "+r.bounds);
        Require(boat.GetComponent("ReportBoatParticles")!=null,"Boat load changes have no waterline particle feedback");
        Debug.Log("PARTICLE_PRESENCE_OK");
    }
}
