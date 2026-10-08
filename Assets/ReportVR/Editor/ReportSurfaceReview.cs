using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

public static class ReportSurfaceReview
{
    const string Output=ReportSurfaceCheck.Output;
    [Serializable] sealed class Album { public List<ReportPhotography.Shot> shots=new List<ReportPhotography.Shot>(); }
    [Serializable] sealed class Entry { public string mesh,name,uv,albedo,ao,surface,preview; }
    [Serializable] sealed class Review { public List<Entry> assets=new List<Entry>();public int checkedMaterials; }
    static string Number(float value)=>value.ToString("0.###",CultureInfo.InvariantCulture);
    static Entry Diagram(Mesh mesh,Material[] materials)
    {
        string name=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(mesh));var uv=mesh.uv;
        var svg=new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"1024\" viewBox=\"0 0 1024 1024\"><rect width=\"1024\" height=\"1024\" fill=\"#f5f6f1\"/>");
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var tint=materials[sub].HasProperty("_Color")?materials[sub].color:Color.white;var t=mesh.GetTriangles(sub);
            for(int i=0;i<t.Length;i+=3)
            {
                string points=string.Join(" ",new[]{t[i],t[i+1],t[i+2]}.Select(index=>Number(uv[index].x*1024)+","+Number((1-uv[index].y)*1024)));
                svg.Append("<polygon points=\"").Append(points).Append("\" fill=\"#").Append(ColorUtility.ToHtmlStringRGB(tint)).Append("\" stroke=\"#334a46\" stroke-width=\"0.65\"/>");
            }
        }
        svg.Append("</svg>");Directory.CreateDirectory(Output+"/UV");File.WriteAllText(Output+"/UV/"+name+".svg",svg.ToString());
        var mat=materials.First(m=>m.shader.name=="ChronoPivot/Low Poly Baked");
        return new Entry{mesh=AssetDatabase.GetAssetPath(mesh),name=name,uv="UV/"+name+".svg",
            albedo=AssetDatabase.GetAssetPath(mat.mainTexture),ao=AssetDatabase.GetAssetPath(mat.GetTexture("_OcclusionMap")),
            surface=AssetDatabase.GetAssetPath(mat.GetTexture("_MetallicGlossMap"))};
    }
    static void Render(string path,Vector3 position,Vector3 focus,float fov)
    {
        var go=new GameObject("Temporary baked surface review");var camera=go.AddComponent<Camera>();camera.enabled=false;
        camera.stereoTargetEye=StereoTargetEyeMask.None;camera.transform.position=position;camera.transform.LookAt(focus);camera.fieldOfView=fov;
        camera.nearClipPlane=.05f;camera.farClipPlane=300;camera.clearFlags=CameraClearFlags.Skybox;camera.allowHDR=false;
        var target=new RenderTexture(1920,1080,24){antiAliasing=4};var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);var old=RenderTexture.active;
        try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,pixels.EncodeToPNG());}
        finally{RenderTexture.active=old;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);Object.DestroyImmediate(go);}
    }
    [MenuItem("Chrono Pivot/表面工艺/验证材质并拍摄表面预览")]
    public static void Run()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        ReportSurfaceCheck.RequireReady();var review=new Review();
        var shader=Shader.Find("ChronoPivot/Low Poly Baked");if(shader==null || ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Baked surface shader failed to compile.");
        foreach(string guid in AssetDatabase.FindAssets("t:Material",new[]{"Assets/ReportVR/BakedSurfaces/Materials"}))
        {
            var mat=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if(mat.shader!=shader || mat.mainTexture==null || mat.GetTexture("_OcclusionMap")==null || mat.GetTexture("_MetallicGlossMap")==null)
                throw new InvalidOperationException("Missing baked texture: "+mat.name);
            string originalGuid=mat.GetTag("OriginalSurfaceMaterial",false,"");var original=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(originalGuid));
            if(original==null || mat.color!=original.color)throw new InvalidOperationException("Interactive palette tint changed: "+mat.name);
            review.checkedMaterials++;
        }
        var used=ReportSurfaceCheck.Gather();
        foreach(string stem in new[]{"015_IvoryMoonGate","087_TwelveHourSundialAndArmillary","046_ElectromagneticAssemblyWorkbench"})
        {
            var pair=used.First(p=>Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(p.Key))==stem);
            review.assets.Add(Diagram(pair.Key,pair.Value));
        }
        var album=JsonUtility.FromJson<Album>(File.ReadAllText("Documentation/ReportPhotography/20261003_Details/manifest.json"));
        var scenes=ReportSurfaceCheck.Scenes();
        foreach(int index in new[]{0,4,7,10,14})
        {
            EditorSceneManager.OpenScene(scenes[index]);string key=Path.GetFileNameWithoutExtension(scenes[index]);var shot=album.shots.First(s=>s.scene==key && s.angle=="01_Hero");
            var level=Object.FindObjectOfType<DemoLevel>();var origin=Object.FindObjectOfType<XROrigin>();
            var hidden=level.stations.SelectMany(s=>s.transform.Find("Puzzle Controls").GetComponentsInChildren<Renderer>())
                .Concat(level.stations.Select(s=>s.label.GetComponent<Renderer>())).Concat(origin.GetComponentsInChildren<Renderer>(true))
                .Concat(new[]{level.board.GetComponent<Renderer>()}).Distinct().ToArray();
            foreach(var renderer in hidden)renderer.enabled=false;
            var authored=Object.FindObjectsOfType<Camera>().FirstOrDefault(c=>c.name==key || index==14 && c.name=="01_Garden_Overview");
            Render(Output+"/Previews/"+key+".png",shot.camera,shot.focus,index==4?65:authored!=null?authored.fieldOfView:62);
            Debug.Log("SURFACE_PREVIEW_OK "+key);
        }
        VerifyReportVR.Validate();EditorSceneManager.OpenScene(scenes[0]);
        File.WriteAllText(Output+"/review.json",JsonUtility.ToJson(review,true));
        Debug.Log("SURFACE_REVIEW_OK materials="+review.checkedMaterials+" previews=5 UV-diagrams="+review.assets.Count);
    }
}
