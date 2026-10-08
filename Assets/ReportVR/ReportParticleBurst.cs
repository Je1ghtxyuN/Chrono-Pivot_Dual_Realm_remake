using UnityEngine;

// Presentation only: explicit world-space particles never apply forces to an experiment.
public static class ReportParticleBurst
{
    public static void Emit(ParticleSystem system,Vector3 position,Vector3 velocity,int count,float spread)
    {
        if(!system.isPlaying)system.Play();
        for(int i=0;i<count;i++)
        {
            var variation=Random.insideUnitSphere*spread;variation.y=Mathf.Abs(variation.y);
            var particle=new ParticleSystem.EmitParams{position=position,velocity=velocity+variation};system.Emit(particle,1);
        }
    }
    public static void Clear(ParticleSystem system){if(system!=null)system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
}
