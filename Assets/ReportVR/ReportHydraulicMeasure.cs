using UnityEngine;

public sealed class ReportHydraulicMeasure : ReportLearningMeasure
{
    public ReportMechanismHandle inlet,plunger;
    public ReportGrabbable seal;
    public Transform sealSeat,output;
    public TextMesh readout;
    [Min(.0001f)] public float inputArea=.002f,outputArea=.008f;
    [Min(1)] public float inputForce=80,loadForce=200;
    public bool Sealed=>seal==null || (!seal.isSelected && Vector3.Distance(seal.transform.position,sealSeat.position)<.2f);
    public float Pressure=>Sealed?inputForce/inputArea*inlet.Value:0;
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
        if(readout!=null)readout.text="压下小活塞，观察大活塞举重\nA₁="+inputArea.ToString("F3")+"m² · A₂="+outputArea.ToString("F3")+"m²\np="+(Pressure/1000).ToString("F1")+"kPa · F₂="+OutputForce.ToString("F0")+"N\n举升 "+(Reading*100).ToString("F1")+"cm · A₁x₁=A₂x₂\n"+(Sealed?"密封有效（准静态液压）":"接口泄漏：先关阀，再装密封圈");
    }
}
