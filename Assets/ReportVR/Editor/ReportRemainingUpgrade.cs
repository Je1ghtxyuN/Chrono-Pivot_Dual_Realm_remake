using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Upgrade the existing experiments in order without regenerating their surrounding scenes.
public static class ReportRemainingUpgrade
{
    static string key;
    static DemoLevel level;
    static Transform Group(Transform parent,string name,Vector3 position)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;return go.transform;}
    static Transform Feedback(Transform root)
    {var old=root.Find("Later level feedback");if(old!=null)Object.DestroyImmediate(old.gameObject);return Group(root,"Later level feedback",Vector3.zero);}
    static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,string material)
    {var m=new LevelModelBuilder();m.Box(Vector3.zero,size,material);var go=m.Save(key+name,parent,false);go.transform.localPosition=position;return go;}
    static LineRenderer Line(Transform parent,string name,string material,float width,bool world=true)
    {var line=Group(parent,name,Vector3.zero).gameObject.AddComponent<LineRenderer>();line.sharedMaterial=LevelModelBuilder.Mat(material);line.useWorldSpace=world;line.startWidth=line.endWidth=width;return line;}
    static TextMesh Label(Transform parent,string name,Vector3 position,string words,float size=.008f)
    {
        var text=Group(parent,name,position).gameObject.AddComponent<TextMesh>();text.text=words;text.characterSize=size;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.96f,.93f,.77f);
        text.GetComponent<Renderer>().sharedMaterial=Object.FindObjectOfType<DemoInteractor>().worldText;
        var facing=text.gameObject.AddComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.FacePlayer;facing.yawOnly=true;facing.viewer=Object.FindObjectOfType<DesktopPlayer>().head.transform;return text;
    }
    static TextMesh Reading(Transform root,string name)
    {var old=root.Find(name);return old!=null?old.GetComponent<TextMesh>():Label(root,name,Vector3.zero,"");}
    static void Panel(Transform feedback,TextMesh text,DemoStation station,string name,Vector3? center=null)
    {
        var group=Group(feedback,name,center??new Vector3(0,2.02f,-.13f));
        group.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(feedback.parent.position-station.viewingPoint,Vector3.up));
        Box(group,name+"_back",Vector3.zero,new Vector3(1.45f,.60f,.025f),"InkTeal");
        text.transform.SetPositionAndRotation(group.position-group.forward*.018f,group.rotation);text.characterSize=.01f;text.color=new Color(.96f,.93f,.77f);
        var facing=text.GetComponent<ReportTextFacing>();if(facing!=null)facing.mode=ReportTextFacingMode.WorldFixed;
    }
    static void Status(Transform group,int step,Vector3 position)
    {
        var node=Group(group,"Step "+step+" feedback",position);var feedback=node.gameObject.AddComponent<ReportTaskFeedback>();feedback.station=level.stations[step];
        feedback.lamp=Box(node,"status_"+step,new Vector3(-.24f,0,0),Vector3.one*.055f,"Ivory").GetComponent<Renderer>();
        Box(node,"progress_track_"+step,Vector3.zero,new Vector3(.40f,.024f,.014f),"DarkGraphite");
        feedback.progress=Box(node,"progress_"+step,new Vector3(0,0,-.012f),new Vector3(.40f,.018f,.014f),"CyanLight").transform;
        Label(node,"Release and observe",Vector3.up*.09f,"步骤"+(step+1)+" · 稳定进度",.007f);
        level.stations[step].label.characterSize=.011f;
    }
    static void Task(int step,string title,string goal,string clue,string hint)
    {var station=level.stations[step];station.instruction=title;station.puzzle.goal=goal;station.puzzle.clue=clue;station.puzzle.hint=hint;station.puzzle.successMessage="实验完成："+title;}
    static void Hide(ReportMechanismHandle handle){if(handle.readout!=null)handle.readout.GetComponent<Renderer>().enabled=false;}
    static void Ring(Transform parent,string name,Vector3 center,float radius,string material)
    {
        var m=new LevelModelBuilder();for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;m.Beam(center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius,center+new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*radius,.012f,material);}m.Save(key+name,parent,false);
    }
    static void ReplaceMesh(GameObject target,string name,LevelModelBuilder model)
    {
        var generated=model.Save(key+name,target.transform.parent,false);var mesh=generated.GetComponent<MeshFilter>().sharedMesh;
        target.GetComponent<MeshFilter>().sharedMesh=mesh;target.GetComponent<Renderer>().sharedMaterials=generated.GetComponent<Renderer>().sharedMaterials;
        var collider=target.GetComponent<MeshCollider>();if(collider!=null)collider.sharedMesh=mesh;Object.DestroyImmediate(generated);
    }
    static void Power(ReportElectromagnetMeasure power,int step)
    {
        Task(step,"装芯安全供电并吸合","铁芯入座后调电流至2～3A，等衔铁实际闭合","B₀=μ₀NI/L是理想空芯激励参考，非铁芯实际磁场；超过3A保护锁止。","铁芯放入青色孔位并松手，拖电流滑杆到绿色区，松手等待向下吸合。");
        var group=Feedback(power.transform);power.armature.localPosition=new Vector3(-.22f,1.5f,0);power.armatureTravel=.13f;
        power.coreSeat.GetComponent<ReportPlacementSocket>().alignRotation=true;power.label="安全线圈激励 B₀";
        Panel(group,power.readout,level.stations[power.latch!=null?0:step],"power_panel");Hide(power.current);
        Box(group,"current_track",new Vector3(.53f,1.15f,-.13f),new Vector3(.035f,.4f,.035f),"DarkGraphite");
        Box(group,"safe_current_band",new Vector3(.53f,1.10f,-.16f),new Vector3(.05f,.10f,.01f),"JadeLeaf");
        Label(group,"Safe current",new Vector3(.64f,1.13f,-.17f),"2～3A\n安全区",.008f);
        Ring(group,"core_seat",new Vector3(-.22f,1.035f,0),.10f,"CyanLight");Label(group,"Core socket",new Vector3(-.22f,1.02f,-.22f),"铁芯入座",.007f);
        Box(group,"armature_guide",new Vector3(-.22f,1.49f,.15f),new Vector3(.04f,.38f,.035f),"SatinBrass");
        Status(group,step,new Vector3(power.latch!=null?.30f:0,1.65f,-.18f));
        if(power.latch!=null)
        {
            Status(group,0,new Vector3(-.30f,1.65f,-.18f));
            power.latch.localAxis=Vector3.right;power.latch.slideRange=new Vector2(0,.3f);power.latch.vrTravelForFullRange=.3f;
            var latch=Feedback(power.latch.transform.parent);Box(latch,"latch_rail",new Vector3(.15f,1.25f,-.02f),new Vector3(.65f,.035f,.035f),"SatinBrass");
            Label(latch,"Latch operation",new Vector3(.10f,1.85f,-.13f),"保持供电、等衔铁闭合\n握门栓沿水平导轨向右拉到底\n断电 / 过流 / 取芯会重新锁住",.011f);Status(latch,2,new Vector3(.15f,1.66f,-.13f));
        }
    }
    static void Hydraulic(ReportHydraulicMeasure hydraulic,int step)
    {
        Task(step,"连通流路并举升","开阀并压小活塞32～40cm，使载荷举升≥8cm","三通阀0位隔离并泄压；连通后p=F₁/A₁，A₁x₁=A₂x₂。","开阀，向下压小活塞，观察输出8～10cm后松手。");
        var group=Feedback(hydraulic.transform);Panel(group,hydraulic.readout,level.stations[hydraulic.seal!=null?0:step],"hydraulic_panel");Hide(hydraulic.inlet);Hide(hydraulic.plunger);
        Label(group,"Valve meaning",new Vector3(-.42f,1.64f,-.18f),"0：隔离并泄压\n开：连通加压",.008f);
        Label(group,"Input stroke",new Vector3(.02f,1.05f,-.28f),"小活塞 0～40cm",.008f);Label(group,"Output stroke",new Vector3(.47f,1.04f,-.23f),"大活塞 0～10cm",.008f);
        for(int i=0;i<=5;i++)Box(group,"output_tick_"+i,new Vector3(.59f,1.15f+i*.02f,0),new Vector3(.065f,.004f,.012f),i==4?"CoralRed":"Ivory");
        Box(group,"input_cylinder",new Vector3(.05f,1.04f,-.1f),new Vector3(.12f,.16f,.12f),"SoftGlass");Status(group,step,new Vector3(hydraulic.seal!=null?.52f:0,1.44f,-.32f));
        if(hydraulic.seal!=null)
        {
            Status(group,0,new Vector3(-.52f,1.44f,-.32f));Status(group,1,new Vector3(0,1.44f,-.32f));
            var socket=hydraulic.sealSeat.GetComponent<ReportPlacementSocket>();socket.requiredClosedValve=hydraulic.inlet;socket.alignRotation=true;
            Ring(group,"seal_mark",hydraulic.sealSeat.localPosition,.13f,"CyanLight");
        }
    }
    static void Pendulum()
    {
        Task(0,"拉摆测周期","拉开15～25°后松手，测得完整周期","同向过零计时；小于10°的拨动不记录周期。","握摆球沿圆弧拉开，松手等周期屏出现读数。");
        Task(1,"改长后重新测量","调摆长至1m，重新拉摆测约2s周期","改长会清除旧周期；T≈2π√(L/g)。","拖摆长至1.0m并松手，再拉摆15～25°，等待新的实测周期。");
        Task(2,"匹配LC与摆频率","调电容使LC频率与当前摆实测频率相差≤3%","fLC=1/(2π√LC)；双波形比较周期，初相位不参与通关。","观察青色摆波与金色LC波，调电容至周期接近，松手。");
        var pendulum=Object.FindObjectOfType<ReportPendulumMeasure>();var group=Feedback(pendulum.transform);Hide(pendulum.length);
        Panel(group,pendulum.readout,level.stations[0],"pendulum_panel",new Vector3(-1.35f,1.93f,-.13f));
        Box(group,"pendulum_frame",new Vector3(0,2.49f,.16f),new Vector3(.84f,.045f,.045f),"SatinBrass");
        foreach(float x in new[]{-.40f,.40f})Box(group,"pendulum_post_"+x,new Vector3(x,1.72f,.18f),new Vector3(.035f,1.54f,.035f),"SatinBrass");
        Label(group,"Length control",new Vector3(.48f,1.62f,-.18f),"摆长0.6～1.4m\n第二步调到1.0m",.008f);
        Status(group,0,new Vector3(-1.35f,1.52f,-.18f));Status(group,1,new Vector3(-1.35f,1.30f,-.18f));
        var lc=Object.FindObjectOfType<ReportLCMeasure>();var lcGroup=Feedback(lc.transform);Hide(lc.capacitance);lc.capacitance.transform.localPosition=new Vector3(.65f,1.2f,-.30f);
        Panel(lcGroup,lc.readout,level.stations[2],"lc_panel");Box(lcGroup,"wave_back",new Vector3(0,1.28f,-.19f),new Vector3(1.05f,.58f,.025f),"InkTeal");
        lc.pendulumWave=Line(lcGroup,"Measured pendulum wave","CyanLight",.012f,false);lc.lcWave=Line(lcGroup,"LC wave","AmberLight",.012f,false);
        Label(lcGroup,"Wave legend",new Vector3(0,1.65f,-.23f),"金色：LC　青色：摆实测\n相同周期，频率接近",.008f);Status(lcGroup,2,new Vector3(0,.96f,-.32f));
    }
    static void Throw(ReportThrowTarget target,int step)
    {
        Task(step,"圈内抛射命中","在青色出手圈内抛球，命中本步骤的金色落点区","真实刚体飞行；近放、圈外出手及提前命中后续靶均不计。","拿本台球，从青色圈内挥手松握把；桌面右键/F蓄力释放。");
        target.requiredStation=level.stations[step];target.launchRadius=.85f;target.acceptedSpeed=new Vector2(.8f,10);
        var root=target.readout.transform.parent;var group=Feedback(root);Panel(group,target.readout,level.stations[step],"throw_panel");Status(group,step,new Vector3(0,1.63f,-.13f));
        var center=root.InverseTransformPoint(target.launchArea.position);center.y=.02f;Ring(group,"launch_boundary",center,.85f,"CyanLight");
        Label(group,"Launch area",new Vector3(0,1.02f,-.48f),"青色圈内出手 · 金色落点区\n挥手松握把；桌面右键 / F蓄力释放",.008f);
        target.ResetMeasurements();
    }
    static void Heat(ReportHeatMeasure heat,int step)
    {
        Task(step,heat.insulationTask?"连续隔热观察":"暖区融化观察",heat.insulationTask?"冰样在暖区、罩住并松手，连续观察5秒":"冰样放入暖区并松手，融化超过25%","0°C冰吸热融化Q=mL；采用缩时示教，罩内热流为裸露的8%。",heat.insulationTask?"冰样入暖区，罩放罩座；移罩或移样会重计连续时间。":"冰样放红色暖区，观察体积变小、融水增多。");
        var group=Feedback(heat.transform);Panel(group,heat.readout,level.stations[step],"heat_panel");Status(group,step,new Vector3(0,1.63f,-.15f));
        var water=Box(group,"melt_water",new Vector3(.25f,.974f,0),new Vector3(.36f,.006f,.36f),"JadeWater");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReportVR/Water/"+Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path)+"_Liquid.mat");if(material!=null)water.GetComponent<Renderer>().sharedMaterial=material;
        heat.meltWater=water.transform;water.transform.localScale=new Vector3(0,1,0);
        Label(group,"Heat comparison",new Vector3(0,1.03f,-.32f),heat.insulationTask?"把冰放在暖区，再罩住\n连续观察5秒，取罩会重计":"冰样入暖区，融化超过25%\n观察冰缩小、融水增加",.008f);
    }
    static void Mirror(int step)
    {
        Task(step,"转镜接通光路","让实际反射光到达接收器，亮绿后松手","入射角与反射角从青色法线起算；两段光路都需畅通。","握铜镜边框转动，看光线与法线；接收器亮绿后松手等稳定。");
        var mirror=Object.FindObjectOfType<ReportMirrorMeasure>();var group=Feedback(mirror.transform);Panel(group,mirror.readout,level.stations[step],"mirror_panel");Status(group,step,new Vector3(0,1.65f,-.13f));
        mirror.normalLine=Line(group,"Actual mirror normal","CyanLight",.008f);
        mirror.mirror.transform.localPosition=Vector3.up*1.4f;mirror.emitter.localPosition=new Vector3(-.6f,1.4f,0);mirror.receiver.transform.localPosition=new Vector3(0,1.4f,.62f);
        var shape=new LevelModelBuilder();shape.Box(Vector3.zero,new Vector3(.40f,.40f,.025f),"Copper");
        foreach(float sign in new[]{-1f,1f}){shape.Box(new Vector3(sign*.22f,0,0),new Vector3(.04f,.48f,.04f),"SatinBrass");shape.Box(new Vector3(0,sign*.22f,0),new Vector3(.40f,.04f,.04f),"SatinBrass");}ReplaceMesh(mirror.mirror.gameObject,"mirror_frame",shape);
    }
    static void Workshop()
    {
        Task(0,"连接件装配","将两侧连接块及横梁装入对应青色座","导向座固定并对齐连接件；承重实验在第十五关。","先两侧连接块，再横梁，对准座位并松手。");
        var placements=level.stations[0].puzzle.placements;var root=placements[0].socket.parent;var group=Feedback(root);
        foreach(var placement in placements){placement.socket.GetComponent<ReportPlacementSocket>().alignRotation=true;Ring(group,"socket_"+placement.label,placement.socket.localPosition-Vector3.up*.12f,.08f,"CyanLight");}
        Label(group,"Assembly guide",new Vector3(0,1.93f,-.13f),"先放两侧连接块，再放横梁\n青色连接座会固定并对齐零件\n这是连接装配；桥梁承重在最后一关",.011f);Status(group,0,new Vector3(0,1.61f,-.13f));
        var domino=Object.FindObjectOfType<ReportDominoMeasure>();var chain=Feedback(domino.transform);Panel(chain,domino.readout,level.stations[1],"domino_panel");Status(chain,1,new Vector3(0,1.64f,-.13f));
        for(int i=0;i<domino.dominoes.Length;i++)Label(chain,"Domino number "+i,new Vector3(-.375f+i*.15f,.952f,-.17f),(i+1).ToString(),.006f);
        Label(chain,"Chain direction",new Vector3(0,1.03f,-.30f),"从1向6推动 → 观察真实碰撞连锁",.007f);Mirror(2);
    }
    static void Canal()
    {
        var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();var group=Feedback(boat.transform);
        var rig=boat.transform.Find("Water Experience");boat.readout.transform.SetParent(rig,false);
        boat.readout.transform.localPosition=new Vector3(.15f,1.85f,.55f);boat.readout.transform.localRotation=Quaternion.identity;
        boat.readout.GetComponent<ReportTextFacing>().mode=ReportTextFacingMode.WorldFixed;
        var sequence=Label(group,"Load sequence",Vector3.zero,"木20kg → 石30kg\n达标后开闸\n铜90kg：过载对照\n取出可复浮",.009f);
        Panel(group,sequence,level.stations[0],"sequence_panel",new Vector3(-1.5f,1.87f,-.25f));
        sequence.characterSize=.011f;
        for(int i=0;i<3;i++)Status(group,i,new Vector3((i-1)*.52f,1.49f,-.68f));
    }
    static void Sundial()
    {
        Task(0,"对齐晷盘南北","把盘面红色指针对齐固定南北线，偏差≤3°","按盘面实际方向测量；场景+Z为北、−Z为南，南北双向均接受。","转晷盘，看红色指针与金色南北线，达标后松手。");
        Task(1,"照准实际星标","让红色照准线指向金色星标，偏差≤3°","测量实际夹角，星标改变位置后正确角度也会改变。","沿圆周拖观测环，看照准线和偏差读数，达标后松手。");
        Task(2,"太阳与针影","将太阳拖至西南225°，观察针影向东北","直立针的影线由当前光照方向计算；这是方位示教，非精确纬度时钟。","拖金色太阳到225°附近，观察晷盘东北方向的针影后松手。");
        var old=Object.FindObjectsOfType<ReportHandleMeasure>().FirstOrDefault(m=>m.label=="方位误差");
        var dial=Object.FindObjectOfType<ReportDialMeasure>();
        if(dial==null){dial=old.handle.transform.parent.gameObject.AddComponent<ReportDialMeasure>();dial.dial=old.handle;}
        var root=dial.transform;var group=Feedback(root);dial.referenceLine=root.GetComponentsInChildren<Transform>().Single(t=>t.name.Contains("固定南北线"));dial.referenceLine.rotation=Quaternion.identity;
        dial.readout=Reading(root,"日晷实际方位读数");dial.label="实际方位偏差";dial.unit="°";
        level.stations[0].puzzle.learningTask.requirements[0].measure=dial;level.stations[0].puzzle.learningTask.requirements[0].acceptedRange=new Vector2(0,3);
        if(old!=null)Object.DestroyImmediate(old);var shape=new LevelModelBuilder();
        for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;shape.Beam(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*.35f,new Vector3(Mathf.Cos(b),0,Mathf.Sin(b))*.35f,.025f,"SatinBrass");}
        shape.Beam(Vector3.zero,Vector3.up*.35f,.025f,"InkTeal");shape.Beam(Vector3.zero,Vector3.forward*.3f,.018f,"CoralRed");ReplaceMesh(dial.dial.gameObject,"dial_gnomon",shape);
        Panel(group,dial.readout,level.stations[0],"dial_panel");Status(group,0,new Vector3(0,1.63f,-.13f));
        var ring=Object.FindObjectsOfType<ReportMechanismHandle>().Single(h=>h.name.Contains("活动观测环"));var sight=ring.transform.parent.GetComponent<ReportSightMeasure>();
        if(sight==null)sight=ring.transform.parent.gameObject.AddComponent<ReportSightMeasure>();sight.ring=ring;sight.target=ring.transform.parent.GetComponentsInChildren<Renderer>().Single(r=>r.name.Contains("45度星标"));
        if(sight.readout==null)sight.readout=ring.readout!=null?ring.readout:Reading(sight.transform,"观测环实际读数");sight.label="照准偏差";sight.unit="°";ring.readout=null;
        var scalar=ring.GetComponent<ReportHandleMeasure>();if(scalar!=null)Object.DestroyImmediate(scalar);
        var sightGroup=Feedback(sight.transform);sight.sightLine=Line(sightGroup,"Actual star sight","CoralRed",.012f);Panel(sightGroup,sight.readout,level.stations[1],"sight_panel");Status(sightGroup,1,new Vector3(0,1.64f,-.13f));
        level.stations[1].puzzle.learningTask.requirements[0].measure=sight;level.stations[1].puzzle.learningTask.requirements[0].acceptedRange=new Vector2(0,3);level.stations[1].puzzle.learningTask.stableDuration=1;
        var sun=Object.FindObjectOfType<ReportSunlightMeasure>();var solar=Feedback(sun.transform);sun.readout=sun.sunModel.readout!=null?sun.sunModel.readout:Reading(sun.transform,"太阳与针影读数");sun.sunModel.readout=null;
        sun.shadowOrigin=dial.dial.transform;sun.shadowLine=Line(solar,"Calculated needle shadow","InkTeal",.016f);sun.gnomonHeight=.35f;Panel(solar,sun.readout,level.stations[2],"sun_panel");Status(solar,2,new Vector3(0,1.35f,-.18f));
        Label(group,"World north south",new Vector3(0,1.16f,.44f),"场景北 N (+Z)",.008f);
    }
    static void Winter()
    {
        Task(0,"实际支撑搭雪人","按大中小搭三层，接触后松手稳定2秒","逐层检查真实支撑、重力与速度；悬空或缺少下层不能通过。","压实面朝下，大球放台中央，再中球与小球；接触后黏结。");
        Task(1,"坡顶轻放滚动","把球轻放红色坡顶，让重力使其到达坡底","坡顶释放速度低于0.35m/s；投掷经过坡顶或直接放接收区不计。","拿球到红色坡顶，停稳后松手，观察真实滚动。");
        var stack=Object.FindObjectOfType<ReportStackMeasure>();var group=Feedback(stack.transform);stack.baseSupport=stack.GetComponentsInChildren<MeshCollider>().First(c=>c.name.Contains("台架"));Panel(group,stack.readout,level.stations[0],"snow_support_panel");Status(group,0,new Vector3(0,1.65f,-.13f));
        Label(group,"Snow structure",new Vector3(0,1.02f,-.32f),"大 → 中 → 小；压实面朝下\n实际接触后黏结，松手静止2秒",.008f);
        var roll=Object.FindObjectOfType<ReportRollMeasure>();var rollRoot=roll.rampStart.parent;var ramp=Feedback(rollRoot);Panel(ramp,roll.readout,level.stations[1],"roll_panel",new Vector3(-1.15f,1.93f,-.13f));Status(ramp,1,new Vector3(-1.15f,1.52f,-.18f));
        Label(ramp,"Release at ramp start",new Vector3(0,1.04f,-.28f),"轻放红色坡顶，松手靠重力滚下\n不能投掷，也不能直接放进接收区",.008f);Heat(Object.FindObjectOfType<ReportHeatMeasure>(),2);
    }
    static void Hall()
    {
        var compass=Object.FindObjectOfType<ReportCompassMeasure>();var group=Feedback(compass.transform);compass.southBearing=Mathf.Repeat(Vector3.SignedAngle(compass.transform.forward,Vector3.back,Vector3.up),360);compass.needle.startingValue=compass.southBearing;
        var old=compass.transform.Find("南刻度");if(old!=null)Object.DestroyImmediate(old.gameObject);
        Vector3[] directions={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};string[] names={"北 N","东 E","南 S","西 W"};
        for(int i=0;i<4;i++)Label(group,"Cardinal "+i,Vector3.up*1.16f+compass.transform.InverseTransformDirection(directions[i])*.4f,names[i]);
        Panel(group,compass.readout,level.stations[0],"compass_panel");Status(group,0,new Vector3(0,1.63f,-.13f));Power(Object.FindObjectOfType<ReportElectromagnetMeasure>(),1);Throw(Object.FindObjectOfType<ReportThrowTarget>(),2);
    }
    static void Garden()
    {
        Task(0,"绳鼓提闸","转水工轮，观察实时绳线与闸板升高≥0.6m","绳鼓无滑动h=rθ，做功W=mgh；红色刻度为0.6m。","握轮辐转动，闸板达到红色刻度后松手。");
        Task(1,"逐层接触承重","青色支撑、桥面、红色载荷逐层接触，松手稳定2秒","实际重力和接触支撑；悬空、缺少下层及持续滑动不计。","先青色支撑，再桥面板，最后红载荷，压稳后松手。");
        var wheel=Object.FindObjectOfType<ReportWheelLiftMeasure>();var group=Feedback(wheel.transform);Panel(group,wheel.readout,level.stations[0],"wheel_panel",new Vector3(-1.15f,1.93f,-.13f));Hide(wheel.wheel);
        wheel.pulley=Group(group,"Cable pulley",new Vector3(.35f,2.0f,0));wheel.cable=Line(group,"Moving lift cable","Ivory",.013f);
        Box(group,"pulley_frame",new Vector3(.35f,1.48f,.12f),new Vector3(.035f,1.06f,.035f),"SatinBrass");
        for(int i=0;i<=3;i++)Box(group,"lift_tick_"+i,new Vector3(.56f,1.1f+i*.2f,0),new Vector3(.06f,.006f,.018f),i==3?"CoralRed":"Ivory");
        Status(group,0,new Vector3(-1.15f,1.5f,-.18f));
        var stack=Object.FindObjectOfType<ReportStackMeasure>();var support=Feedback(stack.transform);stack.baseSupport=stack.GetComponentsInChildren<MeshCollider>().First(c=>c.name.Contains("台架"));Panel(support,stack.readout,level.stations[1],"bridge_panel");Status(support,1,new Vector3(0,1.64f,-.13f));
        BridgeMarker(stack);Label(support,"Bridge order",new Vector3(0,1.04f,-.34f),"青色支撑 → 桥面板 → 红色载荷\n真实接触承重，松手静止2秒",.008f);Mirror(2);
    }
    public static void BridgeMarker(ReportStackMeasure stack)
    {
        const string name="Bridge assembly footprint";var old=stack.transform.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);
        var m=new LevelModelBuilder();var center=new Vector3(0,.944f,.12f);
        Vector3[] corners={new Vector3(-.17f,0,-.20f),new Vector3(.17f,0,-.20f),new Vector3(.17f,0,.20f),new Vector3(-.17f,0,.20f)};
        for(int i=0;i<4;i++)m.Beam(center+corners[i],center+corners[(i+1)%4],.008f,"CyanLight");
        m.Save("AncientGarden_BridgeAssemblyMark",stack.transform,false).name=name;
    }
    public static void ApplyCurrentScene(int index)
    {
        level=Object.FindObjectOfType<DemoLevel>();key=Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path)+"_Later_";
        switch(index)
        {
            case 4:
                Power(Object.FindObjectOfType<ReportElectromagnetMeasure>(),1);
                Task(0,"铁芯入座","把铁芯插入线圈安装孔并松手","导向孔会对齐铁芯；放在线圈旁不算安装。","拿深色铁芯放入青色孔位，再松手。");
                Task(1,"安全供电并吸合","将电流调至2～3A，等衔铁向线圈闭合","B₀=μ₀NI/L是空芯激励参考；铁芯实际场强不以此值代替。超过3A保护锁止。","沿滑杆拖到绿色安全区，松手等待衔铁闭合。");
                Task(2,"保持供电拉门栓","保持铁芯入座及安全供电，水平拉出门栓","衔铁实际闭合后才解锁；断电、过流和取芯会重新锁止。","握门栓沿水平导轨向右拉到底并松手。");break;
            case 5:
                Hydraulic(Object.FindObjectOfType<ReportHydraulicMeasure>(),2);
                Task(0,"隔离并泄压","把三通阀转到0并松手","0位隔离压力源并泄压，以便安全安装密封。","转阀至0，观察漏水停止。");
                Task(1,"卸压后安装密封","保持阀关闭，将红色密封圈装入青色座","带压时安装座不会捕获密封圈；必须先卸压。","关阀后拿密封圈入座并松手。");
                Task(2,"连通流路并举升","开阀、压小活塞，让大活塞举升≥8cm","阀开度控制连通；准静态压力p=F₁/A₁，A₁x₁=A₂x₂。","开阀，再压小活塞32～40cm；观察输出8～10cm后松手。");break;
            case 6: Pendulum();break;
            case 7:
                foreach(var measure in Object.FindObjectsOfType<ReportThrowMeasure>())Throw(measure.target,Array.FindIndex(level.stations,s=>s.puzzle.learningTask.requirements.Any(r=>r.measure==measure)));
                Heat(Object.FindObjectOfType<ReportHeatMeasure>(),2);break;
            case 8: Workshop();break;
            case 9: Power(Object.FindObjectOfType<ReportElectromagnetMeasure>(),0);Mirror(1);Hydraulic(Object.FindObjectOfType<ReportHydraulicMeasure>(),2);break;
            case 10: Canal();break;
            case 11: Sundial();break;
            case 12: Winter();break;
            case 13: Hall();break;
            case 14: Garden();break;
        }
        ReportIntuitivePresentation.ConfigureScene(level);
        level.grabbables=Object.FindObjectsOfType<ReportGrabbable>().Where(g=>g.enabled).ToArray();
        Debug.Log("REMAINING_LEVEL_UPGRADED index="+index+" title="+level.title);
    }
    public static void All()
    {
        for(int index=4;index<15;index++){EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);ApplyCurrentScene(index);EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());}
        AssetDatabase.SaveAssets();Debug.Log("REMAINING_UPGRADE_OK scenes=11");
    }
}
