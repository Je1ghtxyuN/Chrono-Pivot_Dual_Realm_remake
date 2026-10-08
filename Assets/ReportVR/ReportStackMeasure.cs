using UnityEngine;

// Read only released, physically settled pieces; no snap/freeze or scripted stacking.
public sealed class ReportStackMeasure : ReportLearningMeasure
{
    public ReportGrabbable[] pieces;
    public Transform[] positions;
    public Collider baseSupport;
    [Min(.01f)] public float tolerance=.16f,maximumSpeed=.12f;
    public TextMesh readout;
    public override float Reading
    {
        get
        {
            int count=0;
            for(int i=0;i<pieces.Length;i++)
            {
                var body=pieces[i].GetComponent<Rigidbody>();var bounds=pieces[i].GetComponent<Collider>().bounds;
                var support=(i==0?baseSupport:pieces[i-1].GetComponent<Collider>()).bounds;
                bool contact=Mathf.Abs(bounds.min.y-support.max.y)<.03f && bounds.max.x>support.min.x && bounds.min.x<support.max.x && bounds.max.z>support.min.z && bounds.min.z<support.max.z;
                if(count!=i || pieces[i].isSelected || body.isKinematic || !contact || Vector3.Distance(body.position,positions[i].position)>=tolerance || body.velocity.magnitude>=maximumSpeed || body.angularVelocity.magnitude>=.5f)break;
                count++;
            }
            return count;
        }
    }
    public override int Operations=>Reading==pieces.Length?1:0;
    public override bool Held {get{foreach(var p in pieces)if(p.isSelected)return true;return false;}}
    void Update(){if(readout!=null)readout.text="按标记逐层摆放，观察接触与支撑\n"+(Reading==pieces.Length?"结构已稳住：松手观察":"放稳下层，再放上层并松手");}
    public override void ResetMeasurements(){foreach(var p in pieces)p.GetComponent<ReportSnowBond>()?.ResetBond();}
}
