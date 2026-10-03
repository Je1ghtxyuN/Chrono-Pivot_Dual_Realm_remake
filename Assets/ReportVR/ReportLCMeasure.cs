using UnityEngine;

public sealed class ReportLCMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle capacitance;
    public ReportPendulumMeasure pendulum;
    public TextMesh readout;
    [Min(.01f)] public float inductance=1;
    public float Frequency=>1/(2*Mathf.PI*Mathf.Sqrt(inductance*capacitance.Value));
    public override float Reading=>pendulum.Period>0?Mathf.Abs(Frequency-1/pendulum.Period)/(1/pendulum.Period):1;
    public override int Operations=>capacitance.Operations;
    public override bool Held=>capacitance.isSelected;
    void Update(){if(readout!=null)readout.text="L="+inductance.ToString("F1")+"H · C="+capacitance.Value.ToString("F3")+"F\nfLC="+Frequency.ToString("F3")+"Hz · 摆实测="+(pendulum.Period>0?(1/pendulum.Period).ToString("F3"):"等待测周期")+"Hz\n频差 "+(Reading*100).ToString("F1")+"% · 旋转电容片调节";}
}
