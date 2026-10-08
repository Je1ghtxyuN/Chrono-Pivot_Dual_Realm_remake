using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using Object=UnityEngine.Object;

public static class PolishReportLevels
{
    static Transform root;
    static string key;
    static DemoLevel level;
    static readonly List<string> changes=new List<string>();
    static Vector3 P(float x,float z,float y=.085f)=>new Vector3(x,y,z);
    static GameObject Find(string name)=>Object.FindObjectsOfType<Transform>(true).FirstOrDefault(t=>t.name==name)?.gameObject;
    static void Hide(string name){var go=Find(name);if(go!=null)go.SetActive(false);}
    static void NoCollision(string name){var go=Find(name);if(go!=null)foreach(var c in go.GetComponentsInChildren<Collider>())c.enabled=false;}
    public static void Apply()
    {
        ConfigureDemoInput();
        Directory.CreateDirectory("Documentation/LevelReview/SceneBackups");
        foreach(string path in ReportLevelAudit.Scenes())
        {
            string backup="Documentation/LevelReview/SceneBackups/"+Path.GetFileName(path);
            if(!File.Exists(backup))File.Copy(path,backup);
            else {File.Copy(backup,path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);}
            EditorSceneManager.OpenScene(path);key=Path.GetFileNameWithoutExtension(path);
            var previous=Find("Demo Level Layout");if(previous!=null)Object.DestroyImmediate(previous);
            root=new GameObject("Demo Level Layout").transform;level=root.gameObject.AddComponent<DemoLevel>();
            var rig=Object.FindObjectOfType<XROrigin>();level.player=rig.transform;
            foreach(var f in Object.FindObjectsOfType<MeshFilter>())
            {
                string n=f.name;
                if(n.Contains("Static") || n.Contains("Constellation") || n.Contains("WaveIllustration") || n.Contains("CelestialOrbit") || n.Contains("WaterHighlights") || n.Contains("Lotus") || n.Contains("BankPlanting") || n.Contains("Glove"))NoCollision(n);
                if(n.Contains("Pine") || n.Contains("Blossom") || n.Contains("Plum") || n.Contains("Maple") || n.Contains("Bamboo"))NoCollision(n);
            }
            Vector3[] stations=null;string[] instructions=null;DemoAction[] actions=null;Vector3[] route=null;
            int index=Array.IndexOf(ReportLevelAudit.Scenes(),path);
            switch(index)
            {
                case 0:
                    level.title="空间力学 · 材料与投掷";
                    stations=new[]{P(-3.2f,5),P(3.1f,3.4f),P(0,-4.2f)};
                    instructions=new[]{"观察三种材料","观察落点与轨迹","辨认日晷与出口"};actions=new[]{DemoAction.Inspect,DemoAction.Inspect,DemoAction.Inspect};
                    route=new[]{P(0,6.7f),stations[0],P(.5f,5),stations[1],P(3.7f,-3.5f),stations[2],P(4.4f,-4.5f)};break;
                case 1:
                    level.title="浮力水渠 · 配重与闸门";
                    stations=new[]{P(-5.7f,6.1f),P(-1.9f,4.9f),P(6.4f,2.2f)};
                    instructions=new[]{"观察木石铜配重","减载使船体浮起","开启水闸"};actions=new[]{DemoAction.Inspect,DemoAction.Buoyancy,DemoAction.Valve};
                    route=new[]{P(0,7.1f),stations[0],P(-3.5f,6),stations[1],P(6.4f,4.9f),stations[2],P(6.8f,-4.8f)};break;
                case 2:
                    level.title="声学庭院 · 调频与共振";
                    stations=new[]{P(-2,4.9f),P(0,1.8f,.205f),P(4.5f,4.2f,.205f)};
                    instructions=new[]{"认识编钟与钟槌","调频寻找共振","观察石灯与声波"};actions=new[]{DemoAction.Inspect,DemoAction.Resonance,DemoAction.Inspect};
                    route=new[]{P(0,6.8f),stations[0],P(0,4.9f),P(0,4.2f,.205f),stations[1],P(2.2f,1.8f,.205f),P(2.2f,4.2f,.205f),stations[2],P(6.8f,4.2f),P(6.8f,-4.8f)};
                    Ramp(P(0,4.95f,.03f),P(0,4.25f,.18f),2,"BellTerraceApproach");Ramp(P(5.6f,4.2f,.18f),P(6.7f,4.2f,.03f),1.5f,"BellTerraceDeparture");break;
                case 3:
                    level.title="观星台 · 地磁与反射";
                    stations=new[]{P(-4.5f,4.9f,1.235f),P(-3.9f,-2.6f,1.235f),P(4,1.3f,1.235f)};
                    instructions=new[]{"观察司南方位","观察浑天仪","调整铜镜接通光路"};actions=new[]{DemoAction.Compass,DemoAction.Inspect,DemoAction.Mirror};
                    route=new[]{P(0,8.7f),P(0,8,.03f),P(0,4.8f,1.235f),P(-2.8f,4.9f,1.235f),stations[0],P(-6.4f,4.9f,1.235f),P(-6.4f,-2.6f,1.235f),stations[1],P(0,-1,1.235f),P(2.7f,1.3f,1.235f),stations[2],P(6.2f,-5.1f,1.235f)};
                    ReplaceObservatory();break;
                case 4:
                    level.title="电磁实验室 · 线圈与门锁";
                    stations=new[]{P(-1.8f,2.7f),P(1.5f,1.2f),P(4.7f,-3.6f)};
                    instructions=new[]{"认识线圈与电源","调节电流吸合铁芯","检查门锁并前往出口"};actions=new[]{DemoAction.Inspect,DemoAction.Power,DemoAction.Inspect};
                    route=new[]{P(0,5.3f),stations[0],P(1.5f,2.7f),stations[1],P(4.7f,1.2f),stations[2],P(4.7f,-7.2f)};
                    Room("ElectromagneticLaboratoryShell",16,12,4.7f);Hide("GlazedDoorAndElectromagneticLatch");Door(P(4.7f,-5.98f,.06f));break;
                case 5:
                    level.title="液压工房 · 密封与流路";
                    stations=new[]{P(4.4f,2.7f),P(-2.4f,.2f),P(3.2f,-4.8f)};
                    instructions=new[]{"选择密封维修工具","修复泄漏恢复压力","开启流路阀门"};actions=new[]{DemoAction.Inspect,DemoAction.Repair,DemoAction.Valve};
                    route=new[]{P(0,5.8f),P(4.4f,4),stations[0],P(1.5f,2.7f),P(1.5f,.2f),stations[1],P(1.5f,.2f),P(1.5f,-4.8f),stations[2],P(6,-4.8f),P(6,-7.7f)};
                    Room("HydraulicEngineeringRoom",18,13,6);break;
                case 6:
                    level.title="频率耦合 · 双区协作演示";
                    stations=new[]{P(-4.4f,4.6f),P(-4.4f,1.2f),P(4.4f,1.8f)};
                    instructions=new[]{"认识机械摆与 LC 回路","调节摆动频率","调节 LC 频率使其匹配"};actions=new[]{DemoAction.Inspect,DemoAction.Resonance,DemoAction.Coupling};
                    route=new[]{P(-4.4f,6),stations[0],P(-2.2f,4.6f),P(-2.2f,1.2f),stations[1],P(-2.2f,1.2f),P(-2.2f,-3.6f),P(2.1f,-3.6f),P(2.1f,1.8f),stations[2],P(6.9f,1.8f),P(6.9f,-3.6f),P(6,-3.6f),P(6,-8.2f)};
                    Room("CouplingChamberArchitecture",18,14,6);break;
                case 7:
                    level.title="冰雪村落 · 抛射与热学";
                    stations=new[]{P(4.8f,9.3f),P(0,1.8f),P(7,8.2f)};
                    instructions=new[]{"取用演示雪球","发射雪球观察抛物线","观察温度与冰雪形态"};actions=new[]{DemoAction.Inspect,DemoAction.Snowball,DemoAction.Inspect};
                    route=new[]{P(2.8f,10.5f),stations[0],P(2.8f,8),P(2.8f,6.2f),P(2.8f,3.1f),P(2.8f,1.8f),stations[1],P(2.8f,1.8f),P(6.7f,1.8f),P(6.7f,3.1f),P(6.7f,6.4f),stations[2],P(11.8f,8.5f)};
                    SnowBridge(2.8f,4.4f);SnowBridge(6.7f,4.8f);break;
                case 8:
                    level.title="创意工坊 · 模块与连锁";
                    stations=new[]{P(-6.6f,-1.8f),P(0,5.1f),P(5.8f,-1.8f)};
                    instructions=new[]{"认识机械模块","释放多米诺演示链","认识光学模块"};actions=new[]{DemoAction.Inspect,DemoAction.Domino,DemoAction.Inspect};
                    route=new[]{P(0,7),P(-5.3f,5.1f),P(-5.3f,-1.8f),stations[0],P(-5.3f,-1.8f),P(-5.3f,5.1f),stations[1],P(5.1f,5.1f),P(5.1f,-1.8f),stations[2],P(8.9f,-1.8f),P(8.9f,-9.2f)};
                    Room("CreativeWorkshopOpenArchitecture",21,16,8.9f);
                    var plant=Find("WorkshopPlant");plant.transform.position=new Vector3(9,.54f,5.8f);var pot=Find("Fig_2_09_Creative_Workshop_Planter");pot.transform.position=new Vector3(9,0,5.8f);break;
                case 9:
                    level.title="综合实验室 · 三类实验";
                    stations=new[]{P(-5.2f,3.8f),P(4.4f,5),P(0,-3.7f)};
                    instructions=new[]{"调节线圈电流","对齐光学器材","观察液压与共振分区"};actions=new[]{DemoAction.Power,DemoAction.Mirror,DemoAction.Inspect};
                    route=new[]{P(0,7.6f),P(0,5),P(-5.2f,5),stations[0],P(-5.2f,5),P(0,5),stations[1],P(0,5),stations[2],P(0,-2.2f),P(8,-2.2f),P(8,-9.7f)};
                    Room("IntegratedPhysicsLabArchitecture",22,17,8);break;
                case 10:
                    level.title="园林水工 · 配重到巡园";
                    stations=new[]{P(-5.7f,14.6f,.395f),P(-1.9f,13.5f,.395f),P(6.7f,10.8f,.395f)};
                    instructions=new[]{"认识水工配重","减载使船体浮起","开启园林水闸"};actions=new[]{DemoAction.Inspect,DemoAction.Buoyancy,DemoAction.Valve};
                    route=new[]{P(0,16,.395f),stations[0],P(-3.5f,14.6f,.395f),stations[1],P(6.7f,13.5f,.395f),stations[2],P(8,7,.395f),P(8,1.5f,.395f),P(4.5f,.22f,.395f),P(3.4f,.22f,.515f),P(0,.22f,1.325f),P(-3.4f,.22f,.515f),P(-4.5f,.22f,.395f),P(-9.4f,.1f,.395f),P(-10.15f,-1.3f,.395f),P(-10.15f,-3.3f,.395f),P(-10.15f,-5,.395f)};
                    GardenConnections();break;
                case 11:
                    level.title="天体日晷 · 时角与观测";
                    stations=new[]{P(-2.2f,4.4f),P(4.5f,-.5f),P(-1.6f,-1.4f)};
                    instructions=new[]{"观察十二时辰刻度","观察浑天仪结构","调整太阳时角"};actions=new[]{DemoAction.Inspect,DemoAction.Inspect,DemoAction.Sundial};
                    route=new[]{P(0,6.2f),stations[0],P(1.2f,4.4f),P(4.5f,2.5f),stations[1],P(4.5f,-2.5f),P(.6f,-2.5f),P(.6f,-1.4f),stations[2],P(-4.6f,-3.7f),P(-3,-5)};break;
                case 12:
                    level.title="冰雪资产 · 巡展与结构";
                    stations=new[]{P(-5.8f,3.7f),P(0,4.9f),P(5.9f,3.7f)};
                    instructions=new[]{"旋转观察雪人","观察冰屋入口","旋转观察手套"};actions=new[]{DemoAction.RotateExhibit,DemoAction.Inspect,DemoAction.RotateExhibit};
                    route=new[]{P(0,6.6f),P(-3.2f,5),stations[0],P(-3.2f,5),stations[1],P(3.3f,5),stations[2],P(8.9f,3.7f),P(8.9f,-4.6f)};break;
                case 13:
                    level.title="模型展厅 · 器材巡展";
                    stations=new[]{P(-5,5.7f),P(5,5.7f),P(0,-.3f)};
                    instructions=new[]{"观察古代器材","观察现代器材","观察冰雪与交互器材"};actions=new[]{DemoAction.Inspect,DemoAction.Inspect,DemoAction.Inspect};
                    route=new[]{P(0,7),P(0,5.7f),stations[0],P(0,5.7f),stations[1],P(0,5.7f),stations[2],P(0,-5.4f),P(7.5f,-5.4f),P(7.5f,-9.2f)};
                    Room("NativeModelAssetGallery",22,16,7.5f);GalleryPlinths();break;
                default:
                    level.title="古代园林 · 过桥与巡游";
                    stations=new[]{P(4.5f,.22f,.395f),P(-4.5f,.22f,.395f),P(-9.5f,-.3f,.395f)};
                    instructions=new[]{"观察水岸与桥头","过桥观察六角亭","穿过月门巡园"};actions=new[]{DemoAction.Inspect,DemoAction.Inspect,DemoAction.Inspect};
                    route=new[]{P(9,5,.395f),P(8,1.5f,.395f),stations[0],P(3.4f,.22f,.515f),P(0,.22f,1.325f),P(-3.4f,.22f,.515f),stations[1],P(-8.5f,.1f,.395f),stations[2],P(-10.15f,-1.3f,.395f),P(-10.15f,-3.3f,.395f),P(-10.15f,-5,.395f)};
                    GardenConnections();break;
            }
            level.route=route;level.stations=new DemoStation[stations.Length];
            for(int i=0;i<stations.Length;i++)level.stations[i]=Station(stations[i],i,instructions[i],actions[i]);
            WireVisuals(index);Walkway(route,index);Boundaries(index);Entrance(route[0],route[1]-route[0],"入口",false);Entrance(route[route.Length-1],route[route.Length-1]-route[route.Length-2],"出口",true);
            level.spawn=route[0];level.spawnYaw=Quaternion.LookRotation(route[1]-route[0]).eulerAngles.y;rig.transform.SetPositionAndRotation(level.spawn,Quaternion.Euler(0,level.spawnYaw,0));
            // A generous view distance remains below the 4m interaction range.
            var interactor=rig.GetComponent<DemoInteractor>();if(interactor==null)interactor=rig.gameObject.AddComponent<DemoInteractor>();
            interactor.level=level;interactor.head=rig.Camera;interactor.hand=rig.transform.Find("Camera Floor Offset/Right Controller");
            interactor.interact=AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/ReportVR/Input/References/Interact.asset");interactor.resetDemo=AssetDatabase.LoadAssetAtPath<InputActionReference>("Assets/ReportVR/Input/References/ResetDemo.asset");
            interactor.worldText=WorldTextMaterial();
            var rayGo=new GameObject("Interaction pointer");rayGo.transform.SetParent(root,false);var ray=rayGo.AddComponent<LineRenderer>();ray.positionCount=2;ray.startWidth=.008f;ray.endWidth=.003f;ray.useWorldSpace=true;ray.sharedMaterial=PointerMaterial();interactor.ray=ray;
            Vector3 boardBase=route[0]+Quaternion.Euler(0,level.spawnYaw,0)*new Vector3(2.3f,0,.6f);
            var notice=new LevelModelBuilder();notice.Box(Vector3.up*1.48f,new Vector3(2.1f,1.15f,.06f),"Ivory");notice.Box(Vector3.up*2.12f,new Vector3(2.3f,.13f,.26f),"InkTeal");foreach(int side in new[]{-1,1})notice.Box(new Vector3(side*.85f,.63f,0),new Vector3(.07f,1.26f,.08f),"WarmWood");
            var noticeGo=notice.Save(key+"_EntryNoticeBoard",root,false);noticeGo.transform.position=boardBase;noticeGo.transform.rotation=Quaternion.LookRotation(boardBase-route[0]);
            var boardGo=Text("入口任务牌",boardBase+Vector3.up*1.5f-noticeGo.transform.forward*.045f,"",.025f);level.board=boardGo;
            boardGo.transform.rotation=noticeGo.transform.rotation;
            PlaceNoticeBoard(level,noticeGo,boardGo);
            level.Refresh();
            var wrist=rig.transform.Find("Camera Floor Offset/Left Controller/Scene and controller instructions");
            if(wrist!=null)wrist.GetComponent<TextMesh>().text="Left stick: walk | Right stick: turn\nHold grip: grab | Swing + release: throw\nRight trigger: use | Left B / menu: reset\nA / X: next / previous (after completion)";
            Physics.SyncTransforms();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            changes.Add(key+": continuous route; 3 ordered demo stations; entrance/exit; interaction/reset; corrected collisions.");
        }
        AssetDatabase.SaveAssets();File.WriteAllLines("Documentation/LevelReview/changes.txt",changes);
        ReportLevelAudit.DemoTests();Validate();CaptureAll();
        Debug.Log("REPORT_LEVELS_POLISHED_OK 15");
    }
    static TextMesh Text(string name,Vector3 position,string content,float size)
    {
        var go=new GameObject(name);go.transform.SetParent(root,false);go.transform.position=position;
        var t=go.AddComponent<TextMesh>();t.text=content;t.characterSize=size;t.fontSize=48;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.025f,.045f,.055f);return t;
    }
    static void ConfigureDemoInput()
    {
        const string path="Assets/ReportVR/Input/ReportVR.inputactions";var asset=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);var map=asset.FindActionMap("ReportVR");
        if(map.FindAction("Interact")==null)map.AddAction("Interact",InputActionType.Button,"<XRController>{RightHand}/triggerPressed");
        if(map.FindAction("ResetDemo")==null){var reset=map.AddAction("ResetDemo",InputActionType.Button,"<XRController>{LeftHand}/menu");reset.AddBinding("<XRController>{LeftHand}/secondaryButton");}
        File.WriteAllText(path,asset.ToJson());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);asset=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        foreach(string name in new[]{"Interact","ResetDemo"})
        {
            string reference="Assets/ReportVR/Input/References/"+name+".asset";
            var actionReference=AssetDatabase.LoadAssetAtPath<InputActionReference>(reference);
            if(actionReference==null)AssetDatabase.CreateAsset(InputActionReference.Create(asset.FindAction(name)),reference);
            else {actionReference.Set(asset.FindAction(name));EditorUtility.SetDirty(actionReference);}
        }
    }
    static void Ramp(Vector3 a,Vector3 b,float width,string name)
    {
        var m=new LevelModelBuilder();m.Ramp(a,b,width,"Limestone");m.Save(key+"_"+name,root);
    }
    static void Room(string oldName,float width,float depth,float exitX)
    {
        Hide(oldName);var m=new LevelModelBuilder();m.Box(P(0,0,-.11f),new Vector3(width,.34f,depth),"Limestone");
        // The back wall is split around a real 2.4m exit opening.
        float left=-width/2,right=width/2,start=exitX-1.2f,end=exitX+1.2f;
        m.Box(P((left+start)/2,-depth/2,1.96f),new Vector3(start-left,3.8f,.2f),"Ivory");
        m.Box(P((end+right)/2,-depth/2,1.96f),new Vector3(right-end,3.8f,.2f),"Ivory");
        m.Box(P(exitX,-depth/2,3.4f),new Vector3(2.4f,.92f,.2f),"Ivory");
        m.Box(P(left,0,1.96f),new Vector3(.2f,3.8f,depth),"Ivory");
        m.Box(P(right,0,1.96f),new Vector3(.2f,3.8f,depth),"Ivory");
        foreach(int s in new[]{-1,1})m.Box(P(s*(width/4+.65f),depth/2,1.96f),new Vector3(width/2-1.3f,3.8f,.2f),"Ivory");
        for(int i=0;i<8;i++)m.Box(P(left+.15f,-depth/2+1+i*depth/9,2.3f),new Vector3(.04f,1.3f,.8f),"GlacierBlue");
        m.Box(P((left+start)/2,-depth/2+.12f,.27f),new Vector3(start-left,.27f,.06f),"Celadon");
        m.Box(P(exitX,-depth/2-1,.0f),new Vector3(3.2f,.12f,2.4f),"Limestone");
        m.Save(key+"_ConnectedLaboratoryShell",root);
        var ceiling=new LevelModelBuilder();ceiling.Box(P(0,0,3.9f),new Vector3(width+.2f,.12f,depth+.2f),"Ivory");
        for(int beam=0;beam<4;beam++)ceiling.Box(P(-width*.38f+beam*width*.25f,0,3.79f),new Vector3(.07f,.12f,depth),"Celadon");
        for(int panel=0;panel<3;panel++)ceiling.Box(P(0,-depth*.28f+panel*depth*.28f,3.78f),new Vector3(width*.72f,.03f,.15f),"Ivory");
        var roof=ceiling.Save(key+"_LaboratoryCeiling",root);roof.layer=29;
    }
    static void Door(Vector3 p)
    {
        var m=new LevelModelBuilder();foreach(int s in new[]{-1,1})m.Box(p+new Vector3(s*1.2f,1.4f,0),new Vector3(.10f,2.8f,.15f),"InkTeal");m.Box(p+Vector3.up*2.82f,new Vector3(2.5f,.12f,.15f),"InkTeal");m.Save(key+"_ExitDoorFrame",root);
        var leaf=new LevelModelBuilder();leaf.Box(Vector3.up*1.35f,new Vector3(2.2f,2.7f,.08f),"SoftGlass");leaf.Box(new Vector3(.83f,1.1f,.08f),new Vector3(.05f,.28f,.06f),"SatinBrass");
        var go=leaf.Save(key+"_SlidingDoor",root);go.transform.position=p;level.exitDoor=go.transform;level.exitDoorTravel=Vector3.right*2.5f;
    }
    static void ReplaceObservatory()
    {
        Hide("ObservatoryStoneTerrace");var m=new LevelModelBuilder();m.Box(P(0,-1,.57f),new Vector3(15,1.14f,12),"SageStone");m.Box(P(0,-1,1.15f),new Vector3(15.2f,.12f,12.2f),"Limestone");
        foreach(int side in new[]{-1,1}){m.Beam(P(side*7.3f,-6.8f,2.12f),P(side*7.3f,4.8f,2.12f),.10f,"SatinBrass");for(int i=0;i<8;i++)m.Box(P(side*7.3f,-6.8f+i*1.6f,1.65f),new Vector3(.14f,.9f,.14f),"SageStone");}
        m.Beam(P(-7.3f,-6.8f,2.12f),P(7.3f,-6.8f,2.12f),.10f,"SatinBrass");m.Save(key+"_AccessibleObservatoryTerrace",root);
        Ramp(P(0,8,.03f),P(0,5.04f,1.21f),2.6f,"ObservatoryArrivalRamp");
        // Light beams are illustrations, while the four physical mirror supports retain collision.
        NoCollision("FourMirrorStaticOpticalPath");
        var group=Find("FourMirrorStaticOpticalPath");foreach(var p in new[]{P(-5.2f,-.9f),P(-1.4f,1.8f),P(2.7f,-2.8f),P(5.7f,.8f)})
        {var c=group.AddComponent<BoxCollider>();c.center=P(p.x,p.z,1.85f);c.size=new Vector3(.6f,1.3f,.6f);}
    }
    static void GardenConnections()
    {
        NoCollision("CurvingJadePond");NoCollision("QuietWaterHighlights");NoCollision("WindingGardenFlagstones");
        // Bridge landings overlap the deck by 0.1m instead of leaving a step or a gap.
        foreach(int side in new[]{-1,1})Ramp(P(side*4.6f,.22f,.37f),P(side*3.3f,.22f,.537f),1.55f,"BridgeLanding_"+side);
        var gate=Find("IvoryMoonGate");if(gate!=null)
        {
            Vector3 center=gate.transform.position;Vector3 dir=gate.transform.forward;
            Ramp(center+dir*1.0f,center+dir*.08f+Vector3.up*.22f,1.2f,"MoonGateApproach");
            Ramp(center-dir*1.0f,center-dir*.08f+Vector3.up*.22f,1.2f,"MoonGateDeparture");
        }
        var landings=new LevelModelBuilder();landings.Box(P(-4.6f,.22f,.315f),new Vector3(1.6f,.11f,1.7f),"Limestone");landings.Box(P(4.6f,.22f,.315f),new Vector3(1.6f,.11f,1.7f),"Limestone");landings.Save(key+"_BridgeBankLandings",root);
        // Keep decorative shrubs outside the continuous visitor corridor.
        foreach(string name in new[]{"GardenLowShrub_0","GardenLowShrub_5","GardenStoneLantern_Right"})NoCollision(name);
    }
    static void SnowBridge(float x,float z)
    {
        var m=new LevelModelBuilder();m.Box(P(x,z,.12f),new Vector3(1.8f,.16f,3.8f),"WarmWood");
        foreach(int side in new[]{-1,1})
        {m.Beam(P(x+side*.98f,z-1.9f,1),P(x+side*.98f,z+1.9f,1),.07f,"WarmWood");for(int i=0;i<4;i++)m.Box(P(x+side*.98f,z-1.9f+i*1.27f,.55f),new Vector3(.09f,.95f,.09f),"WarmWood");}
        m.Save(key+"_VillageBridge_"+x,root);Ramp(P(x,z-2.45f,.03f),P(x,z-1.7f,.2f),1.7f,"SnowBridgeSouth_"+x);Ramp(P(x,z+2.45f,.03f),P(x,z+1.7f,.2f),1.7f,"SnowBridgeNorth_"+x);
    }
    static void GalleryPlinths()
    {
        Hide("AssetGalleryPresentationPlinths");var m=new LevelModelBuilder();
        for(int i=0;i<8;i++){var p=P(-7.2f+i%4*4.8f,3-i/4*6.2f);m.Box(p+Vector3.up*.27f,new Vector3(3.3f,.7f,3.3f),"Limestone");m.Box(p+Vector3.up*.63f,new Vector3(3.35f,.04f,3.35f),"Celadon");}m.Save(key+"_SpacedGalleryPlinths",root);
    }
    static DemoStation Station(Vector3 point,int index,string instruction,DemoAction action)
    {
        // Place controls to the side of a viewing point, leaving its 1.3m corridor clear.
        Physics.SyncTransforms();Vector3 p=point+Vector3.right*1.3f;float best=-1;
        for(int angle=0;angle<16;angle++)
        {
            Vector3 candidate=point+Quaternion.Euler(0,angle*22.5f,0)*Vector3.right*1.3f;float clearance=float.PositiveInfinity;
            for(int i=0;i<level.route.Length-1;i++)
            {
                Vector3 a=level.route[i],end=level.route[i+1];a.y=end.y=candidate.y;float t=Mathf.Clamp01(Vector3.Dot(candidate-a,end-a)/(end-a).sqrMagnitude);clearance=Mathf.Min(clearance,Vector3.Distance(candidate,Vector3.Lerp(a,end,t)));
            }
            if(Physics.CheckCapsule(candidate+Vector3.up*.3f,candidate+Vector3.up*1.8f,.26f,~0,QueryTriggerInteraction.Ignore))continue;
            if(clearance>best){best=clearance;p=candidate;}
        }
        var m=new LevelModelBuilder();m.Box(Vector3.up*.07f,new Vector3(.42f,.14f,.42f),"SageStone");m.Box(Vector3.up*.54f,new Vector3(.12f,.95f,.12f),"SatinBrass");m.Box(Vector3.up*1.03f,new Vector3(.45f,.10f,.38f),"InkTeal");
        var go=m.Save(key+"_Control_"+index,root);go.transform.position=p;go.transform.rotation=Quaternion.LookRotation(p-point);var s=go.AddComponent<DemoStation>();s.level=level;s.index=index;s.instruction=instruction;s.action=action;s.viewingPoint=point;
        var button=new LevelModelBuilder();button.Box(Vector3.zero,new Vector3(.23f,.06f,.20f),"Celadon");var b=button.Save(key+"_Button_"+index,go.transform);b.transform.localPosition=Vector3.up*1.11f;s.indicator=b.GetComponent<Renderer>();
        s.label=Text("第 "+(index+1)+" 步",p+Vector3.up*1.48f,"",.019f);s.label.transform.SetParent(go.transform,true);
        var plate=new LevelModelBuilder();plate.Box(Vector3.up*1.48f,new Vector3(1.65f,.48f,.06f),"Ivory");plate.Save(key+"_ControlLabel_"+index,go.transform,false);
        s.label.transform.localPosition=new Vector3(0,1.48f,-.045f);s.label.transform.localRotation=Quaternion.identity;
        return s;
    }
    static void Entrance(Vector3 p,Vector3 direction,string title,bool exit)
    {
        var m=new LevelModelBuilder();foreach(int s in new[]{-1,1}){m.Box(new Vector3(s*1.3f,.10f,0),new Vector3(.35f,.2f,.35f),"SageStone");m.Box(new Vector3(s*1.3f,1.35f,0),new Vector3(.14f,2.5f,.14f),"InkTeal");}
        m.Box(Vector3.up*2.6f,new Vector3(2.9f,.18f,.25f),"InkTeal");m.Box(Vector3.up*2.55f-Vector3.forward*.15f,new Vector3(1.2f,.38f,.07f),"Ivory");var portal=m.Save(key+"_"+(exit?"Exit":"Entry")+"Portal",root);portal.transform.position=p;direction.y=0;portal.transform.rotation=Quaternion.LookRotation(direction);
        var label=Text(title,p+Vector3.up*2.55f-portal.transform.forward*.20f,title,.028f);label.transform.rotation=portal.transform.rotation;
        if(exit)
        {
            var station=new GameObject("Next scene after completion");station.transform.SetParent(root,false);station.transform.position=p;
            var trigger=station.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.size=new Vector3(2,2.5f,1);trigger.center=Vector3.up*1.25f;
            var e=station.AddComponent<DemoExit>();e.level=level;
        }
    }
    static void Walkway(Vector3[] route,int index)
    {
        var m=new LevelModelBuilder();bool indoors=new[]{4,5,6,8,9,13}.Contains(index);
        for(int i=0;i<route.Length-1;i++)
        {
            var a=route[i];var b=route[i+1];float distance=Vector3.Distance(a,b);var direction=(b-a).normalized;var side=Vector3.Cross(direction,Vector3.up).normalized;
            // A continuous inlay path on room floors; stone paving outdoors. Never bridge over props.
            int count=Mathf.Max(1,Mathf.CeilToInt(distance/.55f));
            for(int j=0;j<count;j++)
            {
                var c=Vector3.Lerp(a,b,(j+.5f)/count);
                if(index==3 && c.z>4.9f || (index==10 || index==14) && Mathf.Abs(c.x)<3.5f && Mathf.Abs(c.z-.22f)<.8f)continue;
                float y=c.y-.025f;if(Physics.Raycast(c+Vector3.up*.3f,Vector3.down,out var hit,.7f) && hit.point.y<c.y+.12f)y=hit.point.y;
                m.Box(new Vector3(c.x,y+.005f,c.z),new Vector3(indoors?1.3f:1.45f,.009f,distance/count+.03f),indoors?"Ivory":"Limestone",Quaternion.LookRotation(new Vector3(direction.x,0,direction.z)));
                if(j%3==0)m.Box(new Vector3(c.x,y+.012f,c.z)+side*.47f,new Vector3(.05f,.012f,.25f),"Celadon",Quaternion.LookRotation(new Vector3(direction.x,0,direction.z)));
            }
        }
        m.Save(key+"_ContinuousVisitorPath",root,false);
        var nodes=new LevelModelBuilder();foreach(var s in level.stations)
        {
            Vector3 p=s.viewingPoint;
            nodes.Box(p-Vector3.up*.017f,new Vector3(1.2f,.012f,1.2f),"Celadon");
        }nodes.Save(key+"_ObservationPads",root,false);
    }
    static void Boundaries(int index)
    {
        if(new[]{4,5,6,8,9,13}.Contains(index))return;
        // Physical low borders frame the outdoor activity area without hiding its backdrop.
        float x=(index==10 || index==14)?14:index==7?16:12,zMin=(index==10 || index==14)?-13:-10,zMax=index==10?18:index==7?13:11;
        var m=new LevelModelBuilder();foreach(int side in new[]{-1,1})
        {
            m.Box(P(side*x,(zMin+zMax)/2,.38f),new Vector3(.3f,.65f,zMax-zMin),"SageStone");
            for(float z=zMin;z<=zMax;z+=3)m.Box(P(side*x,z,.78f),new Vector3(.45f,.23f,.45f),"Limestone");
        }
        m.Box(P(0,zMin,.38f),new Vector3(x*2,.65f,.3f),"SageStone");m.Save(key+"_OutdoorPerimeter",root);
    }
    static Transform Pivot(string name)
    {
        var go=Find(name);if(go==null)return null;
        var pivot=new GameObject(name+" articulation").transform;pivot.SetParent(root,false);pivot.position=go.GetComponent<Renderer>().bounds.center;go.transform.SetParent(pivot,true);return pivot;
    }
    static Transform Proxy(string name,Vector3 position,Vector3 size,string material)
    {
        var m=new LevelModelBuilder();m.Box(Vector3.zero,size,material);var go=m.Save(key+"_"+name,root,false);go.transform.position=position;return go.transform;
    }
    static void WireVisuals(int index)
    {
        var s=level.stations;
        if(index==1 || index==10)
        {
            s[1].movingPart=Find("HandcraftedCargoPavilionBoat").transform;s[1].values=new[]{250f,170f,100f};
            Vector3 offset=index==10?new Vector3(0,.24f,8.6f):Vector3.zero;
            s[2].movingPart=Extract("CanalSluiceAndCrank","MovingSluice",v=>v.x>4.35f && v.x<4.65f && v.y<2.2f && Mathf.Abs(v.z-2.1f)<1.05f);s[2].travel=Vector3.up*1.65f;
        }
        if(index==2){s[1].movingPart=Extract("FiveGraduatedBronzeBells","AnimatedBell",v=>v.x>-3.2f && v.x<-2 && v.y<3.23f);s[1].target=100;s[1].values=new[]{80f,92f,100f};}
        if(index==3)
        {
            s[0].movingPart=Extract("MagneticLodestoneSpoonAndCompass","CompassSpoon",v=>v.y>1.24f);
            s[2].movingPart=Extract("FourMirrorStaticOpticalPath","AdjustableBronzeMirror",v=>v.x>5.1f && v.x<6.3f && v.y>2.1f,mat=>mat.name=="SatinBrass" || mat.name=="GlacierBlue");
            s[2].target=30;s[2].values=new[]{10f,20f,30f};
            // Keep the hardware present; only the light-path submesh toggles.
            s[2].secondaryPart=Extract("FourMirrorStaticOpticalPath","OpticalBeam",v=>true,mat=>mat.name=="AmberLight");s[2].secondaryPart.gameObject.SetActive(false);
        }
        if(index==4 || index==9)
        {
            int n=index==4?1:0;s[n].values=new[]{.5f,1.2f,2.5f};
            Vector3 core=index==4?new Vector3(-1.4f,1.37f,.38f):new Vector3(-4.8f,1.37f,1.78f);
            s[n].movingPart=Extract(index==4?"ElectromagneticAssemblyWorkbench":"IntegratedLabElectromagneticIsland","MovingIronCore",v=>Mathf.Abs(v.x-core.x)<.9f && Mathf.Abs(v.y-core.y)<.11f && Mathf.Abs(v.z-core.z)<.11f,mat=>mat.name=="SageStone");s[n].travel=Vector3.right*.5f;
            if(index==9)
            {
                s[1].movingPart=Extract("IntegratedLabOpticalIsland","AdjustableMirror",v=>Mathf.Abs(v.x-3.95f)<.3f && v.y>1.06f,mat=>mat.name=="SatinBrass" || mat.name=="GlacierBlue");s[1].values=new[]{10f,20f,30f};s[1].target=30;
                s[1].secondaryPart=Extract("IntegratedLabOpticalIsland","OpticalBeam",v=>true,mat=>mat.name=="AmberLight");s[1].secondaryPart.gameObject.SetActive(false);
            }
        }
        if(index==5)
        {
            s[1].movingPart=Extract("HydraulicTankNetworkAndLeak","LeakDroplets",v=>v.x>-2.25f && v.x<-1.3f && v.z>-2.77f,mat=>mat.name=="JadeWater" || mat.name=="GlacierBlue");
            if(s[1].movingPart==null)s[1].movingPart=Proxy("LeakMarker",P(-2.4f,-1.2f,.4f),new Vector3(.2f,.4f,.2f),"JadeWater");
            s[1].secondaryPart=Proxy("ReplacementPipeSeal",P(-2.155f,-2.8f,1.75f),new Vector3(.39f,.22f,.22f),"Celadon");s[1].secondaryPart.gameObject.SetActive(false);
            s[2].movingPart=Extract("HydraulicTankNetworkAndLeak","ValveHandle",v=>Mathf.Abs(v.x+.8f)<.36f && Mathf.Abs(v.y-2.95f)<.36f && v.z>-2.5f,mat=>mat.name=="CoralRed");
        }
        if(index==6)
        {
            s[1].movingPart=Extract("ZoneAAdjustableMechanicalPendulum","PendulumBobAndRod",v=>v.y>.4f && v.y<2.8f && Mathf.Abs(v.x+4.4f)<.4f);
            s[1].target=DemoPhysics.PendulumFrequency(1);s[1].values=new[]{.3f,.4f,s[1].target};
            s[2].target=s[1].target;s[2].values=new[]{.3f,.4f,s[1].target};s[2].movingPart=Proxy("LCOscillationIndicator",P(4.4f,.2f,1.5f),new Vector3(.6f,.08f,.1f),"CyanLight");
        }
        if(index==7)
        {
            s[1].movingPart=Proxy("DemoSnowball",P(0,1.7f,1.25f),Vector3.one*.28f,"SnowIvory");s[1].travel=new Vector3(0,0,-2.2f);s[1].secondaryPart=Pivot("SnowmanTarget");
        }
        if(index==8)
        {
            // Original joined geometry becomes static background; this second self-built chain has individual pivots.
            var chain=new GameObject("ArticulatedDemoDominoes").transform;chain.SetParent(root,false);chain.position=P(0,2.7f,.26f);
            for(int i=0;i<7;i++){var d=Proxy("Domino_"+i,chain.position+Vector3.up*.325f+Vector3.right*(i*.35f-1),new Vector3(.1f,.65f,.32f),i%2==0?"Celadon":"Ivory");var pivot=new GameObject("Domino pivot "+i).transform;pivot.SetParent(chain,false);pivot.position=d.position-Vector3.up*.325f;d.SetParent(pivot,true);}
            s[1].movingPart=chain;
        }
        if(index==11){s[2].movingPart=RenderSettings.sun.transform;s[2].values=new[]{15f,30f,45f};}
        if(index==12){s[0].movingPart=Pivot("SnowmanAssetDisplay");s[2].movingPart=Pivot("SelfModeledFrostedGlove");}
    }
    static Transform Extract(string sourceName,string part,Func<Vector3,bool> select,Func<Material,bool> materialSelect=null)
    {
        var source=Find(sourceName);if(source==null)return null;var filter=source.GetComponent<MeshFilter>();var renderer=source.GetComponent<Renderer>();var old=filter.sharedMesh;
        var a=Object.Instantiate(old);var b=Object.Instantiate(old);a.name=key+"_"+part+"Static";b.name=key+"_"+part;
        var verts=old.vertices;int selected=0;
        for(int sub=0;sub<old.subMeshCount;sub++)
        {
            var rest=new List<int>();var move=new List<int>();var tri=old.GetTriangles(sub);
            for(int i=0;i<tri.Length;i+=3)
            {
                var center=(verts[tri[i]]+verts[tri[i+1]]+verts[tri[i+2]])/3;
                bool chosen=select(center) && (materialSelect==null || materialSelect(renderer.sharedMaterials[sub]));var list=chosen?move:rest;list.AddRange(new[]{tri[i],tri[i+1],tri[i+2]});if(chosen)selected++;
            }
            a.SetTriangles(rest,sub);b.SetTriangles(move,sub);
        }
        if(selected==0){Object.DestroyImmediate(a);Object.DestroyImmediate(b);return null;}
        string directory="Assets/ReportVR/LevelModels/";AssetDatabase.CreateAsset(a,directory+a.name+".asset");AssetDatabase.CreateAsset(b,directory+b.name+".asset");
        filter.sharedMesh=a;var collider=source.GetComponent<MeshCollider>();if(collider!=null)collider.sharedMesh=a;
        var go=new GameObject(part);go.transform.SetParent(root,false);go.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);go.transform.localScale=source.transform.lossyScale;go.AddComponent<MeshFilter>().sharedMesh=b;go.AddComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials;
        var used=Enumerable.Range(0,b.subMeshCount).SelectMany(i=>b.GetTriangles(i)).Distinct().Select(i=>source.transform.TransformPoint(verts[i])).ToArray();var bounds=new Bounds(used[0],Vector3.zero);foreach(var v in used)bounds.Encapsulate(v);
        var pivot=new GameObject(part+" pivot").transform;pivot.SetParent(root,false);pivot.position=bounds.center;go.transform.SetParent(pivot,true);return pivot;
    }
    public static void Validate()
    {
        var results=new List<string>();int failures=0;
        foreach(string path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);var l=Object.FindObjectOfType<DemoLevel>();if(l==null){results.Add("FAIL missing level "+path);failures++;continue;}
            var input=l.player.GetComponent<DemoInteractor>();
            if(input.interact==null || input.interact.action==null || input.resetDemo==null || input.resetDemo.action==null || input.worldText==null){failures++;results.Add("FAIL demo input or font material reference missing "+path);}
            // The designed locked door is checked in its completed/open state.
            if(l.exitDoor!=null)l.exitDoor.localPosition+=l.exitDoorTravel;
            Physics.SyncTransforms();var controller=l.player.GetComponent<CharacterController>();controller.enabled=false;
            for(int segment=0;segment<l.route.Length-1;segment++)
            {
                Vector3 a=l.route[segment],b=l.route[segment+1];int count=Mathf.CeilToInt(Vector3.Distance(a,b)/.18f);float lastY=float.NaN;
                for(int j=0;j<=count;j++)
                {
                    Vector3 p=Vector3.Lerp(a,b,(float)j/count);bool bad=false;string detail="";
                    if(!Physics.Raycast(p+Vector3.up*.32f,Vector3.down,out var hit,.9f,~0,QueryTriggerInteraction.Ignore)){bad=true;detail="no floor";}
                    else
                    {
                        float y=hit.point.y;
                        if(hit.normal.y<.7f){bad=true;detail="steep floor";}
                        if(!float.IsNaN(lastY) && Mathf.Abs(y-lastY)>.23f){bad=true;detail="step "+Mathf.Abs(y-lastY).ToString("F2");}lastY=y;
                        var bottom=new Vector3(p.x,y+.35f,p.z);var top=new Vector3(p.x,y+1.5f,p.z);
                        var obstacles=Physics.OverlapCapsule(bottom,top,.23f,~0,QueryTriggerInteraction.Ignore).Where(c=>c.enabled && !c.GetComponent<CharacterController>()).ToArray();
                        if(obstacles.Length>0){bad=true;detail="blocked by "+string.Join(",",obstacles.Select(c=>c.name));}
                    }
                    if(bad){results.Add("FAIL "+Path.GetFileNameWithoutExtension(path)+" segment="+segment+" p="+p.ToString("F2")+" "+detail);failures++;break;}
                }
            }
            foreach(var s in l.stations)
            {
                if(s.label==null || s.indicator==null){failures++;results.Add("FAIL station feedback missing "+path);}
                if(s.action!=DemoAction.Inspect && s.movingPart==null){failures++;results.Add("FAIL animated component missing "+path+" "+s.action);}
                if(s.puzzle!=null && s.puzzle.mode==ReportPuzzleMode.Physical)
                {
                    if(s.puzzle.learningTask==null || s.puzzle.learningTask.requirements.Any(r=>r.measure==null))
                    {failures++;results.Add("FAIL physical experiment reference missing "+path+" "+s.instruction);}
                }
                else
                {
                    PuzzleTestSupport.PrepareSolution(s);
                    for(int tries=0;tries<5 && l.Step<=s.index;tries++){l.Use(s);if(s.IsResolving)s.SimulateStep(s.completionDelay+.01f);}
                }
            }
            if(!l.stations.Any(s=>s.puzzle!=null && s.puzzle.mode==ReportPuzzleMode.Physical) && !l.Completed){failures++;results.Add("FAIL demo cannot complete "+path);}
            string walkingError=WalkWithController(l,controller);
            if(walkingError!=null){failures++;results.Add("FAIL "+Path.GetFileNameWithoutExtension(path)+" character walk: "+walkingError);}
            l.ResetLevel();
            results.Add("CHECKED "+Path.GetFileNameWithoutExtension(path)+" route segments="+(l.route.Length-1)+" stations="+l.stations.Length);
        }
        File.WriteAllLines("Documentation/Verification/level-routes.txt",new[]{(failures==0?"PASS":"FAIL")+": 15 scenes; 0.18m path samples, floor + slope + 0.46m capsule clearance; actual CharacterController traversal; input/experiment references. Physical experiment completion requires separate Play Mode checks. Failures="+failures}.Concat(results));
        if(failures>0)throw new InvalidOperationException("Level routes have "+failures+" failures. See level-routes.txt");
        VerifyReportVR.Validate();
    }
    public static void CaptureAll()
    {
        foreach(string path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);var l=Object.FindObjectOfType<DemoLevel>();
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
            var textMaterial=new Material(WorldTextMaterial());
            foreach(var t in Object.FindObjectsOfType<TextMesh>(true)){t.font=font;t.GetComponent<Renderer>().sharedMaterial=textMaterial;}
            l.Refresh();string name=Path.GetFileNameWithoutExtension(path);
            font.RequestCharactersInTexture(string.Concat(Object.FindObjectsOfType<TextMesh>(true).Select(t=>t.text)),48);textMaterial.mainTexture=font.material.mainTexture;
            ReportLevelAudit.Photograph("Documentation/LevelReview/After/"+name+"_top.png",new Vector3(0,30,0),Vector3.zero,true,name.Contains("Garden")?20:13);
            ReportLevelAudit.Photograph("Documentation/LevelReview/After/"+name+"_eye.png",l.spawn+Vector3.up*1.65f,l.spawn+Vector3.up*1.5f+Quaternion.Euler(0,l.spawnYaw,0)*Vector3.forward*6,false,15);
            Object.DestroyImmediate(textMaterial);Object.DestroyImmediate(font);
        }
        EditorSceneManager.OpenScene(ReportLevelAudit.Scenes()[0]);
    }
    static string WalkWithController(DemoLevel l,CharacterController controller)
    {
        Physics.Raycast(l.spawn+Vector3.up*.3f,Vector3.down,out var start,1,~0,QueryTriggerInteraction.Ignore);
        l.player.position=new Vector3(l.spawn.x,start.point.y+.04f,l.spawn.z);controller.enabled=true;Physics.SyncTransforms();
        foreach(Vector3 point in l.route.Skip(1))
        {
            Vector3 delta=point-l.player.position;delta.y=0;int frames=0;
            while(delta.magnitude>.045f && frames++<1200)
            {
                var before=l.player.position;controller.Move(Vector3.ClampMagnitude(delta,.08f)+Vector3.down*.025f);
                delta=point-l.player.position;delta.y=0;
                if(frames>4 && (l.player.position-before).sqrMagnitude<.000002f)
                {controller.enabled=false;return "stuck at "+l.player.position.ToString("F2")+" towards "+point.ToString("F2");}
            }
            if(delta.magnitude>.1f){controller.enabled=false;return "unreachable "+point.ToString("F2")+" from "+l.player.position.ToString("F2");}
        }
        controller.enabled=false;return null;
    }
    public static void FinalChecks(){ReportLevelAudit.DemoTests();Validate();CaptureAll();}
    static Material PointerMaterial()
    {
        const string path="Assets/ReportVR/LevelModels/InteractionRay.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Sprites/Default"));AssetDatabase.CreateAsset(material,path);}
        return material;
    }
    static Material WorldTextMaterial()
    {
        const string path="Assets/ReportVR/LevelModels/WorldText.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("ReportVR/WorldText"));AssetDatabase.CreateAsset(material,path);}
        return material;
    }
    static void PlaceNoticeBoard(DemoLevel l,GameObject notice,TextMesh label)
    {
        Physics.SyncTransforms();Vector3 forward=Quaternion.Euler(0,l.spawnYaw,0)*Vector3.forward;
        Vector3 best=notice.transform.position;float bestScore=float.NegativeInfinity;
        for(int i=0;i<24;i++)
        {
            Vector3 candidate=l.spawn+Quaternion.Euler(0,i*15f,0)*Vector3.forward*3;
            if(Vector3.Dot(candidate-l.spawn,forward)<.3f)continue;
            if(!Physics.Raycast(candidate+Vector3.up*.3f,Vector3.down,out var floor,.8f,~0,QueryTriggerInteraction.Ignore))continue;
            candidate.y=floor.point.y+.025f;
            if(Physics.CheckCapsule(candidate+Vector3.up*.4f,candidate+Vector3.up*2.1f,1.1f,~0,QueryTriggerInteraction.Ignore))continue;
            float controlDistance=l.stations.Min(s=>Vector3.Distance(candidate,s.transform.position));
            if(controlDistance<2.2f)continue;
            float pathDistance=float.PositiveInfinity;
            for(int j=0;j<l.route.Length-1;j++)
            {
                Vector3 a=l.route[j],b=l.route[j+1];a.y=b.y=candidate.y;
                float t=Mathf.Clamp01(Vector3.Dot(candidate-a,b-a)/(b-a).sqrMagnitude);
                pathDistance=Mathf.Min(pathDistance,Vector3.Distance(candidate,Vector3.Lerp(a,b,t)));
            }
            if(pathDistance<1.6f)continue;
            float score=controlDistance+Mathf.Min(pathDistance,3);
            if(score>bestScore){bestScore=score;best=candidate;}
        }
        notice.transform.position=best;notice.transform.rotation=Quaternion.LookRotation(best-l.spawn);
        label.transform.SetPositionAndRotation(best+Vector3.up*1.5f-notice.transform.forward*.045f,notice.transform.rotation);
    }
    public static void FinalizeAndBuild()
    {
        var index=new List<string>{"Build index | Scene | Floor spawn (world meters)"};var material=PointerMaterial();
        var paths=ReportLevelAudit.Scenes();
        for(int i=0;i<paths.Length;i++)
        {
            EditorSceneManager.OpenScene(paths[i]);var l=Object.FindObjectOfType<DemoLevel>();
            var notice=Find(Path.GetFileNameWithoutExtension(paths[i])+"_EntryNoticeBoard");PlaceNoticeBoard(l,notice,l.board);
            foreach(var text in l.GetComponentsInChildren<TextMesh>(true)){text.color=new Color(.025f,.045f,.055f);text.GetComponent<Renderer>().sharedMaterial=WorldTextMaterial();}
            l.player.GetComponent<DemoInteractor>().ray.sharedMaterial=material;
            l.player.GetComponent<DemoInteractor>().worldText=WorldTextMaterial();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            index.Add(i+" | "+Path.GetFileNameWithoutExtension(paths[i])+" | "+l.spawn.ToString("F3"));
        }
        AssetDatabase.SaveAssets();File.WriteAllLines("Documentation/VR_scene_index.txt",index);
        var used=new HashSet<string>(AssetDatabase.GetDependencies(paths,true));var removed=new List<string>();
        foreach(var guid in AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/ReportVR/LevelModels"}))
        {string path=AssetDatabase.GUIDToAssetPath(guid);if(!used.Contains(path)){removed.Add(path);AssetDatabase.DeleteAsset(path);}}
        File.WriteAllLines("Documentation/LevelReview/removed-unused-generated-models.txt",removed);
        ReportLevelAudit.DemoTests();CaptureAll();SetupReportVR.BuildWindows();
    }
}
