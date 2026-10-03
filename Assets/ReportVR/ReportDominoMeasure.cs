using UnityEngine;

public sealed class ReportDominoMeasure : ReportLearningMeasure
{
    public Rigidbody[] dominoes;
    public TextMesh readout;
    public override float Reading {get{int fallen=0;foreach(var d in dominoes)if(Vector3.Dot(d.transform.up,Vector3.up)<.55f)fallen++;return fallen;}}
    public override int Operations=>Reading>0?1:0;
    Vector3[] positions;
    Quaternion[] rotations;
    void Awake(){positions=new Vector3[dominoes.Length];rotations=new Quaternion[dominoes.Length];for(int i=0;i<dominoes.Length;i++){positions[i]=dominoes[i].position;rotations[i]=dominoes[i].rotation;}}
    void Update(){if(readout!=null)readout.text="抛球或手推第一块，启动真实碰撞链\n已倒下 "+Reading.ToString("F0")+" / "+dominoes.Length+"\n高度0.36m · 间距0.15m · 刚体骨牌";}
    public override void ResetMeasurements(){if(positions==null)return;for(int i=0;i<dominoes.Length;i++){dominoes[i].velocity=dominoes[i].angularVelocity=Vector3.zero;dominoes[i].position=positions[i];dominoes[i].rotation=rotations[i];}}
}
