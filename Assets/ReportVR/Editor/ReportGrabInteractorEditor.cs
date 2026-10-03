using UnityEditor;

// The inherited XRI custom Inspector draws only its own fields.
[CustomEditor(typeof(ReportGrabInteractor))]
public sealed class ReportGrabInteractorEditor : Editor
{
    public override void OnInspectorGUI(){DrawDefaultInspector();}
}
