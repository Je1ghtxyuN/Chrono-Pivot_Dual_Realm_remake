using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;
using Object=UnityEngine.Object;

public static class SetupReportVR
{
    const string Root="Assets/ReportVR/";
    static InputActionAsset inputs;
    static readonly Dictionary<string,InputActionReference> actions=new Dictionary<string,InputActionReference>();

    [MenuItem("Report VR/Configure Windows SteamVR Project")]
    public static void Configure()
    {
        ConfigureWindowsXR();
        CreateInputs();
        var scenes=AssetDatabase.FindAssets("t:Scene",new[]{"Assets/ReportScenesLowPoly"}).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToList();
        scenes.Add("Assets/ReportGardenLowPoly/AncientGarden_LowPoly.unity");
        var index=new List<string>{"Build index | Scene | Floor spawn (world meters)"};
        foreach(string path in scenes)
        {
            EditorSceneManager.OpenScene(path);
            var oldRig=GameObject.Find("Report VR Origin");if(oldRig!=null)Object.DestroyImmediate(oldRig);
            foreach(var camera in Object.FindObjectsOfType<Camera>())
            {
                camera.enabled=false;camera.stereoTargetEye=StereoTargetEyeMask.None;camera.tag="Untagged";
                var listener=camera.GetComponent<AudioListener>();if(listener!=null)Object.DestroyImmediate(listener);
            }
            foreach(var model in Object.FindObjectsOfType<MeshFilter>())
            {
                if(model.sharedMesh==null || model.GetComponent<Collider>()!=null)continue;
                model.gameObject.AddComponent<MeshCollider>().sharedMesh=model.sharedMesh;
            }
            string name=Path.GetFileNameWithoutExtension(path);
            Vector3 spawn=SpawnFor(name);
            Physics.SyncTransforms();
            if(!Physics.Raycast(new Vector3(spawn.x,40,spawn.z),Vector3.down,out var hit,80))throw new InvalidOperationException("No ground at spawn: "+name);
            spawn.y=hit.point.y+.025f;
            CreateRig(spawn,name);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            index.Add(scenes.IndexOf(path)+" | "+name+" | "+spawn.ToString("F3"));
        }
        EditorBuildSettings.scenes=scenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("Documentation");File.WriteAllLines("Documentation/VR_scene_index.txt",index);
        VerifyReportVR.Validate();
        EditorSceneManager.OpenScene(scenes[0]);
        Debug.Log("REPORT_VR_SETUP_OK scenes=15");
    }

