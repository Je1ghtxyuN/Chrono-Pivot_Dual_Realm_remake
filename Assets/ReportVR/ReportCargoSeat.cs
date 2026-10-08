using UnityEngine;

// A released load follows the boat; grabbing it immediately frees it again.
public sealed class ReportCargoSeat : MonoBehaviour
{
    public ReportGrabbable item;
    public Transform loadingArea;
    public float captureDistance=.3f;
    public bool Loaded {get;private set;}
    public int Loads {get;private set;}
    Rigidbody body;
    RigidbodyConstraints original;
    void Awake(){body=item.GetComponent<Rigidbody>();original=body.constraints;}
    void FixedUpdate()
    {
        bool near=!item.isSelected && Vector3.Distance(item.transform.position,transform.position)<captureDistance;
        if(loadingArea!=null && !Loaded)
        {
            var position=loadingArea.InverseTransformPoint(item.transform.position);
            near=!item.isSelected && Mathf.Abs(position.x)<=.46f && Mathf.Abs(position.z)<=.25f && position.y>=.22f && position.y<=.80f;
        }
        if(Loaded && !near){body.constraints=original;Loaded=false;}
        if(!near)return;
        if(!Loaded){Loads++;Loaded=true;body.constraints=RigidbodyConstraints.FreezeAll;ReportAudio.Play(ReportSound.Socket,transform.position);ReportAudio.Play(ReportSound.WaterSplash,transform.position,.55f);}
        body.velocity=body.angularVelocity=Vector3.zero;body.position=transform.position;body.rotation=transform.rotation;item.transform.SetPositionAndRotation(transform.position,transform.rotation);
    }
    public void ResetSeat(){Loaded=false;Loads=0;if(body!=null)body.constraints=original;}
    void OnDisable(){ResetSeat();}
}
