using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public static class VerifyReportVR
{
    public static void Validate()
    {
        var paths=AssetDatabase.FindAssets("t:Scene",new[]{"Assets/ReportScenesLowPoly","Assets/ReportGardenLowPoly"})
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
        Require(paths.Length==15,"All 15 self-modeled scenes must be present");
        foreach(string path in paths)
        {
            EditorSceneManager.OpenScene(path);
            Require(UnityEngine.Object.FindObjectsOfType<XROrigin>().Length==1,"Missing single XR Origin in "+path);
            var origin=UnityEngine.Object.FindObjectOfType<XROrigin>();
            Require(origin.RequestedTrackingOriginMode==XROrigin.TrackingOriginMode.Floor,"Floor origin missing in "+path);
            var cameras=UnityEngine.Object.FindObjectsOfType<Camera>().Where(c=>c.enabled).ToArray();
            Require(cameras.Length==1 && cameras[0]==origin.Camera,"Only the VR camera should render: "+path);
            Require(origin.Camera.GetComponent<TrackedPoseDriver>()!=null,"HMD tracking missing: "+path);
            Require(origin.GetComponentsInChildren<TrackedPoseDriver>().Length==3,"Both hand tracking drivers required: "+path);
            Require(UnityEngine.Object.FindObjectOfType<ContinuousMoveProvider>()!=null,"Movement missing: "+path);
            Require(UnityEngine.Object.FindObjectOfType<SnapTurnProvider>()!=null,"Snap turn missing: "+path);
            Require(UnityEngine.Object.FindObjectsOfType<MeshCollider>().Length>0,"Environment collision missing: "+path);
            foreach(var mesh in UnityEngine.Object.FindObjectsOfType<MeshFilter>())
                Require(mesh.sharedMesh!=null,"Missing model: "+mesh.name);
            foreach(var component in origin.GetComponentsInChildren<Transform>())
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(component.gameObject)==0,"Missing VR script: "+component.name);
            Physics.SyncTransforms();
            Require(Physics.Raycast(origin.transform.position+Vector3.up*.2f,Vector3.down,out var ground,1f),"Spawn has no walkable ground: "+path);
            Require(ground.normal.y>.7f,"Spawn is on a slope: "+path);
            Require(ground.point.y<1.1f,"Spawn is on a tall prop rather than the floor: "+path+" collider="+ground.collider.name);
            var browser=origin.GetComponents<MonoBehaviour>().SingleOrDefault(c=>c.GetType().Name=="ReportSceneBrowser");
            Require(browser!=null,"Scene navigation missing: "+path);
            Debug.Log("VR_SCENE_OK "+path);
        }
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
        Require(xr!=null && !xr.InitManagerOnStart,"Use ReportXRStartup manual initialization so -desktop can skip SteamVR");
        Require(xr.Manager.activeLoaders.Count==1 && xr.Manager.activeLoaders[0] is OpenXRLoader,"Windows must use OpenXR only");
        var settings=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Standalone);
        Require(settings.renderMode==OpenXRSettings.RenderMode.SinglePassInstanced,"Single pass instanced rendering missing");
        Require(settings.GetFeatures<UnityEngine.XR.OpenXR.Features.OpenXRFeature>().Count(f=>f.enabled)>=4,"Controller profiles missing");
        Require(EditorUserBuildSettings.activeBuildTarget==BuildTarget.StandaloneWindows64,"Windows x64 target missing");
        Require(PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64).SequenceEqual(new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11}),"Use D3D11 for this PC VR project");
        Require(EditorBuildSettings.scenes.Count(s=>s.enabled)==15,"All scenes must be enabled in build");
        Require(ReportSceneBrowser.AdjacentSceneIndex(14,1,15)==0,"Last scene must advance to the first");
        Require(ReportSceneBrowser.AdjacentSceneIndex(0,-1,15)==14,"First scene must go back to the last");
        Require(ReportSceneBrowser.AdjacentSceneIndex(6,1,15)==7,"Scene navigation must preserve report order");
        Directory.CreateDirectory("Documentation/Verification");
        File.WriteAllText("Documentation/Verification/vr-scene-check.txt","PASS: 15 scenes; floor XR origins; 3 tracked poses per scene; one active camera; locomotion; environment colliders; valid ground; OpenXR Windows x64 D3D11; all scenes in build.\nHardware headset/SteamVR streaming not tested.\n");
        Debug.Log("REPORT_VR_VALIDATION_OK scenes=15");
    }
    static void Require(bool passed,string message){if(!passed)throw new InvalidOperationException(message);}
}
