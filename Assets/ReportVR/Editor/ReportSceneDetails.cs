using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

// Additive, handmade scenery. The existing routes and experiments are preserved.
public static class ReportSceneDetails
{
    const string RootName="Report Details · 自建主题小景";
    static Transform root;
    static DemoLevel level;
    static string key;
    static int model;
    static readonly List<string> report=new List<string>();
    static bool Room(int i)=>new[]{4,5,6,8,9,13}.Contains(i);

    [MenuItem("Chrono Pivot/场景细节/丰富全部15关")]
    public static void Apply()
    {
        report.Clear();Directory.CreateDirectory("Documentation/Verification");var usedMeshes=new HashSet<string>();
        foreach(string path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);key=Path.GetFileNameWithoutExtension(path);model=0;
            level=Object.FindObjectOfType<DemoLevel>();
            string gameplay=GameplayState();
            var previous=GameObject.Find(RootName);if(previous!=null)Object.DestroyImmediate(previous);
            root=new GameObject(RootName).transform;
            int index=Array.IndexOf(ReportLevelAudit.Scenes(),path);
            if(Room(index))ReportInteriorDetails.Add(index,level,root);else Outdoors(index);
            // Remove a proposed decoration if its visible geometry crowds a route or apparatus.
            foreach(Transform group in root.Cast<Transform>().ToArray())
            {
                if(group.GetComponentsInChildren<Renderer>().Any(r=>CrowdsRoute(r.bounds) || CrowdsExperiment(r.bounds)))
                {
                    report.Add(key+": omitted crowded proposal "+group.name);
                    Object.DestroyImmediate(group.gameObject);
                }
            }
            if(gameplay!=GameplayState())throw new InvalidOperationException(key+": gameplay changed during detail authoring.");
            if(root.childCount==0)throw new InvalidOperationException(key+": no detail groups survived clearance checks.");
            int triangles=root.GetComponentsInChildren<MeshFilter>().Sum(f=>f.sharedMesh.triangles.Length/3);
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())usedMeshes.Add(AssetDatabase.GetAssetPath(filter.sharedMesh));
            report.Add(key+": groups="+root.childCount+" triangles="+triangles+"; existing physics and experiment state unchanged.");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/ReportVR/LevelModels"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            if(Path.GetFileName(path).StartsWith("Details_") && !usedMeshes.Contains(path))AssetDatabase.DeleteAsset(path);
        }
        AssetDatabase.SaveAssets();Validate();
        File.WriteAllLines("Documentation/Verification/scene-details.txt",report);
        EditorSceneManager.OpenScene(ReportLevelAudit.Scenes()[0]);
        Debug.Log("REPORT_SCENE_DETAILS_APPLIED_OK scenes=15");
    }
    static string GameplayState()
    {
        var components=Object.FindObjectsOfType<Component>(true).Where(c=>c is DemoLevel || c is Collider || c is Rigidbody || c is ReportGrabbable || c is ReportLearningMeasure);
        return string.Join("\n",components.OrderBy(c=>c.GetInstanceID()).Select(c=>c.GetInstanceID()+" "+EditorJsonUtility.ToJson(c)));
    }
    static bool CrowdsRoute(Bounds bounds)
    {
        bounds.Expand(new Vector3(.85f,0,.85f));
        for(int segment=1;segment<level.route.Length;segment++)
        {
            Vector3 a=level.route[segment-1],b=level.route[segment];int samples=Mathf.CeilToInt(Vector3.Distance(a,b)/.2f);
            for(int sample=0;sample<=samples;sample++)
            {
                var p=Vector3.Lerp(a,b,(float)sample/Mathf.Max(1,samples));
                if(p.x>=bounds.min.x && p.x<=bounds.max.x && p.z>=bounds.min.z && p.z<=bounds.max.z && bounds.max.y>p.y+.12f && bounds.min.y<p.y+2.15f)return true;
            }
        }
        return false;
    }
    static bool CrowdsExperiment(Bounds bounds)
    {
        var layout=level.transform.Find("Natural Learning Layout");if(layout==null)return false;
        bounds.Expand(new Vector3(.25f,.1f,.25f));
        return layout.GetComponentsInChildren<MeshRenderer>().Any(r=>r.enabled && r.gameObject.activeInHierarchy && bounds.Intersects(r.bounds));
    }
    public static void Validate()
    {
        int groups=0,triangles=0;
        foreach(string path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);level=Object.FindObjectOfType<DemoLevel>();var details=GameObject.Find(RootName);
            if(details==null || details.transform.childCount==0 || details.GetComponentsInChildren<Collider>(true).Length>0)throw new InvalidOperationException(path+": missing details or added collider.");
            foreach(var filter in details.GetComponentsInChildren<MeshFilter>())
            {
                var renderer=filter.GetComponent<MeshRenderer>();
                if(filter.sharedMesh==null || renderer.sharedMaterials.Any(m=>m==null))throw new InvalidOperationException(path+": invalid decorative asset.");
                if(CrowdsRoute(renderer.bounds) || CrowdsExperiment(renderer.bounds))throw new InvalidOperationException(path+": decoration crowds route/apparatus: "+filter.name);
                triangles+=filter.sharedMesh.triangles.Length/3;
            }
            groups+=details.transform.childCount;
        }
        Debug.Log("REPORT_SCENE_DETAILS_VALIDATED_OK scenes=15 groups="+groups+" triangles="+triangles+" colliders=0 route/apparatus overlap=0");
    }
    static void Place(LevelModelBuilder mesh,string name,Vector3 position,float yaw=0)
    {
        var go=mesh.Save("Details_"+key+"_"+(model++)+"_"+name,root,false);
        go.transform.SetPositionAndRotation(position,Quaternion.Euler(0,yaw,0));
    }
    static Vector3 Ground(float x,float z)
    {
        var p=new Vector3(x,0,z);
        var hits=Physics.RaycastAll(p+Vector3.up*20,Vector3.down,40,~0,QueryTriggerInteraction.Ignore).Where(h=>h.normal.y>.7f && h.point.y<.55f).OrderByDescending(h=>h.point.y).ToArray();
        if(hits.Length>0)p.y=hits[0].point.y;
        return p;
    }
    static void Outdoors(int index)
    {
        bool snow=index==7 || index==12,garden=index==10 || index==14;
        if(snow)
        {
            SnowShelter(Ground(-14,-13.5f),15);SnowShelter(Ground(14,-13.5f),-15);
            Supply(Ground(-10,8),true);Supply(Ground(13,5),true);
            PlantBed(Ground(-11,1),true);PlantBed(Ground(11,12),true);
            for(int i=0;i<3;i++)SnowDrift(Ground(-13+i*12,-13),i);
            Backdrop(Ground(0,-16),true,28);
        }
        else
        {
            float rear=garden?-15:-12.5f;
            Backdrop(Ground(0,rear),false,garden?28:23);
            Pergola(Ground(garden?-15:index==0?-10.5f:-13.5f,-3.5f),18,index==2);
            Pergola(Ground(garden?15:13,-10.5f),-15,false);
            PlantBed(Ground(garden?-13:-10,3),false);
            PlantBed(Ground(garden?13:10,-1),false);
            RockGarden(Ground(garden?14:11,7),index);
            RockGarden(Ground(garden?-15:-11,-9),index+1);
            Lantern(Ground(garden?11:8.8f,5.2f),false);
            Lantern(Ground(garden?-12:-8.8f,5.2f),false);
            if(index==1 || index==10)Supply(Ground(garden?12:-10,9),false);
            if(index==3 || index==11)StarStudy(Ground(-9.8f,-3),index==3);
        }
    }
    static void Cylinder(LevelModelBuilder m,Vector3 c,float bottom,float top,float height,string material,int sides=8)
    {
        var mat=LevelModelBuilder.Mat(material);
        for(int i=0;i<sides;i++)
        {
            float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
            Vector3 p=c+new Vector3(Mathf.Cos(a)*bottom,-height*.5f,Mathf.Sin(a)*bottom),q=c+new Vector3(Mathf.Cos(b)*bottom,-height*.5f,Mathf.Sin(b)*bottom),r=c+new Vector3(Mathf.Cos(b)*top,height*.5f,Mathf.Sin(b)*top),s=c+new Vector3(Mathf.Cos(a)*top,height*.5f,Mathf.Sin(a)*top);
            m.Face(mat,p,s,r,q);m.Face(mat,c-Vector3.up*height*.5f,p,q);m.Face(mat,c+Vector3.up*height*.5f,r,s);
        }
    }
    static void Rock(LevelModelBuilder m,Vector3 c,Vector3 size,string material)
    {
        var mat=LevelModelBuilder.Mat(material);var ring=new Vector3[7];
        for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;ring[i]=c+new Vector3(Mathf.Cos(a)*size.x,-size.y*.24f,Mathf.Sin(a)*size.z);}
        for(int i=0;i<7;i++){int n=(i+1)%7;m.Face(mat,ring[i],c+new Vector3(size.x*.18f,size.y,0),ring[n]);m.Face(mat,ring[n],c-Vector3.up*size.y*.65f,ring[i]);}
    }
    static void Roof(LevelModelBuilder m,Vector3 c,float width,float depth,bool snow)
    {
        float rise=width*.155f;
        for(int side=-1;side<=1;side+=2)
        {
            var a=c+new Vector3(0,rise,0);var b=c+new Vector3(side*width*.5f,0,0);
            m.Ramp(a,b,depth,"InkTeal");
            if(snow)m.Ramp(a+Vector3.up*.08f,b+Vector3.up*.08f,depth*.94f,"SnowIvory");
            for(int tile=0;tile<7;tile++)m.Beam(a+Vector3.forward*(-depth*.47f+tile*depth*.94f/6),b+Vector3.forward*(-depth*.47f+tile*depth*.94f/6),.035f,"SlateBlue");
            m.Beam(b+new Vector3(0,.04f,-depth*.5f),b+new Vector3(0,.04f,depth*.5f),.10f,"InkTeal");
        }
        m.Beam(c+new Vector3(0,rise+.03f,-depth*.53f),c+new Vector3(0,rise+.03f,depth*.53f),.12f,"InkTeal");
    }
    static void Pergola(Vector3 p,float yaw,bool music)
    {
        var m=new LevelModelBuilder();m.Box(new Vector3(0,.11f,0),new Vector3(3.8f,.22f,2.4f),"Limestone");
        foreach(float x in new[]{-1.55f,1.55f})foreach(float z in new[]{-.82f,.82f})
        {m.Box(new Vector3(x,1.42f,z),new Vector3(.13f,2.62f,.13f),"WarmWood");m.Box(new Vector3(x,.29f,z),new Vector3(.28f,.22f,.28f),"SageStone");}
        m.Box(new Vector3(0,2.64f,0),new Vector3(3.55f,.15f,2),"WarmWood");Roof(m,new Vector3(0,2.76f,0),4,2.8f,false);
        for(int i=0;i<7;i++)m.Box(new Vector3(-1.4f+i*.46f,1.64f,-.85f),new Vector3(.055f,1.45f,.055f),"WarmWood");
        m.Box(new Vector3(0,1.64f,-.85f),new Vector3(2.9f,.05f,.05f),"SatinBrass");
        m.Box(new Vector3(0,.60f,-.53f),new Vector3(2.5f,.14f,.48f),"WarmWood");
        foreach(float x in new[]{-1,1})m.Box(new Vector3(x,.36f,-.53f),new Vector3(.11f,.48f,.32f),"InkTeal");
        if(music)for(int i=0;i<5;i++){float x=-.6f+i*.3f; m.Beam(new Vector3(x,2.58f,0),new Vector3(x,2.28f-i*.06f,0),.018f,"SatinBrass");Cylinder(m,new Vector3(x,2.18f-i*.06f,0),.09f,.045f,.2f,"SatinBrass");}
        Place(m,music?"听风廊与铜铃":"休憩花廊",p,yaw);
    }
    static void Backdrop(Vector3 p,bool snow,float width)
    {
        var m=new LevelModelBuilder();
        for(int i=0;i<8;i++)
        {
            float x=(i-3.5f)*width/8;
            m.Box(new Vector3(x,.19f,0),new Vector3(width/8,.38f,.64f),snow?"SnowIvory":"SageStone");
            if(snow)
            {
                for(int peg=0;peg<6;peg++)m.Box(new Vector3(x+(peg-2.5f)*.43f,.87f,0),new Vector3(.11f,1.45f,.12f),"WarmWood");
                m.Box(new Vector3(x,.55f,-.06f),new Vector3(width/8,.1f,.12f),"WarmWood");m.Box(new Vector3(x,1.25f,-.06f),new Vector3(width/8,.1f,.12f),"WarmWood");
            }
            else
            {
                m.Box(new Vector3(x,.88f,0),new Vector3(width/8-.05f,1.4f,.24f),"Ivory");
                m.Box(new Vector3(x,1.63f,0),new Vector3(width/8,.12f,.4f),"InkTeal");
                m.Box(new Vector3(x,1.62f,0),new Vector3(.19f,.18f,.5f),"SlateBlue");
                m.Box(new Vector3(x,1.08f,.13f),new Vector3(1.35f,.65f,.05f),"InkTeal");
                for(int lattice=0;lattice<5;lattice++)m.Box(new Vector3(x-.52f+lattice*.26f,1.08f,.17f),new Vector3(.04f,.57f,.04f),"WarmWood");
                m.Box(new Vector3(x,1.08f,.17f),new Vector3(1.28f,.035f,.04f),"SatinBrass");
            }
        }
        Place(m,snow?"远侧木围栏":"园墙与漏窗",p);
    }
    static void PlantBed(Vector3 p,bool snow)
    {
        var m=new LevelModelBuilder();m.Box(new Vector3(0,.18f,0),new Vector3(3.6f,.36f,1.75f),snow?"SnowIvory":"Limestone");
        m.Box(new Vector3(0,.38f,0),new Vector3(3.3f,.08f,1.45f),snow?"SnowIvory":"Meadow");
        for(int i=0;i<3;i++)
        {
            float x=(i-1)*1.06f;
            if(snow)
            {Cylinder(m,new Vector3(x,1.1f,0),.10f,.07f,1.5f,"WarmWood");for(int tier=0;tier<3;tier++)Cylinder(m,new Vector3(x,1.3f+tier*.5f,0),.65f-tier*.14f,.01f,.95f,tier%2==0?"JadeLeaf":"SnowIvory");}
            else
            {
                for(int stem=0;stem<4;stem++)
                {
                    var c=new Vector3(x+(stem-1.5f)*.16f,1.1f,stem%2*.2f);Cylinder(m,c,.025f,.025f,1.35f,"JadeLeaf",5);
                    for(int node=0;node<3;node++)
                    {
                        float y=.72f+node*.36f;float side=(stem+node)%2==0?1:-1;
                        Cylinder(m,new Vector3(c.x,y,c.z),.031f,.031f,.025f,"LightLeaf",5);
                        m.Beam(new Vector3(c.x,y,c.z),new Vector3(c.x+side*.28f,y+.19f,c.z),.018f,"JadeLeaf");
                        Rock(m,new Vector3(c.x+side*.24f,y+.22f,c.z),new Vector3(.28f,.08f,.13f),"LightLeaf");
                    }
                }
                for(int bloom=0;bloom<4;bloom++)Rock(m,new Vector3(x+(bloom-1.5f)*.22f,.56f,-.52f),new Vector3(.15f,.12f,.13f),"PeachBlossom");
            }
        }
        Place(m,snow?"积雪松树花池":"层叠竹石花池",p,-12);
    }
    static void RockGarden(Vector3 p,int variation)
    {
        var m=new LevelModelBuilder();
        for(int i=0;i<5;i++){float x=(i-2)*.65f;Rock(m,new Vector3(x,.22f,Mathf.Sin(i+variation)*.42f),new Vector3(.48f,.28f+(i%3)*.2f,.4f),"SageStone");}
        for(int i=0;i<8;i++){var c=new Vector3((i%4-1.5f)*.52f,.13f,(i/4)*1.2f-.55f);Rock(m,c,new Vector3(.25f,.25f,.18f),i%2==0?"JadeLeaf":"LightLeaf");}
        Place(m,"石景与地被",p,variation*37);
    }
    static void Lantern(Vector3 p,bool snow)
    {
        var m=new LevelModelBuilder();m.Box(new Vector3(0,.09f,0),new Vector3(.65f,.18f,.65f),"SageStone");
        m.Box(new Vector3(0,.65f,0),new Vector3(.2f,1.1f,.2f),"SageStone");
        m.Box(new Vector3(0,1.3f,0),new Vector3(.38f,.35f,.38f),"AmberLight");
        foreach(float x in new[]{-.24f,.24f})foreach(float z in new[]{-.24f,.24f})m.Box(new Vector3(x,1.3f,z),new Vector3(.055f,.5f,.055f),"InkTeal");
        Roof(m,new Vector3(0,1.56f,0),.85f,.8f,snow);Place(m,"石灯笼",p);
    }
    static void Supply(Vector3 p,bool snow)
    {
        var m=new LevelModelBuilder();m.Box(new Vector3(0,.1f,0),new Vector3(2.4f,.2f,1.7f),snow?"SnowIvory":"Limestone");
        for(int barrel=0;barrel<2;barrel++)
        {
            var c=new Vector3(-.55f+barrel*.65f,.63f,.16f);Cylinder(m,c,.25f,.23f,.83f,"WarmWood");
            foreach(float y in new[]{-.27f,.27f})Cylinder(m,c+Vector3.up*y,.26f,.26f,.06f,"SatinBrass");
        }
        m.Box(new Vector3(.74f,.42f,-.12f),new Vector3(.55f,.65f,.62f),"WarmWood");
        for(int slat=0;slat<4;slat++)m.Box(new Vector3(.74f,.19f+slat*.16f,.21f),new Vector3(.59f,.025f,.035f),"SatinBrass");
        for(int log=0;log<5;log++)Cylinder(m,new Vector3(-.66f+log*.28f,.31f,-.57f),.13f,.13f,.45f,"WarmWood",6);
        Place(m,snow?"营地补给与木柴":"水工储物与木桶",p,18);
    }
    static void SnowShelter(Vector3 p,float yaw)
    {
        var m=new LevelModelBuilder();m.Box(new Vector3(0,.14f,0),new Vector3(3.2f,.28f,2.4f),"SnowIvory");
        foreach(float x in new[]{-1.25f,1.25f})foreach(float z in new[]{-.8f,.8f})m.Box(new Vector3(x,1.34f,z),new Vector3(.14f,2.4f,.14f),"WarmWood");
        for(int i=0;i<7;i++)m.Box(new Vector3(0,.50f+i*.28f,-.82f),new Vector3(2.6f,.20f,.13f),"WarmWood");
        Roof(m,new Vector3(0,2.58f,0),3.55f,2.8f,true);
        m.Box(new Vector3(0,.75f,-.48f),new Vector3(2.3f,.13f,.5f),"WarmWood");
        foreach(float x in new[]{-1,1})m.Box(new Vector3(x,.43f,-.48f),new Vector3(.12f,.62f,.4f),"WarmWood");
        m.Box(new Vector3(-1.12f,2.08f,.7f),new Vector3(.22f,.35f,.22f),"AmberLight");Place(m,"雪檐休息棚",p,yaw);
    }
    static void SnowDrift(Vector3 p,int seed)
    {
        var m=new LevelModelBuilder();for(int i=0;i<5;i++)Rock(m,new Vector3((i-2)*.8f,0,Mathf.Sin(i+seed)),new Vector3(1.2f,.5f+(i%2)*.24f,1),"SnowIvory");Place(m,"柔缓积雪边坡",p);
    }
    static void StarStudy(Vector3 p,bool night)
    {
        var m=new LevelModelBuilder();m.Box(new Vector3(0,.12f,0),new Vector3(2.5f,.24f,2.1f),"Limestone");
        m.Box(new Vector3(0,.8f,0),new Vector3(1.5f,.12f,.65f),"InkTeal");foreach(float x in new[]{-.58f,.58f})m.Box(new Vector3(x,.46f,0),new Vector3(.11f,.68f,.5f),"SatinBrass");
        for(int i=0;i<3;i++)m.Box(new Vector3(-.32f,.91f+i*.05f,0),new Vector3(.48f,.05f,.35f),i%2==0?"Ivory":"WarmWood");
        Cylinder(m,new Vector3(.4f,1.1f,0),.18f,.18f,.5f,"SatinBrass");
        m.Box(new Vector3(0,1.8f,-.65f),new Vector3(1.65f,1.05f,.07f),"InkTeal");
        for(int i=0;i<7;i++){var c=new Vector3(-.65f+i*.2f,1.9f+Mathf.Sin(i*1.7f)*.31f,-.6f);Rock(m,c,new Vector3(.035f,.035f,.025f),"CyanLight");if(i<6)m.Beam(c,new Vector3(-.45f+i*.2f,1.9f+Mathf.Sin((i+1)*1.7f)*.31f,-.6f),.012f,"SatinBrass");}
        Place(m,night?"观星记录亭台":"星象展示与卷册",p,-15);
    }
}
