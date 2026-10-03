using UnityEngine;

public enum ReportAcousticReading {Strike,Resonance,Beats}
public sealed class ReportAcousticMeasure : ReportLearningMeasure
{
    public ReportAcousticReading experiment;
    public ReportGrabbable mallet;
    public ReportMechanismHandle frequency;
    public Transform vibrationMarker;
    public TextMesh readout;
    [Min(1)] public float naturalFrequency=100;
    [Range(.01f,.3f)] public float damping=.04f;
    [Min(.05f)] public float minimumStrikeSpeed=.35f;
    public int Strikes {get;private set;}
    public float Amplitude {get;private set;}
    public override float Reading=>experiment==ReportAcousticReading.Strike?Strikes:experiment==ReportAcousticReading.Beats?Mathf.Abs(naturalFrequency-frequency.Value):Amplitude;
    public override int Operations=>experiment==ReportAcousticReading.Strike?Strikes:frequency.Operations;
    public override bool Held=>experiment==ReportAcousticReading.Strike?false:frequency.isSelected;
    float ringUntil;
    double phaseA,phaseB;
    volatile float audioFrequency=100,audioGain;
    AudioClip clip;
    Vector3 vibrationBase;
    int sampleRate=44100;
    void Start()
    {
        sampleRate=AudioSettings.outputSampleRate;if(vibrationMarker!=null)vibrationBase=vibrationMarker.localPosition;
        var audio=gameObject.AddComponent<AudioSource>();audio.spatialBlend=1;audio.minDistance=1;audio.maxDistance=12;audio.loop=true;audio.volume=.25f;
        clip=AudioClip.Create("物理实验合成音",44100,1,44100,false);audio.clip=clip;audio.Play();
    }
    void OnCollisionEnter(Collision contact)
    {
        if(experiment!=ReportAcousticReading.Strike || contact.collider.GetComponentInParent<ReportGrabbable>()!=mallet || contact.relativeVelocity.magnitude<minimumStrikeSpeed)return;
        Strikes++;ringUntil=Time.time+2;
    }
    void Update()
    {
        float ratio=frequency!=null?frequency.Value/naturalFrequency:1;
        Amplitude=2*damping/Mathf.Sqrt(Mathf.Pow(1-ratio*ratio,2)+Mathf.Pow(2*damping*ratio,2));
        audioFrequency=frequency!=null?frequency.Value:naturalFrequency;audioGain=experiment==ReportAcousticReading.Strike?Mathf.Clamp01(ringUntil-Time.time):experiment==ReportAcousticReading.Resonance?Amplitude:1;
        if(vibrationMarker!=null)vibrationMarker.localPosition=vibrationBase+Vector3.right*(Mathf.Sin(Time.time*6)*Amplitude*.16f);
        if(readout!=null)readout.text=experiment==ReportAcousticReading.Strike?"实际敲击次数 "+Strikes+"\n用钟槌敲击钟身，轻放不计":experiment==ReportAcousticReading.Resonance?"驱动 "+frequency.Value.ToString("F1")+"Hz · 固有100Hz\n归一化振幅 "+Amplitude.ToString("F2")+"\n振幅标记慢速展示（非100Hz动画）":"两声源：100Hz / "+frequency.Value.ToString("F1")+"Hz\n实算拍频 "+Reading.ToString("F2")+"Hz · 听音调平";
    }
    void OnAudioFilterRead(float[] data,int channels)
    {
        double da=2*System.Math.PI*naturalFrequency/sampleRate,db=2*System.Math.PI*audioFrequency/sampleRate;
        for(int i=0;i<data.Length;i+=channels){float value=(float)(System.Math.Sin(phaseB)+(experiment==ReportAcousticReading.Beats?System.Math.Sin(phaseA):0))*.12f*audioGain;phaseA=(phaseA+da)%(2*System.Math.PI);phaseB=(phaseB+db)%(2*System.Math.PI);for(int c=0;c<channels;c++)data[i+c]=value;}
    }
    public override void ResetMeasurements(){Strikes=0;ringUntil=0;}
    void OnDestroy(){if(clip!=null)Destroy(clip);}
}
