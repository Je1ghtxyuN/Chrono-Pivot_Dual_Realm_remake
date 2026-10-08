using UnityEngine;

public sealed class ReportHydraulicWaterFeedback : MonoBehaviour
{
    public ReportHydraulicMeasure experiment;
    public ParticleSystem leakage;
    public LineRenderer liquid;
    MaterialPropertyBlock properties;
    void Update()
    {
        if(experiment==null || leakage==null)return;
        bool leaking=experiment.seal!=null && !experiment.Sealed && experiment.inlet.Value>.02f;
        var emission=leakage.emission;emission.enabled=leaking;emission.rateOverTime=leaking?24*experiment.inlet.Value:0;
        if(Application.isPlaying && leaking && !leakage.isPlaying)leakage.Play();
        if(liquid!=null)
        {
            if(properties==null)properties=new MaterialPropertyBlock();
            liquid.GetPropertyBlock(properties);
            properties.SetFloat("_WaveAmplitude",0);properties.SetFloat("_Opacity",.8f);
            properties.SetColor("_ShallowColor",Color.Lerp(new Color(.07f,.23f,.25f),new Color(.12f,.58f,.62f),Mathf.Clamp01(experiment.Pressure/40000)));
            liquid.SetPropertyBlock(properties);
        }
    }
    void OnDisable(){if(leakage!=null)leakage.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
}
