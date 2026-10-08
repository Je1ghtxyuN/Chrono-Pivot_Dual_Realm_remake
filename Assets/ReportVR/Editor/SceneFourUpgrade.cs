using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class SceneFourUpgrade
{
    const string Prefix="Scene4_Experience_";
    static void ReplaceMesh(GameObject target,GameObject generated)
    {
        var mesh=generated.GetComponent<MeshFilter>().sharedMesh;
        target.GetComponent<MeshFilter>().sharedMesh=mesh;target.GetComponent<Renderer>().sharedMaterials=generated.GetComponent<Renderer>().sharedMaterials;
        target.GetComponent<MeshCollider>().sharedMesh=mesh;Object.DestroyImmediate(generated);
    }
    static Vector3 Bowl(float radius,float angle,bool outer)
    {
        return new Vector3(Mathf.Sin(angle)*.13f*radius,.006f+.075f*radius*radius-(outer?.012f:0),-.10f+Mathf.Cos(angle)*.18f*radius);
    }
    static void Spoon(ReportCompassMeasure compass)
    {
        var model=new LevelModelBuilder();
        for(int layer=0;layer<5;layer++)for(int i=0;i<24;i++)
        {
            float a=i*Mathf.PI*2/24,b=(i+1)*Mathf.PI*2/24,r0=layer/5f,r1=(layer+1)/5f;
            foreach(bool outer in new[]{false,true})
            {
                var points=new[]{Bowl(r0,a,outer),Bowl(r1,a,outer),Bowl(r1,b,outer),Bowl(r0,b,outer)};
                if(outer)System.Array.Reverse(points);
                model.Face(LevelModelBuilder.Mat(outer?"SatinBrass":"Copper"),points);
            }
            if(layer==4)model.Face(LevelModelBuilder.Mat("SatinBrass"),Bowl(1,a,false),Bowl(1,a,true),Bowl(1,b,true),Bowl(1,b,false));
        }
        model.Box(new Vector3(0,.081f,.23f),new Vector3(.055f,.025f,.30f),"SatinBrass",Quaternion.Euler(-7,0,0));
        var needle=compass.needle;ReplaceMesh(needle.gameObject,model.Save(Prefix+"Spoon",needle.transform.parent,false));needle.name="司南勺子";
        var old=GameObject.Find("CompassSpoon pivot");if(old!=null)old.SetActive(false);
    }
    static void Sky(ReportSightMeasure sight,DemoLevel level)
    {
        var old=GameObject.Find("SevenStarConstellation");if(old!=null)old.SetActive(false);
        var previous=level.transform.Find("联动星空");if(previous!=null)Object.DestroyImmediate(previous.gameObject);
        var root=new GameObject("联动星空").transform;root.SetParent(level.transform,false);root.position=new Vector3(sight.transform.position.x,0,sight.transform.position.z);
        Vector3[] stars={new Vector3(-4,13,12),new Vector3(-2,12.6f,12),new Vector3(0,12,12),new Vector3(2,11.2f,12),new Vector3(3.4f,11.8f,12),new Vector3(3.8f,13.3f,12),new Vector3(2.2f,13,12)};
        var model=new LevelModelBuilder();foreach(var p in stars)
        {
            var top=p+Vector3.up*.18f;var bottom=p-Vector3.up*.18f;
            var edges=new[]{p+Vector3.forward*.14f,p+Vector3.right*.14f,p-Vector3.forward*.14f,p-Vector3.right*.14f};
            for(int i=0;i<4;i++){int j=(i+1)%4;model.Face(LevelModelBuilder.Mat("Ivory"),top,edges[j],edges[i]);model.Face(LevelModelBuilder.Mat("Ivory"),bottom,edges[i],edges[j]);}
        }
        for(int i=0;i<6;i++)model.Beam(stars[i],stars[i+1],.035f,"CyanLight");model.Beam(stars[6],stars[3],.035f,"CyanLight");
        model.Save(Prefix+"北斗七星",root,false);
        var dial=sight.GetComponent<ReportStarDial>();if(dial==null)dial=sight.gameObject.AddComponent<ReportStarDial>();dial.ring=sight.ring;dial.skyRoot=root;sight.ring.name="星盘转环";
    }
    static void Mirrors(ReportMirrorMeasure measure)
    {
        var legacy=GameObject.Find("AdjustableBronzeMirror pivot");if(legacy!=null)legacy.SetActive(false);
        measure.mirror.name="一号铜镜";
        if(measure.secondMirror==null)measure.secondMirror=Object.Instantiate(measure.mirror.gameObject,measure.transform).GetComponent<ReportMechanismHandle>();
        measure.secondMirror.name="二号铜镜";measure.secondMirror.startingValue=45;
        measure.mirror.transform.localPosition=new Vector3(-.20f,1.4f,-.22f);
        measure.secondMirror.transform.localPosition=new Vector3(-.20f,1.4f,.42f);
        measure.emitter.localPosition=new Vector3(-.75f,1.4f,-.22f);
        measure.receiver.transform.localPosition=new Vector3(.50f,1.4f,.42f);
        measure.receiver.transform.localRotation=Quaternion.Euler(0,90,0);
        if(measure.secondNormalLine==null)
        {
            measure.secondNormalLine=Object.Instantiate(measure.normalLine,measure.normalLine.transform.parent);measure.secondNormalLine.name="Second mirror normal";
        }
        // A low support plate makes the two-stage apparatus visibly one experiment.
        var old=measure.transform.Find(Prefix+"OpticalBase");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var baseModel=new LevelModelBuilder();baseModel.Box(new Vector3(-.05f,1.1f,.10f),new Vector3(1.55f,.035f,1.0f),"InkTeal");baseModel.Save(Prefix+"OpticalBase",measure.transform,false);
    }
    public static void Apply()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[3].path);var level=Object.FindObjectOfType<DemoLevel>();
        Spoon(Object.FindObjectOfType<ReportCompassMeasure>());Sky(Object.FindObjectOfType<ReportSightMeasure>(),level);Mirrors(Object.FindObjectOfType<ReportMirrorMeasure>());
        SetText(level);level.grabbables=Object.FindObjectsOfType<ReportGrabbable>();level.Refresh();
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("SCENE4_UPGRADE_OK");
    }
    public static void SetText(DemoLevel level)
    {
        level.mission="先拨动司南勺子看它回南，再转星盘观察星空与北斗旋转，最后用两面铜镜接通光路。";
        var compass=level.stations[0];compass.instruction=compass.puzzle.goal="拨动勺子，观察回南";
        compass.puzzle.successMessage="已观察：勺柄回指南方并逐渐停稳。\n接着：转星盘，观察星空与北斗旋转。";
        var sight=level.stations[1];sight.instruction=sight.puzzle.goal="转星盘，观察星空旋转";
        sight.puzzle.clue="星盘带动示教星空旋转，方位参照保持固定";sight.puzzle.hint="沿圆周拖动星盘，抬头观察北斗七星；让红线指向金色星标，再松手。";
        sight.puzzle.successMessage="已观察：星盘带动天空与北斗旋转。\n接着：调节两面铜镜，接通光路。";
        var mirror=level.stations[2];mirror.instruction=mirror.puzzle.goal="调两面镜，让光到达接收器";
        mirror.puzzle.clue="光依次经过两面镜反射，方向由各镜面决定";mirror.puzzle.hint="先调一号镜，让光照到二号镜；再调二号镜，让光落到接收器。亮绿后松开两面镜等待稳定。";
        mirror.puzzle.successMessage="已观察：光经过两次反射到达接收器。\n本关实验完成，沿步道前往出口。";
    }
}
