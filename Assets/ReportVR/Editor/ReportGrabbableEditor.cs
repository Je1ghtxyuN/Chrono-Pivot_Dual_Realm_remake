using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit.Interactables;
using UnityEngine;

[CustomEditor(typeof(ReportGrabbable))]
public sealed class ReportGrabbableEditor : XRGrabInteractableEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();serializedObject.Update();
        EditorGUILayout.Space();EditorGUILayout.LabelField("释放速度",EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("throwInDesktop"),new GUIContent("桌面允许抛掷"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumReleaseSpeed"),new GUIContent("最大释放速度（米/秒）"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumReleaseAngularSpeed"),new GUIContent("最大释放角速度（弧度/秒）"));
        serializedObject.ApplyModifiedProperties();
    }
}
