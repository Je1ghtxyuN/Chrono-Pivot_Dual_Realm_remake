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
    public bool CoreInstalled=>!core.isSelected && Vector3.Distance(core.transform.position,coreSeat.position)<.25f;
    public float Field=>4*Mathf.PI*1e-7f*turns*current.Value/coilLength*1000;
    public bool Energized=>CoreInstalled && Field>=2 && current.Value<=safeCurrent;
    public override float Reading=>Energized?Field:0;
    public override int Operations=>current.Operations;
    public override bool Held=>current.isSelected || core.isSelected;
    Vector3 rest;
    void Awake(){rest=armature.localPosition;}
    void Update()
    {
        armature.localPosition=Vector3.MoveTowards(armature.localPosition,rest+Vector3.up*(Energized?.12f:0),Time.deltaTime*.25f);
        if(latch!=null && !Energized)latch.SetValue(0);
        if(readout!=null)readout.text="N="+turns+" · L="+coilLength.ToString("F2")+"m\nI="+current.Value.ToString("F2")+"A · B="+Field.ToString("F2")+"mT\n"+(!CoreInstalled?"请把铁芯装入线圈":current.Value>safeCurrent?"过流：磁锁禁止开启":Energized?"铁芯已吸合，可以拉门栓":"磁场不足")+"\nB为理想长螺线管估算";
    }
}
