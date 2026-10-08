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
    void Update(){if(readout!=null)readout.text="推动第一块，观察碰撞接连传递\n"+(Reading==dominoes.Length?"骨牌全部倒下":Reading>0?"碰撞正在沿骨牌传递":"用球轻推第一块骨牌");}
    public override void ResetMeasurements(){if(positions==null)return;for(int i=0;i<dominoes.Length;i++){dominoes[i].velocity=dominoes[i].angularVelocity=Vector3.zero;dominoes[i].position=positions[i];dominoes[i].rotation=rotations[i];}}
}
