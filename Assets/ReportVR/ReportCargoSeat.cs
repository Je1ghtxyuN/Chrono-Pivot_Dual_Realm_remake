using UnityEngine;

// A released load follows the boat; grabbing it immediately frees it again.
public sealed class ReportCargoSeat : MonoBehaviour
{
    public ReportGrabbable item;
    public float captureDistance=.3f;
    public bool Loaded {get;private set;}
    public int Loads {get;private set;}
    Rigidbody body;
    RigidbodyConstraints original;
    void Awake(){body=item.GetComponent<Rigidbody>();original=body.constraints;}
    void FixedUpdate()
    {
        bool near=!item.isSelected && Vector3.Distance(item.transform.position,transform.position)<captureDistance;
        if(Loaded && !near){body.constraints=original;Loaded=false;}
        if(!near)return;
        if(!Loaded){Loads++;Loaded=true;body.constraints=RigidbodyConstraints.FreezeAll;}
        body.velocity=body.angularVelocity=Vector3.zero;body.position=transform.position;item.transform.position=transform.position;
    }
    public void ResetSeat(){Loaded=false;Loads=0;if(body!=null)body.constraints=original;}
    void OnDisable(){ResetSeat();}
}
