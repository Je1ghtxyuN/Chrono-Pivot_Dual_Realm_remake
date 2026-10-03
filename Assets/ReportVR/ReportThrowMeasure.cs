public sealed class ReportThrowMeasure : ReportLearningMeasure
{
    public ReportThrowTarget target;
    public bool countDifferentItems;
    public override float Reading=>countDifferentItems?target.DifferentItems:target.Hits;
    public override int Operations=>target.Hits;
    public override void ResetMeasurements()=>target.ResetMeasurements();
}
