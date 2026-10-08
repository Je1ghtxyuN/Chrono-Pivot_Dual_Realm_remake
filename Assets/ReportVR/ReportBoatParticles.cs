using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class ReportBoatParticles : MonoBehaviour
{
    public ReportBuoyancyMeasure experiment;
    public ReportBoatDisplay display;
    public ParticleSystem splashes,displaySplashes;
    public float displayWaterHeight;
    public Vector3 displayEdge=new Vector3(1,0,.78f);
    float lastMass,wakeDistance;
    Vector3 lastPosition;
    void Awake(){ResetFeedback();}
    void OnEnable(){ResetFeedback();}
    void LateUpdate()
    {
        if(Application.isPlaying && Time.timeScale<=0)return;
        float mass=experiment.TotalMass;
        wakeDistance+=Vector3.ProjectOnPlane(experiment.hull.position-lastPosition,Vector3.up).magnitude;
        bool changed=Mathf.Abs(mass-lastMass)>.01f,moved=wakeDistance>=.07f;
        if((changed || moved) && experiment.hull.position.y+experiment.hullDepth*.5f>experiment.waterHeight-.02f)
        {
            Splash(splashes,experiment.hull,experiment.waterHeight,new Vector3(0,0,.28f),changed?5:1,.8f);
            if(display!=null)Splash(displaySplashes,display.hull,displayWaterHeight,displayEdge,changed?5:1,1);
        }
        if(moved)wakeDistance=0;
        lastMass=mass;lastPosition=experiment.hull.position;
    }
    static void Splash(ParticleSystem system,Transform hull,float height,Vector3 edge,int count,float speed)
    {
        foreach(float side in new[]{-1f,1f})
        {
            for(int i=0;i<count;i++)
            {
                var point=hull.TransformPoint(new Vector3(edge.x+Random.Range(-.16f,.16f)*(edge.z/.28f),0,edge.z*side));point.y=height+.006f;
                ReportParticleBurst.Emit(system,point,Vector3.up*speed+hull.forward*side*.15f,1,.25f);
            }
        }
    }
    public void ResetFeedback(){ReportParticleBurst.Clear(splashes);ReportParticleBurst.Clear(displaySplashes);if(experiment!=null){lastMass=experiment.TotalMass;lastPosition=experiment.hull.position;}wakeDistance=0;}
    void OnDisable(){ResetFeedback();}
}
