using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class EditableLevelCheck
{
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Validate()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        var player=Object.FindObjectOfType<DesktopPlayer>();var settings=new SerializedObject(player);
        Require(settings.FindProperty("pitchLimit")!=null,"Player pitch limit cannot be adjusted in Inspector");
        settings.FindProperty("pitchLimit").floatValue=120;settings.FindProperty("speed").floatValue=-2;settings.ApplyModifiedPropertiesWithoutUndo();
        player.SendMessage("OnValidate");settings.Update();
        Require(settings.FindProperty("pitchLimit").floatValue<=89 && settings.FindProperty("speed").floatValue>=0,"Invalid movement/look values are not constrained");
        var level=Object.FindObjectOfType<DemoLevel>();
        foreach(var text in level.GetComponentsInChildren<TextMesh>())
        {
            var facing=text.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="ReportTextFacing");
            Require(facing!=null,"Text has no explicit facing mode: "+text.name);
            var facingSettings=new SerializedObject(facing);
            Require(facingSettings.FindProperty("mode").enumValueIndex==0,"Mounted sign must default to fixed world orientation: "+text.name);
        }
        for(int i=0;i<3;i++)
        {
            var root=GameObject.Find("HubMaterialCube_"+i);var specimen=root.transform.Find("材料样品");var stand=root.transform.Find("底座");
            Require(specimen!=null && stand!=null,"Material and stand are not independently editable: "+root.name);
            var before=stand.position;specimen.position+=Vector3.up*.3f;
            Require(Vector3.Distance(stand.position,before)<.0001f,"Editing material also moved its stand");
            Require(specimen.GetComponent<MeshCollider>()!=null && stand.GetComponent<MeshCollider>()!=null,"Separated models lost collisions");
        }
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path); // Discard test edits.
        Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/editable-levels.txt","PASS: first-level Inspector values serialize and constrain invalid values; all mounted text explicitly world fixed; wood/stone/bronze specimens editable independently of their collidable bases.\n");
        Debug.Log("EDITABLE_FIRST_LEVEL_OK");
    }
}
