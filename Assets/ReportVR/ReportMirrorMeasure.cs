using UnityEngine;

public sealed class ReportMirrorMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle mirror;
    public ReportMechanismHandle secondMirror;
    public Transform emitter;
    public Collider receiver;
    public LineRenderer beam;
    public LineRenderer normalLine;
    public LineRenderer secondNormalLine;
    public Renderer receiverLamp;
    public TextMesh readout;
    [Min(.1f)] public float rayLength=3;
    public override float Reading {get{return hit?1:0;}}
    public override int Operations=>mirror.Operations+(secondMirror!=null?secondMirror.Operations:0);
    public override bool Held=>mirror.isSelected || secondMirror!=null && secondMirror.isSelected;
    bool hit;
    MaterialPropertyBlock lampProperties;
    void Update()
    {
        var center=mirror.transform.position;
        var incoming=(center-emitter.position).normalized;
        bool incident=Physics.Raycast(emitter.position,incoming,out var entry,Vector3.Distance(center,emitter.position)+.01f,~0,QueryTriggerInteraction.Ignore);
        bool reachesMirror=incident && (entry.collider.transform==mirror.transform || entry.collider.transform.IsChildOf(mirror.transform));
        var reflected=Vector3.Reflect(incoming,mirror.transform.forward);
        var start=entry.point+reflected*.005f;
        RaycastHit contact=default;
        bool reflectedContact=reachesMirror && Physics.Raycast(start,reflected,out contact,rayLength,~0,QueryTriggerInteraction.Ignore);
        bool reachesSecond=secondMirror!=null && reflectedContact && (contact.collider.transform==secondMirror.transform || contact.collider.transform.IsChildOf(secondMirror.transform));
        RaycastHit finalContact=default;
        var finalDirection=reachesSecond?Vector3.Reflect(reflected,secondMirror.transform.forward):reflected;
        var finalStart=contact.point+finalDirection*.005f;
        hit=secondMirror==null?reflectedContact && contact.collider==receiver:reachesSecond && Physics.Raycast(finalStart,finalDirection,out finalContact,rayLength,~0,QueryTriggerInteraction.Ignore) && finalContact.collider==receiver;
        if(beam!=null)
        {
            beam.positionCount=reachesSecond?4:reachesMirror?3:2;beam.SetPosition(0,emitter.position);beam.SetPosition(1,incident?entry.point:center);
            if(reachesMirror)beam.SetPosition(2,contact.collider!=null?contact.point:start+reflected*rayLength);
            if(reachesSecond)beam.SetPosition(3,finalContact.collider!=null?finalContact.point:finalStart+finalDirection*rayLength);
        }
        if(normalLine!=null){normalLine.positionCount=2;normalLine.SetPosition(0,center-mirror.transform.forward*.3f);normalLine.SetPosition(1,center+mirror.transform.forward*.3f);}
        if(secondNormalLine!=null && secondMirror!=null){secondNormalLine.positionCount=2;secondNormalLine.SetPosition(0,secondMirror.transform.position-secondMirror.transform.forward*.3f);secondNormalLine.SetPosition(1,secondMirror.transform.position+secondMirror.transform.forward*.3f);}
        if(receiverLamp!=null)
        {
            if(lampProperties==null)lampProperties=new MaterialPropertyBlock();
            lampProperties.SetColor("_Color",hit?new Color(.3f,1,.62f):new Color(.24f,.35f,.4f));receiverLamp.SetPropertyBlock(lampProperties);
        }
        if(readout!=null)readout.text=(secondMirror!=null?"光源 → 一号镜 → 二号镜 → 接收器\n":"转动铜镜，观察光的反射方向\n")+(!reachesMirror?"光被挡住了，先让光照到镜面":hit?"接收器已亮：松手保持":secondMirror!=null?!reachesSecond?"先调一号镜，让光照到二号镜":"再调二号镜，让光到达接收器":"让反射光落到接收器，蓝线是镜面法线");
    }
    public override void ResetMeasurements(){hit=false;}
}
