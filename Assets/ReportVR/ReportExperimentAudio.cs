using UnityEngine;

// Only moving/energized experiments emit continuous sound; idle apparatus stays quiet.
public sealed class ReportExperimentAudio : MonoBehaviour
{
    [Range(0,1)] public float volume=1;
    DemoLevel level;
    ReportBuoyancyMeasure[] boats;
    ReportHydraulicMeasure[] hydraulics;
    ReportElectromagnetMeasure[] magnets;
    ReportHeatMeasure[] heats;
    ReportMirrorMeasure[] mirrors;
    AudioSource[] water,hum,heat;
    AudioSource door;
    bool[] energized,aligned;
    float[] meltCheckpoint;
    Vector3 doorPosition;
    void Start()
    {
        level=GetComponent<DemoLevel>();boats=FindObjectsOfType<ReportBuoyancyMeasure>();hydraulics=FindObjectsOfType<ReportHydraulicMeasure>();
        magnets=FindObjectsOfType<ReportElectromagnetMeasure>();heats=FindObjectsOfType<ReportHeatMeasure>();mirrors=FindObjectsOfType<ReportMirrorMeasure>();
        water=new AudioSource[boats.Length+hydraulics.Length];hum=new AudioSource[magnets.Length];heat=new AudioSource[heats.Length];
        energized=new bool[magnets.Length];aligned=new bool[mirrors.Length];
        meltCheckpoint=new float[heats.Length];
        for(int i=0;i<boats.Length;i++)water[i]=ReportAudio.CreateLoop(ReportSound.WaterFlow,boats[i].gateLeaf!=null?boats[i].gateLeaf:boats[i].transform);
        for(int i=0;i<hydraulics.Length;i++)water[boats.Length+i]=ReportAudio.CreateLoop(ReportSound.WaterFlow,hydraulics[i].transform);
        for(int i=0;i<magnets.Length;i++){hum[i]=ReportAudio.CreateLoop(ReportSound.ElectricHum,magnets[i].transform);energized[i]=magnets[i].Energized;}
        for(int i=0;i<heats.Length;i++)heat[i]=ReportAudio.CreateLoop(ReportSound.Heat,heats[i].warmZone);
        for(int i=0;i<mirrors.Length;i++)aligned[i]=mirrors[i].Reading>0;
        if(level.exitDoor!=null){doorPosition=level.exitDoor.position;door=ReportAudio.CreateLoop(ReportSound.DoorMove,level.exitDoor);}
    }
    void Update()
    {
        if(boats==null)return;
        for(int i=0;i<boats.Length;i++)ReportAudio.SetLoop(water[i],ReportSound.WaterFlow,volume*Mathf.Clamp01(boats[i].gate.Value));
        for(int i=0;i<hydraulics.Length;i++)ReportAudio.SetLoop(water[boats.Length+i],ReportSound.WaterFlow,volume*hydraulics[i].inlet.Value*(hydraulics[i].Sealed?.3f:.85f));
        for(int i=0;i<magnets.Length;i++)
        {
            var magnet=magnets[i];ReportAudio.SetLoop(hum[i],ReportSound.ElectricHum,magnet.CoreInstalled?volume*Mathf.Clamp01(magnet.current.Value/magnet.safeCurrent):0);
            if(magnet.Energized!=energized[i]){energized[i]=magnet.Energized;ReportAudio.Play(energized[i]?ReportSound.ElectricLatch:ReportSound.ElectricOff,magnet.armature.position,volume);}
        }
        for(int i=0;i<heats.Length;i++)
        {
            var model=heats[i];bool warming=!model.sample.isSelected && Vector3.Distance(model.sample.transform.position,model.warmZone.position)<model.zoneRadius && model.MeltFraction<1;
            bool covered=model.insulation!=null && !model.insulation.isSelected && Vector3.Distance(model.insulation.transform.position,model.coverSeat.position)<.25f;
            ReportAudio.SetLoop(heat[i],ReportSound.Heat,warming?volume*.7f*(covered?model.insulationFactor:1):0);
            if(model.MeltFraction<meltCheckpoint[i])meltCheckpoint[i]=model.MeltFraction;
            if(model.MeltFraction-meltCheckpoint[i]>=.08f){meltCheckpoint[i]=model.MeltFraction;ReportAudio.Play(ReportSound.MeltDrop,model.sample.transform.position,volume*.5f);}
        }
        for(int i=0;i<mirrors.Length;i++)
        {
            bool hit=mirrors[i].Reading>0;if(hit && !aligned[i])ReportAudio.Play(ReportSound.LightAligned,mirrors[i].receiver.transform.position,volume);aligned[i]=hit;
        }
        if(door!=null)
        {
            float distance=Vector3.Distance(doorPosition,level.exitDoor.position);ReportAudio.SetLoop(door,ReportSound.DoorMove,distance>.0002f?volume:0);doorPosition=level.exitDoor.position;
        }
    }
    void OnDisable()
    {
        foreach(var sources in new[]{water,hum,heat})if(sources!=null)foreach(var source in sources)if(source!=null)source.Stop();
        if(door!=null)door.Stop();
    }
}
