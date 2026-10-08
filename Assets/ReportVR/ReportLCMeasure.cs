using UnityEngine;

public sealed class ReportLCMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle capacitance;
    public ReportPendulumMeasure pendulum;
    public TextMesh readout;
    [Min(.01f)] public float inductance=1;
    public LineRenderer pendulumWave,lcWave;
    public float Frequency=>1/(2*Mathf.PI*Mathf.Sqrt(inductance*capacitance.Value));
    public override float Reading=>pendulum.Period>0?Mathf.Abs(Frequency-1/pendulum.Period)/(1/pendulum.Period):1;
    public override int Operations=>capacitance.Operations;
    public override bool Held=>capacitance.isSelected || pendulum.Held;
    void Update()
    {
        if(readout!=null)readout.text="转动调节轮，让两条波形起伏一样快\n"+(pendulum.Period<=0?"先拉开摆球并松手，观察摆动":Reading<=.03f?"节奏已接近：松手观察":Frequency>1/pendulum.Period?"金色起伏更快，继续调慢":"金色起伏更慢，继续调快");
        Plot(lcWave,Frequency,1.4f);Plot(pendulumWave,pendulum.Period>0?1/pendulum.Period:0,1.12f);
    }
    void Plot(LineRenderer line,float frequency,float height)
    {
        if(line==null)return;line.positionCount=65;
        for(int i=0;i<65;i++){float seconds=i/16f;line.SetPosition(i,new Vector3(-.48f+i*.015f,height+Mathf.Sin(2*Mathf.PI*frequency*(Time.time+seconds))*.10f,-.22f));}
    }
}
