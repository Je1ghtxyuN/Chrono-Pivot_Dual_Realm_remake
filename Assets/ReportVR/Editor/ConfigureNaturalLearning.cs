using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Object=UnityEngine.Object;

public static partial class ConfigureNaturalLearning
{
    static Transform layout;
    static DemoLevel level;
    static string key;
    static GameObject Find(string name)=>Object.FindObjectsOfType<Transform>(true).FirstOrDefault(t=>t.name==name)?.gameObject;
    static void Hide(string name){var go=Find(name);if(go!=null)go.SetActive(false);}
    static TextMesh Text(Transform parent,string name,Vector3 position,string value,float size=.014f)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;
        var text=go.AddComponent<TextMesh>();text.text=value;text.characterSize=size;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.03f,.05f,.055f);
        go.GetComponent<Renderer>().sharedMaterial=Object.FindObjectOfType<DemoInteractor>().worldText;go.AddComponent<ReportTextFacing>().mode=ReportTextFacingMode.WorldFixed;return text;
    }
    static void Gem(LevelModelBuilder model,Vector3 center,float radius,string material)
    {
        var p=new[]{Vector3.up,Vector3.down,Vector3.right,Vector3.forward,Vector3.left,Vector3.back};
        for(int i=2;i<6;i++){int next=i==5?2:i+1;model.Face(LevelModelBuilder.Mat(material),center+p[0]*radius,center+p[next]*radius,center+p[i]*radius);model.Face(LevelModelBuilder.Mat(material),center+p[1]*radius,center+p[i]*radius,center+p[next]*radius);}
    }
    static ReportMechanismHandle Handle(GameObject model,ReportHandleMotion motion,Vector3 axis,Vector2 values,float initial)
    {
        model.GetComponent<MeshCollider>().convex=true;var body=model.AddComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;body.interpolation=RigidbodyInterpolation.Interpolate;
        var handle=model.AddComponent<ReportMechanismHandle>();handle.motion=motion;handle.localAxis=axis;handle.valueRange=values;handle.startingValue=initial;
        handle.interactionManager=Object.FindObjectOfType<XRInteractionManager>();handle.throwOnDetach=false;handle.attachEaseInTime=0;
        handle.movementType=UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;
        handle.desktopUnitsPerPixel=(values.y-values.x)*.004f;return handle;
    }
    static ReportLearningRequirement Condition(ReportLearningMeasure source,float minimum,float maximum,int operations=1,bool released=true)=>new ReportLearningRequirement{measure=source,acceptedRange=new Vector2(minimum,maximum),freshOperations=operations,requireReleased=released};
    [MenuItem("Report VR/Natural Learning/Configure First Level")]
    public static void FirstAndCheck(){FirstLevel();NaturalLearningCheck.First();PolishReportLevels.Validate();}
    public static void FirstLevel()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);level=Object.FindObjectOfType<DemoLevel>();key=Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[0].path);
        foreach(string name in new[]{"Natural Learning Layout","Puzzle Layout"}){var old=level.transform.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);}
        layout=new GameObject("Natural Learning Layout").transform;layout.SetParent(level.transform,false);ConfigureReportPuzzles.BeginNatural(level,key,layout);
        Hide("StaticIntroThrowTrajectory");Hide("HubThrowTargetPads");
        var samples=Enumerable.Range(0,3).Select(i=>ConfigureReportPuzzles.I("HubMaterialCube_"+i,"材料样品")).ToArray();
        for(int i=0;i<samples.Length;i++)samples[i].GetComponent<Rigidbody>().mass=i+1;
        var place=ConfigureReportPuzzles.P(0,"比较与归位材料","拿起木、石、铜样品，放入同名槽位","实验样品质量为1、2、3kg；先按标记归位，松手后自动记录。","拿住物件不计归位；错误槽位不会推进。",samples,new[]{"木材 1kg","石材 2kg","铜材 3kg"});
        place.mode=ReportPuzzleMode.Physical;place.station.action=DemoAction.Inspect;place.learningTask=place.gameObject.AddComponent<ReportLearningTask>();
        var status=place.station.transform.Find("Puzzle Controls/验证文字");status.GetComponent<TextMesh>().text="状态";Object.DestroyImmediate(place.station.indicator.GetComponent<Collider>());
        var launchPuzzle=ConfigureReportPuzzles.Natural(1,"同条件抛掷实验","逐件发射三种材料，比较实测轨迹与落点","发射器初速与角度固定，忽略空气阻力；同条件下质量不改变落点。","把一件材料放进发射杯，握住拉杆并拉到底，松手发射；重复三种材料。");
        BuildLauncher(launchPuzzle,samples);
        var shadow=ConfigureReportPuzzles.Natural(2,"晨光与日晷","把太阳模型拖到东方，观察日晷影子向西","太阳在东方时，入射光向西；实际阴影随太阳模型移动。","握住金色太阳模型；VR绕圆环拖动，桌面抓住后拖动鼠标，读数接近90°。 ");
        BuildSun(shadow);
        level.mission="操作器材完成真实实验；条件达成后自动推进，沿步道通过出口。";
        ConfigureReportPuzzles.EnsureExit();level.grabbables=Object.FindObjectsOfType<ReportGrabbable>();level.Refresh();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        File.WriteAllText("Documentation/自然实验第一关.md","# 第一关自然物理实验\n\n1. 拿起三种材料，放入同名槽位并松手，自动记录。\n2. 将材料逐件放进标定发射杯，握住拉杆拉到底并松手；三种材料必须都命中实验落点区。初速和仰角由发射器保持相同，轨迹由真实刚体重力和碰撞形成。\n3. 握住金色太阳模型拖至东方（约90°），观察日晷阴影向西。\n\n自由手抛：VR挥手松开；桌面拿住后按住右键/F蓄力，松开右键/F抛出。标准实验使用标定发射器保证同条件。\n\n状态灯不能触发答案；目标达成并稳定后自动推进。\n");
        Debug.Log("NATURAL_FIRST_LEVEL_CONFIGURED");
    }
    static void BuildLauncher(ReportPuzzle puzzle,ReportGrabbable[] samples)
    {
        var p=ConfigureReportPuzzles.ExperimentLocation(puzzle.station);var destination=new Vector3(p.x,level.spawn.y,p.z-3.6f);
        var root=new GameObject("同速同角标定发射器").transform;root.SetParent(layout,false);root.position=p;root.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(destination-p,Vector3.up));
        var bench=new LevelModelBuilder();bench.Box(Vector3.up*.85f,new Vector3(1.2f,.12f,.8f),"Celadon");foreach(float x in new[]{-.45f,.45f})bench.Beam(new Vector3(x,.04f,0),new Vector3(x,.85f,0),.09f,"SatinBrass");bench.Save(key+"_NaturalLauncherBench",root);
        float half=samples[0].GetComponent<Renderer>().bounds.extents.y;
        var cup=new GameObject("发射杯").transform;cup.SetParent(root,false);cup.localPosition=Vector3.up*(.91f+half+.02f);
        var tray=new LevelModelBuilder();tray.Box(Vector3.up*.95f,new Vector3(.85f,.08f,.75f),"InkTeal");tray.Save(key+"_NaturalLauncherCup",root);
        foreach(var sample in samples)
        {
            if(sample.GetComponent<ReportThrowTrack>()==null)sample.gameObject.AddComponent<ReportThrowTrack>();
            var seat=cup.gameObject.AddComponent<ReportPlacementSocket>();seat.item=sample;seat.captureDistance=.4f;
        }
        var rail=new LevelModelBuilder();rail.Beam(new Vector3(-.72f,.9f,-.1f),new Vector3(-.72f,.9f,-.6f),.055f,"SatinBrass");rail.Save(key+"_NaturalLauncherRail",root,false);
        var knob=new LevelModelBuilder();knob.Box(Vector3.zero,new Vector3(.25f,.12f,.15f),"CoralRed");var go=knob.Save(key+"_NaturalLauncherPull",root);go.transform.localPosition=new Vector3(-.72f,.9f,-.1f);
        var pull=Handle(go,ReportHandleMotion.Slide,Vector3.back,new Vector2(0,1),0);pull.slideRange=new Vector2(0,.5f);pull.unit="拉杆行程";
        var launcher=root.gameObject.AddComponent<ReportMechanicalLauncher>();launcher.pull=pull;launcher.cup=cup;launcher.samples=samples;
        float distance=Vector3.ProjectOnPlane(destination-cup.position,Vector3.up).magnitude;
        float fall=cup.position.y-destination.y-half;
        launcher.speed=Mathf.Sqrt(Physics.gravity.magnitude*distance*distance/(distance+fall));
        launcher.readout=Text(root,"发射操作与读数",new Vector3(0,1.95f,.12f),"一件样品入杯 → 拉到底 → 松手\n初速 "+launcher.speed.ToString("F2")+"m/s · 仰角45°");
        launcher.readout.transform.rotation=Quaternion.LookRotation(launcher.readout.transform.position-puzzle.station.viewingPoint);
        var pad=new LevelModelBuilder();pad.Box(Vector3.up*.016f,new Vector3(1.6f,.032f,1.6f),"Celadon");pad.Box(Vector3.up*.035f,new Vector3(.9f,.008f,.9f),"SatinBrass");var target=pad.Save(key+"_NaturalLandingZone",layout);target.transform.position=destination;
        var zone=target.AddComponent<BoxCollider>();zone.isTrigger=true;zone.center=Vector3.up*.38f;zone.size=new Vector3(1.8f,.7f,1.8f);
        var detector=target.AddComponent<ReportThrowTarget>();detector.launchArea=cup;detector.launchRadius=.8f;detector.minimumFlightDistance=2;detector.acceptedItems=samples.Select(s=>s.GetComponent<ReportThrowTrack>()).ToArray();
        detector.acceptedSpeed=new Vector2(launcher.speed-.12f,launcher.speed+.12f);detector.acceptedAngle=new Vector2(44,46);
        detector.readout=Text(target.transform,"落点读数",Vector3.up*.95f,"等待同条件发射三种材料");detector.readout.transform.rotation=Quaternion.LookRotation(detector.readout.transform.position-puzzle.station.viewingPoint);
        var measure=target.AddComponent<ReportThrowMeasure>();measure.target=detector;measure.countDifferentItems=true;measure.label="同条件命中样品";measure.unit="种";
        puzzle.learningTask.requirements=new[]{Condition(measure,3,3,3)};
    }
    static void BuildSun(ReportPuzzle puzzle)
    {
        var p=ConfigureReportPuzzles.ExperimentLocation(puzzle.station);var root=new GameObject("可操作太阳轨道").transform;root.SetParent(layout,false);root.position=p;
        var stand=new LevelModelBuilder();stand.Beam(Vector3.zero,Vector3.up*1.65f,.055f,"SatinBrass");stand.Box(Vector3.up*.035f,new Vector3(.5f,.07f,.5f),"SageStone");
        for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;stand.Beam(new Vector3(Mathf.Sin(a)*.55f,1.65f,Mathf.Cos(a)*.55f),new Vector3(Mathf.Sin(b)*.55f,1.65f,Mathf.Cos(b)*.55f),.018f,"InkTeal");}
        stand.Save(key+"_NaturalSunOrbit",root,false);
        var shape=new LevelModelBuilder();Gem(shape,Vector3.forward*.55f,.13f,"AmberLight");shape.Beam(Vector3.zero,Vector3.forward*.55f,.025f,"SatinBrass");var model=shape.Save(key+"_NaturalSunHandle",root);model.transform.localPosition=Vector3.up*1.65f;
        var handle=Handle(model,ReportHandleMotion.Rotation,Vector3.up,new Vector2(0,360),180);handle.angleRange=new Vector2(0,360);handle.rotationFromPosition=true;handle.vrDegreesForFullRange=360;handle.unit="° 方位角";
        handle.readout=Text(root,"太阳方位读数",new Vector3(0,2.1f,0),"东方90° · 南方180° · 西方270°");handle.readout.transform.rotation=Quaternion.LookRotation(handle.readout.transform.position-puzzle.station.viewingPoint);
        var measure=root.gameObject.AddComponent<ReportSunlightMeasure>();measure.sunModel=handle;measure.sunlight=RenderSettings.sun;measure.label="太阳方位";measure.unit="°";
        puzzle.learningTask.requirements=new[]{Condition(measure,80,100)};
    }
}
