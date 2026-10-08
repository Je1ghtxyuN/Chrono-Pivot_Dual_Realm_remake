using System.Linq;
using UnityEngine;

// Box hull with vertical inertia: displaced volume produces buoyancy; drag settles the boat.
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
    public ReportWaterSurface waterSurface;
    public Transform draftGauge;
    [Min(.1f)] public float waterDepth=.65f;
    [Min(0)] public float verticalDamping=9;
    public float TotalMass=>hullMass+seats.Where(s=>s.Loaded).Sum(s=>s.item.GetComponent<Rigidbody>().mass);
    public override float Reading=>TotalMass/(waterDensity*maximumVolume);
    public override int Operations=>seats.Sum(s=>s.Loads);
    public override bool Held=>seats.Any(s=>s.item.isSelected);
    public float SubmergedFraction=>Mathf.Clamp01((waterHeight+hullDepth*.5f-hull.position.y)/hullDepth);
    public float Transit=>initialized?Mathf.Clamp01(Vector3.Dot(hull.position-rest,transitTravel)/transitTravel.sqrMagnitude):0;
    float commandedProgress;
    Vector3 rest;
    Vector3 gateRest;
    bool initialized;
    float verticalSpeed,lastMass;
    Quaternion hullRestRotation;
    void Awake(){rest=hull.position;hullRestRotation=hull.rotation;if(gateLeaf!=null)gateRest=gateLeaf.localPosition;lastMass=TotalMass;initialized=true;}
    void FixedUpdate()
    {
        bool safe=Reading>=.47f && Reading<=.53f && !Held;
        if(gateLeaf!=null)gateLeaf.localPosition=gateRest+Vector3.up*gate.Value*.55f;
        if(safe && gate.Value>=.95f && !gate.isSelected)commandedProgress=Mathf.MoveTowards(commandedProgress,1,transitSpeed*Time.fixedDeltaTime);
        float dt=Time.fixedDeltaTime,mass=TotalMass;
        float force=waterDensity*maximumVolume*SubmergedFraction*Physics.gravity.magnitude;
        verticalSpeed+=((force/mass-Physics.gravity.magnitude)-verticalDamping*verticalSpeed)*dt;
        var horizontal=rest+transitTravel*commandedProgress;horizontal.y=hull.position.y;
        var desired=Vector3.MoveTowards(hull.position,horizontal,settlingSpeed*dt);desired.y+=verticalSpeed*dt;
        float bottom=waterHeight-waterDepth+hullDepth*.5f;
        if(desired.y<bottom){desired.y=bottom;verticalSpeed=Mathf.Max(0,verticalSpeed);}
        hull.position=desired;
        float balance=seats.Where(s=>s.Loaded).Sum(s=>s.item.GetComponent<Rigidbody>().mass*s.transform.localPosition.x)/Mathf.Max(1,mass);
        hull.rotation=hullRestRotation*Quaternion.Euler(Mathf.Sin(Time.time*1.2f)*.35f,0,Mathf.Clamp(-balance*12,-3,3));
        if(waterSurface!=null && (Mathf.Abs(mass-lastMass)>.01f || commandedProgress>0 && Time.frameCount%18==0))waterSurface.Disturb(hull.position,.012f);
        lastMass=mass;
        if(draftGauge!=null){float amount=Mathf.Max(.005f,SubmergedFraction);draftGauge.localScale=new Vector3(1,amount,1);draftGauge.localPosition=new Vector3(0,hullDepth*amount*.5f,0);}
        if(readout!=null)readout.text="加配重会下沉，取出配重会上浮\n"+(Reading>1?"过载下沉：取出铜块，让船重新浮起":Held?"移到船面上方松手":Transit>.98f?"闸门打开，船已向前移动":safe?"载荷合适：开闸并松手":!seats[0].Loaded?"先放木块，观察白色水线":"再放石块，观察金色水线");
    }
    public override void ResetMeasurements(){commandedProgress=0;verticalSpeed=0;foreach(var seat in seats)seat.ResetSeat();lastMass=hullMass;if(initialized){hull.position=rest;hull.rotation=hullRestRotation;}}
}
