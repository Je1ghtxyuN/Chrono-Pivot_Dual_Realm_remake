using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using Unity.XR.CoreUtils;

// Update existing rigs without rebuilding level geometry or changing authored tuning.
public static class ConfigureVRPlayer
{
    const string InputPath="Assets/ReportVR/Input/ReportVR.inputactions";
    static void Bind(InputActionAsset asset,string name,params string[] paths)
    {
        var action=asset.FindAction(name,true);
        for(int i=action.bindings.Count-1;i>=0;i--)action.ChangeBinding(i).Erase();
        foreach(string path in paths)action.AddBinding(path);
    }
    [MenuItem("Report VR/Update VR Player Controls")]
    public static void Apply()
    {
        var inputs=AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        inputs.Disable();
        Bind(inputs,"NextScene","<XRController>{RightHand}/primaryButton");
        Bind(inputs,"PreviousScene","<XRController>{LeftHand}/primaryButton");
        Bind(inputs,"Interact","<XRController>{RightHand}/triggerPressed");
        Bind(inputs,"ResetDemo","<XRController>{LeftHand}/menu","<XRController>{LeftHand}/secondaryButton");
        File.WriteAllText(InputPath,inputs.ToJson());AssetDatabase.ImportAsset(InputPath,ImportAssetOptions.ForceUpdate);
        inputs=AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
        int count=0;
        foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(entry.path);
            var origin=UnityEngine.Object.FindObjectOfType<XROrigin>();
            if(origin==null || origin.GetComponent<DesktopPlayer>()==null)throw new Exception("Player rig missing: "+entry.path);
            var manager=origin.GetComponent<InputActionManager>();
            if(manager==null)manager=origin.gameObject.AddComponent<InputActionManager>();
            if(!manager.actionAssets.Contains(inputs))manager.actionAssets.Add(inputs);
            foreach(var tracker in origin.GetComponentsInChildren<TrackedPoseDriver>(true))
            {
                tracker.updateType=TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
                tracker.ignoreTrackingState=false;
            }
            var wrist=origin.transform.Find("Camera Floor Offset/Left Controller/Scene and controller instructions");
            if(wrist!=null)wrist.GetComponent<TextMesh>().text="Left stick: walk | Right stick: turn\nHold grip: grab | Swing + release: throw\nRight trigger: use | Left B / menu: reset\nA / X: next / previous (after completion)";
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());count++;
        }
        AssetDatabase.SaveAssets();VerifyReportVR.Validate();
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        Debug.Log("REPORT_VR_PLAYER_CONFIGURED scenes="+count);
    }
}
