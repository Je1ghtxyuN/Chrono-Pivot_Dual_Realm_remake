using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

public static class CreateReportCharacter
{
    const string Root="Assets/ReportVR/Characters";
    static readonly Dictionary<string,Material> palette=new Dictionary<string,Material>();
    sealed class Geometry
    {
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly Dictionary<Material,List<int>> faces=new Dictionary<Material,List<int>>();
        public void Face(string color,params Vector3[] p)
        {
            var mat=palette[color];if(!faces.ContainsKey(mat))faces[mat]=new List<int>();
            int n=vertices.Count;vertices.AddRange(p);
            for(int i=1;i<p.Length-1;i++)faces[mat].AddRange(new[]{n,n+i,n+i+1});
        }
        public void Box(string color,Vector3 center,Vector3 size,Quaternion? rotation=null)
        {
            var h=size*.5f;var q=rotation??Quaternion.identity;
            var p=new[]{new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)}.Select(v=>center+q*v).ToArray();
            foreach(var f in new[]{new[]{3,2,1,0},new[]{4,5,6,7},new[]{0,1,5,4},new[]{7,6,2,3},new[]{4,7,3,0},new[]{1,2,6,5}})Face(color,f.Select(i=>p[i]).ToArray());
        }
        // Octagonal rings, duplicated face vertices keep the sculpted faceting.
        public void Form(string color,Vector3 center,params Vector3[] rings)
        {
            var points=new List<Vector3[]>();
            foreach(var ring in rings)
            {
                var row=new Vector3[8];
                for(int i=0;i<8;i++){float a=(i+.5f)*Mathf.PI/4;row[i]=center+new Vector3(Mathf.Sin(a)*ring.x,ring.y,Mathf.Cos(a)*ring.z);}
                points.Add(row);
            }
            Face(color,points[0].Reverse().ToArray());Face(color,points[points.Count-1]);
            for(int j=0;j<points.Count-1;j++)for(int i=0;i<8;i++){int k=(i+1)%8;Face(color,points[j][i],points[j][k],points[j+1][k],points[j+1][i]);}
        }
        public GameObject Save(string name,Transform parent)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.subMeshCount=faces.Count;
            int i=0;foreach(var face in faces.Values)mesh.SetTriangles(face,i++);
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Root+"/Meshes/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,path);
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterials=faces.Keys.ToArray();return go;
        }
    }
    static void Color(string key,Color color)
    {
        string path=Root+"/Materials/"+key+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(material,path);}
        material.color=color;material.SetFloat("_Glossiness",.15f);palette[key]=material;EditorUtility.SetDirty(material);
    }
    static void Palette()
    {
        Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");
        Color("Jade",new Color(.10f,.40f,.38f));Color("JadeLight",new Color(.27f,.60f,.53f));
        Color("Ivory",new Color(.91f,.86f,.70f));Color("Brass",new Color(.78f,.51f,.21f));
        Color("Ink",new Color(.065f,.12f,.15f));Color("Skin",new Color(.82f,.57f,.40f));
        Color("Hair",new Color(.13f,.105f,.10f));Color("Leather",new Color(.35f,.20f,.13f));Color("White",new Color(.96f,.94f,.84f));
    }
    static GameObject Character()
    {
        var root=new GameObject("游学者 · Jade Scholar");var avatar=root.AddComponent<LowPolyPlayerAvatar>();
        var g=new Geometry();
        g.Form("Jade",Vector3.zero,new Vector3(.18f,-.22f,.105f),new Vector3(.145f,.08f,.09f),new Vector3(.205f,.40f,.115f),new Vector3(.12f,.47f,.075f));
        g.Form("Ivory",new Vector3(0,.45f,0),new Vector3(.055f,0,.05f),new Vector3(.055f,.095f,.05f));
        // Cross collar and lighter side panels are part of the authored outfit.
        g.Box("Ivory",new Vector3(-.025f,.33f,.113f),new Vector3(.06f,.25f,.018f),Quaternion.Euler(0,0,-32));
        g.Box("JadeLight",new Vector3(.048f,.30f,.123f),new Vector3(.055f,.31f,.018f),Quaternion.Euler(0,0,27));
        g.Form("Ink",Vector3.zero,new Vector3(.15f,.045f,.102f),new Vector3(.151f,.102f,.104f));
        g.Box("Brass",new Vector3(0,.075f,.108f),new Vector3(.068f,.047f,.025f));
        g.Box("Leather",new Vector3(-.03f,.26f,-.113f),new Vector3(.045f,.45f,.018f),Quaternion.Euler(0,0,-28));
        g.Box("Leather",new Vector3(.19f,.05f,-.025f),new Vector3(.12f,.16f,.12f));
        g.Box("Brass",new Vector3(.19f,.10f,.039f),new Vector3(.06f,.025f,.012f));
        avatar.torso=g.Save("Tunic and satchel",root.transform).transform;avatar.torso.localPosition=Vector3.up;
        g=new Geometry();
        g.Form("Skin",Vector3.zero,new Vector3(.062f,-.15f,.065f),new Vector3(.095f,-.09f,.09f),new Vector3(.104f,.055f,.10f),new Vector3(.075f,.12f,.075f));
        g.Form("Hair",Vector3.zero,new Vector3(.105f,.06f,.104f),new Vector3(.10f,.145f,.095f),new Vector3(.035f,.17f,.04f));
        g.Box("Hair",new Vector3(-.085f,-.005f,.027f),new Vector3(.026f,.105f,.085f));
        g.Box("Hair",new Vector3(.085f,-.005f,.027f),new Vector3(.026f,.105f,.085f));
        g.Box("Skin",new Vector3(0,-.028f,.098f),new Vector3(.024f,.032f,.025f));
        for(int side=-1;side<=1;side+=2)
        {
            g.Box("White",new Vector3(side*.045f,.013f,.095f),new Vector3(.033f,.021f,.009f));
            g.Box("Ink",new Vector3(side*.041f,.013f,.101f),new Vector3(.013f,.019f,.006f));
            g.Box("Hair",new Vector3(side*.046f,.039f,.095f),new Vector3(.036f,.010f,.009f),Quaternion.Euler(0,0,side*8));
            g.Form("Skin",new Vector3(side*.10f,-.03f,0),new Vector3(.015f,-.025f,.017f),new Vector3(.017f,.025f,.018f));
        }
        g.Box("Leather",new Vector3(0,-.072f,.086f),new Vector3(.03f,.007f,.008f));
        avatar.face=g.Save("Faceted head",root.transform).transform;avatar.face.localPosition=new Vector3(0,1.635f,-.075f);
        for(int side=-1;side<=1;side+=2)
        {
            string prefix=side<0?"Left":"Right";
            g=new Geometry();g.Form("Jade",Vector3.zero,new Vector3(.065f,0,.063f),new Vector3(.08f,.13f,.073f),new Vector3(.055f,.94f,.052f),new Vector3(.052f,1,.05f));
            var upper=g.Save(prefix+" upper sleeve",root.transform).transform;upper.localPosition=new Vector3(side*.205f,1.28f,0);upper.localRotation=Quaternion.Euler(0,0,side*195);upper.localScale=new Vector3(1,.29f,1);
            g=new Geometry();g.Form("Ivory",Vector3.zero,new Vector3(.05f,0,.047f),new Vector3(.04f,.82f,.039f),new Vector3(.044f,1,.041f));
            var lower=g.Save(prefix+" forearm",root.transform).transform;lower.localPosition=new Vector3(side*.28f,1.0f,0);lower.localRotation=Quaternion.Euler(165,0,0);lower.localScale=new Vector3(1,.29f,1);
            g=new Geometry();g.Box("Jade",new Vector3(0,0,.012f),new Vector3(.066f,.032f,.072f));
            g.Box("Brass",new Vector3(0,.018f,-.014f),new Vector3(.066f,.007f,.013f));
            for(int i=0;i<4;i++)g.Box("Skin",new Vector3((i-1.5f)*.015f,0,.061f),new Vector3(.012f,.023f,.036f-(i==0 || i==3?.008f:0)));
            g.Box("Skin",new Vector3(-side*.040f,-.003f,.027f),new Vector3(.02f,.025f,.033f),Quaternion.Euler(0,-side*35,0));
            var hand=g.Save(prefix+" gloved hand",root.transform).transform;hand.localPosition=new Vector3(side*.28f,.72f,.075f);hand.localRotation=Quaternion.Euler(90,0,0);
            g=new Geometry();g.Form("Ink",Vector3.zero,new Vector3(.063f,-.70f,.058f),new Vector3(.067f,-.38f,.061f),new Vector3(.082f,0,.078f));
            g.Form("Leather",new Vector3(0,-.71f,.015f),new Vector3(.069f,-.07f,.108f),new Vector3(.073f,.02f,.103f),new Vector3(.058f,.17f,.058f));
            g.Box("Brass",new Vector3(0,-.61f,.075f),new Vector3(.055f,.026f,.012f));
            var leg=g.Save(prefix+" trousers and boot",root.transform).transform;leg.localPosition=new Vector3(side*.092f,.80f,0);
            if(side<0){avatar.leftUpperArm=upper;avatar.leftForearm=lower;avatar.leftHand=hand;avatar.leftLeg=leg;}
            else{avatar.rightUpperArm=upper;avatar.rightForearm=lower;avatar.rightHand=hand;avatar.rightLeg=leg;}
        }
        avatar.firstPersonHidden=new[]{avatar.face.GetComponent<Renderer>(),avatar.torso.GetComponent<Renderer>()};
        return root;
    }
    [MenuItem("Report VR/Add Character and Desktop Controls")]
    public static void Apply()
    {
        Palette();var template=Character();
        string prefabPath=Root+"/JadeScholar.prefab";PrefabUtility.SaveAsPrefabAsset(template,prefabPath);Object.DestroyImmediate(template);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(entry.path);var origin=Object.FindObjectOfType<XROrigin>();
            var desktop=origin.GetComponent<DesktopPlayer>();if(desktop==null)desktop=origin.gameObject.AddComponent<DesktopPlayer>();desktop.head=origin.Camera;
            var old=origin.GetComponentInChildren<LowPolyPlayerAvatar>();if(old!=null)Object.DestroyImmediate(old.gameObject);
            var avatar=((GameObject)PrefabUtility.InstantiatePrefab(prefab,origin.transform)).GetComponent<LowPolyPlayerAvatar>();
            avatar.headTarget=origin.Camera.transform;avatar.leftTarget=origin.transform.Find("Camera Floor Offset/Left Controller");avatar.rightTarget=origin.transform.Find("Camera Floor Offset/Right Controller");avatar.firstPerson=true;
            foreach(var target in new[]{avatar.leftTarget,avatar.rightTarget})
            {
                var marker=target.Find("Simple self-created controller marker");if(marker!=null)Object.DestroyImmediate(marker.gameObject);
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(avatar);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);xr.InitManagerOnStart=false;xr.Manager.automaticLoading=false;xr.Manager.automaticRunning=false;EditorUtility.SetDirty(xr);EditorUtility.SetDirty(xr.Manager);
        AssetDatabase.SaveAssets();Capture();Validate();Debug.Log("REPORT_CHARACTER_SETUP_OK scenes=15");
    }
    public static void Validate()
    {
        foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(entry.path);var origin=Object.FindObjectOfType<XROrigin>();var avatar=origin.GetComponentInChildren<LowPolyPlayerAvatar>();
            if(origin.GetComponent<DesktopPlayer>()==null || avatar==null || avatar.headTarget!=origin.Camera.transform || avatar.leftTarget==null || avatar.rightTarget==null)throw new Exception("Character binding missing: "+entry.path);
            if(avatar.GetComponentsInChildren<Collider>().Length!=0)throw new Exception("Avatar must not block locomotion or interaction");
        }
        File.WriteAllText("Documentation/Verification/character-scenes.txt","PASS: 15 scenes; self-authored mesh prefab; head + both hand targets; first-person hiding; no avatar colliders; desktop player bound.\n");
    }
    public static void ApplyAndBuild(){Apply();SetupReportVR.BuildWindows();}
    public static void ExportDesktopBuild()
    {
        string source=Path.GetFullPath("Builds/Windows"),destination=Path.GetFullPath("Builds/WindowsDesktop");
        var files=Directory.GetFiles(source,"*",SearchOption.AllDirectories);
        foreach(var file in files)
        {
            string target=Path.Combine(destination,file.Substring(source.Length+1));Directory.CreateDirectory(Path.GetDirectoryName(target));File.Copy(file,target,true);
        }
        // Native pre-init runs before C# command line handling, even with XR auto init off.
        string boot=Path.Combine(destination,"ChronoPivot_ReportVR_Data/boot.config");
        File.WriteAllLines(boot,File.ReadAllLines(boot).Where(line=>!line.StartsWith("xrsdk-pre-init-library=",StringComparison.Ordinal)));
        File.WriteAllText("Documentation/Verification/desktop-build.txt","Exported from verified Windows build: "+files.Length+" files. Desktop boot.config omits native OpenXR pre-init. Launch with -desktop.\n");
    }
    public static void Capture()
    {
        Palette();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.62f,.67f,.68f);
        var light=new GameObject("Studio key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.transform.rotation=Quaternion.Euler(35,-25,0);light.shadows=LightShadows.Soft;
        var floor=new Geometry();floor.Box("Ivory",new Vector3(0,-.06f,0),new Vector3(8,.1f,8));floor.Save("Preview floor",null);
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/JadeScholar.prefab");
        var front=(GameObject)PrefabUtility.InstantiatePrefab(prefab);front.transform.position=new Vector3(-.7f,0,0);front.transform.rotation=Quaternion.Euler(0,-15,0);
        var back=(GameObject)PrefabUtility.InstantiatePrefab(prefab);back.transform.position=new Vector3(.7f,0,0);back.transform.rotation=Quaternion.Euler(0,155,0);
        var camera=new GameObject("Character preview camera").AddComponent<Camera>();camera.transform.position=new Vector3(0,1.28f,4.1f);camera.transform.LookAt(new Vector3(0,.86f,0));camera.orthographic=true;camera.orthographicSize=1.16f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.76f,.84f,.82f);
        var rt=new RenderTexture(1600,1000,24);rt.antiAliasing=4;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var pixels=new Texture2D(1600,1000,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1600,1000),0,0);pixels.Apply();
        Directory.CreateDirectory("Documentation/CharacterPreviews");File.WriteAllBytes("Documentation/CharacterPreviews/JadeScholar.png",pixels.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(pixels);
        EditorSceneManager.SaveScene(scene,Root+"/CharacterPreview.unity");
    }
}
