using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Object=UnityEngine.Object;

public static partial class ConfigureNaturalLearning
{
    static readonly List<string> naturalGuide=new List<string>();
    static int modelNumber;
    static GameObject Model(LevelModelBuilder model,string name,Transform parent,bool collision=true)=>model.Save(key+"_Natural_"+name+"_"+modelNumber++,parent,collision);
    static Transform Point(Transform parent,string name,Vector3 local)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=local;return go.transform;}
    static void Ring(LevelModelBuilder model,Vector3 center,float radius,float width,string material,Quaternion? rotation=null)
    {
        var q=rotation??Quaternion.identity;
        for(int i=0;i<16;i++){float a=i*Mathf.PI/8,b=(i+1)*Mathf.PI/8;model.Beam(center+q*new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius,center+q*new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*radius,width,material);}
    }
    static void Ball(LevelModelBuilder model,Vector3 center,float radius,string material,float cap=1)
    {
        for(int y=0;y<6;y++)for(int x=0;x<12;x++)
        {
            Vector3 P(int u,int v){float a=u*Mathf.PI/6,b=v*Mathf.PI/6;return center+new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Clamp(Mathf.Cos(b),-cap,cap),Mathf.Sin(b)*Mathf.Sin(a))*radius;}
            model.Face(LevelModelBuilder.Mat(material),P(x,y),P(x+1,y),P(x+1,y+1),P(x,y+1));
        }
    }
    static Transform Bench(int station,string name,float width=1.25f,float depth=.85f)
    {
        var root=Point(layout,name,Vector3.zero);root.position=ConfigureReportPuzzles.ExperimentLocation(level.stations[station]);
        root.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(root.position-level.stations[station].viewingPoint,Vector3.up));
        var m=new LevelModelBuilder();m.Box(Vector3.up*.9f,new Vector3(width,.08f,depth),"Celadon");
        foreach(float x in new[]{-.48f,.48f}){m.Beam(new Vector3(x,.08f,0),new Vector3(x,.87f,0),.065f,"SatinBrass");m.Box(new Vector3(x,.035f,0),new Vector3(.3f,.07f,.65f),"SageStone");}
        Model(m,name+"台架",root);return root;
    }
    static TextMesh Readout(Transform root,string text)=>Text(root,"实验测量读数",new Vector3(0,1.9f,.13f),string.IsNullOrEmpty(text)?"直接操作器材\n观察实测读数":text,.012f);
    static ReportGrabbable Piece(Transform parent,string name,Vector3 position,Vector3 size,string material,bool sphere=false)
    {
        var m=new LevelModelBuilder();if(sphere)Ball(m,Vector3.zero,size.x*.5f,material);else m.Box(Vector3.zero,size,material);
        var go=Model(m,name,parent);go.transform.localPosition=position;var grab=ConfigureReportGrabs.Portable(go,Object.FindObjectOfType<XRInteractionManager>());
        if(sphere){Object.DestroyImmediate(go.GetComponent<MeshCollider>());var collider=go.AddComponent<SphereCollider>();collider.radius=size.x*.5f;grab.colliders.Clear();grab.colliders.Add(collider);}
        go.AddComponent<ReportThrowTrack>();return grab;
    }
    static ReportMechanismHandle Knob(Transform root,string name,Vector3 position,Vector2 values,float initial,string unit,bool slide=false)
    {
        var m=new LevelModelBuilder();if(slide)m.Box(Vector3.zero,new Vector3(.2f,.1f,.18f),"CoralRed");
        else{Ring(m,Vector3.zero,.19f,.035f,"SatinBrass",Quaternion.Euler(90,0,0));m.Beam(Vector3.left*.16f,Vector3.right*.16f,.04f,"CoralRed");m.Beam(Vector3.down*.16f,Vector3.up*.16f,.04f,"CoralRed");}
        var go=Model(m,name,root);go.transform.localPosition=position;
        var h=Handle(go,slide?ReportHandleMotion.Slide:ReportHandleMotion.Rotation,slide?Vector3.down:Vector3.forward,values,initial);h.unit=unit;
        h.readout=Text(root,name+"刻度",position+Vector3.up*.35f+Vector3.back*.05f,"",.009f);
        if(slide){h.slideRange=new Vector2(0,.4f);h.vrTravelForFullRange=.4f;}else{h.angleRange=new Vector2(0,270);h.vrDegreesForFullRange=270;}
        return h;
    }
    static ReportHandleMeasure HandleReading(ReportMechanismHandle h,string label,string unit)
    {var m=h.gameObject.AddComponent<ReportHandleMeasure>();m.handle=h;m.label=label;m.unit=unit;return m;}
    static ReportPuzzle Task(int step,string title,string goal,string principle,string instructions)
    {
        var puzzle=ConfigureReportPuzzles.Natural(step,title,goal,principle,instructions);
        naturalGuide.Add("### "+(step+1)+". "+title+"\n\n目标："+goal+"\n\n操作："+instructions+"\n\n物理关系："+principle+"\n");return puzzle;
    }
    static void Conditions(ReportPuzzle puzzle,params ReportLearningRequirement[] requirements)=>puzzle.learningTask.requirements=requirements;
    static PuzzlePlacement Seat(ReportGrabbable item,Transform root,string name,Vector3 position,float distance=.22f)
    {
        var target=Point(root,name,position);var socket=target.gameObject.AddComponent<ReportPlacementSocket>();socket.item=item;socket.captureDistance=distance;
        return new PuzzlePlacement{item=item,socket=target,tolerance=distance,label=name};
    }
    [MenuItem("Report VR/Natural Learning/Configure All Levels")]
    public static void All()
    {
        naturalGuide.Clear();naturalGuide.Add("# 自然物理实验逐关攻略\n\n全部关卡通过直接操作模型与实测结果自动推进，状态灯不能替代实验。VR：握把拿取、转手/沿轨道拖动，挥手松开抛掷。桌面：WASD移动、鼠标转向；按住左键/E拿取，拿住机关后拖鼠标；拿住普通物件时按住右键/F蓄力，松开右键/F抛出。R重置。\n\n每关三步按顺序完成；全部通过后实体出口打开，走入出口进入下一关。自由跳关默认关闭。\n");
        FirstLevel();naturalGuide.Add("## 1. 材料与抛射\n\n分别拿起三件材料；将三件材料分别自由抛到金色落点区；把太阳模型拖至东方90°，看日晷影子向西。\n");
        for(int index=1;index<EditorBuildSettings.scenes.Length;index++)
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);level=Object.FindObjectOfType<DemoLevel>();key=Path.GetFileNameWithoutExtension(EditorBuildSettings.scenes[index].path);modelNumber=0;
            foreach(string name in new[]{"Natural Learning Layout","Puzzle Layout"}){var old=level.transform.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);}
            layout=Point(level.transform,"Natural Learning Layout",Vector3.zero);ConfigureReportPuzzles.BeginNatural(level,key,layout);
            foreach(var station in level.stations){station.action=DemoAction.Inspect;station.movingPart=null;station.secondaryPart=null;}
            foreach(var t in Object.FindObjectsOfType<Transform>(true))if(t.name.Contains("Trajectory") || t.name=="FourMirrorStaticOpticalPath" || t.name=="StaticAcousticWaveIllustration")t.gameObject.SetActive(false);
            naturalGuide.Add("\n## "+(index+1)+". "+level.title+"\n");
            switch(index)
            {
                case 1:case 10:Water();break;
                case 2:Acoustics();break;
                case 3:Compass(0);ObservationRing(1);Mirror(2);break;
                case 4:Electromagnetic();break;
                case 5:HydraulicRepair();break;
                case 6:Pendulum();break;
                case 7:ThrowChallenge(0,1.5f);ThrowChallenge(1,2.8f);Heat(2,false);break;
                case 8:Assembly(0);Domino(1);Mirror(2);break;
                case 9:PowerTask(0);Mirror(1);HydraulicTask(2);break;
                case 11:DialAlignment(0);ObservationRing(1);SunDial(2);break;
                case 12:SnowStack(0);Roll(1);Heat(2,true);break;
                case 13:Compass(0);PowerTask(1);ThrowChallenge(2,2.2f);break;
                case 14:Wheel(0);BridgeSupport(1);Mirror(2);break;
            }
            foreach(string oldSupply in new[]{"CanalMaterialWeightBench","AcousticMalletAndBronzeIngot","SnowballSupplyBasket"})Hide(oldSupply);
            foreach(var grab in Object.FindObjectsOfType<ReportGrabbable>())if(!grab.transform.IsChildOf(layout))
            {grab.enabled=false;var body=grab.GetComponent<Rigidbody>();body.isKinematic=true;body.useGravity=false;var oldLabel=grab.transform.Find("Puzzle Item Label");if(oldLabel!=null)Object.DestroyImmediate(oldLabel.gameObject);}
            level.mission="拿取、装配、转动与抛掷器材，观察结果；三个实验完成后沿步道进入出口。";
            if(index==2)ReportAcousticUpgrade.ApplyCurrentScene();
            if(index==3)ReportOrientationUpgrade.ApplyCurrentScene();
            ReportWaterUpgrade.ApplyCurrentScene();ReportFloatingStatus.ConfigureScene(level);
            if(index>=4)
            {
                ReportRemainingUpgrade.ApplyCurrentScene(index);
                naturalGuide.RemoveRange(naturalGuide.Count-3,3);
                foreach(var station in level.stations)
                    naturalGuide.Add("### "+(station.index+1)+". "+station.instruction+"\n\n目标："+station.puzzle.goal+"\n\n操作："+station.puzzle.hint+"\n\n物理关系："+station.puzzle.clue+"\n");
            }
            ConfigureReportPuzzles.EnsureExit();level.grabbables=Object.FindObjectsOfType<ReportGrabbable>().Where(g=>g.enabled).ToArray();level.Refresh();Physics.SyncTransforms();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();File.WriteAllLines("Documentation/逐关解谜攻略.md",naturalGuide);
        NaturalLearningCheck.Validate();PolishReportLevels.Validate();Debug.Log("NATURAL_ALL_CONFIGURED scenes=15 experiments=45");
    }
    static void Water()
    {
        var first=Task(0,"木载荷与浮力","将木配重装入船舱，观察25%水线","船体10kg，木20kg，最大排水量120kg；浸没比例=总质量/120。",key=="Fig_2_02_Buoyancy_Canal"?"把木块移到小木船船舱正上方并松手，观察下沉与后方大船同步变化。":"拿木配重放进木槽并松手，观察船壳白色25%刻线降至水面。");
        var second=Task(1,"调整目标水线","再装入石配重，使船达到50%水线","石30kg，木+石+船体=60kg；再装铜90kg会超过最大排水量。",key=="Fig_2_02_Buoyancy_Canal"?"再把石块移到船面上方并松手，观察金色50%水线。铜块只用于可选过载实验。":"把石配重装入石槽并松手，观察船再下沉10cm，金色50%刻线与水面齐平。");
        var third=Task(2,"转闸通航","保持50%水线并打开闸门，让船驶过闸口","只有载荷安全且闸门足够开时，船能通过。","握住闸轮转至全开并松手，等待船体移过闸口。");
        var root=Bench(0,"透明水槽与配重船");
        var tank=new LevelModelBuilder();tank.Box(new Vector3(0,.98f,0),new Vector3(1.1f,.025f,.7f),"GlacierBlue");Model(tank,"实验水面",root,false);
        var hullRoot=Point(root,"箱形船体",new Vector3(-.45f,1.04f,0));var m=new LevelModelBuilder();m.Box(Vector3.zero,new Vector3(.8f,.15f,.2f),"InkTeal");m.Box(Vector3.up*.08f,new Vector3(.82f,.01f,.22f),"SatinBrass");Model(m,"箱形船壳",hullRoot,false);
        var seats=new List<ReportCargoSeat>();float[] masses={20,30,90},sizes={.32f,.23f,.216f};string[] labels={"木20kg","石30kg","铜90kg"};string[] materials={"WarmWood","SageStone","Copper"};
        for(int i=0;i<3;i++)
        {
            string color=LevelModelBuilder.Mat(materials[i])!=null?materials[i]:"CoralRed";
            var load=Piece(root,labels[i],new Vector3((i-1)*.36f,.95f+sizes[i]*.5f,-.29f),Vector3.one*sizes[i],color);load.GetComponent<Rigidbody>().mass=masses[i];
            var seat=Point(hullRoot,"船舱_"+labels[i],new Vector3((i-1)*.26f,.085f+sizes[i]*.5f,0));var cargo=seat.gameObject.AddComponent<ReportCargoSeat>();cargo.item=load;cargo.captureDistance=.16f;seats.Add(cargo);
            Text(root,"配重标记"+i,new Vector3((i-1)*.36f,1.36f,-.3f),labels[i],.009f);
        }
        var gateRoot=Bench(2,"水闸操纵台");var gate=Knob(gateRoot,"闸轮",new Vector3(0,1.25f,-.15f),new Vector2(0,1),0,"开度");
        var boat=root.gameObject.AddComponent<ReportBuoyancyMeasure>();boat.hull=hullRoot;boat.seats=seats.ToArray();boat.gate=gate;boat.hullMass=10;boat.maximumVolume=.12f;boat.hullDepth=.4f;boat.waterHeight=root.position.y+.98f;boat.transitTravel=root.right*1.1f;boat.readout=Readout(root,"");boat.label="浸没比例";
        var transit=gateRoot.gameObject.AddComponent<ReportBoatTransitMeasure>();transit.boat=boat;transit.label="闸口通过";
        Conditions(first,Condition(boat,.24f,.26f));Conditions(second,Condition(boat,.49f,.51f));Conditions(third,Condition(transit,.98f,1));
        var channel=new LevelModelBuilder();channel.Box(new Vector3(.1f,.98f,0),new Vector3(1.95f,.025f,.7f),"GlacierBlue");foreach(float z in new[]{-.35f,.35f})channel.Beam(new Vector3(.15f,1,z),new Vector3(.15f,1.75f,z),.045f,"SatinBrass");Model(channel,"通航闸口",root,false);
        boat.gateLeaf=Point(root,"随闸轮升降闸板",new Vector3(.15f,1.13f,0));var leaf=new LevelModelBuilder();leaf.Box(Vector3.zero,new Vector3(.035f,.32f,.65f),"Celadon");Model(leaf,"闸板",boat.gateLeaf,false);
    }
    static void Acoustics()
    {
        var strike=Task(0,"握槌敲钟，观察余振","拿住木槌敲钟，听到钟声后松手","沿钟身法线的撞击速度至少0.35m/s；轻放、擦碰、投掷及其他物件不计。","VR握把拿住木槌并挥动；桌面按住左键/E拿住，移动视线使槌头撞钟。敲响后松手。");
        var root=Bench(0,"编钟试音台");var mallet=Piece(root,"试音钟槌",new Vector3(.42f,1.07f,-.2f),new Vector3(.12f,.25f,.12f),"CoralRed");
        var m=new LevelModelBuilder();m.Box(Vector3.zero,new Vector3(.32f,.42f,.3f),"SatinBrass");var bell=Model(m,"实际碰撞试验钟",root);bell.transform.localPosition=new Vector3(-.2f,1.34f,0);bell.GetComponent<MeshCollider>().convex=true;
        var body=bell.AddComponent<Rigidbody>();body.constraints=RigidbodyConstraints.FreezeAll;body.useGravity=false;
        var measure=bell.AddComponent<ReportAcousticMeasure>();measure.experiment=ReportAcousticReading.Strike;measure.mallet=mallet;measure.readout=Readout(root,"");measure.label="有效敲击";Conditions(strike,Condition(measure,1,100));
        AcousticFrequency(1,ReportAcousticReading.Resonance);AcousticFrequency(2,ReportAcousticReading.Beats);
    }
    static void AcousticFrequency(int step,ReportAcousticReading kind)
    {
        bool resonance=kind==ReportAcousticReading.Resonance;
        var task=Task(step,resonance?"调频寻找共振峰":"听拍频，微调至平稳",resonance?"相对峰值振幅达到94%，松手保持1秒":"拍频≤0.3Hz，松开两只旋钮保持1.5秒",resonance?"固有频率220Hz、阻尼比0.04，共振峰约219.65Hz；摆片为慢速示意。":"参考220Hz；可调频率=粗调+微调，拍频为频差。同频时变化停止。",resonance?"转调频轮，观察曲线与摆片，亮绿后松手。":"粗调先接近220Hz，再用±1Hz微调；听强弱变化、看同步光柱，亮绿后松手。");
        var root=Bench(step,resonance?"受迫振动实验":"双声源拍频实验");var h=Knob(root,"调频轮",new Vector3(.3f,1.2f,-.2f),new Vector2(75,125),80,"Hz");
        var body=new LevelModelBuilder();body.Box(new Vector3(-.3f,1.16f,0),new Vector3(.25f,.42f,.3f),"InkTeal");Model(body,"声源",root,false);
        var marker=Point(root,"振幅标记",new Vector3(-.3f,1.6f,0));var shape=new LevelModelBuilder();Gem(shape,Vector3.zero,.055f,"AmberLight");Model(shape,"振幅摆片",marker,false);
        var measure=root.gameObject.AddComponent<ReportAcousticMeasure>();measure.experiment=kind;measure.frequency=h;measure.vibrationMarker=marker;measure.readout=Readout(root,"");measure.label=resonance?"归一化振幅":"拍频";measure.unit=resonance?"":"Hz";
        Conditions(task,Condition(measure,resonance?.94f:0,resonance?1.01f:.3f));
    }
    static void Compass(int step)
    {
        var task=Task(step,"拨动司南定向","拨离南向至少20°，松手后回到5°内稳定1秒","简化磁力矩与阻尼使勺柄回摆并趋于南向。","握住勺柄拨开至少20°，再松手，观察回摆逐渐稳定；微小拨动不计。");
        var root=Bench(step,"地磁司南实验");var disc=new LevelModelBuilder();Ring(disc,new Vector3(0,1.1f,0),.38f,.04f,"SatinBrass");Model(disc,"方位盘",root,false);
        var needle=new LevelModelBuilder();needle.Beam(Vector3.zero,Vector3.forward*.3f,.055f,"SatinBrass");Ball(needle,Vector3.back*.1f,.085f,"SatinBrass");var go=Model(needle,"司南勺柄",root);go.transform.localPosition=Vector3.up*1.12f;
        var h=Handle(go,ReportHandleMotion.Rotation,Vector3.up,new Vector2(0,360),180);h.angleRange=new Vector2(0,360);h.vrDegreesForFullRange=360;h.unit="°";
        var measure=root.gameObject.AddComponent<ReportCompassMeasure>();measure.needle=h;measure.readout=Readout(root,"");measure.label="南向偏差";measure.unit="°";Conditions(task,Condition(measure,0,5));task.learningTask.stableDuration=1;
        Text(root,"南刻度",new Vector3(0,1.14f,-.4f),"南180°",.01f);
    }
    static void ObservationRing(int step)
    {
        var task=Task(step,"观测环对准星标","让红色照准线指向星标，偏差≤3°并松手稳定1秒","测量照准线与星标在观测环平面内的实际夹角；这是平面照准示教。","握住圆环沿圆周拖动，看红线靠近星标；读数达标后松手。");
        var root=Bench(step,"观测环实验");var ring=new LevelModelBuilder();Ring(ring,Vector3.zero,.35f,.03f,"SatinBrass",Quaternion.Euler(90,0,0));ring.Beam(Vector3.zero,Vector3.up*.32f,.02f,"CoralRed");var model=Model(ring,"活动观测环",root);model.transform.localPosition=Vector3.up*1.35f;
        var h=Handle(model,ReportHandleMotion.Rotation,Vector3.forward,new Vector2(-90,90),-45);h.angleRange=new Vector2(-90,90);h.vrDegreesForFullRange=180;h.rotationFromPosition=true;h.unit="°";
        var target=new LevelModelBuilder();Gem(target,new Vector3(-.25f,1.6f,.02f),.06f,"AmberLight");var star=Model(target,"45度星标",root,false);
        var measure=root.gameObject.AddComponent<ReportSightMeasure>();measure.ring=h;measure.target=star.GetComponent<Renderer>();measure.label="照准偏差";measure.unit="°";measure.readout=Readout(root,"");Conditions(task,Condition(measure,0,3));task.learningTask.stableDuration=1;
    }
    static void Mirror(int step)
    {
        var task=Task(step,"转镜接通光路","转动铜镜，让反射光束命中接收器","反射方向由入射方向与实际镜面法线计算，不是预画固定光路。","握住铜镜边框并旋转，沿实际光线找到接收器；亮绿后松手。");
        var root=Bench(step,"真实反射实验");var shape=new LevelModelBuilder();shape.Box(Vector3.zero,new Vector3(.42f,.42f,.035f),"SoftGlass");shape.Box(Vector3.zero,new Vector3(.46f,.46f,.018f),"SatinBrass");var go=Model(shape,"可转铜镜",root);go.transform.localPosition=Vector3.up*1.25f;
        var h=Handle(go,ReportHandleMotion.Rotation,Vector3.up,new Vector2(0,180),0);h.angleRange=new Vector2(0,180);h.vrDegreesForFullRange=180;h.unit="°";
        var emitter=Point(root,"光源",new Vector3(-.6f,1.25f,0));var bulb=new LevelModelBuilder();Gem(bulb,Vector3.zero,.055f,"AmberLight");Model(bulb,"光源灯",emitter,false);
        var sensor=new LevelModelBuilder();sensor.Box(Vector3.zero,new Vector3(.24f,.24f,.07f),"InkTeal");var receiver=Model(sensor,"反射接收器",root);receiver.transform.localPosition=new Vector3(0,1.25f,.62f);
        var line=Point(root,"实时入射与反射光束",Vector3.zero).gameObject.AddComponent<LineRenderer>();line.sharedMaterial=LevelModelBuilder.Mat("AmberLight");line.startWidth=line.endWidth=.015f;line.useWorldSpace=true;
        var measure=root.gameObject.AddComponent<ReportMirrorMeasure>();measure.mirror=h;measure.emitter=emitter;measure.receiver=receiver.GetComponent<Collider>();measure.receiverLamp=receiver.GetComponent<Renderer>();measure.beam=line;measure.readout=Readout(root,"");measure.label="接收光束";Conditions(task,Condition(measure,1,1));
    }
    static ReportElectromagnetMeasure PowerApparatus(int step)
    {
        var root=Bench(step,"线圈磁锁实验");var core=Piece(root,"铁磁芯",new Vector3(-.4f,1.15f,-.22f),new Vector3(.12f,.3f,.12f),"InkTeal");
        var seat=Seat(core,root,"铁芯安装孔",new Vector3(-.22f,1.18f,0));
        var coil=new LevelModelBuilder();for(int i=0;i<7;i++)Ring(coil,new Vector3(-.22f,1.04f+i*.04f,0),.135f,.018f,"SatinBrass");Model(coil,"绕组",root,false);
        var current=Knob(root,"电流滑杆",new Vector3(.38f,1.35f,-.12f),new Vector2(0,4),0,"A",true);
        var arm=Point(root,"吸合衔铁",new Vector3(-.22f,1.4f,0));var m=new LevelModelBuilder();m.Box(Vector3.zero,new Vector3(.25f,.065f,.22f),"InkTeal");Model(m,"衔铁",arm,false);
        var measure=root.gameObject.AddComponent<ReportElectromagnetMeasure>();measure.core=core;measure.coreSeat=seat.socket;measure.current=current;measure.armature=arm;measure.readout=Readout(root,"");measure.label="安全磁场";measure.unit="mT";return measure;
    }
    static void PowerTask(int step)
    {
        var task=Task(step,"装芯通电吸合","把铁芯插入绕组，并调电流使衔铁吸合","B=μ₀NI/L；400匝、0.5m，要求至少2mT且电流不超过3A。","拿铁芯装入安装孔并松手，拖电流滑杆，观察实际衔铁与磁场读数，达标后松手。");
        Conditions(task,Condition(PowerApparatus(step),2,3.02f));
    }
    static void Electromagnetic()
    {
        var a=Task(0,"装入铁磁芯","把铁芯安装到线圈中","铁磁材料可被磁化；模型安装是后续通电的必要条件。","拿工作台上的深色铁芯，插入绕组中央的安装孔并松手。");
        var apparatus=PowerApparatus(0);a.placements=new[]{new PuzzlePlacement{item=apparatus.core,socket=apparatus.coreSeat,tolerance=.22f,label="铁芯"}};
        var b=Task(1,"调电流吸合","调节电流，得到2mT以上磁场","B=μ₀NI/L，安全电流上限3A。","回到线圈台拖动电流滑杆，观察衔铁上升，达标后松手。");Conditions(b,Condition(apparatus,2,3.02f));
        var c=Task(2,"保持供电拉门栓","磁锁吸合时拉出门栓","失电或移走铁芯会使衔铁落下，并阻止拉栓。","保持铁芯与电流状态；抓住出口旁门栓沿滑轨拉到底并松手。");
        var root=Bench(2,"电磁门栓");var latch=Knob(root,"受磁锁约束门栓",new Vector3(0,1.25f,-.1f),new Vector2(0,1),0,"行程",true);apparatus.latch=latch;
        Conditions(c,Condition(HandleReading(latch,"门栓行程",""),.95f,1),Condition(apparatus,2,3.02f,0));
    }
    static ReportHydraulicMeasure HydraulicApparatus(int step,bool repair)
    {
        var root=Bench(step,"封闭液压实验");var inlet=Knob(root,"流路阀",new Vector3(-.4f,1.35f,-.13f),new Vector2(0,1),repair?1:0,"开度");
        var input=Knob(root,"小活塞握柄",new Vector3(.05f,1.5f,-.1f),new Vector2(0,.4f),0,"m 输入行程",true);
        var output=Point(root,"大活塞与负载",new Vector3(.4f,1.15f,0));var load=new LevelModelBuilder();load.Box(Vector3.zero,new Vector3(.3f,.12f,.3f),"InkTeal");Model(load,"液压负载200N",output,false);
        var pipe=new LevelModelBuilder();pipe.Beam(new Vector3(-.4f,1.05f,0),new Vector3(.4f,1.05f,0),.035f,"SatinBrass");pipe.Beam(new Vector3(.4f,1.05f,0),new Vector3(.4f,1.15f,0),.055f,"SatinBrass");Model(pipe,"封闭流路",root,false);
        var measure=root.gameObject.AddComponent<ReportHydraulicMeasure>();measure.inlet=inlet;measure.plunger=input;measure.output=output;measure.readout=Readout(root,"");measure.label="输出举升";measure.unit="m";
        if(repair)
        {
            var ring=new LevelModelBuilder();Ring(ring,Vector3.zero,.12f,.035f,"CoralRed");var go=Model(ring,"备用密封圈",root);go.transform.localPosition=new Vector3(.28f,1.02f,-.28f);
            measure.seal=ConfigureReportGrabs.Portable(go,Object.FindObjectOfType<XRInteractionManager>());measure.sealSeat=Seat(measure.seal,root,"漏水接口密封座",new Vector3(-.12f,1.05f,0),.16f).socket;
        }
        return measure;
    }
    static void HydraulicRepair()
    {
        var a=Task(0,"转阀隔离漏点","关闭进水阀，使漏水接口卸压","维修前先隔离压力源；开度为零时才可完成密封安装。","握住阀轮转至0并松手。");var apparatus=HydraulicApparatus(0,true);Conditions(a,Condition(HandleReading(apparatus.inlet,"阀开度",""),0,.02f));
        var b=Task(1,"安装接口密封圈","将备用圈装到接口上，同时保持关阀","接口密封后封闭液体才能传递压强。","回到液压台，拿红色密封圈放进接口密封座并松手，保持阀门关闭。");
        b.placements=new[]{new PuzzlePlacement{item=apparatus.seal,socket=apparatus.sealSeat,tolerance=.16f,label="密封圈"}};Conditions(b,Condition(HandleReading(apparatus.inlet,"卸压阀开度",""),0,.02f,0));
        var c=Task(2,"恢复压力并举升","开启流路，压小活塞使大活塞举升8cm以上","A₁=0.002，A₂=0.008m²；F₂=F₁A₂/A₁，A₁x₁=A₂x₂。","返回液压台开阀；握住小活塞柄向下压到底并松手，观察大活塞抬起载荷。");Conditions(c,Condition(apparatus,.08f,.101f));
    }
    static void HydraulicTask(int step)
    {
        var task=Task(step,"液压举重实验","开阀并压下小活塞，使载荷举升8cm以上","封闭液体等量传递附加压强；力放大同时行程缩小，满足体积守恒。","转阀至全开，抓小活塞柄向下压到底，观察大活塞举重后松手。");Conditions(task,Condition(HydraulicApparatus(step,false),.08f,.101f));
    }
    static void Pendulum()
    {
        var a=Task(0,"拉摆测周期","拉开摆球并松手，记录至少一次完整周期","θ''=-(g/L)sinθ；周期由过零时刻测量。","握住摆球沿圆弧拉开15至25°，松手等待摆动，测量台会记录周期。");
        var b=Task(1,"调整摆长","改变摆长至1m，再拉摆测得约2s周期","小角度T=2π√(L/g)；摆长会同时改变模型长度与重力响应。","拖动摆长滑杆至1m，再拉开摆球并松手；等新的实测周期接近2s。");
        var c=Task(2,"旋电容匹配频率","调LC电容，让频率与摆实测频率相差小于3%","fLC=1/(2π√LC)；与当前实测1/T比较。","到LC台旋转电容调节轮，读取频差并调至3%以下，松手。");
        var root=Bench(0,"可变摆长单摆");var anchor=Point(root,"摆支点",new Vector3(0,2.45f,.08f));
        var rod=new LevelModelBuilder();rod.Beam(Vector3.zero,Vector3.down,.018f,"SatinBrass");var swing=Model(rod,"可拉摆杆",anchor);var angle=Handle(swing,ReportHandleMotion.Rotation,Vector3.forward,new Vector2(-35,35),0);angle.angleRange=new Vector2(-35,35);angle.rotationFromPosition=true;angle.vrDegreesForFullRange=70;angle.unit="°";
        var bob=Point(swing.transform,"摆球",Vector3.down*1.3f);var ball=new LevelModelBuilder();Ball(ball,Vector3.zero,.12f,"CoralRed");var bg=Model(ball,"摆球握点",bob);bg.GetComponent<MeshCollider>().convex=true;angle.colliders.Add(bg.GetComponent<Collider>());
        var length=Knob(root,"摆长调节滑杆",new Vector3(.4f,1.3f,-.15f),new Vector2(.6f,1.4f),1.3f,"m",true);
        var measure=root.gameObject.AddComponent<ReportPendulumMeasure>();measure.angle=angle;measure.length=length;measure.bob=bob;measure.rod=Point(swing.transform,"摆杆视觉缩放",Vector3.zero);swing.GetComponent<MeshRenderer>().enabled=false;
        var visual=new LevelModelBuilder();visual.Beam(Vector3.zero,Vector3.down,.018f,"SatinBrass");Model(visual,"随摆长伸缩杆",measure.rod,false);measure.readout=Readout(root,"");measure.label="实测周期";measure.unit="s";
        Conditions(a,Condition(measure,1,3));Conditions(b,Condition(measure,1.94f,2.1f),Condition(HandleReading(length,"摆长","m"),.98f,1.02f));
        var lcRoot=Bench(2,"LC耦合实验");var capacitor=Knob(lcRoot,"电容片调节轮",new Vector3(0,1.25f,-.1f),new Vector2(.05f,.2f),.05f,"F");var lc=lcRoot.gameObject.AddComponent<ReportLCMeasure>();lc.capacitance=capacitor;lc.pendulum=measure;lc.readout=Readout(lcRoot,"");lc.label="相对频差";Conditions(c,Condition(lc,0,.03f));
    }
    static void ThrowChallenge(int step,float distance)
    {
        var task=Task(step,"真实手抛命中实验","从发射台附近抛球，命中落点区","飞行轨迹由刚体重力产生；记录真实初速、角度、飞行时间与水平距离。","拿台上的球；VR挥手松握把，桌面按住右键/F蓄力后松开。必须从台边抛出，不能走近把球放进靶区。");
        var root=Bench(step,"自由手抛实验");var ball=Piece(root,"练习球",new Vector3(0,1.12f,0),Vector3.one*.28f,"Ivory",true);ball.maximumReleaseSpeed=10;
        var launch=Point(root,"有效出手区域",new Vector3(0,1.3f,-.4f));var targetPos=root.position-root.forward*distance;targetPos.y=root.position.y+.03f;
        // Target is a floor marking, never a solid obstacle on the walking route.
        var pad=new LevelModelBuilder();Ring(pad,Vector3.zero,.55f,.025f,"SatinBrass");var go=Model(pad,"真实落点靶",layout,false);go.transform.position=targetPos;
        var trigger=go.AddComponent<BoxCollider>();trigger.isTrigger=true;trigger.center=Vector3.up*.2f;trigger.size=new Vector3(1.2f,.5f,1.2f);
        var detector=go.AddComponent<ReportThrowTarget>();detector.acceptedItems=new[]{ball.GetComponent<ReportThrowTrack>()};detector.launchArea=launch;detector.launchRadius=1.8f;detector.minimumFlightDistance=distance*.65f;detector.minimumFlightTime=.2f;detector.readout=Readout(root,"");
        var measure=go.AddComponent<ReportThrowMeasure>();measure.target=detector;measure.label="有效命中";Conditions(task,Condition(measure,1,100));
    }
    static void Heat(int step,bool insulated)
    {
        var task=Task(step,insulated?"隔热罩热传递实验":"暖区融化实验",insulated?"把冰样放进暖区并罩住，观察隔热5秒":"把冰样搬入暖区，观察融化超过25%","初始0°C冰吸热融化，Q=mL；隔热罩降低传热系数，展示采用缩时参数。",insulated?"拿冰样放到红色暖区，再把隔热罩放到样品上方的罩座；松手并观察至少5秒。":"拿冰样放到红色暖区并松手，等待模型缩小与融化读数增加。");
        var root=Bench(step,"热量传递对照台");var sample=Piece(root,"0度冰样",new Vector3(-.38f,1.07f,-.08f),Vector3.one*.25f,"GlacierBlue",true);
        var seat=Seat(sample,root,"20度暖区",new Vector3(.25f,1.07f,0),.22f);var zone=new LevelModelBuilder();zone.Box(new Vector3(.25f,.95f,0),new Vector3(.5f,.04f,.55f),"CoralRed");Model(zone,"暖区标记",root,false);
        var measure=root.gameObject.AddComponent<ReportHeatMeasure>();measure.sample=sample;measure.warmZone=seat.socket;measure.insulationTask=insulated;measure.readout=Readout(root,"");measure.label=insulated?"隔热观察时间":"融化比例";measure.unit=insulated?"s":"";
        if(insulated){var cover=Piece(root,"隔热罩",new Vector3(-.25f,1.27f,.23f),new Vector3(.37f,.08f,.37f),"Celadon");measure.insulation=cover;measure.coverSeat=Seat(cover,root,"隔热罩座",new Vector3(.25f,1.29f,0)).socket;}
        Conditions(task,Condition(measure,insulated?5:.25f,insulated?200:1));
    }
    static void Assembly(int step)
    {
        var task=Task(step,"模块装配","把梁与连接块装成实验架","装配槽用于固定结构连接，不是选择答案。","拿两端连接块和中间梁，按形状插入对应连接槽并松手。");var root=Bench(step,"模块装配台");var placements=new List<PuzzlePlacement>();
        for(int i=0;i<3;i++){var size=i==1?new Vector3(.55f,.12f,.16f):new Vector3(.16f,.3f,.16f);var item=Piece(root,i==1?"横梁":"连接块"+i,new Vector3((i-1)*.35f,.96f+size.y*.5f,-.25f),size,i==1?"SatinBrass":"InkTeal");placements.Add(Seat(item,root,"结构连接"+i,new Vector3((i-1)*.35f,1.05f+(i==1?.23f:0),.2f),.14f));}
        task.placements=placements.ToArray();
    }
    static void Domino(int step)
    {
        var task=Task(step,"真实骨牌连锁","推动或抛球击中首块，使六块骨牌全部倒下","骨牌用刚体碰撞传递运动，不按时间播放倾倒动画。","取台上的球击中第一块，或拿住球推倒首块，观察碰撞链。");var root=Bench(step,"骨牌碰撞台");var dominoes=new List<Rigidbody>();
        for(int i=0;i<6;i++){var m=new LevelModelBuilder();m.Box(Vector3.zero,new Vector3(.045f,.36f,.18f),i%2==0?"CoralRed":"Celadon");var go=Model(m,"骨牌"+i,root);go.transform.localPosition=new Vector3(-.375f+i*.15f,1.125f,0);go.GetComponent<MeshCollider>().convex=true;var body=go.AddComponent<Rigidbody>();body.mass=.12f;body.collisionDetectionMode=CollisionDetectionMode.Continuous;dominoes.Add(body);}
        var rails=new LevelModelBuilder();foreach(float z in new[]{-.125f,.125f})rails.Box(new Vector3(0,.97f,z),new Vector3(1.1f,.06f,.025f),"SatinBrass");Model(rails,"骨牌防侧滑导轨",root);
        Piece(root,"击牌球",new Vector3(-.4f,1.07f,-.3f),Vector3.one*.24f,"SatinBrass",true);
        var measure=root.gameObject.AddComponent<ReportDominoMeasure>();measure.dominoes=dominoes.ToArray();measure.readout=Readout(root,"");measure.label="倒下骨牌";Conditions(task,Condition(measure,6,6));
    }
    static void DialAlignment(int step)
    {
        var task=Task(step,"对齐晷盘南北","转动晷盘，使晷针与固定南北线重合","日晷安装必须先校准地理方向；这里只完成方位校准。","握住晷盘边缘转动，使晷针朝盘外金色南北线，松手。");var root=Bench(step,"日晷方位校准台");var m=new LevelModelBuilder();Ring(m,Vector3.zero,.35f,.035f,"SatinBrass");m.Beam(Vector3.zero,new Vector3(0,.35f,.25f),.035f,"InkTeal");var go=Model(m,"晷盘与晷针",root);go.transform.localPosition=Vector3.up*1.1f;
        var h=Handle(go,ReportHandleMotion.Rotation,Vector3.up,new Vector2(-90,90),60);h.angleRange=new Vector2(-90,90);h.rotationFromPosition=true;h.vrDegreesForFullRange=180;
        var line=new LevelModelBuilder();line.Beam(new Vector3(0,1.1f,-.42f),new Vector3(0,1.1f,.42f),.014f,"AmberLight");Model(line,"固定南北线",root,false);Conditions(task,Condition(HandleReading(h,"方位误差","°"),-3,3));
    }
    static void SunDial(int step)
    {
        var task=Task(step,"拖太阳观测影向","将太阳转到西南225°，观察晷影移向东北刻线","此水平针影实验比较太阳方位与影向；225°方位产生45°东北影向，并非精确纬度校准时钟。","握金色太阳沿轨道拖到西南225°附近；真实Directional Light会改变日晷阴影，观察东北刻线后松手。");BuildSun(task);var measure=Object.FindObjectOfType<ReportSunlightMeasure>();Conditions(task,Condition(measure,222,228));
    }
    static void SnowStack(int step)
    {
        var task=Task(step,"稳定搭建雪人","按大中小顺序搭三雪球并让它们实际稳定","压实雪层平整面接触后黏结，使用可断裂固定连接近似；支撑面和重心决定叠放稳定。","先提起大雪球放到台中央，再依次放中球和小球；压实面朝下，接触并松手后黏结，不会自动吸到位置。");var root=Bench(step,"雪人重心实验");var balls=new List<ReportGrabbable>();var targets=new List<Transform>();float[] sizes={.38f,.29f,.2f};float y=.94f;
        for(int i=0;i<3;i++)
        {
            float height=sizes[i]*.78f;y+=height*.5f;targets.Add(Point(root,"雪人层"+i,new Vector3(0,y,.1f)));
            var model=new LevelModelBuilder();Ball(model,Vector3.zero,sizes[i]*.5f,"SnowIvory",.78f);var go=Model(model,"雪球层"+i,root);go.transform.localPosition=new Vector3((i-1)*.4f,.95f+height*.5f,-.27f);
            var ball=ConfigureReportGrabs.Portable(go,Object.FindObjectOfType<XRInteractionManager>());
            Object.DestroyImmediate(go.GetComponent<MeshCollider>());var contact=go.AddComponent<BoxCollider>();contact.size=new Vector3(sizes[i]*.6f,height,sizes[i]*.6f);ball.colliders.Clear();ball.colliders.Add(contact);
            ball.maximumReleaseSpeed=2;ball.GetComponent<Rigidbody>().mass=new[]{2.5f,1.2f,.4f}[i];ball.GetComponent<Rigidbody>().maxDepenetrationVelocity=1;ball.GetComponent<Rigidbody>().solverIterations=12;balls.Add(ball);y+=height*.5f;
        }
        // Compressed flat snow surfaces are represented by the actual convex colliders.
        var joints=new LevelModelBuilder();Ring(joints,new Vector3(0,.945f,.1f),.18f,.012f,"Celadon");Model(joints,"雪人基础标记",root,false);
        ReportSnowBond previous=null;
        for(int i=0;i<3;i++)
        {
            var bond=balls[i].gameObject.AddComponent<ReportSnowBond>();bond.piece=balls[i];bond.assemblyPoint=targets[i];bond.support=i==0?root.GetComponentsInChildren<MeshCollider>().First(c=>c.name.Contains("台架")):balls[i-1].GetComponent<Collider>();bond.supportBond=previous;previous=bond;
        }
        var measure=root.gameObject.AddComponent<ReportStackMeasure>();measure.pieces=balls.ToArray();measure.positions=targets.ToArray();measure.tolerance=.14f;measure.readout=Readout(root,"");measure.label="稳定雪人层数";Conditions(task,Condition(measure,3,3));task.learningTask.stableDuration=2;
    }
    static void Roll(int step)
    {
        var task=Task(step,"斜坡滚动","将球放在坡顶，松手让它滚入坡底接收区","重力沿坡分量使球运动，滚球接收检测真实到达。","拿滚球放到红色坡顶标记后松手，等待它沿斜坡滚到金色接收区。");var root=Bench(step,"斜坡滚球实验");
        var ramp=new LevelModelBuilder();ramp.Ramp(new Vector3(-.45f,1.38f,0),new Vector3(.45f,.97f,0),.38f,"InkTeal");Model(ramp,"真实坡面",root);
        var ball=Piece(root,"坡道滚球",new Vector3(-.35f,1.06f,-.3f),Vector3.one*.2f,"Ivory",true);var start=Point(root,"坡顶放球标记",new Vector3(-.38f,1.45f,0));
        var trigger=Point(root,"坡底接收",new Vector3(.48f,1.1f,0));var box=trigger.gameObject.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(.28f,.4f,.48f);
        var measure=trigger.gameObject.AddComponent<ReportRollMeasure>();measure.ball=ball;measure.rampStart=start;measure.minimumDistance=.65f;measure.readout=Readout(root,"");measure.label="滚动到达";Conditions(task,Condition(measure,1,1));
        var marks=new LevelModelBuilder();Ring(marks,start.localPosition,.12f,.016f,"CoralRed");Ring(marks,trigger.localPosition-Vector3.up*.13f,.15f,.016f,"SatinBrass");Model(marks,"坡道标记",root,false);
    }
    static void Wheel(int step)
    {
        var task=Task(step,"转水工轮提闸","直接转动水工轮，提起闸板至少0.6m","无滑动绳鼓的位移h=rθ，做功W=mgh。","握住轮辐转动，观察绳鼓与闸板位移，达到0.6m后松手。");var root=Bench(step,"绳鼓水工轮");var h=Knob(root,"水工轮",new Vector3(-.3f,1.25f,0),new Vector2(0,270),0,"°");
        var gate=Point(root,"提升闸板",new Vector3(.35f,1.1f,0));var m=new LevelModelBuilder();m.Box(Vector3.zero,new Vector3(.3f,.3f,.05f),"InkTeal");Model(m,"绳鼓负载",gate,false);
        var measure=root.gameObject.AddComponent<ReportWheelLiftMeasure>();measure.wheel=h;measure.gate=gate;measure.readout=Readout(root,"");measure.label="闸板升高";measure.unit="m";Conditions(task,Condition(measure,.6f,.72f));
    }
    static void BridgeSupport(int step)
    {
        var task=Task(step,"桥梁支撑装配","补齐中部支撑，再放桥面和载荷使其稳定","模型通过真实重力与接触支撑；这是桥梁支撑实验台，不代表完整拱桥有限元分析。","将青色支撑块放到中央，再放横向桥面板，最后放红色载荷；松手等待稳定。");var root=Bench(step,"桥梁支撑实验",1.65f,1.7f);
        Vector3[] sizes={new Vector3(.25f,.22f,.3f),new Vector3(.8f,.08f,.32f),Vector3.one*.16f};Vector3[] targets={new Vector3(0,1.05f,.12f),new Vector3(0,1.2f,.12f),new Vector3(0,1.32f,.12f)};var pieces=new List<ReportGrabbable>();var marks=new List<Transform>();
        Vector3[] supplies={new Vector3(-.62f,1.06f,.22f),new Vector3(0,.99f,-.62f),new Vector3(.67f,1.03f,.22f)};
        for(int i=0;i<3;i++){pieces.Add(Piece(root,new[]{"支撑块","桥面板","试验载荷"}[i],supplies[i],sizes[i],new[]{"Celadon","SatinBrass","CoralRed"}[i]));marks.Add(Point(root,"结构目标"+i,targets[i]));}
        var measure=root.gameObject.AddComponent<ReportStackMeasure>();measure.pieces=pieces.ToArray();measure.positions=marks.ToArray();measure.tolerance=.13f;measure.readout=Readout(root,"");measure.label="稳定结构件";Conditions(task,Condition(measure,3,3));task.learningTask.stableDuration=2;
    }
}
