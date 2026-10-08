using UnityEngine;

public sealed class ReportSunlightMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle sunModel;
    public Light sunlight;
    [Range(5,85)] public float elevation=45;
    public Transform shadowOrigin;
    public LineRenderer shadowLine;
    public TextMesh readout;
    public float gnomonHeight=.35f;
    public override float Reading=>sunModel.Value;
    public override int Operations=>sunModel.Operations;
    public override bool Held=>sunModel.isSelected;
    void LateUpdate()
    {
        float azimuth=sunModel.Value*Mathf.Deg2Rad,altitude=elevation*Mathf.Deg2Rad;
        var incoming=new Vector3(-Mathf.Sin(azimuth)*Mathf.Cos(altitude),-Mathf.Sin(altitude),-Mathf.Cos(azimuth)*Mathf.Cos(altitude));
        sunlight.transform.rotation=Quaternion.LookRotation(incoming);
        if(shadowLine!=null)
        {
            var start=shadowOrigin.position+Vector3.up*.006f;shadowLine.positionCount=2;shadowLine.SetPosition(0,start);
            shadowLine.SetPosition(1,start+Vector3.ProjectOnPlane(incoming,Vector3.up)*gnomonHeight/Mathf.Abs(incoming.y));
        }
        if(readout!=null)readout.text="拖动太阳，观察影子向相反方向移动\n太阳换位置，针影也跟着改变";
    }
}
