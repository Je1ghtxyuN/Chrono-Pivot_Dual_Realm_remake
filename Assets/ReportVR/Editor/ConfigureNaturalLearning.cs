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
        ResizeSamples(samples);
        var place=ConfigureReportPuzzles.Natural(0,"拿起三件材料","分别拿起木、石、铜三件材料","每件材料只记录一次拿取。","");
        BuildPickup(place,samples);
        var launchPuzzle=ConfigureReportPuzzles.Natural(1,"投向金色落点区","把三件材料分别抛到金色落点区","真实挥手释放；直接搬入或重复投同一件不计完成。","");
        BuildFreeThrow(launchPuzzle,samples,ConfigureReportPuzzles.ExperimentLocation(launchPuzzle.station)+Vector3.back*3.6f);
        var shadow=ConfigureReportPuzzles.Natural(2,"晨光与日晷","把太阳模型拖到东方，观察日晷影子向西","太阳在东方时，入射光向西；实际阴影随太阳模型移动。","握住金色太阳模型；VR绕圆环拖动，桌面抓住后拖动鼠标，读数接近90°。 ");
        BuildSun(shadow);
        ReportFloatingStatus.ConfigureScene(level);
        level.mission="操作器材完成真实实验；条件达成后自动推进，沿步道通过出口。";
        ConfigureReportPuzzles.EnsureExit();level.grabbables=Object.FindObjectsOfType<ReportGrabbable>();level.Refresh();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        WriteFirstGuide();
        Debug.Log("NATURAL_FIRST_LEVEL_CONFIGURED");
    }
    public static void UpdateFirstSteps(DemoLevel target)
    {
        level=target;key=Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);
        layout=level.transform.Find("Natural Learning Layout");
        var samples=Enumerable.Range(0,3).Select(i=>ConfigureReportPuzzles.I("HubMaterialCube_"+i,"材料样品")).ToArray();
        ResizeSamples(samples);
        var first=level.stations[0].puzzle;
        foreach(var placement in first.placements)if(placement.socket!=null)Object.DestroyImmediate(placement.socket.parent.gameObject);
        first.placements=new PuzzlePlacement[0];first.station.instruction="拿起三件材料";
        first.goal="分别拿起木、石、铜三件材料";first.clue="每件材料只记录一次拿取。";first.hint="";
        BuildPickup(first,samples);
        var second=level.stations[1].puzzle;second.station.instruction="投向金色落点区";
        second.goal="把三件材料分别抛到金色落点区";second.clue="真实挥手释放；直接搬入或重复投同一件不计完成。";second.hint="";
        var previous=Object.FindObjectOfType<ReportThrowTarget>();var location=previous.transform.position;
        Object.DestroyImmediate(previous.gameObject);
        var launcher=Object.FindObjectOfType<ReportMechanicalLauncher>();if(launcher!=null)Object.DestroyImmediate(launcher.gameObject);
        BuildFreeThrow(second,samples,location);
        first.successMessage="三件材料已全部拿取。";second.successMessage="三件材料已全部投入落点区。";
        level.mission="依次拿起三件材料，抛到金色落点区，再把太阳转到东方。";
        WriteFirstGuide();
    }
    static void BuildPickup(ReportPuzzle puzzle,ReportGrabbable[] samples)
    {
        var pickup=puzzle.GetComponent<ReportPickupMeasure>();if(pickup==null)pickup=puzzle.gameObject.AddComponent<ReportPickupMeasure>();
        pickup.station=puzzle.station;pickup.samples=samples;pickup.label="已拿取材料";pickup.unit="件";
        puzzle.learningTask.requirements=new[]{Condition(pickup,3,3,0,false)};
    }
    static void ResizeSamples(ReportGrabbable[] samples)
    {
        for(int i=0;i<samples.Length;i++)
        {
            var sample=samples[i];
            var renderer=sample.GetComponent<Renderer>();var oldBounds=renderer.bounds;
            float bottom=oldBounds.min.y;
            float factor=.14f/Mathf.Max(oldBounds.size.x,oldBounds.size.y,oldBounds.size.z);
            sample.transform.localScale*=factor;Physics.SyncTransforms();
            sample.transform.position+=Vector3.up*(bottom-renderer.bounds.min.y);
            var label=sample.transform.Find("Puzzle Item Label");
            if(label==null)
            {
                label=new GameObject("Puzzle Item Label").transform;label.SetParent(sample.transform,false);
                Text(label,"物件标记",Vector3.zero,"",.012f);
            }
            if(label!=null)
            {
                label.localScale=Vector3.one/sample.transform.lossyScale.x;
                label.position=renderer.bounds.center+Vector3.up*.22f;
                foreach(var r in label.GetComponentsInChildren<MeshRenderer>())if(r.GetComponent<TextMesh>()==null)r.enabled=false;
                var text=label.GetComponentInChildren<TextMesh>();text.text=new[]{"木材","石材","铜材"}[i];text.color=new Color(1,.92f,.65f);
                var facing=text.GetComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.FacePlayer;facing.yawOnly=true;
                facing.viewer=Object.FindObjectOfType<Unity.XR.CoreUtils.XROrigin>().Camera.transform;
            }
        }
    }
    static void BuildFreeThrow(ReportPuzzle puzzle,ReportGrabbable[] samples,Vector3 location)
    {
        var pad=new LevelModelBuilder();
        pad.Box(Vector3.up*.012f,new Vector3(2.8f,.024f,2.8f),"Celadon");
        foreach(int side in new[]{-1,1})
        {
            pad.Box(new Vector3(side*1.4f,.035f,0),new Vector3(.06f,.04f,2.86f),"AmberLight");
            pad.Box(new Vector3(0,.035f,side*1.4f),new Vector3(2.86f,.04f,.06f),"AmberLight");
        }
        for(int i=0;i<3;i++)pad.Box(new Vector3((i-1)*.8f,.03f,0),new Vector3(.6f,.012f,.6f),"SatinBrass");
        var go=pad.Save(key+"_FreeThrowLandingZone",layout);go.transform.position=location;
        var zone=go.AddComponent<BoxCollider>();zone.isTrigger=true;zone.center=Vector3.up*.3f;zone.size=new Vector3(2.8f,.58f,2.8f);
        foreach(var sample in samples)if(sample.GetComponent<ReportThrowTrack>()==null)sample.gameObject.AddComponent<ReportThrowTrack>();
        var detector=go.AddComponent<ReportThrowTarget>();detector.requiredStation=puzzle.station;
        detector.acceptedItems=samples.Select(s=>s.GetComponent<ReportThrowTrack>()).ToArray();
        detector.minimumFlightDistance=.8f;detector.minimumFlightTime=.12f;detector.acceptedSpeed=new Vector2(.8f,20);
        var measure=go.AddComponent<ReportThrowMeasure>();measure.target=detector;measure.countDifferentItems=true;measure.label="已投入材料";measure.unit="件";
        puzzle.learningTask.requirements=new[]{Condition(measure,3,3,0)};
        puzzle.station.label.transform.position=location+Vector3.up*2.1f;
    }
    static void WriteFirstGuide()=>File.WriteAllText("Documentation/自然实验第一关.md","# 第一关\n\n1. 分别拿起木、石、铜三件材料，每件只计一次；无需归位。\n2. 把三件材料分别抛进金色边框落点区。实际释放速度至少0.8m/s，水平飞行至少0.8m；近距离放入与同件重复命中不能代替三件材料。\n3. 原第三步保留：握住金色太阳模型拖到东方（约90°），观察日晷阴影向西。\n\nVR：挥手并松握把。桌面：左键/E拿住后，右键/F蓄力并松开抛出。浮空文字显示步骤状态和拿取/投掷计数；菜单不再提供提示选项。R/菜单重置会清空计数并恢复物件。\n");
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
