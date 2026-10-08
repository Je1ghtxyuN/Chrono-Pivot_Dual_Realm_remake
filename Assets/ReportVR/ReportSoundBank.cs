using System;
using UnityEngine;

public enum ReportSound
{
    Grab,Release,Throw,ImpactWood,ImpactStone,ImpactMetal,ImpactSnow,ImpactGlass,
    Socket,SnowBond,TargetHit,HandleWood,HandleMetal,HandleStone,HandleSlide,BellStrike,
    WaterFlow,WaterSplash,ElectricHum,ElectricLatch,ElectricOff,Heat,MeltDrop,LightAligned,
    Domino,Roll,DoorMove,DoorUnlock,StepComplete,LevelComplete,Reset,
    UIHover,UIClick,UIAdjust,MenuOpen,MenuClose,SceneTravel,FootStone,FootWood,FootSnow
}
[Serializable]
public sealed class ReportSoundEntry
{
    public ReportSound cue;
    public AudioClip clip;
    [Range(0,1)] public float volume=.4f;
    public Vector2 pitchRange=new Vector2(.96f,1.04f);
    [Min(0)] public float minimumInterval=.04f;
    [Range(0,256)] public int priority=128;
}
[CreateAssetMenu(menuName="Chrono Pivot/交互音效库",fileName="InteractionSoundBank")]
public sealed class ReportSoundBank : ScriptableObject
{
    [Range(0,1),Tooltip("音效库总音量；菜单中的音量滑块继续控制 AudioListener 总音量。")]
    public float masterVolume=.8f;
    [Range(8,48)] public int maximumVoices=24;
    [Min(.1f)] public float nearDistance=1.2f;
    [Min(1)] public float farDistance=16;
    public ReportSoundEntry[] sounds=new ReportSoundEntry[0];
    public ReportSoundEntry Find(ReportSound cue)=>Array.Find(sounds,s=>s.cue==cue);
}
