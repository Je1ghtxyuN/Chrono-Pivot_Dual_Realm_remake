using UnityEngine;

public sealed class ReportWheelLiftMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle wheel;
    public Transform gate;
    public TextMesh readout;
    [Min(.01f)] public float drumRadius=.15f,loadMass=20;
    Vector3 rest;
    public override float Reading=>wheel.Value*Mathf.Deg2Rad*drumRadius;
    public override int Operations=>wheel.Operations;
    public override bool Held=>wheel.isSelected;
    void Awake(){rest=gate.localPosition;}
    void Update(){gate.localPosition=rest+Vector3.up*Reading;if(readout!=null)readout.text="转水工轮使绳鼓提起闸板\n升高 "+Reading.ToString("F2")+"m · 重力功 "+(loadMass*Physics.gravity.magnitude*Reading).ToString("F0")+"J\n绳鼓半径0.15m · 载荷20kg（无滑动模型）";}
}
