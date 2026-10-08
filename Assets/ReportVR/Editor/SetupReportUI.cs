using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public static class SetupReportUI
{
    static InputActionReference Input(InputActionAsset asset,string name,params string[] bindings)
    {
        var action=asset.FindAction(name);
        if(action==null){action=asset.actionMaps[0].AddAction(name,InputActionType.Button);foreach(var binding in bindings)action.AddBinding(binding);}
        string path="Assets/ReportVR/Input/References/"+name+".asset";
        var reference=AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
        if(reference==null){reference=InputActionReference.Create(action);AssetDatabase.CreateAsset(reference,path);}
        return reference;
    }
    [MenuItem("Report VR/Install Game UI in All Levels")]
    public static void Apply()
    {
        const string inputPath="Assets/ReportVR/Input/ReportVR.inputactions";
        var asset=AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputPath);
        var menu=Input(asset,"MenuRight","<XRController>{RightHand}/secondaryButton","<XRController>{RightHand}/menu");
        var left=Input(asset,"UILeftTrigger","<XRController>{LeftHand}/triggerPressed");
        var right=AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/ReportVR/Input/References/Interact.asset");
        File.WriteAllText(inputPath,asset.ToJson());AssetDatabase.ImportAsset(inputPath,ImportAssetOptions.ForceUpdate);
        var entries=EditorBuildSettings.scenes.Where(s=>s.enabled).ToArray();var titles=new string[entries.Length];
        for(int i=0;i<entries.Length;i++){EditorSceneManager.OpenScene(entries[i].path);titles[i]=UnityEngine.Object.FindObjectOfType<DemoLevel>().title;}
        foreach(var entry in entries)
        {
            EditorSceneManager.OpenScene(entry.path);var origin=UnityEngine.Object.FindObjectOfType<XROrigin>();
            var ui=origin.GetComponent<ReportGameUI>();if(ui==null)ui=origin.gameObject.AddComponent<ReportGameUI>();
            ui.player=origin.GetComponent<DesktopPlayer>();ui.level=UnityEngine.Object.FindObjectOfType<DemoLevel>();ui.browser=origin.GetComponent<ReportSceneBrowser>();
            ui.menuInput=menu;ui.leftTrigger=left;ui.rightTrigger=right;ui.levelTitles=titles;
            var old=origin.transform.Find("Camera Floor Offset/Left Controller/Scene and controller instructions");if(old!=null)old.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();Validate();EditorSceneManager.OpenScene(entries[0].path);
        Debug.Log("REPORT_UI_INSTALLED scenes="+entries.Length);
    }
    public static void Validate()
    {
        int count=0;
        foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(entry.path);var ui=UnityEngine.Object.FindObjectOfType<ReportGameUI>();
            if(ui==null || ui.player==null || ui.level==null || ui.browser==null || ui.menuInput==null || ui.leftTrigger==null || ui.rightTrigger==null || ui.levelTitles.Length!=15)throw new Exception("UI binding missing: "+entry.path);count++;
        }
        Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/ui-scenes.txt","PASS: "+count+" scenes; playable UI component; player/level/browser references; both triggers and menu actions; 15 level titles.\n");
    }
}
