using UnityEngine;

[CreateAssetMenu(menuName="Chrono Pivot/场景氛围配色",fileName="AtmosphereProfile")]
public sealed class ReportAtmosphereProfile : ScriptableObject
{
    [Header("天空 · 无贴图程序渐变")]
    public Color zenith=new Color(.29f,.65f,.84f);
    public Color horizon=new Color(.93f,.86f,.72f);
    public Color clouds=new Color(1f,.98f,.9f);
    [Range(0,1)] public float cloudCover=.45f;
    [Range(0,1)] public float stars=0;
    [Header("远景日晷 · 仅作主题装饰")]
    [Min(3)] public float dialRadius=9.5f;
    [Min(40)] public float dialDistance=70;
    public float dialHeight=26;
    public float dialSideOffset=19;
    public Color dialFace=new Color(.61f,.79f,.79f);
    public Color brass=new Color(.91f,.71f,.36f);
    [Header("山峦与小景")]
    [Min(25)] public float mountainDistance=62;
    [Range(4,14)] public int mountainCount=9;
    [Range(0,10)] public int plantingCount=6;
    public Color distantMountain=new Color(.57f,.73f,.76f);
    public Color nearbyMountain=new Color(.42f,.63f,.63f);
    public Color foliage=new Color(.28f,.57f,.44f);
    public Color foliageLight=new Color(.54f,.75f,.55f);
    public Color blossom=new Color(.96f,.74f,.65f);
    public Color stone=new Color(.72f,.77f,.72f);
    public Color ivory=new Color(.96f,.94f,.84f);
    public Color wood=new Color(.54f,.39f,.24f);
    public int seed=317;
    [Header("空气透视")]
    public Color fogColor=new Color(.78f,.87f,.85f);
    [Range(0,.02f)] public float fogDensity=.004f;
}
