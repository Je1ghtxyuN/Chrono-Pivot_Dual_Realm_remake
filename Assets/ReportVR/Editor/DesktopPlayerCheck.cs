using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

// Real keyboard/mouse events catch lost input, duplicate locomotion and reset drift.
public static class DesktopPlayerCheck
{
    static Keyboard keyboard;
    static Mouse mouse;
    static int phase;
    static double until,timeout;
    static bool failed,oldOptionsEnabled,oldXRStart;
    static EnterPlayModeOptions oldOptions;
    static InputSettings.UpdateMode oldInputMode;
    static InputSettings.BackgroundBehavior oldBackground;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInput;
    static Vector3 start;
    static float yaw;
    static int attempts;
    static XROrigin origin;
    static MonoBehaviour player;
    static DemoLevel level;
    static string error;

    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXRStart=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        oldInputMode=InputSystem.settings.updateMode;oldBackground=InputSystem.settings.backgroundBehavior;oldEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        phase=0;failed=false;error=null;timeout=EditorApplication.timeSinceStartup+90;
        Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void OnLog(string message,string stack,LogType type){if(type==LogType.Error || type==LogType.Exception)error=message;}
    static bool Read(string property)=>(bool)player.GetType().GetProperty(property).GetValue(player);
    static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    static void Next(int value,double delay=.15){phase=value;until=EditorApplication.timeSinceStartup+delay;}
    static void Keys(params Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));}
    static void Aim()
    {
        var station=level.stations[0];var body=origin.GetComponent<CharacterController>();body.enabled=false;
        var sample=station.GetComponent<ReportPickupMeasure>().samples[0];
        origin.transform.position=new Vector3(sample.transform.position.x,level.spawn.y,sample.transform.position.z+2);body.enabled=true;
        AimView();attempts=station.Attempts;
    }
    static void AimView()
    {
        var station=level.stations[0];
        var sample=station.GetComponent<ReportPickupMeasure>().samples[0];
        Vector3 direction=sample.GetComponent<Collider>().bounds.center-origin.Camera.transform.position;
        origin.transform.rotation=Quaternion.Euler(0,Quaternion.LookRotation(direction).eulerAngles.y,0);
        origin.Camera.transform.localRotation=Quaternion.Euler(Mathf.DeltaAngle(0,Quaternion.LookRotation(direction).eulerAngles.x),0,0);
        Physics.SyncTransforms();
    }
    static void Tick()
    {
        try
        {
            if(phase==99)
            {
                if(EditorApplication.isPlayingOrWillChangePlaymode)return;
                EditorSettings.enterPlayModeOptions=oldOptions;EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).InitManagerOnStart=oldXRStart;
                InputSystem.settings.updateMode=oldInputMode;InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
                if(keyboard!=null && keyboard.added)InputSystem.RemoveDevice(keyboard);
                if(mouse!=null && mouse.added)InputSystem.RemoveDevice(mouse);
                AssetDatabase.SaveAssets();EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;EditorApplication.Exit(failed?1:0);return;
            }
            if(EditorApplication.timeSinceStartup>timeout)throw new Exception("Desktop check timeout phase="+phase);
            if(!EditorApplication.isPlaying)return;
            if(EditorApplication.timeSinceStartup<until)return;
            Require(error==null,"Runtime error: "+error);
            switch(phase)
            {
                case 0:
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    var gameUI=origin.GetComponent<ReportGameUI>();if(gameUI!=null){gameUI.Resume();gameUI.enabled=false;}
                    player=origin.GetComponents<MonoBehaviour>().FirstOrDefault(c=>c.GetType().Name=="DesktopPlayer");
                    Require(player!=null,"Missing desktop player on the playable rig");
                    InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard=InputSystem.AddDevice<Keyboard>();mouse=InputSystem.AddDevice<Mouse>();Next(1,.4);break;
                case 1:
                    Require(Read("IsDesktop"),"No headset must select desktop mode");
                    InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1,position=new Vector2(1000,20)});Next(2);break;
                case 2:
                    Require(Read("Captured"),"Click did not capture mouse; screen="+Screen.width+"x"+Screen.height+" pointer="+mouse.position.ReadValue());
                    InputSystem.QueueStateEvent(mouse,new MouseState());start=origin.transform.position;
                    player.GetType().GetField("speed").SetValue(player,0f);Keys(Key.W);Next(20,.2);break;
                case 20:
                    Require(Vector2.Distance(new Vector2(start.x,start.z),new Vector2(origin.transform.position.x,origin.transform.position.z))<.02f,"Zero configured speed still moves");
                    player.GetType().GetField("speed").SetValue(player,3.2f);start=origin.transform.position;Next(3,.45);break;
                case 3:
                    Require(Vector3.Distance(start,origin.transform.position)>.6f,"Changed movement speed did not affect walking");
                    Require(Mathf.Abs(origin.transform.position.y-start.y)<.15f,"Desktop player fell through floor");
                    Keys();yaw=origin.transform.eulerAngles.y;
                    player.GetType().GetField("sensitivity").SetValue(player,.24f);
                    var limit=player.GetType().GetField("pitchLimit");Require(limit!=null,"No editable pitch limit");limit.SetValue(player,10f);
                    InputSystem.QueueDeltaStateEvent(mouse.delta,new Vector2(100,-1000));Next(4);break;
                case 4:
                    Require(Mathf.Abs(Mathf.Abs(Mathf.DeltaAngle(yaw,origin.transform.eulerAngles.y))-24)<1,"Configured mouse sensitivity did not control yaw");
                    Require(Mathf.Abs(Mathf.DeltaAngle(0,origin.Camera.transform.localEulerAngles.x)-10)<.1f,"Configured pitch limit did not constrain looking");
                    player.GetType().GetField("speed").SetValue(player,1.6f);player.GetType().GetField("sensitivity").SetValue(player,.12f);player.GetType().GetField("pitchLimit").SetValue(player,80f);
                    Keys(Key.R);Next(5);break;
                case 5:
                    Require(Vector2.Distance(new Vector2(origin.transform.position.x,origin.transform.position.z),new Vector2(level.spawn.x,level.spawn.z))<.02f && origin.GetComponent<CharacterController>().isGrounded,"R did not restore grounded spawn; actual="+origin.transform.position+" spawn="+level.spawn);
                    Require(Mathf.Abs(Mathf.DeltaAngle(origin.transform.eulerAngles.y,level.spawnYaw))<.01f,"Reset yaw drifted");
                    Require(Quaternion.Angle(origin.Camera.transform.localRotation,Quaternion.identity)<.01f,"Reset pitch drifted");
                    Keys();Aim();Next(6);break;
                case 6:
                    AimView();
                    InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1});Next(7);break;
                case 7:
                    var pickup=level.stations[0].GetComponent<ReportPickupMeasure>();
                    Require(pickup.samples[0].isSelected && pickup.Reading==1 && level.Step==0,"Left click did not grab resized material or falsely completed three pickups");
                    level.ShowFeedback("Desktop held-click sentinel");
                    Next(8);break;
                case 8:
                    Require(level.Feedback=="Desktop held-click sentinel","Held left click repeats interactions");
                    var label=level.stations[0].label.transform;
                    Require(Vector3.Dot(label.forward,Vector3.ProjectOnPlane(label.position-origin.Camera.transform.position,Vector3.up).normalized)>.999f,"Floating status did not face player");
                    InputSystem.QueueStateEvent(mouse,new MouseState());Keys(Key.Escape);Next(9);break;
                case 9:
                    Require(!Read("Captured"),"Escape did not unlock pointer");Keys();start=origin.transform.position;attempts=level.stations[0].Attempts;
                    Keys(Key.W,Key.E);InputSystem.QueueStateEvent(mouse,new MouseState{buttons=1,position=new Vector2(30,Screen.height-50)});Next(10,.3);break;
                case 10:
                    Require(Vector3.Distance(start,origin.transform.position)<.02f,"Unlocked UI mode still walks");
                    Require(level.stations[0].Attempts==attempts,"UI click also activated a station");
                    origin.GetComponent<ReportSceneBrowser>().allowDebugNavigation=true;
                    Keys();InputSystem.QueueStateEvent(mouse,new MouseState());Keys(Key.PageDown);Next(11,.7);break;
                case 11:
                    Require(SceneManager.GetActiveScene().buildIndex==1,"PageDown did not load next scene");Keys();
                    origin=UnityEngine.Object.FindObjectOfType<XROrigin>();player=origin.GetComponents<MonoBehaviour>().First(c=>c.GetType().Name=="DesktopPlayer");
                    Require(Read("IsDesktop"),"Desktop mode lost after scene load");
                    File.WriteAllText("Documentation/Verification/desktop-playtest.txt","PASS: actual Play Mode; no-headset fallback; mouse capture/release; editable speed including zero; configured mouse sensitivity and pitch limit; WASD ground collision; R position + view reset; left click grabs resized sample once; floating status faces player; unlocked UI blocks walk/use; PageDown scene change.\n");
                    Debug.Log("DESKTOP_PLAYTEST_OK");phase=99;EditorApplication.isPlaying=false;break;
            }
        }
        catch(Exception ex)
        {
            failed=true;Application.logMessageReceived-=OnLog;Debug.LogError(ex);
            Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/desktop-playtest.txt","FAIL\n"+ex);phase=99;EditorApplication.isPlaying=false;
        }
    }
}
