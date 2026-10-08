using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class ReportParticleUpgrade
{
    const string Folder="Assets/ReportVR/Particles";
    static Transform Group(Transform parent)
    {
        var old=parent.Find("Physical particles");if(old!=null)Object.DestroyImmediate(old.gameObject);
        var root=new GameObject("Physical particles").transform;root.SetParent(parent,false);return root;
    }
    static ParticleSystem CreateParticles(Transform parent,string name,bool snow=false)
    {
        var go=new GameObject(name);go.transform.SetParent(parent,false);var particles=go.AddComponent<ParticleSystem>();
        var main=particles.main;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.scalingMode=ParticleSystemScalingMode.Shape;main.startLifetime=snow?.42f:.3f;main.startSpeed=0;
        main.startSize=snow?new ParticleSystem.MinMaxCurve(.018f,.04f):new ParticleSystem.MinMaxCurve(.018f,.032f);main.maxParticles=snow?24:40;main.gravityModifier=snow?.35f:1;
        main.startColor=snow?new Color(.95f,.98f,1,.72f):new Color(.7f,.9f,.95f,.8f);
        var emission=particles.emission;emission.enabled=false;emission.rateOverTime=0;var shape=particles.shape;shape.enabled=false;
        var fade=particles.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.3f),new GradientAlphaKey(0,1)});fade.color=gradient;
        var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Physical particles.mat");renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);return particles;
    }
    static void Material()
    {
        Directory.CreateDirectory(Folder);AssetDatabase.Refresh();string path=Folder+"/Physical particles.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended")){name="Physical particles"};AssetDatabase.CreateAsset(material,path);}
        material.SetColor("_TintColor",new Color(.5f,.5f,.5f,.5f));material.SetFloat("_InvFade",30);material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ReportVR/Water/Droplet.asset");EditorUtility.SetDirty(material);
    }
    static float CanalWaterHeight()
    {
        var canal=GameObject.Find("EnclosedPressureCanal");var renderer=canal.GetComponent<MeshRenderer>();var mesh=canal.GetComponent<MeshFilter>().sharedMesh;var vertices=mesh.vertices;float height=float.MinValue;
        for(int sub=0;sub<mesh.subMeshCount;sub++)
            if(renderer.sharedMaterials[sub].shader.name=="ChronoPivot/Water")
                foreach(int vertex in mesh.GetTriangles(sub))height=Mathf.Max(height,canal.transform.TransformPoint(vertices[vertex]).y);
        if(height==float.MinValue)throw new Exception("Canal water mesh missing");return height;
    }
    public static void ConfigureScene()
    {
        Material();
        if(EditorSceneManager.GetActiveScene().path==EditorBuildSettings.scenes[1].path)ReportBoatDisplayUpgrade.ClassicHull(Object.FindObjectOfType<ReportBuoyancyMeasure>());
        foreach(var boat in Object.FindObjectsOfType<ReportBuoyancyMeasure>())
        {
            var group=Group(boat.transform);var feedback=boat.GetComponent<ReportBoatParticles>();if(feedback==null)feedback=boat.gameObject.AddComponent<ReportBoatParticles>();
            feedback.experiment=boat;feedback.splashes=CreateParticles(group,"Boat waterline splashes");feedback.display=boat.GetComponent<ReportBoatDisplay>();feedback.displaySplashes=null;
            if(feedback.display!=null){feedback.displaySplashes=CreateParticles(group,"Linked canal boat splashes");var main=feedback.displaySplashes.main;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.045f);feedback.displayWaterHeight=CanalWaterHeight();}
            EditorUtility.SetDirty(feedback);
        }
        foreach(var heat in Object.FindObjectsOfType<ReportHeatMeasure>())
        {
            var group=Group(heat.transform);var feedback=heat.GetComponent<ReportMeltParticles>();if(feedback==null)feedback=heat.gameObject.AddComponent<ReportMeltParticles>();
            feedback.experiment=heat;feedback.droplets=CreateParticles(group,"Melting ice drops");EditorUtility.SetDirty(feedback);
        }
        var roll=Object.FindObjectOfType<ReportRollMeasure>();var stack=Object.FindObjectOfType<ReportStackMeasure>();
        if(roll!=null && stack!=null)
        {
            foreach(var piece in stack.pieces.Concat(new[]{roll.ball}))
            {
                var group=Group(piece.transform);var feedback=piece.GetComponent<ReportSnowParticles>();if(feedback==null)feedback=piece.gameObject.AddComponent<ReportSnowParticles>();
                feedback.piece=piece;feedback.powder=CreateParticles(group,"Snow contact powder",true);EditorUtility.SetDirty(feedback);
            }
        }
    }
    static string PhysicsSnapshot()=>string.Join("\n",Object.FindObjectsOfType<Component>().Where(c=>c is ReportLearningMeasure || c is Rigidbody || c is Collider || c is ReportLearningTask || c is ReportGrabbable).OrderBy(c=>c.GetInstanceID()).Select(c=>EditorJsonUtility.ToJson(c)));
    [MenuItem("Report VR/Particles/Apply Planned Effects")]
    public static void All()
    {
        Directory.CreateDirectory("Documentation/ParticleReview");
        foreach(int index in new[]{1,7,10,12})
        {
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[index].path);string before=PhysicsSnapshot();ConfigureScene();
            if(before!=PhysicsSnapshot())throw new Exception("Particle configuration changed physical parameters in level "+(index+1));
            int count=Object.FindObjectsOfType<ParticleSystem>().Length;ConfigureScene();if(count!=Object.FindObjectsOfType<ParticleSystem>().Length)throw new Exception("Particle configuration duplicated emitters");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();File.WriteAllText("Documentation/ParticleReview/configuration.txt","PASS: levels 2,8,11,13 configured in place; physical components unchanged; repeated configuration does not duplicate emitters.\n");Debug.Log("PARTICLE_CONFIGURATION_OK");
    }
    public static void Verify(){ParticleExperienceCheck.ClassicBoat();CargoLoadingCheck.Verify();BoatDisplayCheck.Verify();ParticleExperienceCheck.Boats();ParticleExperienceCheck.SceneBudget();ParticleExperienceCheck.Hydraulic();Debug.Log("PARTICLE_EDIT_CHECK_OK");}
    public static void Build(){Verify();SetupReportVR.BuildWindows();Debug.Log("PARTICLE_BUILD_OK");}
    public static void VerifyAndCapture(){Verify();Capture();}
    public static void Capture()
    {
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);var boat=Object.FindObjectOfType<ReportBuoyancyMeasure>();var feedback=boat.GetComponent<ReportBoatParticles>();var display=feedback.display;
        Call(boat,"Awake");Call(display,"Awake");foreach(var seat in boat.seats){Call(seat,"Awake");seat.item.transform.position=seat.transform.position+Vector3.right*3;}Call(feedback,"Awake");
        Photograph("classic-wooden-boat",boat.transform.position-boat.transform.forward*1.1f+boat.transform.right*.4f+Vector3.up*1.9f,boat.hull.position);
        boat.seats[0].item.transform.position=boat.seats[0].transform.position;Call(boat.seats[0],"FixedUpdate");Call(display,"LateUpdate");Call(feedback,"LateUpdate");
        foreach(var system in feedback.GetComponentsInChildren<ParticleSystem>()){system.Simulate(.10f,true,false);Debug.Log("PARTICLE_PREVIEW count="+system.particleCount+" bounds="+system.GetComponent<Renderer>().bounds);}
        Photograph("boat-waterline",boat.transform.position-boat.transform.forward*1.3f+Vector3.up*1.5f,boat.hull.position);
        Photograph("canal-waterline",display.hull.position+new Vector3(-1.4f,1.9f,-3.2f),display.hull.position+new Vector3(.8f,.45f,0));Debug.Log("PARTICLE_PREVIEW_OK");
    }
    public static void Photograph(string name,Vector3 view,Vector3 target)
    {
        var player=Object.FindObjectOfType<DesktopPlayer>();if(player!=null)foreach(var renderer in player.GetComponentsInChildren<Renderer>())renderer.enabled=false;
        var font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        foreach(var text in Object.FindObjectsOfType<TextMesh>()){text.font=font;text.GetComponent<Renderer>().sharedMaterial=font.material;var facing=text.GetComponent<ReportTextFacing>();if(facing!=null && facing.mode==ReportTextFacingMode.FacePlayer)text.transform.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(text.transform.position-view,Vector3.up));}
        ReportLevelAudit.Photograph("Documentation/ParticleReview/"+name+".png",view,target,false,2);
    }
    static void Call(Component value,string method)=>value.GetType().GetMethod(method,System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(value,null);
    public static void ApplyAndCapture(){All();VerifyAndCapture();}
}
