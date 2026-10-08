using UnityEngine;

public sealed class ReportHeatMeasure : ReportLearningMeasure
{
    public ReportGrabbable sample,insulation;
    public Transform warmZone,coverSeat;
    public TextMesh readout;
    [Min(.05f)] public float zoneRadius=.42f;
    [Min(.01f)] public float thermalConductance=100,sampleMass=.03f,latentHeat=334000;
    public float warmTemperature=20;
    [Range(0,1)] public float insulationFactor=.08f;
    public bool insulationTask;
    public Transform meltWater;
    public float MeltFraction {get;private set;}
    public float InsulatedSeconds {get;private set;}
    public override float Reading=>insulationTask?InsulatedSeconds:MeltFraction;
    public override int Operations=>entries;
    public override bool Held=>sample.isSelected || (insulation!=null && insulation.isSelected);
    int entries;
    bool wasWarm,wasInsulated;
    Vector3 scale;
    void Awake(){scale=sample.transform.localScale;}
    void Update()
    {
        bool warm=!sample.isSelected && Vector3.Distance(sample.transform.position,warmZone.position)<zoneRadius;
        bool covered=insulation!=null && !insulation.isSelected && Vector3.Distance(insulation.transform.position,coverSeat.position)<.25f;
        if(warm && !wasWarm || covered && !wasInsulated)entries++;
        wasWarm=warm;wasInsulated=covered;
        if(warm)MeltFraction=Mathf.Clamp01(MeltFraction+thermalConductance*Mathf.Max(0,warmTemperature)*(covered?insulationFactor:1)*Time.deltaTime/(sampleMass*latentHeat));
        if(warm && covered)InsulatedSeconds+=Time.deltaTime;else InsulatedSeconds=0;
        sample.transform.localScale=scale*Mathf.Pow(Mathf.Max(.001f,1-MeltFraction),1f/3);
        if(meltWater!=null)meltWater.localScale=new Vector3(Mathf.Sqrt(MeltFraction),1,Mathf.Sqrt(MeltFraction));
        if(readout!=null)readout.text="受热时冰变小，融水增加\n"+(!warm?"把冰样放到红色暖区并松手":covered?"隔热罩让融化变慢，保持罩住观察":insulationTask?"放上隔热罩，对比融化快慢":"观察冰样逐渐融化");
    }
    public override void ResetMeasurements(){entries=0;wasWarm=wasInsulated=false;MeltFraction=InsulatedSeconds=0;if(sample!=null)sample.transform.localScale=scale;if(meltWater!=null)meltWater.localScale=new Vector3(0,1,0);}
}
