using UnityEngine;

public sealed class ReportHydraulicMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle inlet,plunger;
    public ReportGrabbable seal;
    public Transform sealSeat,output;
    public TextMesh readout;
    [Min(.0001f)] public float inputArea=.002f,outputArea=.008f;
    [Min(1)] public float inputForce=80,loadForce=200;
    public bool Sealed=>seal==null || sealSeat.GetComponent<ReportPlacementSocket>().Seated;
    public float Pressure=>Sealed && inlet.Value>.02f?inputForce/inputArea:0;
    public float OutputForce=>Pressure*outputArea;
    public override float Reading=>output.localPosition.y-rest.y;
    public override int Operations=>plunger.Operations;
    public override bool Held=>plunger.isSelected || inlet.isSelected || (seal!=null && seal.isSelected);
    Vector3 rest;
    void Awake(){rest=output.localPosition;}
    void Update()
    {
        // Incompressible quasi-static model: A1*x1=A2*x2. No extra displacement is created.
        float lift=OutputForce>=loadForce?plunger.Value*inputArea/outputArea:0;
        output.localPosition=rest+Vector3.up*lift;
        if(readout!=null)readout.text="压下小活塞，大活塞抬起载荷\n"+(!Sealed?"先关阀，再把密封圈放入青色座":inlet.Value<=.02f?"阀已关闭：开阀后再压活塞":OutputForce<loadForce?"载荷尚未抬起，检查流路":"流路已连通：压下小活塞并松手");
    }
    public override void ResetMeasurements(){output.localPosition=rest;}
}
