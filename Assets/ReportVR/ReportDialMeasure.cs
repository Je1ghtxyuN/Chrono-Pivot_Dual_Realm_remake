using UnityEngine;

public sealed class ReportDialMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle dial;
    public Transform referenceLine;
    public TextMesh readout;
    public override float Reading=>Mathf.Acos(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(Vector3.ProjectOnPlane(dial.transform.forward,Vector3.up).normalized,referenceLine.forward))))*Mathf.Rad2Deg;
    public override int Operations=>dial.Operations;
    public override bool Held=>dial.isSelected;
    void Update(){if(readout!=null)readout.text="转动晷盘，让红色指针对齐南北线\n"+(Reading<=3?"已对齐：松手观察":"方向对齐后，再观察针影");}
}
