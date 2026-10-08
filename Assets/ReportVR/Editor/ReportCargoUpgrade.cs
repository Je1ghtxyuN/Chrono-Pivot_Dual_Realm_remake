using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportCargoUpgrade
{
    const string ScenePath="Assets/ReportScenesLowPoly/Scenes/Fig_2_02_Buoyancy_Canal.unity";
    public static void Configure(ReportBuoyancyMeasure boat)
    {
        if(EditorSceneManager.GetActiveScene().path!=ScenePath)return;
        ReportBoatDisplayUpgrade.Configure(boat);
        foreach(var seat in boat.seats)seat.loadingArea=boat.hull;
        var old=boat.hull.Find("Cargo loading area");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var guide=new GameObject("Cargo loading area").transform;guide.SetParent(boat.hull,false);
        var border=new LevelModelBuilder();
        foreach(float z in new[]{-.21f,.21f})border.Box(new Vector3(0,.23f,z),new Vector3(.86f,.012f,.018f),"Ivory");
        foreach(float x in new[]{-.42f,.42f})border.Box(new Vector3(x,.23f,0),new Vector3(.018f,.012f,.44f),"Ivory");
        border.Save("Fig_2_02_Cargo_LoadingBorder",guide,false);
        var sign=new GameObject("装船区提示").transform;sign.SetParent(guide,false);sign.localPosition=new Vector3(0,.24f,0);sign.localRotation=Quaternion.Euler(90,0,0);
        var text=sign.gameObject.AddComponent<TextMesh>();text.text="装船区 · 松手放入";text.characterSize=.022f;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(1,.94f,.62f);
        sign.GetComponent<Renderer>().sharedMaterial=boat.readout.GetComponent<Renderer>().sharedMaterial;
        guide.gameObject.SetActive(false);
        boat.readout.characterSize=.017f;
        boat.readout.transform.localPosition=new Vector3(1.65f,1.85f,.55f);
        foreach(Transform part in boat.readout.transform.parent)
            if(part.name.Contains("_draft_readout_board_"))part.localPosition=Vector3.right*1.5f;
        var level=Object.FindObjectOfType<DemoLevel>();
        level.stations[0].puzzle.goal="把木块放到船面上方，松手观察25%水线";
        level.stations[0].puzzle.clue="教学船排水容量120L；船体10kg加木20kg，目标吃水25%。小船的装载和水线同步显示在后方大船上。";
        level.stations[0].puzzle.hint="拿木块移到小木船船舱正上方，松开握把。装载后观察小船下沉和后方大船同步变化。";
        level.stations[1].puzzle.hint="再把石块移到船面上方松手，观察金色50%水线。铜块是可选过载实验；取出铜块即可恢复。";
        PositionStatus(level);
        ReportIntuitivePresentation.ConfigureScene(level);
        AssetDatabase.SaveAssets();
    }
    public static void PositionStatus(DemoLevel level)
    {
        if(EditorSceneManager.GetActiveScene().path!=ScenePath)return;
        var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        level.stations[0].label.transform.position=boat.transform.TransformPoint(new Vector3(.15f,2.45f,.55f));
        level.stations[0].label.characterSize=.017f;
    }
    [MenuItem("Report VR/Water/Simplify Second Level Loading")]
    public static void Scene()
    {
        EditorSceneManager.OpenScene(ScenePath);Configure(Object.FindObjectOfType<ReportBuoyancyMeasure>());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());Debug.Log("CARGO_SCENE_OK");
    }
    public static void Verify(){Scene();CargoLoadingCheck.Verify();WaterExperienceCheck.Buoyancy();BoatDisplayCheck.Verify();}
    public static void VerifyAndPreview(){Verify();Preview();}
    public static void Preview()
    {
        EditorSceneManager.OpenScene(ScenePath);var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();
        foreach(var renderer in Object.FindObjectOfType<DesktopPlayer>().GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var view=boat.transform.position-boat.transform.forward*1.4f+boat.transform.right*.45f+Vector3.up*1.9f;
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        foreach(var text in Object.FindObjectsOfType<TextMesh>())
        {
            text.font=font;text.GetComponent<Renderer>().sharedMaterial=font.material;
            var facing=text.GetComponent<ReportTextFacing>();if(facing!=null && facing.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));
        }
        boat.readout.text="加配重会下沉，取出配重会上浮\n先放木块，观察白色水线和后方大船";
        ReportLevelAudit.Photograph("Documentation/CargoReview/loading.png",view,boat.transform.position+Vector3.up*1.15f,false,2);
        var wide=new Vector3(-4.5f,3.8f,12.5f);
        foreach(var text in Object.FindObjectsOfType<TextMesh>())
        {
            var facing=text.GetComponent<ReportTextFacing>();if(facing!=null && facing.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-wide,Vector3.up));
        }
        var display=boat.GetComponent<ReportBoatDisplay>();
        System.Action<Component,string> call=(value,method)=>value.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(value,null);
        foreach(var seat in boat.seats)call(seat,"Awake");call(boat,"Awake");call(display,"Awake");
        for(int stage=0;stage<3;stage++)
        {
            if(stage==1)for(int i=0;i<2;i++){boat.seats[i].item.transform.position=boat.seats[i].transform.position;call(boat.seats[i],"FixedUpdate");}
            if(stage==2){boat.seats[2].item.transform.position=boat.seats[2].transform.position;call(boat.seats[2],"FixedUpdate");}
            for(int i=0;i<300;i++){call(boat,"FixedUpdate");foreach(var seat in boat.seats)if(seat.Loaded)call(seat,"FixedUpdate");}
            call(display,"LateUpdate");
            ReportLevelAudit.Photograph("Documentation/CargoReview/boat-"+stage+".png",wide,new Vector3(-4,1.05f,3.1f),false,2);
        }
        Debug.Log("CARGO_PREVIEW_OK");
    }
    public static void Build(){Scene();SetupReportVR.BuildWindows();Debug.Log("CARGO_BUILD_OK");}
}
