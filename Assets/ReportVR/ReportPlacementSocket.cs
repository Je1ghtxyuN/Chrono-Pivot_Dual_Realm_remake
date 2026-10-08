using UnityEngine;

// Seat a released piece so that later grabs cannot knock completed placements away.
public sealed class ReportPlacementSocket : MonoBehaviour
{
    public ReportGrabbable item;
    [Min(.05f)] public float captureDistance=.4f;
    public bool alignRotation;
    public ReportMechanismHandle requiredClosedValve;
    public bool Seated=>seated && !item.isSelected;
    Rigidbody body;
    RigidbodyConstraints originalConstraints;
    bool seated;
    void Awake(){body=item.GetComponent<Rigidbody>();originalConstraints=body.constraints;}
    void Update()
    {
        bool ready=!item.isSelected && Vector3.Distance(item.transform.position,transform.position)<=captureDistance;
        ready &= seated || requiredClosedValve==null || requiredClosedValve.Value<=.02f;
        var track=item.GetComponent<ReportThrowTrack>();if(track!=null && track.InFlight)ready=false;
        if(seated && !ready){body.constraints=originalConstraints;seated=false;}
        if(ready && !seated)
        {
            body.velocity=Vector3.zero;body.angularVelocity=Vector3.zero;
            body.position=transform.position;item.transform.position=transform.position;
            if(alignRotation){body.rotation=transform.rotation;item.transform.rotation=transform.rotation;}
            body.constraints=RigidbodyConstraints.FreezeAll;body.Sleep();seated=true;
            ReportAudio.Play(ReportSound.Socket,transform.position);
        }
    }
    void OnDisable(){if(seated && body!=null)body.constraints=originalConstraints;seated=false;}
}
