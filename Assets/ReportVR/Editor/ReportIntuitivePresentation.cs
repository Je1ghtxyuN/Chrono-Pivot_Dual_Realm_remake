using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Change presentation in place; the existing physical experiments remain authoritative.
public static class ReportIntuitivePresentation
{
    const string Output="Documentation/IntuitiveDemo";
    static readonly Regex Data=new Regex(@"\d[\d.,～±≤≥%°\s-]*(kg|Hz|mT|kPa|cm|m/s|m²|m³|°C|%|°|m\b|A\b|H\b|F\b|N\b|W\b|J\b)|[=≈].*\d|[μθπ₀₁₂]");
    static readonly string[] Missions={
        "先熟悉拿取与松手：辨认材料、尝试抛掷，再移动太阳观察影子，为后面的实验做准备。",
        "给水渠里的船安排载荷：先放木块，再比较加石块后的吃水，最后开闸放行。",
        "在声学庭院寻找发声的规律：先敲钟听余音，再调节共振，最后让两种声音的节奏接近。",
        "在观星台学习确定方向：先拨动司南看它回南，再照准星标，最后调整铜镜的反射方向。",
        "修好电磁门锁：先装入铁芯，再通电观察吸合，保持供电后拉开门栓。",
        "修复漏水管路：先关阀卸压，再装密封圈，最后开阀试验液压举升。",
        "用摆的节奏作参照：先观察往复，再改变摆长，最后调节电路，让两条波形起伏一样快。",
        "雪地实验分为运动与受热两部分：先比较近处和远处的落点，再把冰移入暖区观察融化。",
        "在工坊比较传递的方式：搭好支撑结构，推动骨牌看碰撞连锁，再转镜观察光的反射。",
        "综合比较三类装置：用线圈吸合衔铁、用铜镜转向光束、用液体传递压力并抬起载荷。",
        "把浮力经验用到园林水渠：逐步装载，比较水线，再卸去过载并开闸通航。",
        "在日晷台练习观测：先对齐南北方向，再照准星标，最后移动太阳比较针影的方向。",
        "观察冬日材料的三种变化：叠放雪块看支撑、松开雪球看滚动、罩住冰样比较融化快慢。",
        "在展厅复验熟悉的器材：观察司南回南、线圈吸合和抛球落点，用前面学到的方法操作。",
        "完成园林巡游的三次动手试验：转轮提闸、搭桥承重，最后转镜把光送到接收器。"
    };
    // Each entry is action | visible principle | operation, in build order.
    static readonly string[][] Guides={
        new[]{
            "分别拿起木、石、铜|观察不同材料的外观|依次拿起三种材料，再松手。",
            "把材料抛向金色落点区|抛出的物体沿弧线落下|把三种材料分别从台边抛出，观察飞行和落点。",
            "把太阳拖到东方|影子朝太阳的相反方向|沿圆环拖动金色太阳到东方，松手看针影向西。"},
        new[]{
            "装木块，观察船下沉|增加载荷，船浸入水中更多|把木块移到小木船船舱正上方并松手，观察船下沉和后方大船同步变化。",
            "加石块，比较下沉变化|载荷越重，船沉得越深|再放石块，观察金色水线；可加铜块看过载，再取出让船回浮。",
            "打开闸门，观察船前进|载荷合适、闸门打开后才能通航|保留木块和石块，取出铜块；把闸轮转至全开并松手。"},
        new[]{
            "敲钟，听声音消退|振动产生声音，余振逐渐减弱|拿木槌挥动敲钟，敲响后松手，听声音的变化。",
            "调节旋钮，寻找共振|节奏接近时，振动变得更强|转调频轮，观察摆片；振动最明显且亮绿时松手。",
            "微调旋钮，让声音平稳|接近同音时，忽强忽弱的变化变慢|先粗调，再微调；听声音、看光柱，亮绿后松开两只旋钮。"},
        new[]{
            "拨开司南，观察回南|磁作用让勺柄回指南方|明显拨开勺柄再松手，观察回摆和逐渐停稳。",
            "转观测环，对准星标|照准线指向被观察的目标|沿圆周拖动观测环，让红线指向金色星标，再松手。",
            "转动铜镜，让接收器亮起|镜面改变光的传播方向|转动镜框，观察反射光，接收器亮绿后松手。"},
        new[]{
            "把铁芯装入线圈|铁芯增强通电线圈的磁作用|把深色铁芯放到青色孔位并松手。",
            "通电，观察衔铁吸合|通电产生磁性，断电后吸力消失|把供电滑杆拖到绿色区域，松手等待衔铁吸合。",
            "保持供电，拉开门栓|磁力吸合后门栓才能解锁|保留铁芯和供电，沿水平导轨把门栓拉到底并松手。"},
        new[]{
            "关阀，观察漏水停止|隔离流路可以停止供水|把阀轮转至关闭并松手，观察漏水变化。",
            "把密封圈放入接口|密封后液体才能有效传递压力|保持关阀，把红色密封圈放入青色接口并松手。",
            "开阀压活塞，观察举升|小活塞推动液体，大活塞抬起载荷|开阀，把小活塞压到底并松手，观察另一端举升。"},
        new[]{
            "拉开摆球，观察往复摆动|重力使摆球来回摆动|沿圆弧明显拉开摆球再松手，等待完整往复摆动。",
            "改变摆长，比较摆动快慢|长摆摆得慢，短摆摆得快|把摆长滑杆调到金色标记，再拉开摆球并松手。",
            "调节旋钮，让波形节奏接近|不同系统可以具有相近的振动节奏|比较青色与金色波形；调节电容轮，让起伏一样快，再松手。"},
        new[]{
            "把球抛进近处落点区|抛出的球受重力作用沿弧线下落|从青色圈内挥手抛球；桌面蓄力后松开，看它落入金色区。",
            "再试远处落点区|出手方式会改变飞行轨迹和落点|留在发射台边，改变抛出力度，观察球飞得更远。",
            "把冰放到暖区，观察融化|冰吸热后变小，融水增加|把冰样放在红色暖区并松手，观察它逐渐融化。"},
        new[]{
            "装连接块，再放横梁|连接件支撑并固定结构|先把两侧连接块放入青色座，再放横梁并松手。",
            "推倒首块，观察骨牌连锁|碰撞把运动接连传下去|用球推倒第一块，观察骨牌依次倒下。",
            "转镜，让光落入接收器|镜面转动会改变反射方向|握镜框转动，让反射光落到接收器；亮绿后松手。"},
        new[]{
            "装芯通电，观察吸合|通电线圈产生磁作用|铁芯放入青色孔位，再把供电滑杆拖到绿色区域并松手。",
            "转镜，让接收器亮起|反射光随镜面方向改变|转动铜镜，观察实际光束；接收器亮绿后松手。",
            "压小活塞，观察载荷升起|液体把压力传到另一端|先开阀，再把小活塞压到底并松手，看大活塞抬升。"},
        new[]{
            "装木块，观察水线变化|增加载荷会使船下沉|把木块放入对应装船位并松手，看白色水线靠近水面。",
            "加石块，比较船的吃水|更重的船需要排开更多水|再装石块，观察金色水线；铜块用于过载与回浮对照。",
            "开闸，观察船前进|安全载荷与畅通闸口允许通航|保留木块和石块、取出铜块，转闸轮至全开并松手。"},
        new[]{
            "转晷盘，对齐南北线|先对齐方向，才能比较针影|让红色指针与金色南北线重合，再松手。",
            "转观测环，对准星标|照准线帮助确定目标方向|沿圆周拖动观测环，让红线指向金色星标后松手。",
            "把太阳移到西南，观察针影|太阳换方向，影子向相反方向移动|拖金色太阳到西南，松手看针影指向东北。"},
        new[]{
            "从大到小，堆稳雪块|下层支撑上层，接触后结构才能稳定|大、中、小依次叠放，压实面朝下；松手等待停稳。",
            "在坡顶松手，让雪球滚下|重力让物体沿斜坡向下运动|把雪球轻放红色坡顶后松手，观察它滚入接收区。",
            "罩住暖区冰样，观察融化变慢|隔热罩减缓热的传递|冰样先放红色暖区，再放隔热罩；松手保持罩住观察。"},
        new[]{
            "拨动司南，观察回南|磁作用与阻尼使勺柄回南并停稳|明显拨开勺柄再松手，观察它回摆。",
            "装芯通电，观察衔铁吸合|通电产生磁性，断电后吸力消失|装好铁芯，供电滑杆拖到绿色区域，松手等衔铁吸合。",
            "从台边抛球，观察落点|出手方式改变弧形轨迹|从青色圈内抛出球，让它落入金色区。"},
        new[]{
            "转动水工轮，观察闸板升起|轮带动绳索收紧，提起闸板|握轮辐转动，闸板到红色标记后松手。",
            "搭桥后放载荷，观察支撑|接触的支撑把载荷传到下层|青色支撑先放桌面中央的青色标记区，再放桥面板和红色载荷；松手等待结构稳住。",
            "转镜，让接收器亮起|光在镜面上改变传播方向|转镜框，让实际反射光落进接收器，亮绿后松手。"}
    };

