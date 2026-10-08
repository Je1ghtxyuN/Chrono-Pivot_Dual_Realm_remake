using UnityEngine;

public sealed class ReportWheelLiftMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle wheel;
    public Transform gate;
    public TextMesh readout;
    [Min(.01f)] public float drumRadius=.15f,loadMass=20;
    public Transform pulley;
    public LineRenderer cable;
    Vector3 rest;
    public override float Reading=>wheel.Value*Mathf.Deg2Rad*drumRadius;
    public override int Operations=>wheel.Operations;
    public override bool Held=>wheel.isSelected;
    void Awake(){rest=gate.localPosition;}
    void Update()
    {
        gate.localPosition=rest+Vector3.up*Reading;
        if(cable!=null){cable.positionCount=3;cable.SetPosition(0,wheel.transform.position);cable.SetPosition(1,pulley.position);cable.SetPosition(2,gate.position+Vector3.up*.15f);}
        if(readout!=null)readout.text="转动水工轮，绳索收紧并抬起闸板\n"+(Reading>=.6f?"闸板已到红色标记：松手观察":"转动轮辐，让闸板升到红色标记");
    }
    public override void ResetMeasurements(){gate.localPosition=rest;}
}
