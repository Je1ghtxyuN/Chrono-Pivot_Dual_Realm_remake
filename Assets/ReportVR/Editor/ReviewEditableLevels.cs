using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

// Split authored geometry into editable assemblies without changing the source assets.
public static class ReviewEditableLevels
{
    const string Folder="Assets/ReportVR/EditableModels";
    static string sceneKey;
    static readonly List<string> report=new List<string>();
    static GameObject Find(string name)=>Object.FindObjectsOfType<Transform>(true).FirstOrDefault(t=>t.name==name)?.gameObject;
    static Vector3 Center(Vector3[] p)=>(p[0]+p[1]+p[2])/3;
    static bool TableTop(Vector3[] p)=>p.Min(v=>v.y)>1.047f;
    static string TableParts(Vector3[] p,Material m)=>TableTop(p)?"台上器材":"工作台";
    static string BoatParts(Vector3[] p,Material m)=>m.name=="SageStone"?"石配重":m.name=="SatinBrass"?"铜配重":"船体与亭";
    static string BellParts(Vector3[] p,Material m)
    {
        var c=Center(p);
        if(m.name=="DarkGraphite")return "悬绳";
        if(c.y>3.25f || Math.Abs(c.x)>3.3f || m.name=="SageStone")return "钟架";
        return "编钟_"+(Mathf.Clamp(Mathf.RoundToInt((c.x+2.6f)/1.27f),0,4)+1);
    }
    static string PendulumParts(Vector3[] p,Material m)
    {
        if(Center(p).z>1)return TableTop(p)?"调频表盘":"调频工作台";
        if(m.name=="DarkGraphite" || m.name=="SatinBrass")return "摆杆与摆球";
        return "机械摆支架";
    }
    static string GateParts(Vector3[] p,Material m)
    {
        if(p.Max(v=>v.y)<2.2f && (m.name=="WarmWood" || m.name=="SatinBrass" && p.Min(v=>v.x)>4.57f))return "活动闸板";
        return "闸架与丝杆";
    }
    static Mesh Original(string prefabName)=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ReportScenesLowPoly/Prefabs/"+prefabName+".prefab").GetComponent<MeshFilter>().sharedMesh;
    static Dictionary<string,GameObject> Split(string name,Func<Vector3[],Material,string> classify,Mesh original=null)
    {
        var source=Find(name);if(source==null)return null;
        var filter=source.GetComponent<MeshFilter>();if(filter==null)return null; // Already split.
        if(PrefabUtility.IsPartOfPrefabInstance(source))PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(source),PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        var mesh=original??filter.sharedMesh;var renderer=source.GetComponent<MeshRenderer>();var materials=renderer.sharedMaterials;var oldCollider=source.GetComponent<MeshCollider>();
        Vector3[] vertices=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;
        var groups=new Dictionary<string,Dictionary<int,List<int>>>();int before=0;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
        {
            var triangles=mesh.GetTriangles(sub);before+=triangles.Length/3;
            for(int i=0;i<triangles.Length;i+=3)
            {
                string group=classify(new[]{vertices[triangles[i]],vertices[triangles[i+1]],vertices[triangles[i+2]]},materials[sub]);
                if(!groups.ContainsKey(group))groups[group]=new Dictionary<int,List<int>>();
                if(!groups[group].ContainsKey(sub))groups[group][sub]=new List<int>();
                groups[group][sub].AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});
            }
        }
        if(groups.Count<2)throw new Exception("No meaningful split: "+name);
        var parts=new Dictionary<string,GameObject>();int after=0;
        foreach(var group in groups)
        {
            var used=group.Value.Values.SelectMany(t=>t).Distinct().ToArray();
            var bounds=new Bounds(vertices[used[0]],Vector3.zero);foreach(int index in used)bounds.Encapsulate(vertices[index]);Vector3 pivot=bounds.center;
            var mapping=used.Select((index,n)=>new{index,n}).ToDictionary(pair=>pair.index,pair=>pair.n);
            var partMesh=new Mesh{name=sceneKey+"_"+name+"_"+group.Key};partMesh.indexFormat=used.Length>65535?UnityEngine.Rendering.IndexFormat.UInt32:UnityEngine.Rendering.IndexFormat.UInt16;
            var partVertices=used.Select(index=>vertices[index]-pivot).ToArray();partMesh.vertices=partVertices;
            if(normals.Length==vertices.Length)partMesh.normals=used.Select(index=>normals[index]).ToArray();
            if(uv.Length==vertices.Length)partMesh.uv=used.Select(index=>uv[index]).ToArray();
            var slots=group.Value.Keys.OrderBy(s=>s).ToArray();partMesh.subMeshCount=slots.Length;
            for(int sub=0;sub<slots.Length;sub++)
            {
                var triangles=group.Value[slots[sub]].Select(index=>mapping[index]).ToArray();partMesh.SetTriangles(triangles,sub);after+=triangles.Length/3;
                // Every vertex is checked against its original point before saving a centered mesh.
                foreach(int index in group.Value[slots[sub]])if(Vector3.Distance(partVertices[mapping[index]]+pivot,vertices[index])>.00001f)throw new Exception("Split changed geometry: "+name);
            }
            if(normals.Length!=vertices.Length)partMesh.RecalculateNormals();partMesh.RecalculateBounds();
            string path=Folder+"/"+partMesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(existing!=null){EditorUtility.CopySerialized(partMesh,existing);Object.DestroyImmediate(partMesh);partMesh=existing;EditorUtility.SetDirty(existing);}else AssetDatabase.CreateAsset(partMesh,path);
            var part=new GameObject(group.Key);part.transform.SetParent(source.transform,false);part.transform.localPosition=pivot;
            part.AddComponent<MeshFilter>().sharedMesh=partMesh;var r=part.AddComponent<MeshRenderer>();r.sharedMaterials=slots.Select(sub=>materials[sub]).ToArray();r.shadowCastingMode=renderer.shadowCastingMode;r.receiveShadows=renderer.receiveShadows;part.layer=source.layer;
            if(oldCollider!=null){var collider=part.AddComponent<MeshCollider>();collider.sharedMesh=partMesh;collider.convex=oldCollider.convex;collider.isTrigger=oldCollider.isTrigger;collider.sharedMaterial=oldCollider.sharedMaterial;collider.enabled=oldCollider.enabled;}
            parts[group.Key]=part;
        }
        if(after!=before)throw new Exception("Lost triangles: "+name);
        Object.DestroyImmediate(oldCollider);Object.DestroyImmediate(renderer);Object.DestroyImmediate(filter);
        report.Add("  "+name+" → "+string.Join(" / ",parts.Keys)+"; triangles preserved="+before);
        return parts;
    }
    static void RestOn(Dictionary<string,GameObject> parts,string specimen,string stand)
    {
        if(parts==null)return;
        var model=parts[specimen];var support=parts[stand];var a=model.GetComponent<Renderer>().bounds;var b=support.GetComponent<Renderer>().bounds;
        float difference=b.max.y-a.min.y;model.transform.position+=Vector3.up*difference;
        report.Add("    "+specimen+" support contact correction="+difference.ToString("F4")+"m");
    }
    static void Animated(DemoStation station,string sourceName,string prefabName,Func<Vector3[],Material,string> classify,string partName,Vector3 hinge)
    {
        var source=Find(sourceName);if(source==null || source.GetComponent<MeshFilter>()==null)return;
        var old=station.movingPart;
        var parts=Split(sourceName,classify,Original(prefabName));
        if(old!=null)Object.DestroyImmediate(old.gameObject);
        var pivot=new GameObject(partName+"_转轴").transform;pivot.SetParent(source.transform,false);pivot.localPosition=hinge;
        var part=parts[partName];part.transform.SetParent(pivot,true);
        foreach(var collider in part.GetComponentsInChildren<Collider>())collider.enabled=false;
        station.movingPart=pivot;
        report.Add("    restored complete animated part and rebound "+station.name+" to "+pivot.name);
    }
    [MenuItem("Report VR/Review Editable Levels in Report Order")]
    public static void Apply()
    {
        Directory.CreateDirectory(Folder);AssetDatabase.Refresh();report.Clear();
        Directory.CreateDirectory("Documentation/LevelReview/EditabilityBackups");
        int index=0;
        foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            string backup="Documentation/LevelReview/EditabilityBackups/"+Path.GetFileName(entry.path);if(!File.Exists(backup))File.Copy(entry.path,backup);
            EditorSceneManager.OpenScene(entry.path);sceneKey=Path.GetFileNameWithoutExtension(entry.path);report.Add("\n"+index+" | "+sceneKey);
            var level=Object.FindObjectOfType<DemoLevel>();var origin=Object.FindObjectOfType<XROrigin>();int signs=0;
            foreach(var text in Object.FindObjectsOfType<TextMesh>(true))
            {
                var facing=text.GetComponent<ReportTextFacing>();if(facing==null)facing=text.gameObject.AddComponent<ReportTextFacing>();
                facing.viewer=origin.Camera.transform;facing.mode=text.GetComponentInParent<XROrigin>()!=null?ReportTextFacingMode.FollowParent:ReportTextFacingMode.WorldFixed;signs++;
            }
            foreach(var station in level.stations)if(station.label!=null && station.label.transform.parent==station.transform)station.label.transform.localRotation=Quaternion.identity;
            report.Add("  explicit text facing; mounted signs fixed to world surfaces; text objects="+signs);
            switch(index)
            {
                case 0:
                    for(int i=0;i<3;i++)RestOn(Split("HubMaterialCube_"+i,(p,m)=>Center(p).y>.8f?"材料样品":"底座"),"材料样品","底座");
                    Split("HubNavigationalSundialAndPortal",(p,m)=>Center(p).z<-1.8f?"引导光环":m.name=="SageStone" || m.name=="Limestone"?"日晷底座":"刻度盘与晷针");
                    Split("HubThrowTargetPads",(p,m)=>"落点_"+(Center(p).z<-1?1:Center(p).z>1?3:2));break;
                case 1:
                case 10:
                    var weights=Split("CanalMaterialWeightBench",(p,m)=>TableTop(p)?"配重_"+(Mathf.Clamp(Mathf.RoundToInt((Center(p).x+6.6f)/.87f),0,2)+1):"工作台");
                    for(int i=1;i<=3;i++)RestOn(weights,"配重_"+i,"工作台");
                    Split("HandcraftedCargoPavilionBoat",BoatParts);
                    Split("CanalWaterLevelGauge",(p,m)=>m.name=="SageStone"?"底座":"水位标尺");
                    Animated(level.stations[2],"CanalSluiceAndCrank","CanalSluiceAndCrank",GateParts,"活动闸板",new Vector3(4.5f,1.17f,2.1f));break;
                case 2:
                    Split("FiveGraduatedBronzeBells",BellParts);
                    var mallet=Split("AcousticMalletAndBronzeIngot",(p,m)=>m.name=="SatinBrass"?"铜锭":TableTop(p)?"钟槌":"工作台");RestOn(mallet,"铜锭","工作台");break;
                case 3:
                    var sphere=Split("ObservatoryArmillary",(p,m)=>Center(p).y<=2.25f?"底座与支柱":"浑天仪环与轴");RestOn(sphere,"浑天仪环与轴","底座与支柱");
                    Split("MagneticLodestoneSpoonAndCompass",(p,m)=>m.name=="SageStone"?"底座":"方位盘");
                    var mirrors=new[]{new Vector3(-5.2f,0,-.9f),new Vector3(-1.4f,0,1.8f),new Vector3(2.7f,0,-2.8f),new Vector3(5.7f,0,.8f)};
                    Split("FourMirrorStaticOpticalPath",(p,m)=>"镜架_"+(Nearest(Center(p),mirrors)+1)+(m.name=="SageStone"?"_底座":"_镜体与支杆"));break;
                case 4:Split("ElectromagneticAssemblyWorkbench",TableParts);break;
                case 5:
                    Split("PipePartsSealsAndTorqueWrenchBench",TableParts);Split("HydraulicTankNetworkAndLeak",(p,m)=>p.Max(v=>v.y)<.241f?"装置底座":"水箱与管网");break;
                case 6:
                    Animated(level.stations[1],"ZoneAAdjustableMechanicalPendulum","ZoneAAdjustableMechanicalPendulum",PendulumParts,"摆杆与摆球",new Vector3(-4.4f,3.35f,-.8f));
                    Split("ZoneBLCFrequencyConsole",TableParts);break;
                case 7:Split("SnowballSupplyBasket",(p,m)=>m.name=="SnowIvory"?"演示雪球":"供给篮");break;
                case 8:
                    Split("WorkshopOpticalModuleBench",TableParts);
                    Split("WorkshopSpringWheelModuleBench",(p,m)=>m.name=="SatinBrass"?"弹簧":m.name=="WarmWood" && p.All(v=>Math.Abs(v.z+3.7f)<.061f)?"木轮":"工作台");
                    Split("WorkshopModularPhysicsPartsLibrary",(p,m)=>m.name=="WarmWood" || m.name=="Ivory"?"置物架":"模块_"+(Center(p).x<0?"左":"右")+"_"+Mathf.Clamp(Mathf.RoundToInt((Center(p).y-.8f)/.95f),0,2)+"_"+Mathf.Clamp(Mathf.RoundToInt((Center(p).x-(Center(p).x<0?-5.6f:5.6f)+1.8f)/.9f),0,4));break;
                case 9:
                    Split("IntegratedLabElectromagneticIsland",TableParts);Split("IntegratedLabOpticalIsland",TableParts);
                    Split("IntegratedLabHydraulicStation",(p,m)=>p.Max(v=>v.y)<.241f?"装置底座":"水箱与管网");
                    Split("IntegratedLabResonanceStation",(p,m)=>Center(p).z<-2?"机械摆":TableTop(p)?"LC 器材":"LC 工作台");break;
                case 11:
                    var instruments=Split("TwelveHourSundialAndArmillary",(p,m)=>(Center(p).x<0?"日晷":"浑天仪")+(Center(p).y<=1.05f?"_底座":"_仪器"));RestOn(instruments,"浑天仪_仪器","浑天仪_底座");
                    Split("StylizedCelestialOrbitDisplay",(p,m)=>m.name=="SatinBrass" && p.All(v=>Math.Abs(v.x+1.6f)<.061f && Math.Abs(v.z+3.5f)<.061f)?"支杆":"天体与轨道");break;
                case 12:
                    var snowBases=new[]{new Vector3(-5.8f,0,0),new Vector3(.1f,0,1.4f),new Vector3(5.9f,0,0),new Vector3(-3.3f,0,-6),new Vector3(3.6f,0,-6)};
                    Split("SnowAssetExhibitionPlinths",(p,m)=>"展台_"+(Nearest(Center(p),snowBases)+1));
                    Split("SnowballAssetComparisons",(p,m)=>"雪球_"+(Mathf.Clamp(Mathf.RoundToInt((Center(p).x-5.15f)/.65f),0,2)+1));break;
                case 13:
                    Split("AssetGallery_0",BoatParts);Split("AssetGallery_1",BellParts);
                    Split("AssetGallery_2",(p,m)=>Center(p).y>1.24f?"司南勺":m.name=="SageStone"?"底座":"方位盘");
                    Split("AssetGallery_3",TableParts);Split("AssetGallery_4",(p,m)=>p.Max(v=>v.y)<.241f?"装置底座":"水箱与管网");Split("AssetGallery_5",PendulumParts);
                    var galleryBases=Enumerable.Range(0,8).Select(i=>new Vector3(-7.2f+i%4*4.8f,0,3-i/4*6.2f)).ToArray();
                    Split(sceneKey+"_SpacedGalleryPlinths",(p,m)=>"展台_"+(Nearest(Center(p),galleryBases)+1));break;
                case 14:report.Add("  independent garden architecture/planting retained; no exhibit-base merge in this tour level");break;
            }
            level.Refresh();Physics.SyncTransforms();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());index++;
        }
        AssetDatabase.SaveAssets();File.WriteAllLines("Documentation/LevelReview/逐关编辑性检查.txt",report);
        EditableLevelCheck.Validate();PolishReportLevels.Validate();
        CaptureFirst();
        Debug.Log("EDITABLE_LEVEL_REVIEW_OK scenes=15");
    }
    public static void CaptureReview()
    {
        PolishReportLevels.CaptureAll();CaptureFirst();Debug.Log("EDITABLE_REVIEW_PHOTOS_OK");
    }
    static void CaptureFirst()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        var level=Object.FindObjectOfType<DemoLevel>();
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
        var texts=level.GetComponentsInChildren<TextMesh>(true);
        foreach(var text in texts){text.font=font;text.GetComponent<Renderer>().sharedMaterial=material;}
        level.Refresh();font.RequestCharactersInTexture(string.Concat(texts.Select(text=>text.text)),48);material.mainTexture=font.material.mainTexture;
        ReportLevelAudit.Photograph("Documentation/LevelReview/Editability/第一关材料分离.png",new Vector3(-3.2f,2.6f,6.3f),new Vector3(-2.65f,.85f,3.5f),false,10);
        Object.DestroyImmediate(material);Object.DestroyImmediate(font);
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
    }
    static int Nearest(Vector3 point,Vector3[] centers)
    {
        point.y=0;return Enumerable.Range(0,centers.Length).OrderBy(i=>(point-centers[i]).sqrMagnitude).First();
    }
}