    public static void ConfigureScene(DemoLevel level)
    {
        int index=Array.FindIndex(EditorBuildSettings.scenes,s=>s.path==EditorSceneManager.GetActiveScene().path);
        if(index<0 || index>=Guides.Length)return;
        for(int step=0;step<level.stations.Length;step++)
        {
            var guide=Guides[index][step].Split('|');var station=level.stations[step];
            station.instruction=station.puzzle.goal=guide[0];station.puzzle.clue=guide[1];station.puzzle.hint=guide[2];
            station.puzzle.successMessage="已观察："+guide[1]+"。\n"+(step+1<level.stations.Length?"接着："+Guides[index][step+1].Split('|')[0]:"本关实验完成，沿步道前往出口。");
            station.label.characterSize=Mathf.Min(station.label.characterSize,.012f);
        }
        level.mission=Missions[index];
        if(index==3 && Object.FindObjectOfType<ReportMirrorMeasure>().secondMirror!=null)SceneFourUpgrade.SetText(level);
        var live=new HashSet<TextMesh>();
        foreach(var component in Object.FindObjectsOfType<MonoBehaviour>())
        {
            var field=component.GetType().GetField("readout");
            if(field!=null && field.FieldType==typeof(TextMesh) && field.GetValue(component) is TextMesh text && text!=null)
            {live.Add(text);text.text="";}
        }
        foreach(var text in Object.FindObjectsOfType<TextMesh>())
        {
            if(live.Contains(text) || level.stations.Any(s=>s.label==text) || text==level.board)continue;
            switch(text.name)
            {
                case "Safe current":text.text="绿色区域\n安全供电";break;
                case "Valve meaning":text.text="关：停止供水\n开：连通流路";break;
                case "Input stroke":text.text="压下小活塞";break;
                case "Output stroke":text.text="观察大活塞举升";break;
                case "Length control":text.text="改变摆长\n滑杆对准金色标记";break;
                case "Heat comparison":text.text="观察冰变小、融水增加\n罩住后比较融化快慢";break;
                case "Load sequence":text.text="先木块，再石块\n加铜会过载，取出会回浮";break;
                case "Chain direction":text.text="推动首块 → 观察碰撞连锁";break;
                case "Snow structure":text.text="大 → 中 → 小\n压实面朝下，松手观察";break;
                case "Bridge order":text.text="支撑 → 桥面 → 载荷\n放稳后松手观察";break;
                case "World north south":text.text="北 N";break;
                case "Release and observe":text.GetComponent<Renderer>().enabled=false;continue;
            }
            if(text.text.Contains("白线25%"))text.text="白线：木块\n金线：木块＋石块";
            if(text.text.Contains("180Hz"))text.text="转动调节轮，观察振动强弱";
            if(text.text=="参考220Hz")text.text="参考声音";
            if(text.text.Contains("微调 ±1Hz"))text.text="微调";
            text.text=Regex.Replace(text.text,@"(木|石|铜)\d+kg", "$1块");
            if(Data.IsMatch(text.text))text.GetComponent<Renderer>().enabled=false;
        }
        foreach(var handle in Object.FindObjectsOfType<ReportMechanismHandle>())
            if(handle.readout!=null && !handle.name.Contains("太阳"))handle.readout.GetComponent<Renderer>().enabled=false;
        foreach(var sun in Object.FindObjectsOfType<ReportSunlightMeasure>())
        {
            var parent=sun.sunModel.transform.parent;
            if(parent.Find("Intuitive directions")!=null)continue;
            var directions=new GameObject("Intuitive directions").transform;directions.SetParent(parent,false);
            var template=sun.readout!=null?sun.readout:sun.sunModel.readout;
            Vector3[] points={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};string[] names={"北","东","南","西"};
            for(int i=0;i<points.Length;i++)
            {
                var label=new GameObject(names[i]).AddComponent<TextMesh>();label.transform.SetParent(directions,false);
                label.transform.position=sun.sunModel.transform.position+points[i]*.72f;
                label.text=names[i];label.font=template.font;label.fontSize=48;label.characterSize=.012f;label.anchor=TextAnchor.MiddleCenter;label.color=new Color(1,.9f,.55f);
                label.GetComponent<Renderer>().sharedMaterial=template.GetComponent<Renderer>().sharedMaterial;
                var facing=label.gameObject.AddComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.FacePlayer;facing.yawOnly=true;facing.viewer=Object.FindObjectOfType<DesktopPlayer>().head.transform;
            }
        }
        var pendulum=Object.FindObjectOfType<ReportPendulumMeasure>();
        if(pendulum!=null && pendulum.length.transform.parent.Find("Intuitive length target")==null)
        {
            var mark=new GameObject("Intuitive length target").transform;mark.SetParent(pendulum.length.transform.parent,false);
            var model=new LevelModelBuilder();var handle=pendulum.length;
            float amount=Mathf.InverseLerp(handle.valueRange.x,handle.valueRange.y,1);
            var position=handle.transform.localPosition+handle.localAxis.normalized*Mathf.Lerp(handle.slideRange.x,handle.slideRange.y,amount);
            model.Box(position+Vector3.right*.16f,new Vector3(.16f,.012f,.04f),"SatinBrass");
            model.Save("Fig_2_07_IntuitiveLengthTarget",mark,false);
        }
        level.Refresh();
    }

