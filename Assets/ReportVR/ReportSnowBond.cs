using UnityEngine;

// Compressed snow bonds only after the released surfaces make physical contact.
public sealed class ReportSnowBond : MonoBehaviour
{
    public ReportGrabbable piece;
    public Transform assemblyPoint;
    public Collider support;
    public ReportSnowBond supportBond;
    [Min(.01f)] public float alignmentTolerance=.12f,contactTolerance=.012f;
    [Min(1)] public float breakForce=1500;
    public bool Bonded=>joint!=null;
    FixedJoint joint;
    void FixedUpdate()
    {
        if(piece.isSelected){ResetBond();return;}
        if(Bonded || supportBond!=null && !supportBond.Bonded)return;
        if(Vector3.Distance(piece.transform.position,assemblyPoint.position)>alignmentTolerance)return;
        var body=piece.GetComponent<Rigidbody>();var collider=piece.GetComponent<Collider>();
        float gap=collider.bounds.min.y-support.bounds.max.y;
        if(Mathf.Abs(gap)>contactTolerance || Vector3.Dot(piece.transform.up,Vector3.up)<.995f)return;
        // Loss of kinetic energy models compaction. The joint never teleports/snaps the piece.
        body.velocity=body.angularVelocity=Vector3.zero;
        joint=piece.gameObject.AddComponent<FixedJoint>();joint.connectedBody=support.attachedRigidbody;joint.breakForce=joint.breakTorque=breakForce;
    }
    public void ResetBond(){if(joint!=null){Destroy(joint);joint=null;}}
}
