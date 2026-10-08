using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

// Migrate the existing scenes in place; do not rebuild their authored geometry/materials.
public static class ReportFloatingStatus
{
    const string Output="Documentation/LevelReview/20261006_Status";
    [MenuItem("Report VR/Level Review/Floating status and first level")]
    public static void Apply()
    {
        Directory.CreateDirectory(Output+"/Backups");
        int index=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            string backup=Output+"/Backups/"+Path.GetFileName(scene.path);
            if(!File.Exists(backup))File.Copy(scene.path,backup);
            EditorSceneManager.OpenScene(scene.path);var level=Object.FindObjectOfType<DemoLevel>();
            if(index++==0)ConfigureNaturalLearning.UpdateFirstSteps(level);
            ConfigureScene(level);level.Refresh();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();Validate();Capture();
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        Debug.Log("FLOATING_STATUS_APPLIED scenes="+index);
    }
    public static void ConfigureScene(DemoLevel level)
    {
        var viewer=Object.FindObjectOfType<XROrigin>().Camera.transform;
        foreach(var t in Object.FindObjectsOfType<Transform>(true).Where(t=>
            t.name.Contains("_PuzzleClue_") || t.name.Contains("_ControlLabel_") ||
            t.name.Contains("_EntryNoticeBoard") || t.name.Contains("_PuzzleButtonShelf_")).ToArray())
            Object.DestroyImmediate(t.gameObject);
        foreach(var station in level.stations)
        {
            var puzzle=station.puzzle;
            foreach(var button in puzzle.buttons.Where(b=>b!=null && b.kind==ReportPuzzleButtonKind.Hint))Object.DestroyImmediate(button.gameObject);
            puzzle.buttons=puzzle.buttons.Where(b=>b!=null).ToArray();
            var controls=station.transform.Find("Puzzle Controls");
            foreach(string name in new[]{"目标与线索","验证文字"})
            {
                var text=controls?.Find(name);if(text!=null)Object.DestroyImmediate(text.gameObject);
            }
            // Old verification plinths are decoration-only after physical goals replaced buttons.
            if(puzzle.mode==ReportPuzzleMode.Physical)
            {
                var renderer=station.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=false;
                foreach(var collider in station.GetComponents<Collider>())collider.enabled=false;
                station.indicator.enabled=false;
                foreach(var collider in station.indicator.GetComponents<Collider>())collider.enabled=false;
            }
            station.label.transform.position=station.transform.position+Vector3.up*2.1f;
            var pickup=station.GetComponent<ReportPickupMeasure>();
            if(pickup!=null)
            {
                var samples=pickup.samples;
                station.label.transform.position=samples.Aggregate(Vector3.zero,(p,s)=>p+s.transform.position)/samples.Length+Vector3.up*1.25f;
            }
            var throwTarget=Object.FindObjectsOfType<ReportThrowTarget>().FirstOrDefault(t=>t.requiredStation==station);
            if(throwTarget!=null)station.label.transform.position=throwTarget.transform.position+Vector3.up*1.85f;
            Floating(station.label,viewer,.027f);
        }
        if(level.board!=null){Floating(level.board,viewer,.03f);level.board.color=new Color(1,.9f,.55f);}
        ReportCargoUpgrade.PositionStatus(level);
        ReportIntuitivePresentation.ConfigureScene(level);
    }
    static void Floating(TextMesh text,Transform viewer,float size)
    {
        text.characterSize=size;text.anchor=TextAnchor.MiddleCenter;text.fontSize=48;
        var facing=text.GetComponent<ReportTextFacing>();if(facing==null)facing=text.gameObject.AddComponent<ReportTextFacing>();
        facing.mode=ReportTextFacingMode.FacePlayer;facing.viewer=viewer;facing.yawOnly=true;
        text.GetComponent<Renderer>().enabled=true;
    }
    public static void Validate()
    {
        int count=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);var level=Object.FindObjectOfType<DemoLevel>();
            if(Object.FindObjectsOfType<ReportPuzzleButton>(true).Any(b=>b.kind==ReportPuzzleButtonKind.Hint))throw new Exception("Hint remains: "+scene.path);
            if(Object.FindObjectsOfType<Transform>(true).Any(t=>t.name.Contains("_PuzzleClue_") || t.name.Contains("_ControlLabel_") || t.name.Contains("_EntryNoticeBoard")))throw new Exception("Task backing remains: "+scene.path);
            foreach(var station in level.stations)
            {
                var facing=station.label.GetComponent<ReportTextFacing>();
                if(facing==null || facing.mode!=ReportTextFacingMode.FacePlayer || !facing.yawOnly || facing.viewer==null)throw new Exception("Floating label invalid: "+station.name);
                if(station.puzzle.learningTask==null || station.puzzle.mode!=ReportPuzzleMode.Physical)throw new Exception("Physical task changed: "+station.name);
            }
            count++;
        }
        var shader=Shader.Find("ReportVR/WorldText");if(shader==null || ShaderUtil.ShaderHasError(shader))throw new Exception("Text shader compile failed");
        NaturalLearningCheck.First();
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"/verification.txt","PASS: "+count+" scenes; floating player-facing status labels; no task boards or hint buttons; physical tasks preserved.\n");
        Debug.Log("FLOATING_STATUS_VALIDATED scenes="+count);
    }
    public static void Capture()
    {
        Directory.CreateDirectory(Output);
        foreach(int index in new[]{0,1})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);var level=Object.FindObjectOfType<DemoLevel>();level.Refresh();
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
            var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
            var texts=Object.FindObjectsOfType<TextMesh>(true);
            foreach(var text in texts){text.font=font;font.RequestCharactersInTexture(text.text,text.fontSize);text.GetComponent<Renderer>().sharedMaterial=material;}
            material.mainTexture=font.material.mainTexture;
            var rig=Object.FindObjectOfType<XROrigin>();foreach(var r in rig.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            foreach(var station in level.stations)
            {
                Vector3 point=station.viewingPoint+Vector3.up*1.65f;
                if(index==0 && station.index==1)point=station.label.transform.position+Vector3.up*.3f+Vector3.forward*4.2f;
                foreach(var text in texts)
                {
                    var facing=text.GetComponent<ReportTextFacing>();
                    if(facing!=null && facing.mode==ReportTextFacingMode.FacePlayer)
                        text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-point,Vector3.up));
                }
                Render(Output+"/Level"+(index+1)+"_Step"+(station.index+1)+".png",point,station.label.transform.position-Vector3.up*.45f,material,font);
            }
            Object.DestroyImmediate(material);Object.DestroyImmediate(font);
        }
        File.WriteAllText(Output+"/index.html","<!doctype html><meta charset='utf-8'><title>关卡状态调整</title><style>body{background:#142d31;color:#f7ead0;font:18px sans-serif;max-width:1200px;margin:40px auto}img{width:100%;border-radius:12px}h2{margin-top:35px}</style><h1>浮空状态 · 第一关新流程</h1><p>第一步拿起三件材料；第二步分别抛进金色落点区；第三步太阳拖到东方。已删除任务白板和提示选项。</p>"+string.Join("",Enumerable.Range(0,2).SelectMany(i=>Enumerable.Range(0,3).Select(s=>"<h2>第"+(i+1)+"关 · 第"+(s+1)+"步</h2><img src='Level"+(i+1)+"_Step"+(s+1)+".png'>"))));
    }
    static void Render(string path,Vector3 position,Vector3 focus,Material material,Font font)
    {
        var go=new GameObject("Temporary status camera");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.stereoTargetEye=StereoTargetEyeMask.None;
        camera.transform.position=position;camera.transform.LookAt(focus);camera.fieldOfView=67;camera.nearClipPlane=.04f;camera.farClipPlane=220;camera.clearFlags=CameraClearFlags.Skybox;
        var target=new RenderTexture(1600,900,24){antiAliasing=4};var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try
        {
            camera.targetTexture=target;camera.Render();material.mainTexture=font.material.mainTexture;camera.Render();RenderTexture.active=target;
            pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);Object.DestroyImmediate(go);}
    }
}
