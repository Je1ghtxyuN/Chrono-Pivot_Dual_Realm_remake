using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class SceneFourExperienceCheck
{
    static void Require(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(value,null);
    public static void Inspect()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[3].path);
        var names=new[]{"SevenStarConstellation","CompassSpoon pivot"};
        foreach(var name in names)
        {
            var go=GameObject.Find(name);if(go==null)continue;
            Debug.Log("SCENE4_INSPECT "+name+" parent="+go.transform.parent.name+" position="+go.transform.position+" scale="+go.transform.lossyScale+" bounds="+go.GetComponentInChildren<Renderer>()?.bounds);
        }
        Debug.Log("SCENE4_SKY "+AssetDatabase.GetAssetPath(RenderSettings.skybox));
        var mirror=Object.FindObjectOfType<ReportMirrorMeasure>();
        foreach(var r in Object.FindObjectsOfType<Renderer>().Where(r=>Vector3.Distance(r.bounds.center,mirror.mirror.transform.position)<1.4f && r.bounds.size.magnitude>.6f))
            Debug.Log("SCENE4_NEAR_MIRROR "+r.name+" parent="+r.transform.parent?.name+" bounds="+r.bounds);
    }
    public static void Verify()
    {
        Directory.CreateDirectory("Documentation/SceneFourReview");var results=new System.Collections.Generic.List<string>();
        foreach(var check in new Action[]{Spoon,Sky,Mirrors})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[3].path);
            try{check();results.Add("PASS "+check.Method.Name);}catch(Exception e){results.Add("FAIL "+check.Method.Name+": "+e.Message);}
        }
        File.WriteAllLines("Documentation/SceneFourReview/geometry.txt",results);
        foreach(var result in results)Debug.Log(result);
        if(results.Any(r=>r.StartsWith("FAIL")))throw new Exception("Scene 4 experience checks failed");
        Debug.Log("SCENE4_GEOMETRY_OK");
    }
    static void Spoon()
    {
        var needle=Object.FindObjectOfType<ReportCompassMeasure>().needle;
        var mesh=needle.GetComponent<MeshFilter>().sharedMesh;
        Require(mesh.bounds.size.x>.20f && mesh.bounds.size.z>.48f,"Compass still has a narrow ball-and-rod shape rather than a broad spoon bowl and long handle");
        var vertices=mesh.vertices;var center=vertices.Where(v=>Mathf.Abs(v.x)<.04f && v.z<-.05f && v.z>-.19f).ToArray();
        var rim=vertices.Where(v=>Mathf.Abs(v.x)>.10f && v.z<.02f).ToArray();
        Require(center.Length>0 && rim.Length>0 && center.Min(v=>v.y)<rim.Max(v=>v.y)-.025f,"Spoon bowl has no visible concavity");
    }
    static void Sky()
    {
        var dial=Object.FindObjectsOfType<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="ReportStarDial");
        Require(dial!=null,"Star dial does not rotate the sky or Big Dipper");
        var ring=(ReportMechanismHandle)dial.GetType().GetField("ring").GetValue(dial);
        var root=(Transform)dial.GetType().GetField("skyRoot").GetValue(dial);
        Call(ring,"Awake");Call(dial,"Awake");var before=root.rotation;
        var star=root.GetComponentsInChildren<Renderer>().First(r=>r.name.Contains("北斗"));var vertex=star.GetComponent<MeshFilter>().sharedMesh.vertices[0];var starBefore=star.transform.TransformPoint(vertex);
        ring.SetValue(0);Call(dial,"LateUpdate");
        Require(Quaternion.Angle(before,root.rotation)>40 && Vector3.Distance(starBefore,star.transform.TransformPoint(vertex))>.3f,"Dial motion does not move the visible Big Dipper");
        Require(RenderSettings.skybox.HasProperty("_SkyRotation") && RenderSettings.skybox.GetVector("_SkyRotation")!=new Vector4(0,0,0,1),"Procedural sky remains stationary while constellation moves");
        ring.ResetPose();Call(dial,"LateUpdate");Require(Quaternion.Angle(root.rotation,before)<.01f,"Reset failed to restore the sky");Call(dial,"OnDestroy");
    }
    static void Mirrors()
    {
        var measure=Object.FindObjectOfType<ReportMirrorMeasure>();
        var second=measure.GetType().GetField("secondMirror")?.GetValue(measure) as ReportMechanismHandle;
        Require(second!=null,"Optics task has only one reflection surface");
        Call(measure.mirror,"Awake");Call(second,"Awake");measure.mirror.SetValue(135);second.SetValue(135);Physics.SyncTransforms();Call(measure,"Update");
        Debug.Log("SCENE4_RAY first="+measure.mirror.transform.position+" normal="+measure.mirror.transform.forward+" second="+second.transform.position+" normal="+second.transform.forward+" receiver="+measure.receiver.bounds+" enabled="+measure.receiver.enabled+" beam="+string.Join(";",Enumerable.Range(0,measure.beam.positionCount).Select(i=>measure.beam.GetPosition(i).ToString())));
        Require(measure.Reading==1 && measure.beam.positionCount==4,"Two real reflections do not reach the receiver");
        second.SetValue(45);Physics.SyncTransforms();Call(measure,"Update");Require(measure.Reading==0,"Incorrect second mirror still passes");second.SetValue(135);
        Physics.SyncTransforms();Call(measure,"Update");
        var points=Enumerable.Range(0,measure.beam.positionCount).Select(i=>measure.beam.GetPosition(i)).ToArray();
        for(int i=0;i<3;i++)
        {
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.transform.position=Vector3.Lerp(points[i],points[i+1],.5f);block.transform.localScale=Vector3.one*.10f;
            Physics.SyncTransforms();Call(measure,"Update");Require(measure.Reading==0,"Blocked optical segment "+i+" still passes");
            Object.DestroyImmediate(block);Physics.SyncTransforms();Call(measure,"Update");Require(measure.Reading==1,"Removing optical obstruction does not restore the two-reflection path");
        }
        second.GetComponent<Collider>().enabled=false;Physics.SyncTransforms();Call(measure,"Update");Require(measure.Reading==0,"Bypassing the second surface counts as success");
    }
    public static void Preview()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[3].path);Directory.CreateDirectory("Documentation/SceneFourReview");
        var compass=Object.FindObjectOfType<ReportCompassMeasure>();Call(compass.needle,"Awake");
        var sight=Object.FindObjectOfType<ReportSightMeasure>();Call(sight.ring,"Awake");
        var dial=Object.FindObjectOfType<ReportStarDial>();Call(dial,"Awake");
        var mirror=Object.FindObjectOfType<ReportMirrorMeasure>();Call(mirror.mirror,"Awake");Call(mirror.secondMirror,"Awake");
        mirror.mirror.SetValue(135);mirror.secondMirror.SetValue(135);Physics.SyncTransforms();Call(mirror,"Update");
        foreach(var r in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())r.enabled=false;
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
        foreach(var text in Object.FindObjectsOfType<TextMesh>()){text.font=font;font.RequestCharactersInTexture(text.text,text.fontSize);text.GetComponent<Renderer>().sharedMaterial=material;}
        material.mainTexture=font.material.mainTexture;
        var p=compass.needle.transform.position;
        ReportLevelAudit.Photograph("Documentation/SceneFourReview/spoon.png",p+Vector3.up*.55f-compass.needle.transform.forward*.65f+compass.needle.transform.right*.25f,p,false,1);
        var root=mirror.transform;
        ReportLevelAudit.Photograph("Documentation/SceneFourReview/two-reflections.png",root.position-root.forward*1.8f-root.right*.8f+Vector3.up*2.2f,root.position+Vector3.up*1.35f,false,1);
        var star=dial.skyRoot.GetComponentInChildren<Renderer>();var view=sight.ring.transform.position-sight.transform.forward*1.6f+Vector3.up*.30f;var focus=star.bounds.center;
        ReportLevelAudit.Photograph("Documentation/SceneFourReview/sky-before.png",view,focus,false,1);
        sight.ring.SetValue(45);Call(dial,"LateUpdate");
        ReportLevelAudit.Photograph("Documentation/SceneFourReview/sky-after.png",view,focus,false,1);
        Call(dial,"OnDestroy");Object.DestroyImmediate(material);Object.DestroyImmediate(font);Debug.Log("SCENE4_PREVIEW_OK");
    }
    public static void FinalCheck()
    {
        Verify();OrientationExperienceCheck.VerifyObstruction();ReportReasonabilityCheck.VerifyAndPreview();Preview();
        Debug.Log("SCENE4_FINAL_CHECK_OK");
    }
}
