using UnityEngine;

public sealed class ReportSightMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle ring;
    public Renderer target;
    public LineRenderer sightLine;
    public TextMesh readout;
    public override int Operations=>ring.Operations;
    public override bool Held=>ring.isSelected;
    public Vector3 TargetDirection=>Vector3.ProjectOnPlane(target.bounds.center-ring.transform.position,transform.forward).normalized;
    public override float Reading=>Vector3.Angle(ring.transform.up,TargetDirection);
    void Update()
    {
        if(sightLine!=null)
        {
            sightLine.positionCount=2;sightLine.SetPosition(0,ring.transform.position);
            sightLine.SetPosition(1,ring.transform.position+ring.transform.up*.48f);
            sightLine.startColor=sightLine.endColor=Reading<=3?new Color(.3f,1,.62f):new Color(1,.42f,.22f);
        }
        if(readout!=null)readout.text=(GetComponent<ReportStarDial>()!=null?"转星盘看北斗旋转，让红线对准金色星标\n":"转动观测环，让红线指向金色星标\n")+(Reading<=3?"已对准：松手保持":"沿圆周拖动，观察红线靠近星标");
    }
}
