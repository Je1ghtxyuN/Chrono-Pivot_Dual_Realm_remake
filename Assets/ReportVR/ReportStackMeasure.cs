using UnityEngine;

// Read only released, physically settled pieces; no snap/freeze or scripted stacking.
public sealed class ReportStackMeasure : ReportLearningMeasure
{
    public ReportGrabbable[] pieces;
    public Transform[] positions;
    [Min(.01f)] public float tolerance=.16f,maximumSpeed=.12f;
    public TextMesh readout;
    public override float Reading
    {
        get{int count=0;for(int i=0;i<pieces.Length;i++){var body=pieces[i].GetComponent<Rigidbody>();if(!pieces[i].isSelected && Vector3.Distance(body.position,positions[i].position)<tolerance && body.velocity.magnitude<maximumSpeed)count++;}return count;}
    }
    public override int Operations=>Reading==pieces.Length?1:0;
    public override bool Held {get{foreach(var p in pieces)if(p.isSelected)return true;return false;}}
    void Update(){if(readout!=null)readout.text="按结构标记摆放，松手并等待稳定\n实际重力与接触支撑 "+Reading.ToString("F0")+" / "+pieces.Length+"\n手持 / 悬空 / 持续滑落均不计完成";}
    public override void ResetMeasurements(){foreach(var p in pieces)p.GetComponent<ReportSnowBond>()?.ResetBond();}
}
