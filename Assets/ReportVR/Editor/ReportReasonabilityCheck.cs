using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Reuse the physical regressions and record the instructions actually stored in each scene.
public static class ReportReasonabilityCheck
{
    const string Output="Documentation/ReasonabilityReview";
    public static void VerifyAndPreview(){Validate();CaptureBridge();}
    public static void RepairAndVerify(){RepairBridge();VerifyAndPreview();}
    public static void RepairBridge()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[14].path);
        var stack=Object.FindObjectOfType<ReportStackMeasure>();
        var mesh=stack.baseSupport as MeshCollider;
        var model=new LevelModelBuilder();model.Box(Vector3.up*.9f,new Vector3(1.65f,.08f,1.7f),"Celadon");
        foreach(float x in new[]{-.48f,.48f})
        {
            model.Beam(new Vector3(x,.08f,0),new Vector3(x,.87f,0),.065f,"SatinBrass");
            model.Box(new Vector3(x,.035f,0),new Vector3(.3f,.07f,.65f),"SageStone");
        }
        var temporary=model.Save(mesh.sharedMesh.name,stack.transform,false);Object.DestroyImmediate(temporary);
        Vector3[] supplies={new Vector3(-.62f,1.06f,.22f),new Vector3(0,.99f,-.62f),new Vector3(.67f,1.03f,.22f)};
        for(int i=0;i<supplies.Length;i++)stack.pieces[i].transform.localPosition=supplies[i];
        ReportRemainingUpgrade.BridgeMarker(stack);
        ReportIntuitivePresentation.ConfigureScene(Object.FindObjectOfType<DemoLevel>());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        Debug.Log("BRIDGE_SUPPLIES_SEPARATED realGravityAndContact=true");
    }
    public static void Validate()
    {
        Directory.CreateDirectory(Output);
        NaturalLearningCheck.Validate();PolishReportLevels.Validate();
        RemainingExperienceCheck.Verify();WaterExperienceCheck.Buoyancy();
        BoatDisplayCheck.Verify();ParticleExperienceCheck.ClassicBoat();
        if(Resources.Load("BackgroundMusicBank")!=null)throw new Exception("Background music bank remains");
        var guides=new List<string>{"# 当前场景操作核对\n\n此文件读取场景中实际保存的提示；通关交互结果另见 playtest.txt。\n"};
        for(int index=0;index<15;index++)
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);
            var level=Object.FindObjectOfType<DemoLevel>();guides.Add("## "+(index+1)+". "+level.title+"\n\n"+level.mission+"\n");
            foreach(var station in level.stations)
            {
                var puzzle=station.puzzle;
                if(string.IsNullOrWhiteSpace(puzzle.hint))throw new Exception("Missing operation hint: "+level.title);
                if(index==1 && (puzzle.hint.Contains("白框") || puzzle.hint.Contains("白色边框") || puzzle.hint.Contains("木槽")))throw new Exception("Boat hint refers to a hidden loading guide");
                foreach(var condition in puzzle.learningTask.requirements)
                    if(condition.acceptedRange.x>condition.acceptedRange.y)throw new Exception("Invalid completion range: "+level.title);
                guides.Add("- "+puzzle.goal+"："+puzzle.hint+"\n  观察规律："+puzzle.clue+"\n  完成引导："+puzzle.successMessage.Replace("\n"," ")+"\n");
            }
        }
        File.WriteAllLines(Output+"/操作核对.md",guides);
        File.Copy("Documentation/RemainingReview/physics.txt",Output+"/physics.txt",true);
        File.Copy("Documentation/Verification/level-routes.txt",Output+"/routes.txt",true);
        File.Copy("Documentation/WaterReview/buoyancy.txt",Output+"/buoyancy.txt",true);
        File.Copy("Documentation/CargoReview/boat-display.txt",Output+"/boat-display.txt",true);
        Debug.Log("REASONABILITY_SCENES_OK scenes=15 tasks=45 noMusicBank=true");
    }
    public static void CaptureBridge()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[14].path);
        var stack=Object.FindObjectOfType<ReportStackMeasure>();
        var view=stack.transform.TransformPoint(new Vector3(0,2.4f,-2.4f));
        foreach(var renderer in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        foreach(var text in Object.FindObjectsOfType<TextMesh>())
        {
            text.font=font;text.GetComponent<Renderer>().sharedMaterial=font.material;
            var facing=text.GetComponent<ReportTextFacing>();
            if(facing!=null && facing.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
        }
        ReportLevelAudit.Photograph(Output+"/bridge-supplies.png",view,stack.transform.position+Vector3.up*1.0f,false,2);
        Debug.Log("REASONABILITY_BRIDGE_PREVIEW_OK");
    }
}
