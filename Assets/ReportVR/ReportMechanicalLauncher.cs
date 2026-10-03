using UnityEngine;

public sealed class ReportMechanicalLauncher : MonoBehaviour
{
    public ReportMechanismHandle pull;
    public Transform cup;
    public ReportGrabbable[] samples;
    public Vector3 localLaunchDirection=new Vector3(0,1,1);
    [Min(.1f)] public float speed=6.3f;
    [Min(.05f)] public float loadingRadius=.4f;
    [Range(0,1)] public float cockThreshold=.75f;
    public TextMesh readout;
    bool cocked;
    void Update()
    {
        if(pull.isSelected && Mathf.InverseLerp(pull.valueRange.x,pull.valueRange.y,pull.Value)>=cockThreshold)cocked=true;
        if(!cocked || pull.isSelected)return;
        cocked=false;pull.SetValue(pull.valueRange.x);
        ReportGrabbable loaded=null;int count=0;
        foreach(var sample in samples)if(!sample.isSelected && Vector3.Distance(sample.transform.position,cup.position)<=loadingRadius){loaded=sample;count++;}
        if(count!=1){if(readout!=null)readout.text="请将一个样品放入发射杯\n握住拉杆，拉到底，再松手";return;}
        var body=loaded.GetComponent<Rigidbody>();body.constraints=RigidbodyConstraints.None;body.isKinematic=false;
        body.position=cup.position;loaded.transform.position=cup.position;body.angularVelocity=Vector3.zero;
        body.velocity=transform.TransformDirection(localLaunchDirection.normalized)*speed;
        loaded.GetComponent<ReportThrowTrack>().RecordRelease(body.velocity);
        if(readout!=null)readout.text="已发射："+loaded.name+"\n标定初速 "+speed.ToString("F2")+"m/s · 仰角45°\n轨迹和落点由真实刚体产生";
    }
}
