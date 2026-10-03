public sealed class ReportHandleMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle handle;
    public override float Reading=>handle.Value;
    public override int Operations=>handle.Operations;
    public override bool Held=>handle.isSelected;
}
