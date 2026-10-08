using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

[CustomEditor(typeof(DesktopPlayer))]
public sealed class DesktopPlayerEditor : Editor
{
    void Field(string name,string label)
    {
        var property=serializedObject.FindProperty(name);
        EditorGUILayout.PropertyField(property,new GUIContent(label,property.tooltip),true);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Field("head","视角相机");Field("controlMode","操作模式（自动 / 键鼠 / VR）");Field("speed","WASD 移动速度（米/秒）");Field("sensitivity","鼠标转向灵敏度（度/像素）");
        Field("pitchLimit","俯仰角限制（度）");Field("invertMouseY","反转鼠标垂直方向");
        Field("eyeHeight","桌面眼睛高度（米）");Field("bodyHeight","桌面胶囊高度（米）");Field("bodyCenter","桌面胶囊中心（米）");
        Field("gravityMultiplier","重力倍率");Field("maximumFallSpeed","最大下落速度（米/秒）");Field("groundStickSpeed","贴地速度（米/秒）");
        Field("showControls","显示操作提示");Field("showCrosshair","显示准星");
        serializedObject.ApplyModifiedProperties();
        var player=(DesktopPlayer)target;
        EditorGUILayout.Space();EditorGUILayout.LabelField("SteamVR 玩家参数",EditorStyles.boldLabel);
        Provider(player.GetComponent<ContinuousMoveProvider>(),new[]{"m_MoveSpeed"},new[]{"VR 行走速度（米/秒）"});
        Provider(player.GetComponent<SnapTurnProvider>(),new[]{"m_TurnAmount","m_DebounceTime"},new[]{"每次 VR 转向角度（度）","VR 转向间隔（秒）"});
        EditorGUILayout.HelpBox("VR 当前使用分段转向：角度与间隔决定转向节奏。以上 VR 字段直接编辑原有 Provider，支持撤销和场景保存。运行时调参退出 Play 后会恢复，请在停止播放后保存正式值。",MessageType.Info);
        if(Application.isPlaying)EditorGUILayout.LabelField("当前操作模式",player.IsDesktop?"键鼠":"VR");
    }
    static void Provider(Component provider,string[] names,string[] labels)
    {
        if(provider==null){EditorGUILayout.HelpBox("该物体未配置对应 VR Provider。请选择 Report VR Origin。",MessageType.Warning);return;}
        var settings=new SerializedObject(provider);settings.Update();
        for(int i=0;i<names.Length;i++)EditorGUILayout.PropertyField(settings.FindProperty(names[i]),new GUIContent(labels[i]));
        settings.ApplyModifiedProperties();
    }
}
