using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class WaterExperienceCheck
{
    static void Call(Component component,string method)=>component.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(component,null);
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    public static void VerifyAndPreview()
    {
        Buoyancy();HydraulicLeaks();
        var shader=Shader.Find("ChronoPivot/Water");Require(shader!=null && !ShaderUtil.ShaderHasError(shader),"Water shader must compile");
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            foreach(var renderer in Object.FindObjectsOfType<MeshRenderer>(true))
                foreach(var material in renderer.sharedMaterials)
                    Require(material==null || !material.name.Contains("JadeWater"),"Old opaque water remains: "+renderer.name);
            foreach(var surface in Object.FindObjectsOfType<ReportWaterSurface>())
                Require(surface.GetComponent<Renderer>().sharedMaterials.Any(m=>m!=null && m.shader==shader),"Water surface has no water material");
        }
        ReportWaterUpgrade.Preview();Debug.Log("WATER_VERIFICATION_OK");
    }
    public static void HydraulicLeaks()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[5].path);
        var experiment=Object.FindObjectOfType<ReportHydraulicMeasure>();
        var feedback=experiment.GetComponent("ReportHydraulicWaterFeedback");
        Require(feedback!=null,"Open unsealed hydraulic valve has no moving water leakage feedback");
        var drops=experiment.GetComponentsInChildren<ParticleSystem>().Single();
        experiment.seal.transform.position=experiment.sealSeat.position+Vector3.right;
        experiment.inlet.SetValue(1);Call(feedback,"Update");Require(drops.emission.enabled && drops.emission.rateOverTime.constant>0,"Open unsealed valve must emit water droplets");
        experiment.inlet.SetValue(0);Call(feedback,"Update");Require(!drops.emission.enabled,"Closing valve must stop new leakage");
        experiment.seal.transform.position=experiment.sealSeat.position;experiment.inlet.SetValue(1);Call(feedback,"Update");Require(!drops.emission.enabled,"Installed seal must stop new leakage when pressure returns");
        Directory.CreateDirectory("Documentation/WaterReview");File.WriteAllText("Documentation/WaterReview/hydraulic.txt","PASS: open unsealed valve emits water; closing or sealing stops emission.\n");Debug.Log("WATER_HYDRAULIC_OK");
    }
    public static void Audit()
    {
        var lines=new List<string>();
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);
            foreach(var renderer in Object.FindObjectsOfType<MeshRenderer>(true))
            {
                var materials=renderer.sharedMaterials;
                if(materials.Any(m=>m!=null && (m.name.IndexOf("water",StringComparison.OrdinalIgnoreCase)>=0 || m.name.Contains("GlacierBlue"))) || renderer.name.IndexOf("water",StringComparison.OrdinalIgnoreCase)>=0 || renderer.name.Contains("水面"))
                    lines.Add(Path.GetFileNameWithoutExtension(scene.path)+" | "+renderer.name+" | "+renderer.bounds+" | "+string.Join(",",materials.Select(m=>m!=null?m.name:"null")));
            }
        }
        Directory.CreateDirectory("Documentation/WaterReview");File.WriteAllLines("Documentation/WaterReview/inventory.txt",lines);
        Debug.Log("WATER_INVENTORY_OK renderers="+lines.Count);
    }
    public static void Buoyancy()
    {
        var lines=new List<string>();
        foreach(int index in new[]{1,10})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);
            var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
            Require(boat!=null,"Boat measurement missing");
            Call(boat,"Awake");
            foreach(var seat in boat.seats){Call(seat,"Awake");seat.item.transform.position=seat.transform.position+Vector3.right*2;}
            for(int frame=0;frame<300;frame++)Call(boat,"FixedUpdate");
            float empty=boat.hull.position.y;
            var wood=boat.seats[0];
            wood.item.transform.position=wood.transform.position;Call(wood,"FixedUpdate");
            for(int frame=0;frame<300;frame++)Call(boat,"FixedUpdate");
            float light=boat.hull.position.y;Require(Mathf.Abs(boat.Reading-.25f)<.001f,"Wood load must displace 25%");
            var stone=boat.seats[1];
            stone.item.transform.position=stone.transform.position;Call(stone,"FixedUpdate");
            for(int frame=0;frame<300;frame++)Call(boat,"FixedUpdate");
            float half=boat.hull.position.y;Require(Mathf.Abs(boat.Reading-.5f)<.001f,"Wood + stone must displace 50%");
            Require(light-half>=.08f,"25% to 50% load changes visible draft by only "+(light-half).ToString("F3")+"m; need at least 0.08m");
            var copper=boat.seats[2];
            copper.item.transform.position=copper.transform.position;Call(copper,"FixedUpdate");
            boat.gate.SetValue(1);for(int frame=0;frame<300;frame++)Call(boat,"FixedUpdate");
            Require(boat.Reading>1 && boat.hull.position.y<boat.waterHeight-boat.hullDepth*.5f,"Overloaded boat must visibly sink below its full draft");
            Require(boat.Transit<.01f,"Overloaded boat must not pass the gate");
            foreach(var seat in boat.seats){seat.item.transform.position=seat.transform.position+Vector3.right*2;Call(seat,"FixedUpdate");}
            for(int frame=0;frame<300;frame++)Call(boat,"FixedUpdate");
            Require(Mathf.Abs(boat.hull.position.y-empty)<.02f,"Unloading must restore empty draft");
            boat.ResetMeasurements();Require(boat.seats.All(s=>!s.Loaded && s.Loads==0),"Reset must clear cargo and load counts");
            lines.Add("PASS scene="+index+" wood=.25 wood+stone=.50 visibleChange="+(light-half).ToString("F3")+"m overload blocks transit; unloading and reset restore boat");
        }
        Directory.CreateDirectory("Documentation/WaterReview");File.WriteAllLines("Documentation/WaterReview/buoyancy.txt",lines);Debug.Log("WATER_BUOYANCY_OK");
    }
}