    [MenuItem("Report VR/Presentation/Use intuitive physical demonstrations")]
    public static void All()
    {
        Directory.CreateDirectory(Output);var report=new List<string>();int count=0;
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);var level=Object.FindObjectOfType<DemoLevel>();
            var physics=Object.FindObjectsOfType<Component>().Where(c=>c is ReportLearningMeasure || c is ReportLearningTask || c is ReportMechanismHandle || c is ReportThrowTarget || c is ReportPlacementSocket || c is Rigidbody).ToDictionary(c=>c,c=>EditorJsonUtility.ToJson(c));
            ConfigureScene(level);
            if(physics.Any(p=>EditorJsonUtility.ToJson(p.Key)!=p.Value))throw new Exception("Physical configuration changed: "+scene.path);
            if(Object.FindObjectsOfType<TextMesh>().Any(t=>t.GetComponent<Renderer>().enabled && Data.IsMatch(t.text)))throw new Exception("Numeric display remains: "+scene.path);
            report.Add("PASS "+level.title+": qualitative guidance; physical components and task thresholds unchanged.");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());count++;
        }
        AssetDatabase.SaveAssets();File.WriteAllLines(Output+"/presentation-check.txt",report);
        if(count!=15)throw new Exception("Expected 15 demonstration scenes");
        Debug.Log("INTUITIVE_PRESENTATION_OK scenes="+count);
    }
    public static void Build(){All();Capture();SetupReportVR.BuildWindows();Debug.Log("INTUITIVE_BUILD_OK");}
    public static void Capture()
    {
        foreach(int index in new[]{1,2,4,6,12})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);var level=Object.FindObjectOfType<DemoLevel>();level.Refresh();
            var measure=Object.FindObjectsOfType<ReportLearningMeasure>().First(m=>m.GetType().GetField("readout")?.GetValue(m) is TextMesh text && text!=null);
            var view=measure.transform.position-measure.transform.forward*2.5f+Vector3.up*2;
            foreach(var renderer in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())renderer.enabled=false;
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
            foreach(var text in Object.FindObjectsOfType<TextMesh>())
            {
                text.font=font;text.GetComponent<Renderer>().sharedMaterial=font.material;
                var facing=text.GetComponent<ReportTextFacing>();if(facing!=null && facing.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
            }
            var readout=(TextMesh)measure.GetType().GetField("readout").GetValue(measure);
            var station=level.stations.FirstOrDefault(s=>s.puzzle.learningTask.requirements.Any(r=>r.measure==measure))??level.stations[0];
            readout.text=station.puzzle.clue+"\n"+station.puzzle.goal;
            ReportLevelAudit.Photograph(Output+"/level-"+(index+1)+".png",view,measure.transform.position+Vector3.up*1.25f,false,2);
        }
        Debug.Log("INTUITIVE_PREVIEW_OK");
    }
    public static void ApplyAndPreview(){All();Capture();}
}
