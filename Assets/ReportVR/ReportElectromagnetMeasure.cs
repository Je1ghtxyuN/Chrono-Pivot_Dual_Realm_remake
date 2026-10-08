using UnityEngine;

public sealed class ReportElectromagnetMeasure : ReportLearningMeasure
{
    public ReportGrabbable core;
    public Transform coreSeat,armature;
    public ReportMechanismHandle current,latch;
    public TextMesh readout;
    [Min(1)] public float turns=400;
    [Min(.01f)] public float coilLength=.5f;
    [Min(.1f)] public float safeCurrent=3;
    public float armatureTravel=.13f;
    public bool CoreInstalled=>coreSeat.GetComponent<ReportPlacementSocket>().Seated;
    public float Field=>4*Mathf.PI*1e-7f*turns*current.Value/coilLength*1000;
    public bool Energized=>CoreInstalled && Field>=2 && current.Value<=safeCurrent;
    public bool ArmatureEngaged=>Energized && Mathf.Abs(armature.localPosition.y-(rest.y-armatureTravel))<.003f;
    public override float Reading=>ArmatureEngaged?Field:0;
    public override int Operations=>current.Operations;
    public override bool Held=>current.isSelected || core.isSelected;
    Vector3 rest;
    void Awake(){rest=armature.localPosition;}
    void Update()
    {
        armature.localPosition=Vector3.MoveTowards(armature.localPosition,rest-Vector3.up*(Energized?armatureTravel:0),Time.deltaTime*.25f);
        if(latch!=null && !ArmatureEngaged)latch.SetValue(0);
        if(readout!=null)readout.text="通电产生磁性，断电后衔铁落下\n"+(!CoreInstalled?"把铁芯放入青色孔位并松手":current.Value>safeCurrent?"供电过强：把滑杆退回绿色区域":ArmatureEngaged?"衔铁已吸合，可以拉门栓":Energized?"等待衔铁吸合":"向绿色区域拖动供电滑杆");
    }
    public override void ResetMeasurements(){armature.localPosition=rest;if(latch!=null)latch.SetValue(0);}
}
