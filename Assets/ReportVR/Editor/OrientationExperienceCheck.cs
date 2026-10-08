using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class OrientationExperienceCheck
{
    static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(value,null);
    public static void VerifyObstruction()
    {
        foreach(int index in new[]{3,8,9,14})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);
            var mirror=Object.FindObjectOfType<ReportMirrorMeasure>();Call(mirror.mirror,"Awake");mirror.mirror.SetValue(135);Physics.SyncTransforms();Call(mirror,"Update");
            if(mirror.secondMirror!=null){Call(mirror.secondMirror,"Awake");mirror.secondMirror.SetValue(135);Physics.SyncTransforms();Call(mirror,"Update");}
            Require(mirror.Reading==1,"Unblocked reflected light must reach receiver in scene "+index);
            var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="Temporary incident light blocker";
            blocker.transform.position=Vector3.Lerp(mirror.emitter.position,mirror.mirror.transform.position,.5f);blocker.transform.localScale=Vector3.one*.12f;
            Physics.SyncTransforms();Call(mirror,"Update");Require(mirror.Reading==0,"Blocked incident light still reaches receiver in scene "+index);
            Object.DestroyImmediate(blocker);Physics.SyncTransforms();Call(mirror,"Update");Require(mirror.Reading==1,"Removing blocker must restore light");
            blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.name="Temporary reflected light blocker";
            blocker.transform.position=Vector3.Lerp(mirror.mirror.transform.position,mirror.secondMirror!=null?mirror.secondMirror.transform.position:mirror.receiver.bounds.center,.5f);blocker.transform.localScale=Vector3.one*.12f;
            Physics.SyncTransforms();Call(mirror,"Update");Require(mirror.Reading==0,"Blocked reflected light still reaches receiver in scene "+index);
            Object.DestroyImmediate(blocker);Physics.SyncTransforms();Call(mirror,"Update");Require(mirror.Reading==1,"Removing reflected blocker must restore light");
        }
        Debug.Log("ORIENTATION_OBSTRUCTION_OK");
    }
    public static void VerifyAndPreview()
    {
        VerifyObstruction();EditorSceneManager.OpenScene(EditorBuildSettings.scenes[3].path);
        var compass=Object.FindObjectOfType<ReportCompassMeasure>();Call(compass.needle,"Awake");compass.needle.SetValue(compass.southBearing);
        Require(Vector3.Angle(compass.needle.transform.forward,Vector3.back)<.01f,"Compass south is not calibrated to world south");
        var sight=Object.FindObjectOfType<ReportSightMeasure>();Call(sight.ring,"Awake");sight.ring.SetValue(45);
        Require(sight.Reading<.01f,"Sight must aim at actual target within its plane");
        var original=sight.target.transform.localPosition;sight.target.transform.localPosition+=Vector3.right*.2f;
        Require(sight.Reading>25,"Moving the star must invalidate the old 45-degree answer");sight.target.transform.localPosition=original;
        sight.ring.SetValue(0);Require(sight.Reading>40,"Incorrect sight must not pass");sight.ring.SetValue(45);Call(sight,"Update");
        var mirror=Object.FindObjectOfType<ReportMirrorMeasure>();Call(mirror.mirror,"Awake");mirror.mirror.SetValue(135);Physics.SyncTransforms();Call(mirror,"Update");Call(compass,"Update");
        if(mirror.secondMirror!=null){Call(mirror.secondMirror,"Awake");mirror.secondMirror.SetValue(135);Physics.SyncTransforms();Call(mirror,"Update");}
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
        foreach(var text in Object.FindObjectsOfType<TextMesh>()){text.font=font;text.GetComponent<Renderer>().sharedMaterial=material;}
        foreach(var renderer in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())renderer.enabled=false;
        Directory.CreateDirectory("Documentation/OrientationReview");
        var roots=new[]{compass.transform,sight.transform,mirror.transform};string[] names={"Compass","Sight","Mirror"};
        for(int i=0;i<roots.Length;i++)
        {
            var root=roots[i];var view=root.position-root.forward*2.4f+Vector3.up*1.75f;
            foreach(var text in Object.FindObjectsOfType<TextMesh>())
                if(text.GetComponent<ReportTextFacing>()?.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
            material.mainTexture=font.material.mainTexture;
            ReportLevelAudit.Photograph("Documentation/OrientationReview/"+names[i]+".png",view,root.position+Vector3.up*1.3f,false,1);
            if(i==2)
            {
                view=root.position-root.forward*1.35f-root.right*2f+Vector3.up*1.75f;
                foreach(var text in Object.FindObjectsOfType<TextMesh>())
                    if(text.GetComponent<ReportTextFacing>()?.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
                ReportLevelAudit.Photograph("Documentation/OrientationReview/MirrorRay.png",view,root.position+Vector3.up*1.3f,false,1);
            }
        }
        File.WriteAllText("Documentation/OrientationReview/geometry.txt","PASS: world south calibration; actual planar sight angle; relocated target rejects old angle; incident and reflected obstruction and recovery in scenes 3,8,9,14.\n");
        Debug.Log("ORIENTATION_GEOMETRY_PREVIEW_OK");
    }
    public static void UpgradeAndVerify(){ReportOrientationUpgrade.Scene();VerifyAndPreview();}
    public static void BuildVerified(){ReportOrientationUpgrade.Scene();ReportOrientationUpgrade.SharedInstructions();VerifyAndPreview();SetupReportVR.BuildWindows();Debug.Log("ORIENTATION_RELEASE_OK");}
}
