using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

// Exercise the actual scene scripts with virtual input devices, without starting SteamVR.
public static class OfflineVRCheck
{
    static XRHMD head;
    static XRController left,right;
    static readonly List<string> errors=new List<string>();
    static readonly HashSet<int> visited=new HashSet<int>();
    static bool oldOptionsEnabled,failed,oldXRStart;
    static EnterPlayModeOptions oldOptions;
    static int phase,index,switches;
    static int demoAttempts,demoStep;
    static bool demoChecked;
    static Vector3[] demoInitialPositions;
    static Quaternion[] demoInitialRotations;
    static Vector3 demoDoorClosed;
    static double waitUntil,timeout;
    static Vector3 startPosition;
    static float startYaw;
    static InputSettings.BackgroundBehavior oldBackgroundBehavior;
    static InputSettings.EditorInputBehaviorInPlayMode oldEditorInputBehavior;
    static InputSettings.UpdateMode oldInputUpdateMode;

    public static void Run()
    {
        oldOptions=EditorSettings.enterPlayModeOptions;oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;
        EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
        var xr=XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);oldXRStart=xr.InitManagerOnStart;xr.InitManagerOnStart=false;
        EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
        phase=0;index=0;switches=0;demoChecked=false;failed=false;visited.Clear();errors.Clear();
        timeout=EditorApplication.timeSinceStartup+150;
        Application.logMessageReceived+=OnLog;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
    }
    static void OnLog(string message,string stack,LogType type)
    {
        if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)errors.Add(message);
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
                AssetDatabase.SaveAssets();EditorApplication.update-=Tick;Application.logMessageReceived-=OnLog;
                InputSystem.settings.backgroundBehavior=oldBackgroundBehavior;
                InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInputBehavior;
                InputSystem.settings.updateMode=oldInputUpdateMode;
                EditorApplication.Exit(failed?1:0);return;
            }
            if(EditorApplication.timeSinceStartup>timeout)throw new Exception("Offline VR check timed out in phase "+phase);
            if(!EditorApplication.isPlaying)return;
            if(head!=null && head.added)InputSystem.Update();
            if(EditorApplication.timeSinceStartup<waitUntil)return;
            if(errors.Count>0)throw new Exception("Runtime error: "+errors[0]);
            switch(phase)
            {
                case 0:
                    oldBackgroundBehavior=InputSystem.settings.backgroundBehavior;
                    oldEditorInputBehavior=InputSystem.settings.editorInputBehaviorInPlayMode;
                    oldInputUpdateMode=InputSystem.settings.updateMode;
                    InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.RegisterLayout(@"{
                        ""name"":""ReportOfflineController"", ""extend"":""XRController"",
                        ""controls"":[
                            {""name"":""devicePosition"",""layout"":""Vector3"",""offset"":0},
                            {""name"":""deviceRotation"",""layout"":""Quaternion"",""offset"":12},
                            {""name"":""trackingState"",""layout"":""Integer"",""offset"":28},
                            {""name"":""isTracked"",""layout"":""Button"",""format"":""BIT"",""offset"":32,""bit"":0},
                            {""name"":""primary2DAxis"",""layout"":""Stick"",""offset"":36},
                            {""name"":""triggerPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":44},
                            {""name"":""primaryButton"",""layout"":""Button"",""format"":""FLT"",""offset"":48},
                            {""name"":""secondaryButton"",""layout"":""Button"",""format"":""FLT"",""offset"":52},
                            {""name"":""gripPressed"",""layout"":""Button"",""format"":""FLT"",""offset"":56},
                            {""name"":""menu"",""layout"":""Button"",""format"":""FLT"",""offset"":60}
                        ]}");
                    head=InputSystem.AddDevice<XRHMD>();
                    left=(XRController)InputSystem.AddDevice("ReportOfflineController");
                    right=(XRController)InputSystem.AddDevice("ReportOfflineController");
                    InputSystem.SetDeviceUsage(left,CommonUsages.LeftHand);InputSystem.SetDeviceUsage(right,CommonUsages.RightHand);
                    InputSystem.QueueDeltaStateEvent(head.centerEyePosition,new Vector3(.12f,1.72f,-.08f));
                    InputSystem.QueueDeltaStateEvent(head.centerEyeRotation,Quaternion.identity);
                    InputSystem.QueueDeltaStateEvent(head.trackingState,3);
                    QueueHand(left,new Vector3(-.25f,1.25f,.35f));QueueHand(right,new Vector3(.25f,1.25f,.35f));
                    NextPhase(1,.5);break;
                case 1:
                    CheckPoses();visited.Add(index);
                    if(!demoChecked)
                    {
                        var initial=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                        demoInitialPositions=initial.stations.Select(s=>s.movingPart!=null?s.movingPart.localPosition:Vector3.zero).ToArray();
                        demoInitialRotations=initial.stations.Select(s=>s.movingPart!=null?s.movingPart.localRotation:Quaternion.identity).ToArray();
                        demoDoorClosed=initial.exitDoor!=null?initial.exitDoor.localPosition:Vector3.zero;
                        demoStep=0;demoAttempts=0;AimAtStation();NextPhase(10,.15);break;
                    }
                    if(index==0 || index==7 || index==10)Snapshot();
                    if(switches==15){Finish();break;}
                    if(index==0 && switches==0)
                    {
                        startPosition=UnityEngine.Object.FindObjectOfType<XROrigin>().transform.position;
                        InputSystem.QueueDeltaStateEvent(left.GetChildControl<Vector2Control>("primary2DAxis"),new Vector2(0,.8f));NextPhase(2,.5);
                    }
                    else NextPhase(4,.2);
                    break;
                case 2:
                    var move=UnityEngine.Object.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement.ContinuousMoveProvider>();
                    Require(Vector3.Distance(startPosition,UnityEngine.Object.FindObjectOfType<XROrigin>().transform.position)>.12f,"Left stick failed to move XR Origin; input="+move.leftHandMoveInput.ReadValue()+" state="+move.locomotionState+" frames="+Time.frameCount+" delta="+Time.deltaTime+" position="+UnityEngine.Object.FindObjectOfType<XROrigin>().transform.position);
                    InputSystem.QueueDeltaStateEvent(left.GetChildControl<Vector2Control>("primary2DAxis"),Vector2.zero);
                    startYaw=UnityEngine.Object.FindObjectOfType<XROrigin>().transform.eulerAngles.y;
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<Vector2Control>("primary2DAxis"),new Vector2(.9f,0));NextPhase(3,.18);break;
                case 3:
                    Require(Mathf.Abs(Mathf.DeltaAngle(startYaw,UnityEngine.Object.FindObjectOfType<XROrigin>().transform.eulerAngles.y))>20,"Right stick failed to snap turn");
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<Vector2Control>("primary2DAxis"),Vector2.zero);NextPhase(4,.2);break;
                case 4:
                    UnityEngine.Object.FindObjectOfType<ReportSceneBrowser>().allowDebugNavigation=true;
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("triggerPressed"),1f);index=(index+1)%15;switches++;NextPhase(5,.6);break;
                case 5:
                    if(SceneManager.GetActiveScene().buildIndex!=index){NextPhase(5,.1);break;}
                    NextPhase(6,.25);break;
                case 6:
                    Require(SceneManager.GetActiveScene().buildIndex==index,"Held trigger advanced a second scene, expected "+index);
                    CheckPoses();demoChecked=false;InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("triggerPressed"),0f);NextPhase(1,.3);break;
                case 10:
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("gripPressed"),1f);NextPhase(11,.12);break;
                case 11:
                    var level=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    Require(level.stations[demoStep].Attempts==demoAttempts+1,"Grip ray did not activate the pointed station: scene="+index+" step="+demoStep);
                    demoAttempts++;
                    InputSystem.QueueDeltaStateEvent(right.GetChildControl<ButtonControl>("gripPressed"),0f);NextPhase(12,.12);break;
                case 12:
                    var demo=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    if(demo.Step>demoStep){demoStep++;demoAttempts=0;}
                    Require(demoAttempts<5,"Demo parameter adjustment never completes");
                    if(demo.Completed)
                    {
                        NextPhase(14,2.25);
                    }
                    else if(demo.stations[demoStep].IsResolving)NextPhase(12,.2);
                    else {AimAtStation();NextPhase(10,.15);}
                    break;
                case 13:
                    var resetLevel=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    Require(resetLevel.Step==0 && resetLevel.stations.All(s=>s.Attempts==0),"Controller menu did not reset objective states");
                    for(int i=0;i<resetLevel.stations.Length;i++)
                    {
                        var part=resetLevel.stations[i].movingPart;if(part==null)continue;
                        Require(Vector3.Distance(part.localPosition,demoInitialPositions[i])<.001f && Quaternion.Angle(part.localRotation,demoInitialRotations[i])<.01f,"Reset did not restore model pose in scene "+index);
                    }
                    if(resetLevel.exitDoor!=null)Require(Vector3.Distance(resetLevel.exitDoor.localPosition,demoDoorClosed)<.001f,"Reset did not close the exit door");
                    InputSystem.QueueDeltaStateEvent(left.GetChildControl<ButtonControl>("menu"),0f);QueueHand(right,new Vector3(.25f,1.25f,.35f));
                    demoChecked=true;Debug.Log("OFFLINE_DEMO_OK scene="+index+" grip-ray ordered completion + menu reset");NextPhase(1,.15);break;
                case 14:
                    var completed=UnityEngine.Object.FindObjectOfType<DemoLevel>();
                    if(completed.exitDoor!=null)Require(Vector3.Distance(completed.exitDoor.localPosition,demoDoorClosed)>2,"Completed electromagnetic level did not open its physical door");
                    InputSystem.QueueDeltaStateEvent(left.GetChildControl<ButtonControl>("menu"),1f);NextPhase(13,.18);break;
            }
        }
        catch(Exception exception)
        {
            failed=true;Application.logMessageReceived-=OnLog;Debug.LogError(exception.ToString());
            Directory.CreateDirectory("Documentation/Verification");File.WriteAllText("Documentation/Verification/offline-playtest.txt","FAIL\n"+exception);
            phase=99;EditorApplication.isPlaying=false;
        }
    }
    static void QueueHand(XRController hand,Vector3 position)
    {
        InputSystem.QueueDeltaStateEvent(hand.devicePosition,position);InputSystem.QueueDeltaStateEvent(hand.deviceRotation,Quaternion.identity);
        InputSystem.QueueDeltaStateEvent(hand.trackingState,3);
    }
    static void AimAtStation()
    {
        var level=UnityEngine.Object.FindObjectOfType<DemoLevel>();var station=level.stations[demoStep];
        PuzzleTestSupport.PrepareSolution(station);
        var origin=UnityEngine.Object.FindObjectOfType<XROrigin>();var body=origin.GetComponent<CharacterController>();body.enabled=false;
        origin.transform.position=station.viewingPoint;body.enabled=true;
        Vector3 handPosition=origin.transform.TransformPoint(new Vector3(.25f,1.25f,.35f));
        var rotation=Quaternion.Inverse(origin.transform.rotation)*Quaternion.LookRotation(station.indicator.bounds.center-handPosition);
        InputSystem.QueueDeltaStateEvent(right.deviceRotation,rotation);
    }
    static void CheckPoses()
    {
        Require(SceneManager.GetActiveScene().buildIndex==index,"Wrong scene loaded");
        var origin=UnityEngine.Object.FindObjectOfType<XROrigin>();
        var desktop=origin.GetComponent<DesktopPlayer>();
        Require(desktop!=null && !desktop.IsDesktop,"Tracked HMD must select VR mode");
        var avatar=origin.GetComponentInChildren<LowPolyPlayerAvatar>();
        Require(avatar!=null && Vector3.Distance(avatar.leftHand.position,avatar.leftTarget.position)<.001f && Vector3.Distance(avatar.rightHand.position,avatar.rightTarget.position)<.001f,"Avatar hands did not follow tracked controllers");
        Require(Vector3.Distance(origin.Camera.transform.localPosition,new Vector3(.12f,1.72f,-.08f))<.001f,"HMD pose did not drive camera: "+index);
        Require(Vector3.Distance(origin.transform.Find("Camera Floor Offset/Left Controller").localPosition,new Vector3(-.25f,1.25f,.35f))<.001f,"Left hand tracking failed");
        Require(Vector3.Distance(origin.transform.Find("Camera Floor Offset/Right Controller").localPosition,new Vector3(.25f,1.25f,.35f))<.001f,"Right hand tracking failed");
        Debug.Log("OFFLINE_VR_SCENE_OK "+index+" "+SceneManager.GetActiveScene().name);
    }
    static void Snapshot()
    {
        var camera=UnityEngine.Object.FindObjectOfType<XROrigin>().Camera;
        var target=new RenderTexture(1280,720,24);var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
        camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
        Directory.CreateDirectory("Documentation/VRPreviews");File.WriteAllBytes("Documentation/VRPreviews/"+SceneManager.GetActiveScene().name+".png",pixels.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(pixels);
    }
    static void Finish()
    {
        Require(visited.Count==15,"Not all scenes visited");
        Directory.CreateDirectory("Documentation/Verification");
        File.WriteAllText("Documentation/Verification/offline-playtest.txt","PASS: actual Play Mode; 15 scenes; grip ray input completes every ordered station; menu resets states; trigger navigation and wrap; held-button protection; HMD and hand tracking; VR mode selected; avatar hands follow controllers; stick movement and snap turn; runtime errors=0.\nVirtual input devices used; physical headset, stereo compositor and SteamVR streaming not tested.\n");
        Debug.Log("REPORT_VR_OFFLINE_PLAYTEST_OK scenes=15 errors=0");phase=99;EditorApplication.isPlaying=false;
    }
    static void NextPhase(int value,double delay){phase=value;waitUntil=EditorApplication.timeSinceStartup+delay;}
    static void Require(bool passed,string message){if(!passed)throw new Exception(message);}
}
