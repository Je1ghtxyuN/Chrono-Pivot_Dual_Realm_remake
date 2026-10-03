public sealed class ReportBoatTransitMeasure : ReportLearningMeasure
{
    public ReportBuoyancyMeasure boat;
    public override float Reading=>boat.Transit;
    public override int Operations=>boat.gate.Operations;
    public override bool Held=>boat.gate.isSelected || boat.Held;
}
