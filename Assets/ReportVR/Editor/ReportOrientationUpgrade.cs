using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportOrientationUpgrade
{
    const string Key="Fig_2_04_Geomagnetic_Optics_Orientation_";
    static Transform Group(Transform parent,string name,Vector3 position)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;return go.transform;}
    static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,string material)
    {var model=new LevelModelBuilder();model.Box(position,size,material);return model.Save(Key+name,parent,false);}
    static Transform Feedback(Transform parent)
    {var old=parent.Find("Orientation feedback");if(old!=null)Object.DestroyImmediate(old.gameObject);return Group(parent,"Orientation feedback",Vector3.zero);}
    static void Label(Transform parent,string name,Vector3 position,string words,float size=.008f)
    {
        var text=Group(parent,name,position).gameObject.AddComponent<TextMesh>();text.text=words;text.fontSize=48;text.characterSize=size;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.96f,.93f,.77f);
        text.GetComponent<Renderer>().sharedMaterial=Object.FindObjectOfType<DemoInteractor>().worldText;
        var facing=text.gameObject.AddComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.FacePlayer;facing.yawOnly=true;
        facing.viewer=Object.FindObjectOfType<DesktopPlayer>().head.transform;
    }
    static void Panel(Transform root,Transform feedback,TextMesh readout,string name)
    {
        readout.transform.localPosition=new Vector3(0,1.96f,-.13f);readout.characterSize=.01f;readout.color=new Color(.96f,.93f,.77f);
        readout.transform.localRotation=Quaternion.identity;
        var facing=readout.GetComponent<ReportTextFacing>();if(facing!=null)facing.mode=ReportTextFacingMode.WorldFixed;
        Box(feedback,name,new Vector3(0,1.96f,-.09f),new Vector3(1.5f,.48f,.035f),"InkTeal");
    }
    static LineRenderer Line(Transform root,string name,string material,float width)
    {var line=Group(root,name,Vector3.zero).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.sharedMaterial=LevelModelBuilder.Mat(material);line.startWidth=line.endWidth=width;return line;}
    static void Task(DemoStation station,string title,string goal,string clue,string hint,float high,float stable)
    {
        station.instruction=title;station.puzzle.goal=goal;station.puzzle.clue=clue;station.puzzle.hint=hint;
        station.puzzle.successMessage="已观察到测量结果："+title;station.puzzle.learningTask.requirements[0].acceptedRange=new Vector2(0,high);station.puzzle.learningTask.stableDuration=stable;
    }
    public static void Scene()
    {EditorSceneManager.OpenScene(EditorBuildSettings.scenes[3].path);ApplyCurrentScene();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("ORIENTATION_UPGRADE_OK scene=3");}
    public static void SharedInstructions()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[13].path);var level=Object.FindObjectOfType<DemoLevel>();var puzzle=level.stations[0].puzzle;
        puzzle.goal="拨离南向至少20°，松手后回到5°内稳定1秒";
        puzzle.hint="握住勺柄拨开至少20°，再松手，观察回摆逐渐稳定；微小拨动不计。";
        puzzle.clue="简化磁力矩与阻尼使勺柄回摆并趋于盘上南向刻度。";
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
    }
    public static void ApplyCurrentScene()
    {
        var level=Object.FindObjectOfType<DemoLevel>();
        var compass=Object.FindObjectOfType<ReportCompassMeasure>();var compassFeedback=Feedback(compass.transform);
        compass.southBearing=Mathf.Repeat(Vector3.SignedAngle(compass.transform.forward,Vector3.back,Vector3.up),360);
        compass.needle.startingValue=compass.southBearing;
        var oldSouth=compass.transform.Find("南刻度");if(oldSouth!=null)Object.DestroyImmediate(oldSouth.gameObject);
        Vector3[] directions={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};string[] names={"北 N","东 E","南 S","西 W"};
        for(int i=0;i<4;i++)
        {
            var local=compass.transform.InverseTransformDirection(directions[i]);
            Label(compassFeedback,"Cardinal "+i,Vector3.up*1.16f+local*.40f,names[i]);
            Box(compassFeedback,"cardinal_"+i,Vector3.up*1.125f+local*.35f,new Vector3(.035f,.012f,.035f),i==2?"CoralRed":"Ivory");
        }
        Panel(compass.transform,compassFeedback,compass.readout,"compass_panel");
        Task(level.stations[0],"拨开司南，观察回南","拨离南向至少20°，松手后回到5°内稳定1秒","简化磁力矩随偏差的正弦变化，阻尼使回摆逐渐稳定；方位标记与世界南北一致。","握住勺柄拨开明显角度，再松手看它回摆；微小拨动不计。",5,1);
        var ring=Object.FindObjectsOfType<ReportMechanismHandle>().Single(h=>h.name.Contains("活动观测环"));var root=ring.transform.parent;
        var sight=root.GetComponent<ReportSightMeasure>();if(sight==null)sight=root.gameObject.AddComponent<ReportSightMeasure>();sight.ring=ring;
        sight.target=root.GetComponentsInChildren<Renderer>().Single(r=>r.name.Contains("45度星标"));sight.label="照准偏差";sight.unit="°";
        var oldMeasure=ring.GetComponent<ReportHandleMeasure>();
        if(oldMeasure!=null){sight.readout=ring.readout;Object.DestroyImmediate(oldMeasure);}
        if(sight.readout==null)sight.readout=root.GetComponentsInChildren<TextMesh>().First(t=>t.name=="实验测量读数");
        ring.readout=null;
        var sightFeedback=Feedback(root);sight.sightLine=Line(sightFeedback,"Actual sight direction","CoralRed",.012f);
        Panel(root,sightFeedback,sight.readout,"sight_panel");
        level.stations[1].puzzle.learningTask.requirements[0].measure=sight;
        Task(level.stations[1],"照准线指向星标","让红线指向金色星标，偏差≤3°并松手稳定1秒","测量照准线与星标在观测环平面内的实际夹角；这是平面照准示教。","握住圆环沿圆周拖动，观察红线靠近星标；读数达标后松手。",3,1);
        var mirror=Object.FindObjectOfType<ReportMirrorMeasure>();var mirrorFeedback=Feedback(mirror.transform);
        mirror.mirror.transform.localPosition=Vector3.up*1.4f;
        mirror.emitter.localPosition=new Vector3(-.6f,1.4f,0);mirror.receiver.transform.localPosition=new Vector3(0,1.4f,.62f);
        var model=new LevelModelBuilder();model.Box(Vector3.zero,new Vector3(.40f,.40f,.025f),"Copper");
        foreach(float sign in new[]{-1f,1f})
        {model.Box(new Vector3(sign*.22f,0,0),new Vector3(.04f,.48f,.04f),"SatinBrass");model.Box(new Vector3(0,sign*.22f,0),new Vector3(.40f,.04f,.04f),"SatinBrass");}
        var generated=model.Save(Key+"mirror_frame",mirror.mirror.transform.parent,false);var mesh=generated.GetComponent<MeshFilter>().sharedMesh;
        mirror.mirror.GetComponent<MeshFilter>().sharedMesh=mesh;mirror.mirror.GetComponent<Renderer>().sharedMaterials=generated.GetComponent<Renderer>().sharedMaterials;
        mirror.mirror.GetComponent<MeshCollider>().sharedMesh=mesh;Object.DestroyImmediate(generated);
        mirror.normalLine=Line(mirrorFeedback,"Mirror normal","CyanLight",.008f);Panel(mirror.transform,mirrorFeedback,mirror.readout,"mirror_panel");
        Task(level.stations[2],"转镜接通光路","入射与反射光路畅通，接收器亮绿后松手稳定1秒","入射角等于反射角，角度相对镜面法线测量；物体会遮断光路。","握住铜镜边框转动，观察金色光束与蓝色法线，让光真正落进接收器。",1,1);
        level.stations[2].puzzle.learningTask.requirements[0].acceptedRange=Vector2.one;
        level.mission="拨动司南观察回南 → 照准线指向星标 → 铜镜接通真实光路";level.Refresh();ReportFloatingStatus.ConfigureScene(level);
        var roots=new[]{compass.transform,root,mirror.transform};
        for(int i=0;i<3;i++){level.stations[i].label.transform.position=roots[i].position+Vector3.up*2.45f;level.stations[i].label.characterSize=.014f;}
    }
}
