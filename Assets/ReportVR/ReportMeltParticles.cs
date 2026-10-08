using UnityEngine;

public sealed class ReportMeltParticles : MonoBehaviour
{
    public ReportHeatMeasure experiment;
    public ParticleSystem droplets;
    float lastFraction,pending;
    void Awake(){ResetFeedback();}
    void OnEnable(){ResetFeedback();}
    void LateUpdate()
    {
        if(Time.timeScale<=0)return;
        float fraction=experiment.MeltFraction,delta=fraction-lastFraction;lastFraction=fraction;
        if(delta<0){ResetFeedback();return;}
        if(delta<=0 || experiment.sample.isSelected)return;
        pending+=delta*70;int count=Mathf.Min(4,Mathf.FloorToInt(pending));pending-=count;
        var bounds=experiment.sample.GetComponent<Collider>().bounds;
        for(int i=0;i<count;i++)
        {
            var point=new Vector3(bounds.center.x+Random.Range(-.25f,.25f)*bounds.size.x,bounds.min.y+.025f,bounds.center.z+Random.Range(-.25f,.25f)*bounds.size.z);
            var particle=new ParticleSystem.EmitParams{position=point,velocity=Vector3.down*.12f};
            // End at the existing melt pool instead of falling through the tabletop.
            float fall=experiment.meltWater!=null?Mathf.Max(.015f,point.y-experiment.meltWater.position.y):.08f;
            particle.startLifetime=Mathf.Clamp((Mathf.Sqrt(.0144f+19.62f*fall)-.12f)/9.81f,.04f,.45f);
            if(!droplets.isPlaying)droplets.Play();droplets.Emit(particle,1);
        }
    }
    public void ResetFeedback(){ReportParticleBurst.Clear(droplets);lastFraction=experiment!=null?experiment.MeltFraction:0;pending=0;}
    void OnDisable(){ResetFeedback();}
}
