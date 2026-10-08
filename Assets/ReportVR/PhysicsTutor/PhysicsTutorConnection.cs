using UnityEngine;

[CreateAssetMenu(menuName="Chrono Pivot/Physics Tutor Connection")]
public sealed class PhysicsTutorConnection : ScriptableObject
{
    [Tooltip("基础模型路径预留字段；此插件通过 HTTP 调用服务，不直接加载模型。")]
    public string baseModelPath="";
    [Tooltip("模型服务准备好后填写完整 /api/tutor 地址；留空时只调试输入。")]
    public string serviceEndpoint="";
}
