using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

// Publication views of the current scenes. All presentation changes are restored.
public static class ReportPhotography
{
    const string Output="Documentation/ReportPhotography/20261003_Details";
    static readonly string[] Figures={"2.1","2.2","2.3","2.4","2.5","2.6","2.7","2.8","2.9","2.10","2.11","2.12","2.13","2.15","附图"};
    static readonly string[] Titles={"基础力学与空间感知引导","浮力与压强实验","声学与共振实验","地磁感应与反射光学实验","电磁锁组装与电路连通","流体管网排故与修复","频率耦合实验","冰雪竞技场与热学环境","创意工坊与自由探索","物理实验室设计","园林水渠场景设计","天体运行与日晷设计","冰雪实验场场景与资产设计","三维场景与交互资产建模","古代园林补充场景"};
    static readonly string[] Features={"HubNavigationalSundialAndPortal","HandcraftedCargoPavilionBoat","FiveGraduatedBronzeBells","MagneticLodestoneSpoonAndCompass","ElectromagneticAssemblyWorkbench","HydraulicTankNetworkAndLeak","ZoneAAdjustableMechanicalPendulum","SnowmanTarget","WorkshopRampDominoLeverAssembly","IntegratedLabElectromagneticIsland","LowArchStoneBridge","TwelveHourSundialAndArmillary","SelfModeledFrostedGlove","AssetGallery_0","HexagonalLakesidePavilion"};
    [Serializable] public sealed class Shot
    {
        public string scene,figure,title,angle,file,subject;
        public Vector3 camera,focus;
        public int width=2560,height=1440;
    }
    [Serializable] sealed class Album{public List<Shot> shots=new List<Shot>();}
    static Album album;
    static bool Room(int i)=>new[]{4,5,6,8,9,13}.Contains(i);
    static Vector2 RoomSize(int i)=>new Vector2(i==4?16:i==5 || i==6?18:i==8?21:22,i==4?12:i==5?13:i==6?14:i==8 || i==13?16:17);
    static Vector3 InRoom(Vector3 position,int i)
    {
        if(!Room(i))return position;
        var size=RoomSize(i);
        return new Vector3(Mathf.Clamp(position.x,-size.x*.5f+.55f,size.x*.5f-.55f),Mathf.Clamp(position.y,1.65f,3.1f),Mathf.Clamp(position.z,-size.y*.5f+.55f,size.y*.5f-.55f));
    }
    static Bounds BoundsOf(Transform subject)
    {
        var renderers=subject.GetComponentsInChildren<MeshFilter>().Select(f=>f.GetComponent<Renderer>()).Where(r=>r!=null && r.enabled && r.gameObject.activeInHierarchy).ToArray();
        if(renderers.Length==0)throw new InvalidOperationException("No visible geometry: "+subject.name);
        var bounds=renderers[0].bounds;foreach(var r in renderers.Skip(1))bounds.Encapsulate(r.bounds);return bounds;
    }
    static void Frame(int index,string key,string angle,Transform subject,float yaw,float elevation=25)
    {
        var bounds=BoundsOf(subject);float radius=Mathf.Max(.55f,bounds.extents.magnitude),distance=radius/Mathf.Sin(26*Mathf.Deg2Rad)*1.13f;
        var direction=new Vector3(Mathf.Sin(yaw*Mathf.Deg2Rad)*Mathf.Cos(elevation*Mathf.Deg2Rad),Mathf.Sin(elevation*Mathf.Deg2Rad),Mathf.Cos(yaw*Mathf.Deg2Rad)*Mathf.Cos(elevation*Mathf.Deg2Rad));
        Shoot(index,key,angle,InRoom(bounds.center+direction*distance,index),bounds.center,52,subject.name);
    }
    static void Shoot(int index,string key,string angle,Vector3 position,Vector3 focus,float fov,string subject)
    {
        string relative="Photos/"+key+"/"+angle+".png";
        Render(Output+"/"+relative,position,focus,fov);
        album.shots.Add(new Shot{scene=key,figure=Figures[index],title=Titles[index],angle=angle,file=relative,subject=subject,camera=position,focus=focus});
    }
    static void Render(string path,Vector3 position,Vector3 focus,float fov)
    {
        var go=new GameObject("Temporary report photograph");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.stereoTargetEye=StereoTargetEyeMask.None;
        camera.transform.position=position;camera.transform.LookAt(focus);camera.fieldOfView=fov;camera.nearClipPlane=.05f;camera.farClipPlane=210;camera.clearFlags=CameraClearFlags.Skybox;camera.allowHDR=false;
        var target=new RenderTexture(2560,1440,24){antiAliasing=4};var pixels=new Texture2D(2560,1440,TextureFormat.RGB24,false);var previous=RenderTexture.active;
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,2560,1440),0,0);pixels.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,pixels.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);Object.DestroyImmediate(go);}
    }
    [MenuItem("Chrono Pivot/报告摄影/拍摄全部15场景多角度")]
    public static void CaptureAll()
    {
        album=new Album();var scenes=ReportLevelAudit.Scenes();if(scenes.Length!=15)throw new InvalidOperationException("Expected 15 scenes.");
        string original=EditorSceneManager.GetActiveScene().path;
        for(int index=0;index<scenes.Length;index++)
        {
            EditorSceneManager.OpenScene(scenes[index]);var level=Object.FindObjectOfType<DemoLevel>();string key=Path.GetFileNameWithoutExtension(scenes[index]);
            var texts=Object.FindObjectsOfType<TextMesh>(true);var fonts=texts.Select(t=>t.font).ToArray();var textMaterials=texts.Select(t=>t.GetComponent<Renderer>().sharedMaterials).ToArray();
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);var material=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
            Action<Font> atlas=f=>{if(f==font)material.mainTexture=font.material.mainTexture;};Font.textureRebuilt+=atlas;
            var hidden=level.stations.SelectMany(s=>s.transform.Find("Puzzle Controls").GetComponentsInChildren<Renderer>()).Concat(level.stations.Select(s=>s.label.GetComponent<Renderer>()))
                .Concat(Object.FindObjectsOfType<MeshRenderer>().Where(r=>r.name.Contains("_ControlLabel_") || r.name.Contains("_EntryNoticeBoard")))
                .Concat(new[]{level.board.GetComponent<Renderer>()}).Concat(Object.FindObjectOfType<XROrigin>().GetComponentsInChildren<Renderer>(true)).Distinct().ToArray();
            var visibility=hidden.Select(r=>r.enabled).ToArray();
            var pendulum=Object.FindObjectOfType<ReportPendulumMeasure>();Vector3 rodScale=pendulum!=null?pendulum.rod.localScale:Vector3.one;
            try
            {
                foreach(var text in texts){text.font=font;text.GetComponent<Renderer>().sharedMaterial=material;}level.Refresh();
                foreach(var group in texts.GroupBy(t=>new{t.fontSize,t.fontStyle}))font.RequestCharactersInTexture(string.Concat(group.Select(t=>t.text)),group.Key.fontSize,group.Key.fontStyle);material.mainTexture=font.material.mainTexture;
                foreach(var renderer in hidden)renderer.enabled=false;
                // Static preview uses the same initial rod length applied by the runtime's first physics tick.
                if(pendulum!=null)pendulum.rod.localScale=new Vector3(1,pendulum.length.startingValue,1);
                if(Room(index))
                {
                    var size=RoomSize(index);
                    if(index==6)Shoot(index,key,"01_Hero",new Vector3(0,3.1f,5.8f),new Vector3(0,1.35f,-1.6f),68,"机械摆与LC两区布局");
                    else Shoot(index,key,"01_Hero",new Vector3(size.x*.36f,2.95f,size.y*.35f),new Vector3(-size.x*.1f,1.15f,-size.y*.13f),65,"室内整体布局");
                    if(index==5)Shoot(index,key,"02_Reverse",new Vector3(-3.8f,2.8f,4.6f),new Vector3(-2.4f,1.5f,-3.1f),70,"水箱与中央管路");
                    else if(index==9)Shoot(index,key,"02_Reverse",new Vector3(0,2.8f,-6.7f),new Vector3(0,1.2f,4),70,"中央走道与实验岛");
                    else Shoot(index,key,"02_Reverse",new Vector3(-size.x*.36f,2.95f,-size.y*.35f),new Vector3(size.x*.1f,1.15f,size.y*.13f),65,"室内反向全景");
                }
                else
                {
                    bool large=index==7 || index==10 || index==14;
                    var authored=Object.FindObjectsOfType<Camera>().FirstOrDefault(c=>c.name==key || index==14 && c.name=="01_Garden_Overview");
                    if(authored!=null)Shoot(index,key,"01_Hero",authored.transform.position,authored.transform.position+authored.transform.forward*15,authored.fieldOfView,"场景主体全景");
                    else Shoot(index,key,"01_Hero",large?new Vector3(18,7,22):new Vector3(12,4.8f,17),new Vector3(0,2.5f,-1),62,"环境与主题双环");
                    Shoot(index,key,"02_Reverse",large?new Vector3(-18,11,19):new Vector3(-12,8,14),new Vector3(0,1.25f,-1),58,"另一侧环境布局");
                }
                var forward=Quaternion.Euler(0,level.spawnYaw,0)*Vector3.forward;
                Shoot(index,key,"03_Eye",level.spawn+Vector3.up*1.65f,level.spawn+Vector3.up*1.8f+forward*8,65,"玩家高度与进入路线");
                var feature=Object.FindObjectsOfType<Transform>().FirstOrDefault(t=>t.name==Features[index]);if(feature==null)throw new InvalidOperationException(key+": missing photographic subject "+Features[index]);
                Frame(index,key,"04_Model",feature,index==4?-45:35,27);
                var layout=level.transform.Find("Natural Learning Layout");var apparatus=layout.Cast<Transform>().Where(t=>t.GetComponentsInChildren<ReportLearningMeasure>().Length>0 || t.GetComponent<ReportMechanicalLauncher>()!=null).ToArray();
                var primary=pendulum!=null?pendulum.transform:apparatus.FirstOrDefault(t=>t.GetComponentsInChildren<MeshFilter>().Any(f=>f.name.Contains("台架") || f.name.Contains("NaturalLauncherBench")))??apparatus.First();
                var secondary=apparatus.LastOrDefault(t=>t!=primary && t.GetComponentsInChildren<ReportThrowMeasure>().Length==0)??primary;
                if(index==6)Shoot(index,key,"05_Experiment",new Vector3(-3.8f,2.5f,3.3f),BoundsOf(primary).center,52,primary.name);
                else Frame(index,key,"05_Experiment",primary,primary.eulerAngles.y+(index==7?245:155),index==7?35:25);
                Frame(index,key,"06_Experiment",index==4 || index==13?primary:secondary,(index==4 || index==13?primary:secondary).eulerAngles.y+(index==13?115:205),30);
                Debug.Log("REPORT_PHOTOGRAPHY_SCENE_OK "+key+" views=6");
            }
            finally
            {
                if(pendulum!=null)pendulum.rod.localScale=rodScale;
                for(int i=0;i<hidden.Length;i++)hidden[i].enabled=visibility[i];
                Font.textureRebuilt-=atlas;for(int i=0;i<texts.Length;i++){texts[i].font=fonts[i];texts[i].GetComponent<Renderer>().sharedMaterials=textMaterials[i];}
                Object.DestroyImmediate(font);Object.DestroyImmediate(material);
            }
        }
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"/manifest.json",JsonUtility.ToJson(album,true));
        EditorSceneManager.OpenScene(string.IsNullOrEmpty(original)?scenes[0]:original);
        Debug.Log("REPORT_PHOTOGRAPHY_OK scenes=15 photos="+album.shots.Count+" resolution=2560x1440");
    }
}
