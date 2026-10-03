using UnityEngine;

public sealed class ReportCompassMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle needle;
    public float southBearing=180,returnSeconds=.7f;
    public TextMesh readout;
    float velocity;
    public override float Reading=>Mathf.Abs(Mathf.DeltaAngle(needle.Value,southBearing));
    public override int Operations=>needle.Operations;
    public override bool Held=>needle.isSelected;
    void Update()
    {
        if(!needle.isSelected)needle.SetValue(Mathf.SmoothDampAngle(needle.Value,southBearing,ref velocity,returnSeconds));
        else velocity=0;
        if(readout!=null)readout.text="拨动勺柄后松手，观察回指南\n与南向偏差 "+Reading.ToString("F1")+"°\n简化地磁恢复力矩";
    }
    public override void ResetMeasurements(){velocity=0;}
}
