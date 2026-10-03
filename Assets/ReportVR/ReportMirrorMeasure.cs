using UnityEngine;

public sealed class ReportMirrorMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle mirror;
    public Transform emitter;
    public Collider receiver;
    public LineRenderer beam;
    public Renderer receiverLamp;
    public TextMesh readout;
    [Min(.1f)] public float rayLength=3;
    public override float Reading {get{return hit?1:0;}}
    public override int Operations=>mirror.Operations;
    public override bool Held=>mirror.isSelected;
    bool hit;
    void Update()
    {
        var incoming=(mirror.transform.position-emitter.position).normalized;
        var reflected=Vector3.Reflect(incoming,mirror.transform.forward);
        var start=mirror.transform.position+reflected*.07f;
        hit=Physics.Raycast(start,reflected,out var contact,rayLength,~0,QueryTriggerInteraction.Collide) && contact.collider==receiver;
        if(beam!=null){beam.positionCount=3;beam.SetPosition(0,emitter.position);beam.SetPosition(1,mirror.transform.position);beam.SetPosition(2,contact.collider!=null?contact.point:start+reflected*rayLength);}
        if(receiverLamp!=null)receiverLamp.material.color=hit?new Color(.3f,1,.62f):new Color(.24f,.35f,.4f);
        if(readout!=null)readout.text="握住铜镜，转动使光束落入接收器\n反射线来自实际镜面法线\n"+(hit?"接收器已接通":"接收器未接通");
    }
    public override void ResetMeasurements(){hit=false;}
}
