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
    public override bool Held=>angle.isSelected || length.isSelected;
    float speed,lastAngle,lastCross,previousLength;
    bool measurable;
    void FixedUpdate()
    {
        float theta=angle.Value*Mathf.Deg2Rad;
        if(Mathf.Abs(previousLength-length.Value)>.001f){Period=lastCross=0;measurable=false;previousLength=length.Value;}
        if(Held){speed=0;lastCross=Period=0;measurable=angle.isSelected && Mathf.Abs(angle.Value)>=10;}
        else
        {
            speed+=(-Physics.gravity.magnitude/length.Value*Mathf.Sin(theta)-damping*speed)*Time.fixedDeltaTime;
            angle.SetValue(angle.Value+speed*Time.fixedDeltaTime*Mathf.Rad2Deg);
            if(measurable && lastAngle<0 && angle.Value>=0)
            {
                float crossing=Time.fixedTime-Time.fixedDeltaTime+Time.fixedDeltaTime*(-lastAngle)/(angle.Value-lastAngle);
                if(lastCross>0)Period=crossing-lastCross;lastCross=crossing;
            }
        }
        lastAngle=angle.Value;
        bob.localPosition=Vector3.down*length.Value;rod.localScale=new Vector3(1,length.Value,1);
        if(readout!=null)readout.text="拉开摆球再松手，观察往复摆动\n"+(Held?"明显拉开后松手":Period>0?"改变摆长，再比较摆动快慢":"等待摆球完成往复摆动");
    }
    public override void ResetMeasurements(){speed=lastAngle=lastCross=Period=0;previousLength=length.Value;measurable=false;}
}
