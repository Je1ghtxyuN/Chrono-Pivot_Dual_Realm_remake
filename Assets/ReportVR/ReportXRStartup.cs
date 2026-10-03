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
        if(settings==null || settings.Manager==null)yield break;
        manager=settings.Manager;
        if(!manager.isInitializationComplete)yield return manager.InitializeLoader();
        if(manager.activeLoader!=null)manager.StartSubsystems();
    }
    void OnApplicationQuit()
    {
        if(manager==null || !manager.isInitializationComplete)return;
        if(manager.activeLoader!=null)manager.StopSubsystems();
        manager.DeinitializeLoader();
    }
}
