using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

// Decorative native geometry only. Does not replace apparatus, lights, physics or routes.
public static class EnhanceReportAtmosphere
{
    const string Folder="Assets/ReportVR/Atmosphere";
    const string RootName="Report Atmosphere · 自建远景与细节";
    static string sceneKey;
    static Transform root;
    static ReportAtmosphereProfile profile;
    static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();

    [MenuItem("Chrono Pivot/视觉氛围/应用全部15关")]
    public static void Apply()
    {
        Directory.CreateDirectory(Folder+"/Meshes");Directory.CreateDirectory(Folder+"/Materials");Directory.CreateDirectory(Folder+"/Profiles");
        AssetDatabase.Refresh();
        var paths=ReportLevelAudit.Scenes();
        if(paths.Length!=15)throw new InvalidOperationException("Expected the 15 report scenes.");
        for(int index=0;index<paths.Length;index++)
        {
            EditorSceneManager.OpenScene(paths[index]);sceneKey=Path.GetFileNameWithoutExtension(paths[index]);
            var previous=GameObject.Find(RootName);if(previous!=null)Object.DestroyImmediate(previous);
            root=new GameObject(RootName).transform;
            string style=index==3 || index==11?"Twilight":index==7 || index==12?"Snow":IsRoom(index)?"Laboratory":"Garden";
            profile=Profile(style);materials.Clear();
            var level=Object.FindObjectOfType<DemoLevel>();
            if(level==null)throw new InvalidOperationException(sceneKey+" has no DemoLevel.");
            ConfigureSky(style);
            GiantDial(level);
            Mountains(index);
            if(IsRoom(index))RoomDetails(index,level);else Planting(index,level);
            foreach(var camera in Object.FindObjectsOfType<Camera>(true))
            {camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=Mathf.Max(camera.farClipPlane,190);}
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();Validate();
        EditorSceneManager.OpenScene(paths[0]);
        Debug.Log("REPORT_ATMOSPHERE_APPLIED_OK 15 · Native meshes, four skies, no gameplay colliders.");
    }
    static bool IsRoom(int i)=>i==4 || i==5 || i==6 || i==8 || i==9 || i==13;
    static ReportAtmosphereProfile Profile(string style)
    {
        string path=Folder+"/Profiles/"+style+".asset";
        var p=AssetDatabase.LoadAssetAtPath<ReportAtmosphereProfile>(path);if(p!=null)return p;
        p=ScriptableObject.CreateInstance<ReportAtmosphereProfile>();p.name=style;
        if(style=="Twilight")
        {
            p.zenith=new Color(.13f,.20f,.39f);p.horizon=new Color(.65f,.60f,.74f);p.clouds=new Color(.66f,.66f,.83f);p.cloudCover=.18f;p.stars=.9f;
            p.distantMountain=new Color(.39f,.46f,.64f);p.nearbyMountain=new Color(.28f,.39f,.53f);p.dialFace=new Color(.34f,.44f,.60f);p.brass=new Color(.93f,.77f,.43f);
            p.fogColor=new Color(.53f,.59f,.72f);p.fogDensity=.003f;
        }
        else if(style=="Snow")
        {
            p.zenith=new Color(.33f,.62f,.81f);p.horizon=new Color(.84f,.90f,.98f);p.clouds=Color.white;p.cloudCover=.55f;
            p.distantMountain=new Color(.68f,.82f,.91f);p.nearbyMountain=new Color(.49f,.69f,.80f);p.dialFace=new Color(.72f,.84f,.9f);p.brass=new Color(.88f,.70f,.39f);
            p.foliage=new Color(.18f,.44f,.43f);p.foliageLight=new Color(.72f,.86f,.91f);p.blossom=new Color(.83f,.92f,.97f);p.stone=new Color(.71f,.84f,.92f);p.fogColor=new Color(.83f,.91f,.96f);p.seed=509;
        }
        else if(style=="Laboratory")
        {
            p.zenith=new Color(.38f,.70f,.86f);p.horizon=new Color(.93f,.94f,.89f);p.cloudCover=.3f;p.dialRadius=8.5f;p.fogDensity=.002f;
        }
        AssetDatabase.CreateAsset(p,path);return p;
    }
    static void ConfigureSky(string style)
    {
        var shader=Shader.Find("ChronoPivot/Faceted Sky");if(shader==null)throw new InvalidOperationException("Faceted sky shader failed to import.");
        string path=Folder+"/Materials/"+style+"_Sky.mat";var sky=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(sky==null){sky=new Material(shader);AssetDatabase.CreateAsset(sky,path);}
        sky.SetColor("_Zenith",profile.zenith);sky.SetColor("_Horizon",profile.horizon);sky.SetColor("_CloudColor",profile.clouds);sky.SetFloat("_CloudCover",profile.cloudCover);sky.SetFloat("_Stars",profile.stars);EditorUtility.SetDirty(sky);
        RenderSettings.skybox=sky;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=profile.fogColor;RenderSettings.fogDensity=profile.fogDensity;
        // Existing directional lights and their shadows are part of the experiments.
        // The sky is changed without changing those lights or the ambient intensity.
    }
    static Material Mat(string name,Color color,bool unlit=false)
    {
        if(materials.TryGetValue(name,out var m))return m;
        string path=Folder+"/Materials/"+profile.name+"_"+name+".mat";m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(m==null){m=new Material(Shader.Find(unlit?"Unlit/Color":"Standard"));AssetDatabase.CreateAsset(m,path);}
        m.color=color;if(!unlit){m.SetFloat("_Glossiness",.18f);m.SetFloat("_Metallic",name=="Brass"?.25f:0);}
        EditorUtility.SetDirty(m);materials[name]=m;return m;
    }
    static void GiantDial(DemoLevel level)
    {
        float r=profile.dialRadius;var mesh=new Geometry();var brass=Mat("Far Brass",profile.brass,true);var face=Mat("Dial Face",profile.dialFace,true);var inset=Mat("Dial Shade",profile.dialFace*.89f,true);
        mesh.Ring(Vector3.zero,r,r*.925f,.28f,72,brass);
        mesh.Ring(new Vector3(0,0,-.07f),r*.86f,r*.838f,.12f,72,brass);
        for(int i=0;i<12;i++)
        {
            float a=i*Mathf.PI*2/12,b=(i+1)*Mathf.PI*2/12;
            mesh.Face(i%2==0?face:inset,Vector3.zero,new Vector3(Mathf.Cos(a)*r*.91f,Mathf.Sin(a)*r*.91f,0),new Vector3(Mathf.Cos(b)*r*.91f,Mathf.Sin(b)*r*.91f,0));
        }
        for(int i=0;i<60;i++)
        {
            float a=i*360f/60;bool major=i%5==0;
            mesh.Box(Quaternion.Euler(0,0,a)*new Vector3(0,r*.785f,.10f),new Vector3(major?.11f:.045f,r*(major?.12f:.045f),.10f),brass,Quaternion.Euler(0,0,a));
        }
        mesh.Ring(new Vector3(0,0,.14f),r*.085f,r*.055f,.12f,12,brass);
        // A triangular gnomon projects from the engraved dial face.
        var a0=new Vector3(-.10f,0,.18f);var b0=new Vector3(-.10f,r*.49f,.18f);var c0=new Vector3(-.10f,r*.29f,r*.46f);Vector3 shift=new Vector3(.20f,0,0);
        mesh.Face(brass,a0,b0,c0);mesh.Face(brass,c0+shift,b0+shift,a0+shift);
        mesh.Face(brass,a0,a0+shift,b0+shift,b0);mesh.Face(brass,b0,b0+shift,c0+shift,c0);mesh.Face(brass,c0,c0+shift,a0+shift,a0);
        var go=mesh.Save("Sky Sundial · 巨型主题日晷",root,"SkyDial");
        Vector3 forward=Quaternion.Euler(0,level.spawnYaw,0)*Vector3.forward;Vector3 right=Vector3.Cross(Vector3.up,forward);
        go.transform.position=new Vector3(level.spawn.x,0,level.spawn.z)+forward*profile.dialDistance+right*profile.dialSideOffset+Vector3.up*profile.dialHeight;
        go.transform.rotation=Quaternion.LookRotation((level.spawn+Vector3.up*2-go.transform.position).normalized)*Quaternion.Euler(-8,0,-12);
    }
    static void Mountains(int index)
    {
        var rng=new System.Random(profile.seed+index*11);var mesh=new Geometry();var near=Mat("Mountain",profile.nearbyMountain,true);var far=Mat("Far Mountain",profile.distantMountain,true);var snow=Mat("Snow Peak",new Color(.94f,.98f,1f),true);
        for(int i=0;i<profile.mountainCount;i++)
        {
            float angle=(i+.22f)*Mathf.PI*2/profile.mountainCount;float distance=profile.mountainDistance+(i%2==0?0:12);
            Vector3 c=new Vector3(Mathf.Sin(angle)*distance,-1,Mathf.Cos(angle)*distance);
            float height=10+(float)rng.NextDouble()*10, radius=13+(float)rng.NextDouble()*6;
            var basePoints=new Vector3[7];var ridgePoints=new Vector3[7];Vector3 peak=c+new Vector3(radius*.18f,height,radius*-.12f);
            for(int s=0;s<7;s++){float a=s*Mathf.PI*2/7;basePoints[s]=c+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);ridgePoints[s]=Vector3.Lerp(basePoints[s],peak,.47f)+Vector3.up*((float)rng.NextDouble()-.5f)*1.3f;}
            for(int s=0;s<7;s++){int n=(s+1)%7;mesh.Face(i%2==0?near:far,basePoints[s],ridgePoints[s],ridgePoints[n],basePoints[n]);mesh.Face(index==7 || index==12?snow:far,ridgePoints[s],peak,ridgePoints[n]);}
        }
        mesh.Save("Layered Far Peaks · 环绕远山",root,"FarPeaks");
    }
    static float RouteDistance(DemoLevel level,Vector3 p)
    {
        float best=float.PositiveInfinity;
        for(int i=1;i<level.route.Length;i++)
        {Vector3 a=level.route[i-1],b=level.route[i];a.y=0;b.y=0;p.y=0;Vector3 v=b-a;float t=v.sqrMagnitude>.001f?Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude):0;best=Mathf.Min(best,Vector3.Distance(p,a+v*t));}
        return best;
    }
    static float Ground(Vector3 p)
    {return Physics.Raycast(p+Vector3.up*30,Vector3.down,out var hit,60,~0,QueryTriggerInteraction.Ignore)?hit.point.y:.03f;}
    static void Planting(int index,DemoLevel level)
    {
        for(int i=0;i<profile.plantingCount;i++)
        {
            float a=(i+.35f)*Mathf.PI*2/Mathf.Max(1,profile.plantingCount);float radius=index==10 || index==14?26:21;
            Vector3 p=new Vector3(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius);if(RouteDistance(level,p)<2.2f)continue;p.y=Ground(p);
            var mesh=new Geometry();var wood=Mat("Wood",profile.wood);var leaves=Mat("Leaves",profile.foliage);var light=Mat("Light Leaves",profile.foliageLight);var blossom=Mat("Blossom",profile.blossom);var stone=Mat("Stone",profile.stone);
            if(index==7 || index==12 || i%2==0)
            {
                mesh.Cylinder(new Vector3(0,1.35f,0),.17f,.17f,2.7f,6,wood);
                for(int layer=0;layer<3;layer++)mesh.Cylinder(new Vector3(0,1.45f+layer*.83f,0),1.35f-layer*.28f,.02f,1.6f,7,index==7 || index==12?light:layer%2==0?leaves:light);
            }
            else
            {
                mesh.Cylinder(new Vector3(0,1.1f,0),.14f,.10f,2.2f,6,wood);
                mesh.Beam(new Vector3(0,1.2f,0),new Vector3(.85f,2.1f,.1f),.10f,wood);mesh.Beam(new Vector3(0,1.5f,0),new Vector3(-.65f,2.3f,-.1f),.10f,wood);
                mesh.Octa(new Vector3(.8f,2.35f,.05f),new Vector3(1.25f,.68f,.9f),blossom);mesh.Octa(new Vector3(-.7f,2.55f,0),new Vector3(1.15f,.7f,.95f),blossom);mesh.Octa(new Vector3(.05f,3f,.08f),new Vector3(1.1f,.68f,.85f),light);
            }
            mesh.Octa(new Vector3(1.6f,.2f,.35f),new Vector3(.6f,.34f,.42f),stone);mesh.Octa(new Vector3(-1,.14f,-.5f),new Vector3(.48f,.24f,.32f),stone);
            for(int tuft=0;tuft<4;tuft++)mesh.Cylinder(new Vector3(-1.1f+tuft*.28f,.2f,.8f),.18f,.03f,.38f,5,light);
            var go=mesh.Save("Border Grove "+i+" · 外围树石",root,"Grove_"+i);go.transform.position=p;
        }
    }
    static void RoomDetails(int index,DemoLevel level)
    {
        float width=index==4?16:index==5 || index==6?18:index==8?21:22;
        float depth=index==4?12:index==5?13:index==6?14:index==8 || index==13?16:17;
        var mesh=new Geometry();var brass=Mat("Brass",profile.brass);var trim=Mat("Wall Teal",profile.foliage);var ivory=Mat("Ivory",profile.ivory);
        for(int side=-1;side<=1;side+=2)
        {
            float x=side*(width*.5f-.14f);
            mesh.Box(new Vector3(x,3.25f,0),new Vector3(.035f,.06f,depth*.84f),brass);
            for(int panel=0;panel<3;panel++)
            {
                float z=-depth*.27f+panel*depth*.27f;
                mesh.Box(new Vector3(x,2.88f,z),new Vector3(.045f,.52f,1.7f),trim);
                for(int bar=0;bar<4;bar++)mesh.Box(new Vector3(x-side*.025f,2.88f,z-.64f+bar*.42f),new Vector3(.035f,.48f,.035f),brass);
                mesh.Box(new Vector3(x-side*.025f,2.88f,z),new Vector3(.04f,.035f,1.65f),ivory);
            }
        }
        mesh.Save("High Wall Lattice · 高位黄铜格栅",root,"WallLattice");
        var ceiling=new Geometry();var ringMaterial=Mat("Brass",profile.brass);
        ceiling.Ring(Vector3.zero,1.05f,.97f,.04f,32,ringMaterial);
        for(int tick=0;tick<12;tick++)
        {float angle=tick*30f;ceiling.Box(Quaternion.Euler(0,0,angle)*new Vector3(0,.82f,0),new Vector3(.035f,.17f,.04f),ringMaterial,Quaternion.Euler(0,0,angle));}
        var medallion=ceiling.Save("Ceiling Time Medallion · 顶部时辰纹样",root,"CeilingMedallion");medallion.transform.position=new Vector3(0,3.60f,depth*.28f);medallion.transform.rotation=Quaternion.Euler(90,0,0);
        for(int side=-1;side<=1;side+=2)
        {
            Vector3 p=new Vector3(side*(width*.5f-.8f),.05f,depth*.30f);if(RouteDistance(level,p)<1.8f)continue;
            var planter=new Geometry();var leaves=Mat("Leaves",profile.foliage);var light=Mat("Light Leaves",profile.foliageLight);var wood=Mat("Wood",profile.wood);
            planter.Cylinder(new Vector3(0,.26f,0),.32f,.42f,.52f,8,ivory);planter.Cylinder(new Vector3(0,.50f,0),.44f,.44f,.05f,8,brass);
            planter.Cylinder(new Vector3(0,1.1f,0),.07f,.05f,1.15f,6,wood);
            for(int crown=0;crown<3;crown++)planter.Octa(new Vector3((crown%2==0?.22f:-.22f),1.1f+crown*.34f,0),new Vector3(.6f,.32f,.48f),crown%2==0?light:leaves);
            var go=planter.Save("Corner Planter "+side+" · 角落盆景",root,"RoomPlanter_"+side);go.transform.position=p;
        }
    }
    public static void Validate()
    {
        int count=0;
        foreach(var path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);var decor=GameObject.Find(RootName);var level=Object.FindObjectOfType<DemoLevel>();
            if(decor==null || decor.GetComponentsInChildren<Collider>(true).Length!=0)throw new InvalidOperationException(path+": missing atmosphere or a decorative collider.");
            if(RenderSettings.skybox==null || RenderSettings.skybox.shader.name!="ChronoPivot/Faceted Sky")throw new InvalidOperationException(path+": missing procedural sky.");
            foreach(var renderer in decor.GetComponentsInChildren<MeshRenderer>())
            {
                if(renderer.sharedMaterials.Any(m=>m==null) || renderer.GetComponent<MeshFilter>().sharedMesh==null)throw new InvalidOperationException(path+": unassigned decorative mesh/material.");
                if(renderer.name.StartsWith("Border Grove") || renderer.name.StartsWith("Corner Planter"))
                    if(RouteDistance(level,renderer.transform.position)<1.7f)throw new InvalidOperationException(path+": decoration too close to walking route.");
            }
            count++;
        }
        Debug.Log("REPORT_ATMOSPHERE_VALIDATED_OK "+count);
    }
    public static void CaptureAll()
    {
        const string target="Documentation/LevelReview/Atmosphere";
        int captured=0,sceneIndex=0;
        foreach(var path in ReportLevelAudit.Scenes())
        {
            EditorSceneManager.OpenScene(path);var level=Object.FindObjectOfType<DemoLevel>();string key=Path.GetFileNameWithoutExtension(path);
            var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
            var textMaterial=new Material(Object.FindObjectOfType<DemoInteractor>().worldText);
            var texts=Object.FindObjectsOfType<TextMesh>(true);var previousFonts=texts.Select(t=>t.font).ToArray();
            var previousMaterials=texts.Select(t=>t.GetComponent<Renderer>().sharedMaterials).ToArray();
            Action<Font> updateAtlas=f=>{if(f==font)textMaterial.mainTexture=font.material.mainTexture;};
            Font.textureRebuilt+=updateAtlas;
            try
            {
                foreach(var text in texts){text.font=font;text.GetComponent<Renderer>().sharedMaterial=textMaterial;}
                level.Refresh();
                foreach(var group in texts.GroupBy(t=>new {t.fontSize,t.fontStyle}))
                    font.RequestCharactersInTexture(string.Concat(group.Select(t=>t.text)),group.Key.fontSize,group.Key.fontStyle);
                textMaterial.mainTexture=font.material.mainTexture;
                Vector3 forward=Quaternion.Euler(0,level.spawnYaw,0)*Vector3.forward;
                ReportLevelAudit.Photograph(target+"/"+key+"_eye.png",level.spawn+Vector3.up*1.65f,level.spawn+Vector3.up*1.85f+forward*8,false,15);
                Vector3 overviewPosition=new Vector3(17,14,23),overviewLook=new Vector3(0,3,0);
                if(IsRoom(sceneIndex))
                {
                    float width=sceneIndex==4?16:sceneIndex==5 || sceneIndex==6?18:sceneIndex==8?21:22;
                    float depth=sceneIndex==4?12:sceneIndex==5?13:sceneIndex==6?14:sceneIndex==8 || sceneIndex==13?16:17;
                    overviewPosition=new Vector3(width*.34f,3.25f,depth*.32f);overviewLook=new Vector3(0,1.2f,-depth*.12f);
                }
                ReportLevelAudit.Photograph(target+"/"+key+"_overview.png",overviewPosition,overviewLook,false,20);captured+=2;
                var layout=level.transform.Find("Natural Learning Layout");
                var bench=layout==null?null:layout.GetComponentsInChildren<MeshFilter>().FirstOrDefault(f=>f.name.Contains("台架") || f.name.Contains("NaturalLauncherBench"));
                if(bench!=null)
                {
                    var apparatus=sceneIndex==6?Object.FindObjectOfType<ReportPendulumMeasure>().transform:bench.transform.parent;
                    // Show the apparatus from its rear/right side; the eye image shows instructions.
                    Vector3 position=apparatus.position+apparatus.forward*.95f+apparatus.right*.85f+Vector3.up*1.65f;
                    Vector3 focus=apparatus.position+Vector3.up*1.2f;
                    if(sceneIndex==6)
                    {
                        // Opposite side, above the 2.64m clue-board top, to include the pivot and bob.
                        position=apparatus.position+apparatus.forward*.65f-apparatus.right*1.35f+Vector3.up*2.85f;
                        focus=apparatus.position+Vector3.up*1.85f;
                    }
                    // Apparatus detail image: temporarily hide surrounding instruction boards.
                    // Eye/overview images above retain the full authored layout.
                    var signs=level.stations.SelectMany(s=>s.transform.Find("Puzzle Controls").GetComponentsInChildren<Renderer>())
                        .Concat(level.stations.Select(s=>s.label.GetComponent<Renderer>())).Distinct().ToArray();
                    var visible=signs.Select(r=>r.enabled).ToArray();
                    try
                    {
                        foreach(var sign in signs)sign.enabled=false;
                        ReportLevelAudit.Photograph(target+"/"+key+"_experiment.png",position,focus,false,15);captured++;
                    }
                    finally{for(int i=0;i<signs.Length;i++)signs[i].enabled=visible[i];}
                }
                else throw new InvalidOperationException(key+": no natural experiment bench to photograph.");
            }
            finally
            {
                Font.textureRebuilt-=updateAtlas;
                for(int i=0;i<texts.Length;i++){texts[i].font=previousFonts[i];texts[i].GetComponent<Renderer>().sharedMaterials=previousMaterials[i];}
                Object.DestroyImmediate(textMaterial);Object.DestroyImmediate(font);
            }
            sceneIndex++;
        }
        EditorSceneManager.OpenScene(ReportLevelAudit.Scenes()[0]);Debug.Log("REPORT_ATMOSPHERE_CAPTURED_OK "+captured);
    }
    sealed class Geometry
    {
        readonly List<Vector3> vertices=new List<Vector3>();readonly Dictionary<Material,List<int>> triangles=new Dictionary<Material,List<int>>();
        public void Face(Material m,params Vector3[] p)
        {int n=vertices.Count;vertices.AddRange(p);if(!triangles.ContainsKey(m))triangles[m]=new List<int>();triangles[m].AddRange(new[]{n,n+1,n+2});if(p.Length==4)triangles[m].AddRange(new[]{n,n+2,n+3});}
        public void Box(Vector3 c,Vector3 size,Material m,Quaternion? rotation=null)
        {
            Vector3 h=size*.5f;var q=rotation??Quaternion.identity;
            Vector3[] p={new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)};
            for(int i=0;i<8;i++)p[i]=c+q*p[i];
            foreach(var f in new[]{new[]{3,2,1,0},new[]{4,5,6,7},new[]{0,1,5,4},new[]{7,6,2,3},new[]{4,7,3,0},new[]{1,2,6,5}})Face(m,f.Select(i=>p[i]).ToArray());
        }
        public void Beam(Vector3 a,Vector3 b,float width,Material m)=>Box((a+b)*.5f,new Vector3(width,Vector3.Distance(a,b),width),m,Quaternion.FromToRotation(Vector3.up,b-a));
        public void Cylinder(Vector3 c,float bottom,float top,float height,int sides,Material m)
        {
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;
                Vector3 p=c+new Vector3(Mathf.Cos(a)*bottom,-height*.5f,Mathf.Sin(a)*bottom),q=c+new Vector3(Mathf.Cos(b)*bottom,-height*.5f,Mathf.Sin(b)*bottom),r=c+new Vector3(Mathf.Cos(b)*top,height*.5f,Mathf.Sin(b)*top),s=c+new Vector3(Mathf.Cos(a)*top,height*.5f,Mathf.Sin(a)*top);
                Face(m,p,s,r,q);Face(m,c-Vector3.up*height*.5f,p,q);Face(m,c+Vector3.up*height*.5f,r,s);
            }
        }
        public void Octa(Vector3 c,Vector3 scale,Material m)
        {
            var p=new[]{c+Vector3.right*scale.x,c+Vector3.forward*scale.z,c-Vector3.right*scale.x,c-Vector3.forward*scale.z};
            for(int i=0;i<4;i++){int n=(i+1)%4;Face(m,p[i],c+Vector3.up*scale.y,p[n]);Face(m,p[n],c-Vector3.up*scale.y,p[i]);}
        }
        public void Ring(Vector3 c,float outer,float inner,float depth,int sides,Material m)
        {
            for(int i=0;i<sides;i++)
            {
                float a=i*Mathf.PI*2/sides,b=(i+1)*Mathf.PI*2/sides;Vector3 p=c+new Vector3(Mathf.Cos(a)*outer,Mathf.Sin(a)*outer,depth*.5f),q=c+new Vector3(Mathf.Cos(b)*outer,Mathf.Sin(b)*outer,depth*.5f),r=c+new Vector3(Mathf.Cos(b)*inner,Mathf.Sin(b)*inner,depth*.5f),s=c+new Vector3(Mathf.Cos(a)*inner,Mathf.Sin(a)*inner,depth*.5f),back=Vector3.forward*-depth;
                Face(m,p,q,r,s);Face(m,s+back,r+back,q+back,p+back);Face(m,p+back,q+back,q,p);Face(m,s,r,r+back,s+back);
            }
        }
        public GameObject Save(string name,Transform parent,string key)
        {
            var mesh=new Mesh{name=sceneKey+"_"+key};mesh.SetVertices(vertices);mesh.subMeshCount=triangles.Count;int sub=0;foreach(var t in triangles.Values)mesh.SetTriangles(t,sub++);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string path=Folder+"/Meshes/"+mesh.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;EditorUtility.SetDirty(old);}else AssetDatabase.CreateAsset(mesh,path);
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterials=triangles.Keys.ToArray();renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return go;
        }
    }
}
