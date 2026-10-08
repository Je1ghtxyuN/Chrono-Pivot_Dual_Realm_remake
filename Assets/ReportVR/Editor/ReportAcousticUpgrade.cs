using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportAcousticUpgrade
{
    const string Key="Fig_2_03_Acoustic_Resonance_Acoustic_";
    static Transform Group(Transform parent,string name,Vector3 position)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;return go.transform;}
    static GameObject Box(Transform parent,string name,Vector3 center,Vector3 size,string material)
    {var model=new LevelModelBuilder();model.Box(center,size,material);return model.Save(Key+name,parent,false);}
    static void ReplaceMesh(GameObject target,string name,LevelModelBuilder model)
    {
        var generated=model.Save(Key+name,target.transform.parent,false);var mesh=generated.GetComponent<MeshFilter>().sharedMesh;
        target.GetComponent<MeshFilter>().sharedMesh=mesh;target.GetComponent<MeshRenderer>().sharedMaterials=generated.GetComponent<MeshRenderer>().sharedMaterials;
        var collider=target.GetComponent<MeshCollider>();if(collider!=null)collider.sharedMesh=mesh;Object.DestroyImmediate(generated);
    }
    static void Label(Transform parent,Vector3 position,string words,float size=.009f)
    {
        var node=Group(parent,"Acoustic explanation",position);var text=node.gameObject.AddComponent<TextMesh>();text.text=words;text.fontSize=48;text.characterSize=size;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.96f,.91f,.72f);
        node.GetComponent<Renderer>().sharedMaterial=Object.FindObjectOfType<DemoInteractor>().worldText;
        var facing=node.gameObject.AddComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.FacePlayer;facing.yawOnly=true;
        var player=Object.FindObjectOfType<DesktopPlayer>();if(player!=null)facing.viewer=player.head.transform;
    }
    static Transform Feedback(ReportAcousticMeasure measure)
    {
        var old=measure.transform.Find("Acoustic feedback");
        if(old!=null){if(measure.readout.transform.IsChildOf(old))measure.readout.transform.SetParent(measure.transform,true);Object.DestroyImmediate(old.gameObject);}
        var group=Group(measure.transform,"Acoustic feedback",Vector3.zero);
        measure.readout.transform.SetParent(group,false);measure.readout.transform.localPosition=new Vector3(0,1.94f,-.13f);measure.readout.characterSize=.011f;measure.readout.color=new Color(.96f,.93f,.77f);
        Box(group,measure.experiment+"_readout",new Vector3(0,1.94f,-.09f),new Vector3(1.45f,.48f,.035f),"InkTeal");
        return group;
    }
    static void Task(DemoStation station,string title,string goal,string clue,string hint,float low,float high,float stable)
    {
        station.instruction=title;station.puzzle.goal=goal;station.puzzle.clue=clue;station.puzzle.hint=hint;
        station.puzzle.successMessage="已观察到真实测量结果："+title;station.puzzle.learningTask.requirements[0].acceptedRange=new Vector2(low,high);station.puzzle.learningTask.stableDuration=stable;
    }
    [MenuItem("Report VR/Acoustics/Upgrade Third Level")]
    public static void Scene()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[2].path);ApplyCurrentScene();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        Debug.Log("ACOUSTIC_UPGRADE_OK scene=2");
    }
    public static void ApplyCurrentScene()
    {
        var level=Object.FindObjectOfType<DemoLevel>();var measures=Object.FindObjectsOfType<ReportAcousticMeasure>();
        var strike=measures.Single(m=>m.experiment==ReportAcousticReading.Strike);
        var resonance=measures.Single(m=>m.experiment==ReportAcousticReading.Resonance);
        var beats=measures.Single(m=>m.experiment==ReportAcousticReading.Beats);
        foreach(var measure in measures){measure.naturalFrequency=220;measure.station=level.stations[(int)measure.experiment];}
        var shell=new LevelModelBuilder();var brass=LevelModelBuilder.Mat("SatinBrass");var inner=LevelModelBuilder.Mat("Copper");
        float[] y={-.21f,-.10f,.12f,.22f},r={.20f,.18f,.13f,.075f};
        for(int side=0;side<20;side++)
        {
            Vector3 Point(float radius,float height,int index)=>new Vector3(Mathf.Cos(index*Mathf.PI/10)*radius,height,Mathf.Sin(index*Mathf.PI/10)*radius*.82f);
            for(int layer=0;layer<3;layer++)
            {
                shell.Face(brass,Point(r[layer],y[layer],side),Point(r[layer+1],y[layer+1],side),Point(r[layer+1],y[layer+1],side+1),Point(r[layer],y[layer],side+1));
                shell.Face(inner,Point(r[layer]-.015f,y[layer],side),Point(r[layer]-.015f,y[layer],side+1),Point(r[layer+1]-.015f,y[layer+1],side+1),Point(r[layer+1]-.015f,y[layer+1],side));
            }
            shell.Face(brass,Point(r[0],y[0],side),Point(r[0],y[0],side+1),Point(r[0]-.015f,y[0],side+1),Point(r[0]-.015f,y[0],side));
            shell.Face(brass,Vector3.up*.22f,Point(r[3],y[3],side+1),Point(r[3],y[3],side));
        }
        ReplaceMesh(strike.gameObject,"bell",shell);
        var mallet=new LevelModelBuilder();mallet.Box(Vector3.zero,new Vector3(.035f,.25f,.035f),"WarmWood");mallet.Box(Vector3.up*.10f,new Vector3(.26f,.09f,.09f),"WarmWood");ReplaceMesh(strike.mallet.gameObject,"mallet",mallet);strike.mallet.GetComponent<Rigidbody>().mass=.25f;
        var strikeOld=strike.transform.Find("Acoustic feedback");if(strikeOld!=null)Object.DestroyImmediate(strikeOld.gameObject);
        var strikeGroup=Group(strike.transform,"Acoustic feedback",Vector3.zero);var support=strike.transform.parent;
        var frame=support.Find("Acoustic bell support");if(frame!=null)Object.DestroyImmediate(frame.gameObject);frame=Group(support,"Acoustic bell support",Vector3.zero);
        Box(frame,"bell_frame",new Vector3(-.2f,1.65f,0),new Vector3(.72f,.04f,.06f),"WarmWood");
        foreach(float x in new[]{-.52f,.12f})Box(frame,"bell_post_"+x,new Vector3(x,1.29f,.08f),new Vector3(.045f,.70f,.045f),"WarmWood");
        Box(frame,"bell_hanger",new Vector3(-.2f,1.58f,0),new Vector3(.025f,.14f,.025f),"SatinBrass");
        var pulse=Group(strikeGroup,"Decay indicator",new Vector3(0,.29f,0));strike.vibrationMarker=pulse;strike.feedbackIndicator=Box(pulse,"strike_glow",Vector3.zero,new Vector3(.09f,.025f,.04f),"AmberLight").GetComponent<Renderer>();
        strike.readout.transform.localPosition=new Vector3(0,2.03f,-.14f);strike.readout.characterSize=.011f;strike.readout.color=new Color(.96f,.93f,.77f);
        Box(frame,"strike_readout_back",new Vector3(0,2.03f,-.10f),new Vector3(1.48f,.48f,.035f),"InkTeal");
        Task(level.stations[0],"握槌敲钟，观察余振","拿住木槌敲击钟身，听到钟声后松手","沿钟身法线的撞击速度至少0.35m/s；拿住敲击才计入，轻放与投掷不计。","VR握把拿住木槌并挥动；桌面按住左键/E拿住，再移动视线使槌头撞钟。敲响后松手。",1,100,.5f);
        resonance.frequency.valueRange=new Vector2(180,260);resonance.frequency.startingValue=185;resonance.frequency.desktopUnitsPerPixel=.08f;resonance.frequency.vrDegreesForFullRange=180;
        resonance.vibrationMarker.localPosition=new Vector3(-.3f,1.17f,-.19f);
        resonance.frequency.readout.GetComponent<Renderer>().enabled=false;
        var resGroup=Feedback(resonance);
        var curve=resGroup.gameObject.AddComponent<LineRenderer>();curve.useWorldSpace=false;curve.positionCount=81;curve.startWidth=curve.endWidth=.009f;curve.sharedMaterial=LevelModelBuilder.Mat("Ivory");
        for(int i=0;i<=80;i++)curve.SetPosition(i,new Vector3(-.68f+.9f*i/80,1.35f+.3f*resonance.ResponseAt(180+i),-.15f));
        Box(resGroup,"curve_back",new Vector3(-.23f,1.52f,-.12f),new Vector3(1,.41f,.025f),"InkTeal");
        resonance.responsePointer=Group(resGroup,"Current frequency on response curve",Vector3.zero);resonance.feedbackIndicator=Box(resonance.responsePointer,"response_point",Vector3.zero,new Vector3(.035f,.035f,.02f),"AmberLight").GetComponent<Renderer>();
        Label(resGroup,new Vector3(-.23f,1.26f,-.16f),"180Hz ← 驱动频率 → 260Hz\n摆片为慢速振幅示意",.008f);
        Task(level.stations[1],"调频寻找共振峰","调驱动频率，使相对峰值振幅达到94%，松手保持1秒","固有频率220Hz、阻尼比0.04；有阻尼时共振峰略低于固有频率。","转动调频轮，观察曲线上的亮点；接近峰顶时摆片明显变大，亮绿后松手。",.94f,1.01f,1);
        var beatGroup=Feedback(beats);
        beats.frequency.valueRange=new Vector2(210,230);beats.frequency.startingValue=215;beats.frequency.desktopUnitsPerPixel=.05f;beats.frequency.vrDegreesForFullRange=180;
        beats.frequency.readout.GetComponent<Renderer>().enabled=false;
        var fineModel=new LevelModelBuilder();fineModel.Box(Vector3.zero,new Vector3(.12f,.12f,.06f),"SatinBrass");var fineObject=fineModel.Save(Key+"fine_control",beatGroup,true);fineObject.transform.localPosition=new Vector3(.56f,1.18f,-.22f);
        fineObject.GetComponent<MeshCollider>().convex=true;var fineBody=fineObject.AddComponent<Rigidbody>();fineBody.isKinematic=true;fineBody.useGravity=false;fineBody.interpolation=RigidbodyInterpolation.Interpolate;
        var handle=fineObject.AddComponent<ReportMechanismHandle>();handle.motion=ReportHandleMotion.Rotation;handle.localAxis=Vector3.forward;handle.valueRange=new Vector2(-1,1);handle.startingValue=0;handle.angleRange=new Vector2(-90,90);handle.vrDegreesForFullRange=180;handle.desktopUnitsPerPixel=.0025f;handle.unit="Hz";handle.maximumReleaseSpeed=0;handle.attachEaseInTime=0;
        handle.interactionManager=Object.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();handle.movementType=UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;handle.throwOnDetach=false;handle.colliders.Clear();handle.colliders.Add(fineObject.GetComponent<Collider>());handle.interactionLayers=beats.frequency.interactionLayers;
        beats.fineFrequency=handle;
        var oldMarker=beats.vibrationMarker;if(oldMarker!=null)oldMarker.gameObject.SetActive(false);
        beats.vibrationMarker=Group(beatGroup,"Audible beat envelope",new Vector3(-.3f,1.45f,-.08f));beats.feedbackIndicator=Box(beats.vibrationMarker,"beat_column",Vector3.zero,new Vector3(.09f,.35f,.035f),"AmberLight").GetComponent<Renderer>();
        Box(beatGroup,"reference_source",new Vector3(-.55f,1.12f,0),new Vector3(.19f,.26f,.24f),"SatinBrass");
        Label(beatGroup,new Vector3(-.53f,1.30f,-.18f),"参考220Hz",.008f);Label(beatGroup,new Vector3(.30f,1.42f,-.22f),"粗调",.008f);Label(beatGroup,new Vector3(.57f,1.37f,-.22f),"微调 ±1Hz",.008f);
        Task(level.stations[2],"听拍频，微调至平稳","让拍频≤0.3Hz，松开粗调和微调旋钮并保持1.5秒","拍频=两声源频率差；差越小，强弱变化越慢。同频时变化停止，不以静音为通关依据。","先用粗调轮把频差降到1Hz以内，再用小旋钮微调。听强弱变化、看同步光柱；亮绿后松手。",0,.3f,1.5f);
        level.mission="握槌敲钟 → 找共振峰 → 听拍频调谐；声音只在当前实验或拿住其旋钮时持续播放。";
        level.grabbables=Object.FindObjectsOfType<ReportGrabbable>().Where(g=>g.enabled).ToArray();level.Refresh();ReportFloatingStatus.ConfigureScene(level);
    }
}
