using UnityEngine;

public sealed class ReportRollMeasure : ReportLearningMeasure
{
    public ReportGrabbable ball;
    public Transform rampStart;
    public TextMesh readout;
    [Min(.1f)] public float minimumDistance=.7f;
    bool armed;
    public override float Reading=>arrived?1:0;
    public override int Operations=>arrived?1:0;
    public override bool Held=>ball.isSelected;
    bool arrived;
    void Update()
    {
        if(!ball.isSelected && Vector3.Distance(ball.transform.position,rampStart.position)<.25f && ball.GetComponent<Rigidbody>().velocity.magnitude<.35f && !ball.GetComponent<ReportThrowTrack>().HasThrown)armed=true;
        if(ball.isSelected)armed=false;
        if(readout!=null)readout.text=arrived?"雪球已由坡顶滚至接收区":"把雪球放到坡顶，松手让重力使它滚下\n无需抛掷，接收器记录真实到达";
    }
    void OnTriggerEnter(Collider other){if(!arrived && other.GetComponentInParent<ReportGrabbable>()==ball && armed && !ball.isSelected && Vector3.Distance(ball.transform.position,rampStart.position)>minimumDistance){arrived=true;ReportAudio.Play(ReportSound.TargetHit,ball.transform.position);}}
    public override void ResetMeasurements(){armed=arrived=false;}
}
