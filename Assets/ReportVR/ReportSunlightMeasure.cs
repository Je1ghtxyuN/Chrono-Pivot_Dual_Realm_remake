using UnityEngine;

public sealed class ReportSunlightMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle sunModel;
    public Light sunlight;
    [Range(5,85)] public float elevation=45;
    public override float Reading=>sunModel.Value;
    public override int Operations=>sunModel.Operations;
    public override bool Held=>sunModel.isSelected;
    void LateUpdate()
    {
        float azimuth=sunModel.Value*Mathf.Deg2Rad,altitude=elevation*Mathf.Deg2Rad;
        var incoming=new Vector3(-Mathf.Sin(azimuth)*Mathf.Cos(altitude),-Mathf.Sin(altitude),-Mathf.Cos(azimuth)*Mathf.Cos(altitude));
        sunlight.transform.rotation=Quaternion.LookRotation(incoming);
    }
}
