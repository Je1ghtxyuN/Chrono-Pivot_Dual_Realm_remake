using System.Linq;
using UnityEngine;

// Quasi-static box hull: displaced volume = mass / water density, including hull mass.
public sealed class ReportBuoyancyMeasure : ReportLearningMeasure
{
    public Transform hull;
    public ReportCargoSeat[] seats;
    public ReportMechanismHandle gate;
    public Transform gateLeaf;
    [Min(1)] public float hullMass=2,waterDensity=1000;
    [Min(.01f)] public float maximumVolume=.024f,hullDepth=.15f;
    [Min(.01f)] public float settlingSpeed=.25f,transitSpeed=.3f;
    public float waterHeight;
    public Vector3 transitTravel=Vector3.right*.8f;
    public TextMesh readout;
    public float TotalMass=>hullMass+seats.Where(s=>s.Loaded).Sum(s=>s.item.GetComponent<Rigidbody>().mass);
    public override float Reading=>TotalMass/(waterDensity*maximumVolume);
    public override int Operations=>seats.Sum(s=>s.Loads);
    public override bool Held=>seats.Any(s=>s.item.isSelected);
    public float Transit=>initialized?Mathf.Clamp01(Vector3.Dot(hull.position-rest,transitTravel)/transitTravel.sqrMagnitude):0;
    float commandedProgress;
    Vector3 rest;
    Vector3 gateRest;
    bool initialized;
    void Awake(){rest=hull.position;if(gateLeaf!=null)gateRest=gateLeaf.localPosition;initialized=true;}
    void FixedUpdate()
    {
        bool safe=Reading>=.47f && Reading<=.53f && !Held;
        if(gateLeaf!=null)gateLeaf.localPosition=gateRest+Vector3.up*gate.Value*.55f;
        if(safe && gate.Value>=.95f && !gate.isSelected)commandedProgress=Mathf.MoveTowards(commandedProgress,1,transitSpeed*Time.fixedDeltaTime);
        var desired=rest+transitTravel*commandedProgress;desired.y=waterHeight+hullDepth*.5f-Mathf.Min(Reading,1.4f)*hullDepth;
        hull.position=Vector3.MoveTowards(hull.position,desired,settlingSpeed*Time.fixedDeltaTime);
        if(readout!=null)readout.text="船体"+hullMass.ToString("F0")+"kg + 载荷 = "+TotalMass.ToString("F0")+"kg\n浸没 "+(Reading*100).ToString("F0")+"% · 排水 "+(TotalMass/waterDensity).ToString("F3")+"m³\n"+(Reading>1?"过载：排水量不足":safe?"目标水线50%：可通航":"继续调节装载")+"\n箱形船准静态浮力；闸口通过 "+(Transit*100).ToString("F0")+"%";
    }
    public override void ResetMeasurements(){commandedProgress=0;foreach(var seat in seats)seat.ResetSeat();if(initialized)hull.position=rest;}
}
