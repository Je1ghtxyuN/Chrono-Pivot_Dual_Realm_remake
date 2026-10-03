using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;

[DefaultExecutionOrder(-100)]
public sealed class DesktopPlayer : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("键鼠和 VR 共用的视角相机。")]
    public Camera head;
    [Header("键鼠移动与转向")]
    [Min(0),Tooltip("WASD 移动速度，单位为米/秒；0 表示停止移动。")]
    public float speed=1.6f;
    [Min(0),Tooltip("鼠标转向灵敏度，单位为度/像素；水平和垂直共用。")]
    public float sensitivity=.12f;
    [Range(1,89),Tooltip("最大向上/向下观察角度，单位为度。")]
    public float pitchLimit=80;
    [Tooltip("勾选后反转鼠标垂直方向。")]
    public bool invertMouseY;
    [Header("桌面视角与碰撞")]
    [Min(.1f),Tooltip("相机相对于玩家根节点的高度，单位为米；不改变头显实际跟踪高度。")]
    public float eyeHeight=1.65f;
    [Min(.1f),Tooltip("桌面模式胶囊高度，单位为米；实际高度不能小于直径。")]
    public float bodyHeight=1.65f;
    [Tooltip("桌面胶囊中心相对于玩家根节点的位置；半径和台阶高度在 CharacterController 组件中调整。")]
    public Vector3 bodyCenter=new Vector3(0,.85f,0);
    [Header("桌面重力")]
    [Min(0),Tooltip("Physics.gravity 的倍率；VR 重力由 Gravity Provider 控制。")]
    public float gravityMultiplier=1;
    [Min(0),Tooltip("最大下落速度，单位为米/秒。")]
    public float maximumFallSpeed=30;
    [Min(0),Tooltip("站在地面时向下保持接触的速度，单位为米/秒。")]
    public float groundStickSpeed=2;
    [Header("桌面提示")]
    public bool showControls=true;
    public bool showCrosshair=true;
    public bool IsDesktop {get;private set;}
    public bool Captured {get;private set;}
    public bool CanInteract=>IsDesktop && Captured && !capturedThisFrame;
    bool configured,capturedThisFrame;
    float verticalSpeed;
    CharacterController body;
    TrackedPoseDriver[] trackers;
    ContinuousMoveProvider move;
    SnapTurnProvider turn;
    GravityProvider gravity;
    Transform offset;
    void Awake()
    {
        body=GetComponent<CharacterController>();trackers=GetComponentsInChildren<TrackedPoseDriver>();
        move=GetComponent<ContinuousMoveProvider>();turn=GetComponent<SnapTurnProvider>();gravity=GetComponent<GravityProvider>();
        offset=GetComponent<XROrigin>().CameraFloorOffsetObject.transform;
    }
    void Update()
    {
        bool tracked=XRSettings.isDeviceActive || InputSystem.devices.OfType<XRHMD>().Any(d=>d.trackingState.ReadValue()!=0);
        bool desktop=ReportXRStartup.ForceDesktop || !tracked;
        if(!configured || desktop!=IsDesktop)SetMode(desktop);
        capturedThisFrame=false;
        if(!IsDesktop)return;
        var keyboard=Keyboard.current;var mouse=Mouse.current;
        if(keyboard!=null && keyboard.escapeKey.wasPressedThisFrame)Capture(false);
        if(!Captured)
        {
            if(mouse!=null && mouse.leftButton.wasPressedThisFrame && !PointerOverMenu(mouse.position.ReadValue())){Capture(true);capturedThisFrame=true;}
            return;
        }
        if(mouse!=null)
        {
            var delta=mouse.delta.ReadValue()*sensitivity;
            transform.Rotate(0,delta.x,0,Space.World);
            float pitch=Mathf.Clamp(Mathf.DeltaAngle(0,head.transform.localEulerAngles.x)+delta.y*(invertMouseY?1:-1),-pitchLimit,pitchLimit);
            head.transform.localRotation=Quaternion.Euler(pitch,0,0);
        }
        Vector2 axis=keyboard==null?Vector2.zero:new Vector2((keyboard.dKey.isPressed?1:0)-(keyboard.aKey.isPressed?1:0),(keyboard.wKey.isPressed?1:0)-(keyboard.sKey.isPressed?1:0));
        axis=Vector2.ClampMagnitude(axis,1);
        verticalSpeed=body.isGrounded?-groundStickSpeed:Mathf.Max(verticalSpeed+Physics.gravity.y*gravityMultiplier*Time.deltaTime,-maximumFallSpeed);
        body.Move((transform.right*axis.x*speed+transform.forward*axis.y*speed+Vector3.up*verticalSpeed)*Time.deltaTime);
    }
    void SetMode(bool desktop)
    {
        configured=true;IsDesktop=desktop;verticalSpeed=0;
        foreach(var tracker in trackers)tracker.enabled=!desktop;
        move.enabled=turn.enabled=gravity.enabled=!desktop;
        var wrist=offset.Find("Left Controller/Scene and controller instructions");if(wrist!=null)wrist.gameObject.SetActive(!desktop);
        if(desktop)
        {
            ApplyDesktopBody();head.transform.localRotation=Quaternion.identity;
        }
        Capture(false);
    }
    public void ResetView(){if(IsDesktop){head.transform.localRotation=Quaternion.identity;verticalSpeed=0;}}
    void ApplyDesktopBody()
    {
        body.height=Mathf.Max(bodyHeight,body.radius*2);body.center=bodyCenter;
        offset.localPosition=Vector3.zero;head.transform.localPosition=Vector3.up*eyeHeight;
    }
    void OnValidate()
    {
        speed=Mathf.Max(0,speed);sensitivity=Mathf.Max(0,sensitivity);pitchLimit=Mathf.Clamp(pitchLimit,1,89);
        eyeHeight=Mathf.Max(.1f,eyeHeight);bodyHeight=Mathf.Max(.1f,bodyHeight);
        gravityMultiplier=Mathf.Max(0,gravityMultiplier);maximumFallSpeed=Mathf.Max(0,maximumFallSpeed);groundStickSpeed=Mathf.Max(0,groundStickSpeed);
        if(Application.isPlaying && IsDesktop && body!=null)ApplyDesktopBody();
    }
    void Capture(bool capture){Captured=capture;Cursor.lockState=capture?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!capture;}
    static bool PointerOverMenu(Vector2 position)
    {
        // Existing IMGUI panels occupy the upper left. Keep capture clicks off them.
        return position.x<460 && position.y>Screen.height-510;
    }
    void OnApplicationFocus(bool focus){if(!focus)Capture(false);}
    void OnDestroy(){if(IsDesktop)Capture(false);}
    void OnGUI()
    {
        if(!IsDesktop)return;
        if(showControls)GUI.Label(new Rect(16,Screen.height-55,1100,45),Captured?"WASD: move   Mouse: look   Click / E: use; hold to grab   Release: drop   R: reset   Esc: unlock   PgUp / PgDn: scene":"DESKTOP — Click the scene to play. Esc unlocks the mouse for menus.");
        if(Captured && showCrosshair)GUI.Label(new Rect(Screen.width*.5f-5,Screen.height*.5f-10,20,25),"+");
    }
}