    static void ConfigureWindowsXR()
    {
        PlayerSettings.companyName="ChronoPivot";PlayerSettings.productName="ChronoPivot Report VR";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone,"com.chronopivot.reportvr");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone,"USE_STICK_CONTROL_THUMBSTICKS;USE_INPUT_SYSTEM_POSE_CONTROL");
        PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64,false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{GraphicsDeviceType.Direct3D11});
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone,ManagedStrippingLevel.Low);
        PlayerSettings.runInBackground=true;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;
        PlayerSettings.graphicsJobs=false;
        QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=4;
        QualitySettings.shadowDistance=50;QualitySettings.shadows=ShadowQuality.All;
        QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowCascades=2;
        GraphicsSettings.renderPipelineAsset=null;
        Directory.CreateDirectory(Root+"Settings");
        const string xrPath=Root+"Settings/XRGeneralSettingsPerBuildTarget.asset";
        var targets=AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(xrPath);
        if(targets==null){targets=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(targets,xrPath);}
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,targets,true);
        if(!targets.HasSettingsForBuildTarget(BuildTargetGroup.Standalone))targets.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Standalone);
        if(!targets.HasManagerSettingsForBuildTarget(BuildTargetGroup.Standalone))targets.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Standalone);
        var general=targets.SettingsForBuildTarget(BuildTargetGroup.Standalone);general.InitManagerOnStart=false;
        general.Manager.automaticLoading=false;general.Manager.automaticRunning=false;
        if(!XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.OpenXR.OpenXRLoader",BuildTargetGroup.Standalone))throw new InvalidOperationException("OpenXR loader assignment failed");
        // Unity 1.14 exposes settings access publicly but keeps package-asset creation internal.
        var packageType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("UnityEditor.XR.OpenXR.OpenXRPackageSettings")).First(t=>t!=null);
        packageType.GetMethod("GetOrCreateInstance",BindingFlags.Public|BindingFlags.Static).Invoke(null,null);
        FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
        var settings=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
        settings.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;
        settings.depthSubmissionMode=OpenXRSettings.DepthSubmissionMode.None;
        foreach(var feature in settings.GetFeatures<UnityEngine.XR.OpenXR.Features.OpenXRFeature>())feature.enabled=false;
        settings.GetFeature<OculusTouchControllerProfile>().enabled=true;
        settings.GetFeature<ValveIndexControllerProfile>().enabled=true;
        settings.GetFeature<HTCViveControllerProfile>().enabled=true;
        settings.GetFeature<KHRSimpleControllerProfile>().enabled=true;
        EditorUtility.SetDirty(targets);EditorUtility.SetDirty(general);EditorUtility.SetDirty(general.Manager);EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    static void CreateInputs()
    {
        Directory.CreateDirectory(Root+"Input/References");
        const string path=Root+"Input/ReportVR.inputactions";
        if(!File.Exists(path))
        {
            var asset=ScriptableObject.CreateInstance<InputActionAsset>();var map=new InputActionMap("ReportVR");asset.AddActionMap(map);
            AddPose(map,"Head","<XRHMD>","centerEyePosition","centerEyeRotation");
            AddPose(map,"Left","<XRController>{LeftHand}","devicePosition","deviceRotation");
            AddPose(map,"Right","<XRController>{RightHand}","devicePosition","deviceRotation");
            map.AddAction("Move",InputActionType.Value,"<XRController>{LeftHand}/primary2DAxis",processors:"stickDeadzone(min=0.2,max=0.95)",expectedControlLayout:"Vector2");
            map.AddAction("Turn",InputActionType.Value,"<XRController>{RightHand}/primary2DAxis",processors:"stickDeadzone(min=0.3,max=0.95)",expectedControlLayout:"Vector2");
            map.AddAction("NextScene",InputActionType.Button,"<XRController>{RightHand}/primaryButton");
            map.AddAction("PreviousScene",InputActionType.Button,"<XRController>{RightHand}/secondaryButton");
            File.WriteAllText(path,asset.ToJson());Object.DestroyImmediate(asset);AssetDatabase.ImportAsset(path);
        }
        inputs=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        if(!inputs.FindAction("NextScene").bindings.Any(b=>b.path.EndsWith("/triggerPressed")))
        {
            inputs.FindAction("NextScene").AddBinding("<XRController>{RightHand}/triggerPressed");
            inputs.FindAction("PreviousScene").AddBinding("<XRController>{LeftHand}/triggerPressed");
            File.WriteAllText(path,inputs.ToJson());AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            inputs=AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        actions.Clear();
        foreach(var action in inputs)
        {
            string referencePath=Root+"Input/References/"+action.name+".asset";
            var reference=AssetDatabase.LoadAssetAtPath<InputActionReference>(referencePath);
            if(reference==null){reference=InputActionReference.Create(action);AssetDatabase.CreateAsset(reference,referencePath);}
            actions.Add(action.name,reference);
        }
    }
    static void AddPose(InputActionMap map,string name,string device,string position,string rotation)
    {
        map.AddAction(name+"Position",InputActionType.Value,device+"/"+position,expectedControlLayout:"Vector3");
        map.AddAction(name+"Rotation",InputActionType.Value,device+"/"+rotation,expectedControlLayout:"Quaternion");
        map.AddAction(name+"Tracking",InputActionType.Value,device+"/trackingState",expectedControlLayout:"Integer");
    }
    static void Track(GameObject go,string name)
    {
        var tracker=go.AddComponent<TrackedPoseDriver>();
        tracker.positionInput=new InputActionProperty(actions[name+"Position"]);
        tracker.rotationInput=new InputActionProperty(actions[name+"Rotation"]);
        tracker.trackingStateInput=new InputActionProperty(actions[name+"Tracking"]);
    }
    static XRInputValueReader<Vector2> ReadAxis(string name)
    {
        return new XRInputValueReader<Vector2>{inputSourceMode=XRInputValueReader.InputSourceMode.InputActionReference,inputActionReference=actions[name]};
    }
    static void CreateRig(Vector3 spawn,string sceneName)
    {
        var rig=new GameObject("Report VR Origin");rig.transform.position=spawn;rig.transform.rotation=Quaternion.Euler(0,180,0);
        var offset=new GameObject("Camera Floor Offset");offset.transform.SetParent(rig.transform,false);
        var head=new GameObject("VR Main Camera");head.tag="MainCamera";head.transform.SetParent(offset.transform,false);head.transform.localPosition=Vector3.up*1.65f;
        var camera=head.AddComponent<Camera>();camera.nearClipPlane=.05f;camera.farClipPlane=150;camera.stereoTargetEye=StereoTargetEyeMask.Both;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=RenderSettings.ambientSkyColor;camera.allowHDR=false;
        head.AddComponent<AudioListener>();Track(head,"Head");
        var origin=rig.AddComponent<XROrigin>();origin.Origin=rig;origin.CameraFloorOffsetObject=offset;origin.Camera=camera;
        origin.RequestedTrackingOriginMode=XROrigin.TrackingOriginMode.Floor;origin.CameraYOffset=1.65f;
        rig.AddComponent<InputActionManager>().actionAssets=new List<InputActionAsset>{inputs};
        var body=rig.AddComponent<CharacterController>();body.radius=.20f;body.height=1.65f;body.center=Vector3.up*.85f;body.skinWidth=.025f;body.stepOffset=.25f;body.minMoveDistance=0;
        var transformer=rig.AddComponent<XRBodyTransformer>();transformer.xrOrigin=origin;
        var mediator=rig.AddComponent<LocomotionMediator>();
        var move=rig.AddComponent<ContinuousMoveProvider>();move.mediator=mediator;move.moveSpeed=1.6f;move.forwardSource=head.transform;move.enableFly=false;
        move.leftHandMoveInput=ReadAxis("Move");move.rightHandMoveInput=new XRInputValueReader<Vector2>{inputSourceMode=XRInputValueReader.InputSourceMode.Unused};
        var turn=rig.AddComponent<SnapTurnProvider>();turn.mediator=mediator;turn.turnAmount=30;turn.debounceTime=.35f;turn.enableTurnAround=false;
        turn.rightHandTurnInput=ReadAxis("Turn");turn.leftHandTurnInput=new XRInputValueReader<Vector2>{inputSourceMode=XRInputValueReader.InputSourceMode.Unused};
        var gravity=rig.AddComponent<GravityProvider>();gravity.mediator=mediator;
        var browser=rig.AddComponent<ReportSceneBrowser>();browser.nextScene=actions["NextScene"];browser.previousScene=actions["PreviousScene"];
        for(int i=0;i<2;i++)
        {
            string handName=i==0?"Left":"Right";var hand=new GameObject(handName+" Controller");hand.transform.SetParent(offset.transform,false);Track(hand,handName);
            var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);marker.name="Simple self-created controller marker";marker.transform.SetParent(hand.transform,false);
            marker.transform.localScale=new Vector3(.04f,.04f,.10f);Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/ReportScenesLowPoly/Materials/"+(i==0?"Celadon":"SatinBrass")+".mat");
            if(i==0)
            {
                var label=new GameObject("Scene and controller instructions");label.transform.SetParent(hand.transform,false);label.transform.localPosition=new Vector3(0,.06f,.04f);
                label.transform.localRotation=Quaternion.Euler(70,0,0);
                var text=label.AddComponent<TextMesh>();text.text=sceneName.Replace("Fig_","Figure ").Replace('_',' ')+"\nLeft stick: walk\nRight stick: 30 deg turn\nR / L trigger: next / previous";
                text.characterSize=.006f;text.fontSize=48;text.anchor=TextAnchor.MiddleCenter;text.color=Color.white;
            }
        }
    }
    static Vector3 SpawnFor(string name)
    {
        if(name.Contains("Introduction"))return new Vector3(0,0,5.3f);
        if(name.Contains("Frequency"))return new Vector3(-4,0,5.8f);
        if(name.Contains("Garden_Canal"))return new Vector3(7.2f,0,13.8f);
        if(name.Contains("AncientGarden"))return new Vector3(6.5f,0,8.5f);
        if(name.Contains("Geomagnetic"))return new Vector3(1.5f,0,7.5f);
        if(name.Contains("Laboratory") || name.Contains("Workshop") || name.Contains("Collection"))return new Vector3(4.5f,0,5.5f);
        return new Vector3(3.8f,0,5.5f);
    }

    [MenuItem("Report VR/Build Windows x64")]
    public static void BuildWindows()
    {
        PolishReportLevels.Validate();
        Directory.CreateDirectory("Builds/Windows");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Windows/ChronoPivot_ReportVR.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        File.WriteAllText("Documentation/Verification/windows-build.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nSize: "+report.summary.totalSize+"\n");
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Windows build failed");
        CreateReportCharacter.ExportDesktopBuild();
        Debug.Log("REPORT_VR_WINDOWS_BUILD_OK");
    }
}
