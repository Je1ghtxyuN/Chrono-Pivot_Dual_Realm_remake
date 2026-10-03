using UnityEngine;

public sealed class ReportPendulumMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle angle,length;
    public Transform bob,rod;
    public TextMesh readout;
    [Min(0)] public float damping=.035f;
    public float Period {get;private set;}
    public override float Reading=>Period;
    public override int Operations=>angle.Operations;
    public override bool Held=>angle.isSelected;
    float speed,lastAngle,lastCross;
    void FixedUpdate()
    {
        float theta=angle.Value*Mathf.Deg2Rad;
        if(angle.isSelected){speed=0;lastCross=0;}
        else
        {
            speed+=(-Physics.gravity.magnitude/length.Value*Mathf.Sin(theta)-damping*speed)*Time.fixedDeltaTime;
            angle.SetValue(angle.Value+speed*Time.fixedDeltaTime*Mathf.Rad2Deg);
            if(lastAngle<0 && angle.Value>=0){if(lastCross>0)Period=Time.time-lastCross;lastCross=Time.time;}
        }
        lastAngle=angle.Value;
        bob.localPosition=Vector3.down*length.Value;rod.localScale=new Vector3(1,length.Value,1);
        if(readout!=null)readout.text="拉开摆球后松手：由重力积分摆动\n摆长 "+length.Value.ToString("F2")+"m · 实测周期 "+Period.ToString("F2")+"s\n小角度理论 f="+(Mathf.Sqrt(Physics.gravity.magnitude/length.Value)/(2*Mathf.PI)).ToString("F3")+"Hz";
    }
    public override void ResetMeasurements(){speed=lastAngle=lastCross=Period=0;}
}
