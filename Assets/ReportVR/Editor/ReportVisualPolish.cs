using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

// Shared surface detail preserves the authored hard edges, palette and baked UVs.
public static class ReportVisualPolish
{
    const string Folder="Assets/ReportVR/SurfaceDetails";
    const string Output="Documentation/VisualPolish/20261006";
    static string Family(Material material,string meshName)
    {
        string guid=material.GetTag("OriginalSurfaceMaterial",false,"");
        var original=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
        string name=(original==null?material.name:original.name).ToLowerInvariant();
        if(name.Contains("wood") || name.Contains("cedar") || name.Contains("timber"))return "Wood";
        if(name.Contains("roof") || name=="inkteal" && (meshName.Contains("Pavilion") || meshName.Contains("Boat") || meshName.Contains("Roof")))return "Tile";
        if(name.Contains("snow"))return "Snow";
        if(name.Contains("brass") || name=="copper" || name=="darkgraphite")return "Metal";
        if(name.Contains("plaster") || name=="ivory")return "Plaster";
        if(name.Contains("stone") || name.Contains("limestone") || name.StartsWith("rock") || name=="slateblue")return "Stone";
        if(name.StartsWith("meadow") || name=="shoresand")return "Ground";
        return null;
    }
    static Material[] Materials()=>AssetDatabase.FindAssets("t:Material",new[]{"Assets/ReportVR/BakedSurfaces/Materials"})
        .Select(g=>AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
    [MenuItem("Chrono Pivot/视觉优化/应用精细表面与光照")]
    public static void Apply()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        AssetDatabase.Refresh();var counts=new Dictionary<string,int>();
        foreach(string family in new[]{"Wood","Stone","Plaster","Tile","Metal","Snow","Ground"})
        foreach(string suffix in new[]{"Detail","Normal"})
        {
            string path=Folder+"/"+family+"_"+suffix+".png";
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            if(importer==null)throw new InvalidOperationException("Missing surface texture: "+path);
            importer.textureType=suffix=="Normal"?TextureImporterType.NormalMap:TextureImporterType.Default;
            importer.sRGBTexture=suffix!="Normal";importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.convertToNormalmap=false;importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;
            importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=8;importer.maxTextureSize=1024;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
        }
        foreach(var material in Materials())
        {
            string stem=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(material));
            string meshPath=AssetDatabase.GUIDToAssetPath(stem.Substring(0,32));
            string family=Family(material,Path.GetFileNameWithoutExtension(meshPath));
            if(family==null)continue;
            Color tint=material.color;
            material.SetTexture("_SurfaceDetail",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/"+family+"_Detail.png"));
            material.SetTexture("_SurfaceNormal",AssetDatabase.LoadAssetAtPath<Texture2D>(Folder+"/"+family+"_Normal.png"));
            material.SetFloat("_SurfaceScale",family=="Tile"?1.5f:family=="Ground"?.65f:1.25f);
            material.SetFloat("_SurfaceStrength",family=="Plaster"?.32f:family=="Ground"?.48f:.7f);
            material.SetFloat("_SurfaceNormalStrength",family=="Tile"?.35f:family=="Metal"?.12f:.2f);
            material.SetFloat("_SurfaceRoughness",family=="Metal"?.2f:.1f);
            material.SetFloat("_OcclusionStrength",.55f);material.SetFloat("_UseSurfaceDetail",1);material.EnableKeyword("_SURFACE_DETAIL");
            EditorUtility.SetDirty(material);
            if(material.color!=tint)throw new InvalidOperationException("Interactive color changed: "+material.name);
            counts[family]=counts.ContainsKey(family)?counts[family]+1:1;
        }
        AssetDatabase.SaveAssets();
        string originalScene=EditorSceneManager.GetActiveScene().path;
        foreach(string path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);string before=GameplayState();
            var sun=Object.FindObjectsOfType<Light>().FirstOrDefault(l=>l.type==LightType.Directional);
            if(sun!=null)
            {
                sun.shadowResolution=LightShadowResolution.High;sun.shadowStrength=.65f;
                sun.shadowBias=.015f;sun.shadowNormalBias=.15f;sun.shadows=LightShadows.Soft;
            }
            // Cooler bounce separates fresh ivory walls from the warm wooden structure.
            RenderSettings.ambientEquatorColor=new Color(.62f,.69f,.68f);
            RenderSettings.ambientGroundColor=new Color(.4f,.47f,.45f);
            RenderSettings.defaultReflectionResolution=256;
            if(before!=GameplayState())throw new InvalidOperationException("Gameplay changed: "+path);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        EditorSceneManager.OpenScene(string.IsNullOrEmpty(originalScene)?ReportLevelAudit.Scenes()[0]:originalScene);
        Directory.CreateDirectory(Output);File.WriteAllLines(Output+"/materials.txt",counts.Select(p=>p.Key+": "+p.Value));
        Validate();Debug.Log("VISUAL_POLISH_APPLIED materials="+counts.Values.Sum()+" scenes=15");
    }
    static string GameplayState()=>string.Join("\n",Object.FindObjectsOfType<Component>(true)
        .Where(c=>c is MonoBehaviour || c is Collider || c is Rigidbody || c is Transform)
        .OrderBy(c=>c.GetInstanceID()).Select(c=>c.GetInstanceID()+" "+EditorJsonUtility.ToJson(c)));
    public static void Validate()
    {
        var shader=Shader.Find("ChronoPivot/Low Poly Baked");
        if(shader==null || ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Surface shader failed to compile");
        int checkedCount=0;
        foreach(var material in Materials())
        {
            string stem=Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(material));
            string mesh=Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(stem.Substring(0,32)));
            if(Family(material,mesh)==null)continue;
            string guid=material.GetTag("OriginalSurfaceMaterial",false,"");
            var original=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if(original==null || material.color!=original.color || !material.IsKeywordEnabled("_SURFACE_DETAIL"))
                throw new InvalidOperationException("Palette/detail configuration invalid: "+material.name);
            foreach(string property in new[]{"_SurfaceDetail","_SurfaceNormal"})
            {
                var texture=material.GetTexture(property) as Texture2D;
                if(texture==null || texture.width!=1024 || texture.mipmapCount<2)
                    throw new InvalidOperationException("Missing high-resolution mipmapped surface: "+material.name);
            }
            checkedCount++;
        }
        if(checkedCount==0)throw new InvalidOperationException("No detailed materials configured");
        Debug.Log("VISUAL_POLISH_READY materials="+checkedCount+" palette=preserved shader=compiled");
    }
    [MenuItem("Chrono Pivot/视觉优化/拍摄优化前对比")]
    public static void CaptureBefore()=>Capture("Before");
    [MenuItem("Chrono Pivot/视觉优化/拍摄优化后对比")]
    public static void CaptureAfter(){Validate();Capture("After");}
    static void Capture(string phase)
    {
        string original=EditorSceneManager.GetActiveScene().path;
        var scenes=ReportLevelAudit.Scenes();
        foreach(int i in new[]{0,1,4,7,10})
        {
            EditorSceneManager.OpenScene(scenes[i]);string key=Path.GetFileNameWithoutExtension(scenes[i]);
            var origin=Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if(origin!=null)foreach(var r in origin.GetComponentsInChildren<Renderer>(true))r.enabled=false;
            var level=Object.FindObjectOfType<DemoLevel>();
            foreach(var station in level.stations)
                foreach(var r in station.transform.Find("Puzzle Controls").GetComponentsInChildren<Renderer>())r.enabled=false;
            foreach(var r in Object.FindObjectsOfType<MeshRenderer>().Where(r=>r.name.Contains("_ControlLabel_") || r.name.Contains("_EntryNoticeBoard")))r.enabled=false;
            foreach(var text in Object.FindObjectsOfType<TextMesh>(true))text.GetComponent<Renderer>().enabled=false;
            var authored=Object.FindObjectsOfType<Camera>().FirstOrDefault(c=>c.name==key);
            var position=authored!=null?authored.transform.position:i==4?new Vector3(5.76f,2.95f,4.2f):new Vector3(12,4.8f,17);
            var focus=authored!=null?position+authored.transform.forward*15:new Vector3(0,1.3f,-1);
            Render(Output+"/"+phase+"/"+key+"_Overview.png",position,focus,authored!=null?authored.fieldOfView:62);
            string feature=i==1 || i==10?"HandcraftedCargoPavilionBoat":i==0?"HubNavigationalSundialAndPortal":i==4?"ElectromagneticAssemblyWorkbench":"SnowmanTarget";
            var subject=Object.FindObjectsOfType<Transform>().First(t=>t.name==feature);
            var renderers=subject.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();
            var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);
            float radius=Mathf.Max(.5f,bounds.extents.magnitude);
            var detailPosition=bounds.center+new Vector3(.65f,.45f,.75f).normalized*radius*3.1f;
            if(i==4)detailPosition=new Vector3(Mathf.Clamp(detailPosition.x,-7.45f,7.45f),Mathf.Clamp(detailPosition.y,1.65f,3.1f),Mathf.Clamp(detailPosition.z,-5.45f,5.45f));
            Render(Output+"/"+phase+"/"+key+"_Detail.png",detailPosition,bounds.center,42);
            Debug.Log("VISUAL_POLISH_PHOTO "+phase+" "+key);
        }
        EditorSceneManager.OpenScene(string.IsNullOrEmpty(original)?scenes[0]:original);
        Debug.Log("VISUAL_POLISH_CAPTURE_OK "+phase);
    }
    static void Render(string path,Vector3 position,Vector3 focus,float fov)
    {
        var go=new GameObject("Temporary visual comparison");var camera=go.AddComponent<Camera>();camera.enabled=false;
        camera.stereoTargetEye=StereoTargetEyeMask.None;camera.transform.position=position;camera.transform.LookAt(focus);
        camera.fieldOfView=fov;camera.nearClipPlane=.04f;camera.farClipPlane=220;camera.clearFlags=CameraClearFlags.Skybox;
        var target=new RenderTexture(1920,1080,24){antiAliasing=4};var pixels=new Texture2D(1920,1080,TextureFormat.RGB24,false);
        var old=RenderTexture.active;
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1920,1080),0,0);pixels.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,pixels.EncodeToPNG());
        }
        finally{RenderTexture.active=old;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);Object.DestroyImmediate(go);}
    }
}
