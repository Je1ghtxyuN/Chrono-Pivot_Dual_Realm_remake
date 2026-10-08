using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportWaterUpgrade
{
    const string Folder="Assets/ReportVR/Water";
    static string key;
    static int number;
    static readonly List<string> changes=new List<string>();
    [MenuItem("Report VR/Water/Upgrade All Water")]
    public static void All()
    {
        changes.Clear();Directory.CreateDirectory(Folder);Directory.CreateDirectory("Documentation/WaterReview");
        foreach(var scene in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            EditorSceneManager.OpenScene(scene.path);ApplyCurrentScene();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();File.WriteAllLines("Documentation/WaterReview/changes.txt",changes);
        Debug.Log("WATER_UPGRADE_OK scenes=15");
    }
    static Material Material(string name,Shader shader)
    {
        string path=Folder+"/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(shader){name=name};AssetDatabase.CreateAsset(material,path);}else material.shader=shader;
        return material;
    }
    static GameObject Box(Transform parent,string name,Vector3 center,Vector3 size,string material,bool collision=false)
    {
        var model=new LevelModelBuilder();model.Box(center,size,material);return model.Save(key+"_Water_"+name+"_"+number++,parent,collision);
    }
    static TextMesh Label(Transform parent,Vector3 position,string words,float size=.009f)
    {
        var go=new GameObject("Water explanation");go.transform.SetParent(parent,false);go.transform.localPosition=position;
        var text=go.AddComponent<TextMesh>();text.text=words;text.characterSize=size;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.92f,.84f,.52f);
        var interactor=Object.FindObjectOfType<DemoInteractor>();if(interactor!=null)go.GetComponent<Renderer>().sharedMaterial=interactor.worldText;
        var facing=go.AddComponent<ReportTextFacing>();facing.mode=ReportTextFacingMode.FacePlayer;facing.yawOnly=true;
        var player=Object.FindObjectOfType<DesktopPlayer>();if(player!=null)facing.viewer=player.head.transform;
        return text;
    }
    static Transform Group(Transform parent,string name)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
    static void Hydraulic(ReportHydraulicMeasure experiment,Material water)
    {
        var old=experiment.transform.Find("Water feedback");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var group=Group(experiment.transform,"Water feedback");
        var line=Group(group,"Visible liquid in pipe").gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=3;
        line.SetPositions(new[]{new Vector3(-.4f,1.05f,0),new Vector3(.4f,1.05f,0),new Vector3(.4f,1.15f,0)});line.startWidth=line.endWidth=.018f;line.sharedMaterial=water;line.generateLightingData=true;
        line.gameObject.AddComponent<ReportWaterSurface>().waveAmplitude=0;
        foreach(var renderer in experiment.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.Contains("封闭流路")))renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>LevelModelBuilder.Mat("SoftGlass")).ToArray();
        var emitter=Group(group,"Leak particles");emitter.localPosition=new Vector3(-.12f,1.05f,-.03f);emitter.localRotation=Quaternion.Euler(65,0,0);
        var particles=emitter.gameObject.AddComponent<ParticleSystem>();var main=particles.main;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startLifetime=.6f;main.startSpeed=1.2f;main.startSize=.026f;main.gravityModifier=.8f;main.maxParticles=40;
        var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.015f;shape.angle=8;
        var emission=particles.emission;emission.enabled=false;emission.rateOverTime=0;
        var collision=particles.collision;collision.enabled=true;collision.type=ParticleSystemCollisionType.World;collision.mode=ParticleSystemCollisionMode.Collision3D;collision.bounce=.08f;collision.lifetimeLoss=.6f;
        var material=Material("Leak droplets",Shader.Find("Legacy Shaders/Particles/Alpha Blended"));material.SetColor("_TintColor",new Color(.25f,.72f,.77f,.8f));
        string texturePath=Folder+"/Droplet.asset";var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if(texture==null)
        {
            texture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Droplet",wrapMode=TextureWrapMode.Clamp};
            var colors=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++){float distance=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;colors[y*32+x]=new Color(1,1,1,Mathf.Clamp01((1-distance)*5));}
            texture.SetPixels(colors);texture.Apply();AssetDatabase.CreateAsset(texture,texturePath);
        }
        material.mainTexture=texture;EditorUtility.SetDirty(material);particles.GetComponent<ParticleSystemRenderer>().sharedMaterial=material;
        var feedback=experiment.GetComponent<ReportHydraulicWaterFeedback>();if(feedback==null)feedback=experiment.gameObject.AddComponent<ReportHydraulicWaterFeedback>();feedback.experiment=experiment;feedback.leakage=particles;feedback.liquid=line;
        foreach(var renderer in Object.FindObjectsOfType<MeshRenderer>().Where(r=>r.name=="LeakDroplets"))renderer.enabled=false;
        changes.Add(key+" hydraulic: visible liquid in clear pipes; valve/actual seal control gravitational leakage particles");
    }
    static void Boat(ReportBuoyancyMeasure boat)
    {
        var level=Object.FindObjectOfType<DemoLevel>();
        var approach=Vector3.ProjectOnPlane(boat.transform.position-level.stations[0].viewingPoint,Vector3.up);
        if(approach.magnitude<2.1f)boat.transform.position+=approach.normalized*(2.1f-approach.magnitude);
        var old=boat.transform.Find("Water Experience");
        if(old!=null){if(boat.readout!=null && boat.readout.transform.IsChildOf(old))boat.readout.transform.SetParent(boat.transform,true);Object.DestroyImmediate(old.gameObject);}
        var rig=Group(boat.transform,"Water Experience");
        foreach(var renderer in boat.GetComponentsInChildren<MeshRenderer>())
            if(!renderer.transform.IsChildOf(rig) && (renderer.name.Contains("台架") || renderer.name.Contains("实验水面")))
            {renderer.enabled=false;foreach(var collider in renderer.GetComponents<Collider>())collider.enabled=false;}
        foreach(var renderer in boat.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.Contains("通航闸口")))
        {
            var filter=renderer.GetComponent<MeshFilter>();var source=filter.sharedMesh;var frame=Object.Instantiate(source);
            for(int sub=0;sub<frame.subMeshCount;sub++)if(renderer.sharedMaterials[sub].name.Contains("GlacierBlue") || renderer.sharedMaterials[sub].shader.name=="ChronoPivot/Water")frame.SetTriangles(new int[0],sub);
            string path=Folder+"/"+key+"_GateFrame.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(saved!=null){EditorUtility.CopySerialized(frame,saved);Object.DestroyImmediate(frame);frame=saved;EditorUtility.SetDirty(frame);}else AssetDatabase.CreateAsset(frame,path);
            filter.sharedMesh=frame;
        }
        foreach(var renderer in boat.hull.GetComponentsInChildren<MeshRenderer>())if(renderer.GetComponent<TextMesh>()==null)renderer.enabled=false;
        var oldMarks=boat.hull.Find("Visible draft marks");if(oldMarks!=null)Object.DestroyImmediate(oldMarks.gameObject);
        var hull=Group(boat.hull,"Visible draft marks");
        Box(hull,"sealed_float",Vector3.zero,new Vector3(.8f,.4f,.375f),"InkTeal");
        Box(hull,"deck",Vector3.up*.205f,new Vector3(.82f,.025f,.4f),"WarmWood");
        foreach(float z in new[]{-.19f,.19f})
        {
            Box(hull,"25_percent_mark",new Vector3(0,-.1f,z),new Vector3(.82f,.012f,.012f),"Ivory");
            Box(hull,"50_percent_mark",new Vector3(0,0,z),new Vector3(.82f,.015f,.014f),"SatinBrass");
            Box(hull,"100_percent_mark",new Vector3(0,.2f,z),new Vector3(.82f,.012f,.012f),"CoralRed");
        }
        boat.hullDepth=.4f;boat.maximumVolume=.12f;boat.hullMass=10;boat.waterDepth=.65f;
        boat.waterHeight=boat.transform.position.y+.98f;
        var position=boat.hull.localPosition;position.y=.98f+.2f-.4f/12;boat.hull.localPosition=position;
        float[] masses={20,30,90},sizes={.32f,.23f,.216f};string[] names={"木20kg","石30kg","铜90kg"};
        for(int i=0;i<boat.seats.Length;i++)
        {
            var seat=boat.seats[i];var item=seat.item;var renderer=item.GetComponent<Renderer>();float original=renderer.bounds.size.x;
            float ratio=sizes[i]/Mathf.Max(.001f,original);item.transform.localScale*=ratio;
            var supply=item.transform.localPosition;supply.y=.98f+sizes[i]*.5f;supply.z=-.60f;item.transform.localPosition=supply;
            item.GetComponent<Rigidbody>().mass=masses[i];
            seat.transform.localPosition=new Vector3((i-1)*.26f,.22f+sizes[i]*.5f,0);
            // Cancel inherited enlargement of a label, while retaining its world-readable size.
            var label=item.transform.Find("Puzzle Item Label");if(label!=null){label.localScale/=ratio;foreach(var text in label.GetComponentsInChildren<TextMesh>())text.text=names[i];}
        }
        foreach(var text in boat.GetComponentsInChildren<TextMesh>())
        {
            text.text=text.text.Replace("木4kg","木20kg").Replace("石6kg","石30kg").Replace("铜18kg","铜90kg");
            if(text.name.Contains("配重标记")){var p=text.transform.localPosition;p.z=-.60f;text.transform.localPosition=p;}
        }
        level.stations[0].puzzle.clue="封闭浮箱0.8×0.375×0.4m，最多排水120kg；船体10kg加木20kg，吃水25%。";
        level.stations[1].puzzle.clue="再加石30kg，总质量60kg，吃水50%；再加铜90kg将过载，卸下后重新浮起。";
        Box(rig,"basin_floor",new Vector3(.15f,.30f,0),new Vector3(2.2f,.06f,1),"Limestone",true);
        foreach(float z in new[]{-.48f,.48f})
        {
            Box(rig,"glass_side",new Vector3(.15f,.65f,z),new Vector3(2.2f,.66f,.025f),"SoftGlass",z>0);
            Box(rig,"brass_rim",new Vector3(.15f,1.01f,z),new Vector3(2.25f,.025f,.035f),"SatinBrass");
        }
        foreach(float x in new[]{-.96f,1.26f})Box(rig,"basin_end",new Vector3(x,.65f,0),new Vector3(.025f,.66f,1),"SoftGlass",true);
        foreach(float x in new[]{-.85f,1.1f})Box(rig,"leg",new Vector3(x,.15f,0),new Vector3(.08f,.30f,.75f),"InkTeal",true);
        Box(rig,"cargo_tray",new Vector3(0,.94f,-.60f),new Vector3(1.3f,.08f,.25f),"WarmWood",true);
        var plane=Group(rig,"Experiment water surface");var mesh=new Mesh{name=key+"_ExperimentWater"};
        var vertices=new List<Vector3>();var indices=new List<int>();const int columns=32,rows=12;
        for(int z=0;z<=rows;z++)for(int x=0;x<=columns;x++)vertices.Add(new Vector3(-.94f+2.18f*x/columns,.98f,-.46f+.92f*z/rows));
        for(int z=0;z<rows;z++)for(int x=0;x<columns;x++){int a=z*(columns+1)+x;indices.AddRange(new[]{a,a+columns+1,a+1,a+1,a+columns+1,a+columns+2});}
        mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        string meshPath=Folder+"/"+key+"_ExperimentWater.asset";var previous=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(previous!=null){EditorUtility.CopySerialized(mesh,previous);Object.DestroyImmediate(mesh);mesh=previous;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,meshPath);
        plane.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;plane.gameObject.AddComponent<MeshRenderer>();
        var surface=plane.gameObject.AddComponent<ReportWaterSurface>();surface.waveAmplitude=.003f;surface.flowDirection=new Vector2(boat.transform.right.x,boat.transform.right.z);boat.waterSurface=surface;
        Label(rig,new Vector3(.16f,1.32f,.46f),"装载下沉 · 卸载上浮\n白线25% / 金线50% / 红线满载",.011f);
        Box(rig,"draft_readout_board",new Vector3(.15f,1.85f,.58f),new Vector3(1.7f,.60f,.035f),"InkTeal");
        boat.readout.transform.SetParent(rig,false);boat.readout.transform.localPosition=new Vector3(.15f,1.85f,.55f);boat.readout.transform.localRotation=Quaternion.identity;
        boat.readout.color=new Color(.95f,.95f,.85f);boat.readout.characterSize=.0105f;
        var gauge=Group(rig,"Live draft gauge");gauge.localPosition=new Vector3(-1.02f,.38f,-.49f);
        Box(gauge,"gauge_back",Vector3.up*.2f,new Vector3(.035f,.42f,.012f),"Ivory");
        var bar=Group(gauge,"Displaced water");Box(bar,"gauge_fill",Vector3.zero,new Vector3(.04f,.4f,.014f),"Celadon");boat.draftGauge=bar;
        changes.Add(key+" float: 120L hull, 10kg boat, 20/30/90kg cargo, 0.10m change between 25% and 50%, inertia and visible draft marks");
        ReportCargoUpgrade.Configure(boat);
    }
    public static void ApplyCurrentScene()
    {
        Directory.CreateDirectory(Folder);key=Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().path);number=0;
        var shader=Shader.Find("ChronoPivot/Water");if(shader==null)throw new InvalidOperationException("Water shader missing");
        foreach(var boat in Object.FindObjectsOfType<ReportBuoyancyMeasure>())Boat(boat);
        bool hasWater=Object.FindObjectsOfType<MeshRenderer>(true).Any(r=>r.name=="Experiment water surface" || r.sharedMaterials.Any(m=>m!=null && (m.shader==shader || m.name.Contains("JadeWater") || m.name.Contains("GlacierBlue") && (r.name.Contains("Frozen") || r.name.Contains("冰样") || r.name.Contains("实验水面") || r.name.Contains("通航闸口")))));
        if(!hasWater)return;
        var water=Material(key+"_Liquid",shader);water.SetColor("_ShallowColor",new Color(.035f,.34f,.32f));water.SetColor("_DeepColor",new Color(.006f,.10f,.14f));water.SetFloat("_Opacity",.58f);water.SetFloat("_Clarity",.8f);water.SetFloat("_Ice",0);
        var ice=Material(key+"_Ice",shader);ice.SetColor("_ShallowColor",new Color(.36f,.62f,.69f));ice.SetColor("_DeepColor",new Color(.08f,.22f,.31f));ice.SetFloat("_Ice",1);ice.SetFloat("_WaveAmplitude",0);ice.SetFloat("_Opacity",.48f);
        foreach(var hydraulic in Object.FindObjectsOfType<ReportHydraulicMeasure>())Hydraulic(hydraulic,water);
        var surfaces=new List<MeshRenderer>();
        foreach(var renderer in Object.FindObjectsOfType<MeshRenderer>(true))
        {
            if(renderer.name.Contains("QuietWaterHighlights")){renderer.enabled=false;continue;}
            var materials=renderer.sharedMaterials;bool used=false,frozen=renderer.name.Contains("Frozen") || renderer.name.Contains("冰样");
            bool experiment=renderer.GetComponent<ReportWaterSurface>()!=null || renderer.name.Contains("实验水面") || renderer.name.Contains("通航闸口");
            for(int i=0;i<materials.Length;i++)
            {
                var m=materials[i];if(m==null)continue;
                if(m.shader==shader || m.name.Contains("JadeWater") || (m.name.Contains("GlacierBlue") && (frozen || experiment)))
                {materials[i]=frozen?ice:water;used=true;}
            }
            if(renderer.name=="Experiment water surface"){materials=new[]{water};used=true;}
            if(!used)continue;renderer.sharedMaterials=materials;
            var surface=renderer.GetComponent<ReportWaterSurface>();if(surface==null)surface=renderer.gameObject.AddComponent<ReportWaterSurface>();surface.frozen=frozen;
            if(!experiment){surface.waveAmplitude=renderer.name.Contains("Pond")?.006f:.009f;surface.flowSpeed=renderer.name.Contains("Pond")?.25f:.6f;}
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            if(renderer.enabled && renderer.gameObject.activeInHierarchy)surfaces.Add(renderer);
            changes.Add(key+" | "+renderer.name+" | "+(frozen?"stationary reflective ice":"transparent moving water"));
        }
        if(surfaces.Count>0)
        {
            var center=surfaces[0].bounds.center+Vector3.up*1.5f;
            var go=new GameObject("Water reflection capture");var camera=go.AddComponent<Camera>();camera.enabled=false;camera.transform.position=center;camera.clearFlags=CameraClearFlags.Skybox;camera.farClipPlane=120;camera.stereoTargetEye=StereoTargetEyeMask.None;
            var cube=new Cubemap(128,TextureFormat.RGBA32,true){name=key+"_Reflection"};
            foreach(var r in surfaces)r.enabled=false;
            try{if(!camera.RenderToCubemap(cube))throw new InvalidOperationException("Water reflection capture failed");}
            finally{foreach(var r in surfaces)r.enabled=true;Object.DestroyImmediate(go);}
            string cubePath=Folder+"/"+key+"_Reflection.asset";var oldCube=AssetDatabase.LoadAssetAtPath<Cubemap>(cubePath);
            if(oldCube!=null){EditorUtility.CopySerialized(cube,oldCube);Object.DestroyImmediate(cube);cube=oldCube;EditorUtility.SetDirty(cube);}else AssetDatabase.CreateAsset(cube,cubePath);
            water.SetTexture("_Environment",cube);ice.SetTexture("_Environment",cube);
        }
        foreach(var camera in Object.FindObjectsOfType<Camera>(true))camera.depthTextureMode|=DepthTextureMode.Depth;
        EditorUtility.SetDirty(water);EditorUtility.SetDirty(ice);
    }
    public static void Preview()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();var level=Object.FindObjectOfType<DemoLevel>();
        var player=Object.FindObjectOfType<DesktopPlayer>();foreach(var renderer in player.GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        foreach(var text in Object.FindObjectsOfType<TextMesh>()){text.font=font;text.GetComponent<Renderer>().sharedMaterial=font.material;}
        var view=boat.transform.position-boat.transform.forward*2.5f+Vector3.up*1.8f;
        foreach(var seat in boat.seats)seat.GetType().GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(seat,null);
        boat.GetType().GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(boat,null);
        foreach(var seat in boat.seats)seat.item.transform.position=seat.transform.position+Vector3.right*3;
        for(int stage=0;stage<4;stage++)
        {
            if(stage>0){var seat=boat.seats[stage-1];seat.item.transform.position=seat.transform.position;seat.GetType().GetMethod("FixedUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(seat,null);}
            for(int frame=0;frame<300;frame++)boat.GetType().GetMethod("FixedUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(boat,null);
            foreach(var seat in boat.seats)if(seat.Loaded)seat.item.transform.position=seat.transform.position;
            ReportLevelAudit.Photograph("Documentation/WaterReview/boat_"+stage+".png",view,boat.transform.position+Vector3.up*.85f,false,2);
        }
        ReportLevelAudit.Photograph("Documentation/WaterReview/canal.png",new Vector3(-2.8f,2.3f,-.8f),new Vector3(0,.4f,2.1f),false,20);
        foreach(int index in new[]{10,14,7})
        {EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);var l=Object.FindObjectOfType<DemoLevel>();var position=index==7?new Vector3(-2.5f,2.4f,2):new Vector3(-3,2.6f,-4);var target=index==7?new Vector3(0,.2f,4.5f):new Vector3(0,.1f,1);ReportLevelAudit.Photograph("Documentation/WaterReview/scene_"+index+".png",position,target,false,20);}
        Debug.Log("WATER_PREVIEW_OK");
    }
}
