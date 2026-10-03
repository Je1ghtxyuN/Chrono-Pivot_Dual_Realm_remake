using UnityEngine;

// Seat a released piece so that later grabs cannot knock completed placements away.
public sealed class ReportPlacementSocket : MonoBehaviour
{
    public ReportGrabbable item;
    [Min(.05f)] public float captureDistance=.4f;
    Rigidbody body;
    RigidbodyConstraints originalConstraints;
    bool seated;
    void Awake(){body=item.GetComponent<Rigidbody>();originalConstraints=body.constraints;}
    void Update()
    {
        bool ready=!item.isSelected && Vector3.Distance(item.transform.position,transform.position)<=captureDistance;
        var track=item.GetComponent<ReportThrowTrack>();if(track!=null && track.InFlight)ready=false;
        if(seated && !ready){body.constraints=originalConstraints;seated=false;}
        if(ready && !seated)
        {
            body.velocity=Vector3.zero;body.angularVelocity=Vector3.zero;
            body.position=transform.position;item.transform.position=transform.position;
            body.constraints=RigidbodyConstraints.FreezeAll;body.Sleep();seated=true;
        }
    }
    void OnDisable(){if(seated && body!=null)body.constraints=originalConstraints;seated=false;}
}
