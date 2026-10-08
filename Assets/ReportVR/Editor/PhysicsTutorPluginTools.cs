using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PhysicsTutorPluginTools
{
    const string Root="Assets/ReportVR/PhysicsTutor";
    [MenuItem("Report VR/AI Tutor/Select Connection Settings")]
    public static void SelectSettings()
    {
        Selection.activeObject=AssetDatabase.LoadAssetAtPath<PhysicsTutorConnection>(Root+"/Resources/PhysicsTutorConnection.asset");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }
    [MenuItem("Report VR/AI Tutor/Export Game Plugin")]
    public static void Export()
    {
        var settings=AssetDatabase.LoadAssetAtPath<PhysicsTutorConnection>(Root+"/Resources/PhysicsTutorConnection.asset");
        if(settings==null)throw new InvalidOperationException("Missing tutor connection settings");
        Directory.CreateDirectory("Builds/PhysicsTutor");
        AssetDatabase.ExportPackage(new[]{Root,"Assets/ReportVR/ReportGameUI.cs","Assets/ReportVR/Editor/PhysicsTutorPluginTools.cs"},"Builds/PhysicsTutor/ChronoPivot_PhysicsTutor.unitypackage",ExportPackageOptions.Recurse);
        Debug.Log("PHYSICS_TUTOR_PLUGIN_EXPORTED");
    }
    public static void BuildAndExport()
    {
        SetupReportUI.Validate();
        Export();
        SetupReportVR.BuildWindows();
    }
}
