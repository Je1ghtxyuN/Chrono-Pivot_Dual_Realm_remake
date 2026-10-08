using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class BoatDisplayCheck
{
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(value,null);
    public static void Audit()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);
        var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        Debug.Log("BOAT_LAYOUT table="+boat.transform.position+" rotation="+boat.transform.eulerAngles+" viewing="+Object.FindObjectOfType<DemoLevel>().stations[0].viewingPoint);
        foreach(var renderer in Object.FindObjectsOfType<MeshRenderer>().Where(r=>r.name.Contains("Canal") || r.transform.parent!=null && (r.transform.parent.name.Contains("HandcraftedCargo") || r.transform.parent.name.Contains("CanalSluice"))))
            if(!renderer.name.Contains("Natural_") && !renderer.name.Contains("Water_") && !renderer.name.Contains("Details") && !renderer.name.Contains("Grove") && !renderer.name.Contains("SkyDial") && !renderer.name.Contains("FarPeaks"))
                Debug.Log("BOAT_LAYOUT model="+renderer.name+" parent="+(renderer.transform.parent!=null?renderer.transform.parent.name:"")+" bounds="+renderer.bounds);
        foreach(var t in Object.FindObjectsOfType<Transform>().Where(t=>t.name=="HandcraftedCargoPavilionBoat" || t.name=="CanalSluiceAndCrank" || t.name=="活动闸板" || t.name.Contains("CanalChannel")))Debug.Log("BOAT_LAYOUT root="+t.name+" pos="+t.position+" children="+t.childCount);
    }
    public static void Verify()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);
        var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        var canal=GameObject.Find("EnclosedPressureCanal").GetComponent<Renderer>().bounds;
        var renderers=boat.transform.Find("Water Experience").GetComponentsInChildren<Renderer>();var table=renderers[0].bounds;foreach(var r in renderers.Skip(1))table.Encapsulate(r.bounds);
        Require(!new Rect(table.min.x,table.min.z,table.size.x,table.size.z).Overlaps(new Rect(canal.min.x,canal.min.z,canal.size.x,canal.size.z)),"The tabletop basin must not overlap the large canal");
        var display=Object.FindObjectsOfType<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="ReportBoatDisplay");
        Require(display!=null,"The large canal boat must mirror the tabletop experiment's cargo, draft and gate");
        var hull=(Transform)display.GetType().GetField("hull").GetValue(display);
        var gate=(Transform)display.GetType().GetField("gate").GetValue(display);
        var cargo=(GameObject[])display.GetType().GetField("cargo").GetValue(display);
        foreach(var seat in boat.seats)Call(seat,"Awake");Call(boat,"Awake");Call(display,"Awake");Call(display,"LateUpdate");
        var emptyPosition=hull.position;float empty=hull.position.y;Require(cargo.All(c=>!c.activeSelf),"The large boat must start without cargo");
        boat.seats[0].item.transform.position=boat.seats[0].transform.position;Call(boat.seats[0],"FixedUpdate");
        for(int i=0;i<300;i++)Call(boat,"FixedUpdate");Call(display,"LateUpdate");
        Require(hull.position.y<empty-.08f && cargo[0].activeSelf && !cargo[1].activeSelf && !cargo[2].activeSelf,"Loading wood must visibly lower the large boat and show only wood aboard");
        float wood=hull.position.y;
        boat.seats[1].item.transform.position=boat.seats[1].transform.position;Call(boat.seats[1],"FixedUpdate");
        for(int i=0;i<300;i++)Call(boat,"FixedUpdate");Call(display,"LateUpdate");
        Require(hull.position.y<wood-.12f && cargo[1].activeSelf,"Stone must further lower the large boat and appear on deck");
        float half=hull.position.y;
        boat.seats[2].item.transform.position=boat.seats[2].transform.position;Call(boat.seats[2],"FixedUpdate");
        boat.gate.SetValue(1);float closed=gate.position.y;for(int i=0;i<300;i++)Call(boat,"FixedUpdate");Call(display,"LateUpdate");
        Require(hull.position.y<half-.4f && cargo[2].activeSelf && boat.Transit==0,"Overload must visibly sink the large boat and block transit");
        Require(gate.position.y>closed+1,"Turning the small gate wheel must open the large sluice");
        boat.seats[2].item.transform.position=boat.seats[2].transform.position+Vector3.up*2;Call(boat.seats[2],"FixedUpdate");
        for(int i=0;i<300;i++)Call(boat,"FixedUpdate");Call(display,"LateUpdate");
        Require(hull.position.y>half-.02f && !cargo[2].activeSelf && boat.Transit>.98f,"Unloading copper must refloat the large boat and restore transit");
        Require(hull.position.x>emptyPosition.x+2,"Safe transit must move the large boat towards the sluice");
        boat.ResetMeasurements();boat.gate.SetValue(0);Call(display,"LateUpdate");
        Require(Vector3.Distance(hull.position,emptyPosition)<.001f && Mathf.Abs(gate.position.y-closed)<.001f && cargo.All(c=>!c.activeSelf),"Reset must restore the large boat, cargo and gate");
        Directory.CreateDirectory("Documentation/CargoReview");File.WriteAllText("Documentation/CargoReview/boat-display.txt","PASS: tabletop/canal separation; large boat cargo, visible draft, overload, unloading, gate opening, transit and reset follow the tabletop experiment.\n");
        Debug.Log("BOAT_DISPLAY_OK");
    }
}
