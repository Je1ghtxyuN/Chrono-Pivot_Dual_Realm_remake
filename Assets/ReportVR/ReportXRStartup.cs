using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

// Manual startup lets -desktop launch without opening SteamVR.
public sealed class ReportXRStartup : MonoBehaviour
{
    XRManagerSettings manager;
    public static bool ForceDesktop=>Array.Exists(Environment.GetCommandLineArgs(),s=>s=="-desktop");
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void StartRuntime()
    {
        if(Application.isBatchMode || ForceDesktop)return;
        var go=new GameObject("Report XR startup");DontDestroyOnLoad(go);go.AddComponent<ReportXRStartup>();
    }
    IEnumerator Start()
    {
        var settings=XRGeneralSettings.Instance;
        if(settings==null || settings.Manager==null){Debug.LogWarning("[Report VR] XR Management settings missing; using desktop controls.");yield break;}
        manager=settings.Manager;
        if(!manager.isInitializationComplete)yield return manager.InitializeLoader();
        if(manager.activeLoader!=null)
        {
            manager.StartSubsystems();
            Debug.Log("[Report VR] Started "+manager.activeLoader.name+". SteamVR must be the active Windows OpenXR runtime.");
        }
        else Debug.LogWarning("[Report VR] OpenXR initialization failed. Start SteamVR, connect the headset, and select SteamVR as the OpenXR runtime, then restart the demo. Desktop controls remain available.");
    }
    void OnApplicationQuit()
    {
        if(manager==null || !manager.isInitializationComplete)return;
        if(manager.activeLoader!=null)manager.StopSubsystems();
        manager.DeinitializeLoader();
    }
}
