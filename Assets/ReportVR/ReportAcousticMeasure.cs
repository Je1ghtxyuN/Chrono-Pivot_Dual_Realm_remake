using UnityEngine;

public enum ReportAcousticReading {Strike,Resonance,Beats}
public sealed class ReportAcousticMeasure : ReportLearningMeasure
{
    public ReportAcousticReading experiment;
    public DemoStation station;
    public ReportGrabbable mallet;
    public ReportMechanismHandle frequency;
    public ReportMechanismHandle fineFrequency;
    public Transform vibrationMarker;
    public Transform responsePointer;
    public Renderer feedbackIndicator;
    public TextMesh readout;
    [Min(1)] public float naturalFrequency=100;
    [Range(.01f,.3f)] public float damping=.04f;
    [Min(.05f)] public float minimumStrikeSpeed=.35f;
    [Range(0,1)] public float experimentVolume=.25f;
    [Range(0,1)] public float strikeVolume=.6f;
    public int Strikes {get;private set;}
    public float Amplitude {get;private set;}
    public float DriveFrequency=>frequency!=null?frequency.Value+(fineFrequency!=null?fineFrequency.Value:0):naturalFrequency;
    public float BeatEnvelope=>beatEnvelope;
    public override float Reading=>experiment==ReportAcousticReading.Strike?Strikes:experiment==ReportAcousticReading.Beats?Mathf.Abs(naturalFrequency-DriveFrequency):Amplitude;
    public override int Operations=>experiment==ReportAcousticReading.Strike?Strikes:frequency.Operations+(fineFrequency!=null?fineFrequency.Operations:0);
    public override bool Held=>experiment==ReportAcousticReading.Strike?mallet.isSelected:frequency.isSelected || fineFrequency!=null && fineFrequency.isSelected;
    float lastStrike=-100,strikeStrength;
    Vector3 previousMalletHead;
    bool trackingMallet;
    double phaseA,phaseB;
    volatile float audioFrequency=100,audioGain;
    volatile float beatEnvelope=1;
    AudioClip clip;
    AudioSource experimentAudio;
    Vector3 vibrationBase;
    MaterialPropertyBlock feedback;
    int sampleRate=44100;
    void Start()
    {
        sampleRate=AudioSettings.outputSampleRate;if(vibrationMarker!=null)vibrationBase=vibrationMarker.localPosition;
        // The accepted collision plays one decaying bell sound through ReportAudio.
        if(experiment==ReportAcousticReading.Strike)return;
        var audio=gameObject.AddComponent<AudioSource>();audio.spatialBlend=1;audio.minDistance=1;audio.maxDistance=12;audio.loop=true;audio.volume=experimentVolume;experimentAudio=audio;
        clip=AudioClip.Create("物理实验合成音",44100,1,44100,false);audio.clip=clip;audio.Play();
    }
    void OnCollisionEnter(Collision contact)
    {
        if(experiment!=ReportAcousticReading.Strike || contact.collider.GetComponentInParent<ReportGrabbable>()!=mallet || !mallet.isSelected)return;
        float speed=Mathf.Abs(Vector3.Dot(contact.relativeVelocity,contact.GetContact(0).normal));
        RegisterStrike(speed);
    }
    void FixedUpdate()
    {
        if(experiment!=ReportAcousticReading.Strike)return;
        if(!mallet.isSelected){trackingMallet=false;return;}
        var head=mallet.transform.TransformPoint(Vector3.up*.10f);
        if(trackingMallet)
        {
            var travel=head-previousMalletHead;float distance=travel.magnitude;
            // Sweep the held wooden head so a fast kinematic swing cannot tunnel through the bell.
            if(distance>=minimumStrikeSpeed*Time.fixedDeltaTime)
                foreach(var hit in Physics.SphereCastAll(previousMalletHead,.045f,travel/distance,distance,~0,QueryTriggerInteraction.Ignore))
                    if(hit.collider.GetComponent<ReportAcousticMeasure>()==this)RegisterStrike(Mathf.Max(0,-Vector3.Dot(travel/Time.fixedDeltaTime,hit.normal)));
        }
        previousMalletHead=head;trackingMallet=true;
    }
    void RegisterStrike(float speed)
    {
        if(speed<minimumStrikeSpeed || Time.time-lastStrike<.25f)return;
        Strikes++;lastStrike=Time.time;strikeStrength=Mathf.Lerp(.3f,1,Mathf.Clamp01(speed/2));
        ReportAudio.Play(ReportSound.BellStrike,transform.position,strikeVolume*strikeStrength,false,naturalFrequency/220);
    }
    public float ResponseAt(float drive)
    {
        float ratio=drive/naturalFrequency;
        return 2*damping*Mathf.Sqrt(1-damping*damping)/Mathf.Sqrt(Mathf.Pow(1-ratio*ratio,2)+Mathf.Pow(2*damping*ratio,2));
    }
    void Update()
    {
        Amplitude=experiment==ReportAcousticReading.Strike?strikeStrength*Mathf.Exp(-2.3f*(Time.time-lastStrike)):experiment==ReportAcousticReading.Resonance?ResponseAt(DriveFrequency):BeatEnvelope;
        audioFrequency=DriveFrequency;audioGain=experiment==ReportAcousticReading.Resonance?Amplitude:experiment==ReportAcousticReading.Beats?1:0;
        if(station!=null && station.level.Step!=station.index && !Held)audioGain=0;
        if(Time.timeScale==0)audioGain=0;
        if(experimentAudio!=null)experimentAudio.volume=experimentVolume;
        if(vibrationMarker!=null)
        {
            if(experiment==ReportAcousticReading.Beats)vibrationMarker.localScale=new Vector3(1,Mathf.Max(.02f,BeatEnvelope),1);
            else vibrationMarker.localPosition=vibrationBase+Vector3.right*(Mathf.Sin(Time.time*6)*Amplitude*(experiment==ReportAcousticReading.Strike?.012f:.16f));
        }
        if(responsePointer!=null)responsePointer.localPosition=new Vector3(-.68f+.9f*Mathf.InverseLerp(frequency.valueRange.x,frequency.valueRange.y,DriveFrequency),1.35f+.3f*Amplitude,-.17f);
        if(feedbackIndicator!=null)
        {
            if(feedback==null)feedback=new MaterialPropertyBlock();feedbackIndicator.GetPropertyBlock(feedback);
            bool matched=experiment==ReportAcousticReading.Strike?Strikes>0:experiment==ReportAcousticReading.Resonance?Amplitude>=.94f:Reading<=.3f;
            feedback.SetColor("_Color",matched?new Color(.3f,.85f,.45f):new Color(.95f,.65f,.22f));feedbackIndicator.SetPropertyBlock(feedback);
        }
        if(readout!=null)readout.text=experiment==ReportAcousticReading.Strike?"拿木槌敲钟，听声音逐渐消退\n敲响后松手观察":experiment==ReportAcousticReading.Resonance?(Amplitude>=.94f?"振动最明显：松手观察共振":"转动调节轮，寻找振动最明显的位置\n接近共振时，摆片振动更强"):(Reading<=.3f?"声音已平稳：松开两只旋钮":"调节旋钮，听声音忽强忽弱\n"+(DriveFrequency<naturalFrequency?"往高音方向调，再用微调轮靠近":"往低音方向调，再用微调轮靠近"));
    }
    void OnAudioFilterRead(float[] data,int channels)
    {
        if(experiment==ReportAcousticReading.Strike){System.Array.Clear(data,0,data.Length);return;}
        double da=2*System.Math.PI*naturalFrequency/sampleRate,db=2*System.Math.PI*audioFrequency/sampleRate;
        for(int i=0;i<data.Length;i+=channels){float value=(float)(System.Math.Sin(phaseB)+(experiment==ReportAcousticReading.Beats?System.Math.Sin(phaseA):0))*.12f*audioGain;phaseA=(phaseA+da)%(2*System.Math.PI);phaseB=(phaseB+db)%(2*System.Math.PI);for(int c=0;c<channels;c++)data[i+c]=value;}
        beatEnvelope=(float)System.Math.Abs(System.Math.Cos((phaseA-phaseB)*.5));
    }
    public override void ResetMeasurements(){Strikes=0;lastStrike=-100;strikeStrength=0;trackingMallet=false;Amplitude=0;audioGain=0;phaseA=phaseB=0;beatEnvelope=1;}
    void OnDestroy(){if(clip!=null)Destroy(clip);}
}
