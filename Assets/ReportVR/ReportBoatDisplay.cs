using UnityEngine;

// The tabletop experiment is the only simulation; the canal assembly displays its state.
public sealed class ReportBoatDisplay : MonoBehaviour
{
    public ReportBuoyancyMeasure experiment;
    public Transform hull,gate;
    public GameObject[] cargo;
    public float draftScale=1.8f,gateTravel=1.65f;
    public Vector3 transitTravel=Vector3.right*2.8f;
    Vector3 hullRest,gateRest;
    Quaternion rotationRest;
    float sourceHeight;
    void Awake(){hullRest=hull.localPosition;gateRest=gate.localPosition;rotationRest=hull.localRotation;sourceHeight=experiment.hull.position.y;}
    void LateUpdate()
    {
        hull.localPosition=hullRest+Vector3.up*(experiment.hull.position.y-sourceHeight)*draftScale+transitTravel*experiment.Transit;
        hull.localRotation=rotationRest*experiment.hull.localRotation;
        gate.localPosition=gateRest+Vector3.up*experiment.gate.Value*gateTravel;
        for(int i=0;i<cargo.Length;i++)cargo[i].SetActive(experiment.seats[i].Loaded);
    }
}
