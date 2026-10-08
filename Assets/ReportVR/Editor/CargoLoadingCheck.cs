using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class CargoLoadingCheck
{
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(value,null);
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    public static void Verify()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);
        var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        foreach(var seat in boat.seats)Call(seat,"Awake");
        Call(boat,"Awake");
        var wood=boat.seats[0];
        // Releasing over the opposite end used to miss the wood's hidden point.
        wood.item.transform.position=boat.hull.TransformPoint(new Vector3(.30f,.55f,.12f));
        Call(wood,"FixedUpdate");
        Require(wood.Loaded && Mathf.Abs(boat.TotalMass-30)<.01f,"Released wood anywhere over the deck must load and add its weight");
        Require(Vector3.Distance(wood.item.transform.position,wood.transform.position)<.001f,"Loaded cargo must follow its safe storage position");
        var stone=boat.seats[1];
        stone.item.transform.position=boat.transform.TransformPoint(new Vector3(0,1.2f,-.60f));Call(stone,"FixedUpdate");
        Require(!stone.Loaded,"Cargo left on the supply tray must not count as aboard");
        stone.item.transform.position=boat.hull.TransformPoint(new Vector3(-.30f,.55f,-.12f));Call(stone,"FixedUpdate");
        Require(stone.Loaded && Mathf.Abs(boat.Reading-.5f)<.001f,"Wood and stone released across the deck must reach the safe waterline");
        var copper=boat.seats[2];
        copper.item.transform.position=boat.hull.TransformPoint(new Vector3(0,.55f,0));Call(copper,"FixedUpdate");
        Require(copper.Loaded && boat.Reading>1,"Copper over the deck must produce overload");
        copper.item.transform.position=boat.hull.TransformPoint(new Vector3(0,.55f,-.65f));Call(copper,"FixedUpdate");
        Require(!copper.Loaded && Mathf.Abs(boat.Reading-.5f)<.001f,"Taking copper back off the boat must restore the safe load");
        wood.item.transform.position=boat.hull.TransformPoint(new Vector3(0,.05f,0));wood.ResetSeat();Call(wood,"FixedUpdate");
        Require(!wood.Loaded,"Cargo below the deck must not be mistaken for loading");
        boat.ResetMeasurements();
        foreach(var seat in boat.seats)Require(!seat.Loaded && seat.Loads==0,"Reset must clear cargo and operation counts");
        Directory.CreateDirectory("Documentation/CargoReview");
        File.WriteAllText("Documentation/CargoReview/loading.txt","PASS: deck-wide release, weight, storage, tray exclusion, safe waterline, overload, unloading, below-deck exclusion and reset.\n");
        Debug.Log("CARGO_LOADING_OK");
    }
}
