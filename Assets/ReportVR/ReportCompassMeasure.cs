using UnityEngine;

public sealed class ReportCompassMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle needle;
    public float southBearing=180,returnSeconds=.7f;
    public TextMesh readout;
    float velocity;
    int displacements,previousOperations;
    bool counted;
    public override float Reading=>Mathf.Abs(Mathf.DeltaAngle(needle.Value,southBearing));
    public override int Operations=>displacements;
    public override bool Held=>needle.isSelected;
    void FixedUpdate()
    {
        if(needle.isSelected)
        {
            velocity=0;
            if(!counted && Reading>=20 && needle.Operations>previousOperations){displacements++;counted=true;}
        }
        else
        {
            counted=false;
            previousOperations=needle.Operations;
            float frequency=4/Mathf.Max(.1f,returnSeconds);
            float error=Mathf.DeltaAngle(southBearing,needle.Value)*Mathf.Deg2Rad;
            velocity+=(-frequency*frequency*Mathf.Sin(error)*Mathf.Rad2Deg-1.3f*frequency*velocity)*Time.fixedDeltaTime;
            needle.SetValue(Mathf.Repeat(needle.Value+velocity*Time.fixedDeltaTime,360));
        }
    }
    void Update()
    {
        if(readout!=null)readout.text="拨开勺柄再松手，观察它回指南方\n"+(displacements==0?"明显拨开后再观察":Reading<=5?"勺柄回到南向，等待它停稳":"勺柄正在回摆，阻尼让它逐渐停稳");
    }
    public override void ResetMeasurements(){velocity=0;displacements=0;previousOperations=0;counted=false;}
}
