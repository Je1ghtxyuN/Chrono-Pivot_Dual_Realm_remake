using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Let XRI own selection, attachment, Rigidbody motion and throwing.
public sealed class ReportGrabInteractor : XRBaseInteractor
{
    [Header("抓取输入")]
    public DesktopPlayer player;
    [Tooltip("相机上的抓取器勾选此项；左右手柄抓取器不勾选。")]
    public bool desktopOnly;
    [Tooltip("VR 握把键输入，支持数字握把或模拟握力；桌面使用左键/E。")]
    public InputActionReference grabInput;
    [Header("抓取距离（米）")]
    [Min(0),Tooltip("手部周围优先抓取的范围；手进入物件内部也可抓取。")]
    public float nearRadius=.16f;
    [Min(0),Tooltip("远距离抓取上限，实体障碍会遮挡抓取射线。")]
    public float rayDistance=4;
    [Min(.25f),Tooltip("桌面拿住物件时，物件中心距相机的距离。")]
    public float desktopHoldDistance=1.2f;
    public LayerMask physicsLayers=~0;
    [Header("桌面主动抛掷")]
    [Min(.1f)] public float throwChargeDuration=1.1f;
    [Min(0)] public float minimumThrowSpeed=2;
    [Min(0)] public float maximumThrowSpeed=8;
    public float ThrowCharge01 {get;private set;}
    public ReportGrabbable Candidate {get;private set;}
    bool blockedUntilRelease=true;
    bool chargingThrow;
    bool RawPressed=>desktopOnly
        ? (Mouse.current!=null && Mouse.current.leftButton.isPressed || Keyboard.current!=null && Keyboard.current.eKey.isPressed)
        : grabInput!=null && grabInput.action!=null && grabInput.action.IsPressed();
    bool Active
    {
        get
        {
            if(player==null || player.IsDesktop!=desktopOnly)return false;
            if(desktopOnly)return player.CanInteract;
            var usage=handedness==InteractorHandedness.Left?CommonUsages.LeftHand:CommonUsages.RightHand;
            return InputSystem.devices.OfType<UnityEngine.InputSystem.XR.XRController>().Any(d=>d.usages.Contains(usage) && d.trackingState.ReadValue()!=0);
        }
    }
    public override bool isHoverActive=>base.isHoverActive && Active;
    public override bool isSelectActive=>base.isSelectActive && Active && RawPressed && !blockedUntilRelease;
    public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        base.PreprocessInteractor(phase);
        if(phase!=XRInteractionUpdateOrder.UpdatePhase.Dynamic)return;
        if(!RawPressed)blockedUntilRelease=false;
        else if(!Active)blockedUntilRelease=true;
        if(desktopOnly && attachTransform!=null)attachTransform.localPosition=Vector3.forward*desktopHoldDistance;
    }
    public override bool CanSelect(IXRSelectInteractable target)=>base.CanSelect(target) && (!hasSelection || IsSelecting(target));
    void Update()
    {
        if(!desktopOnly || !Active || !hasSelection || firstInteractableSelected is ReportMechanismHandle){chargingThrow=false;ThrowCharge01=0;return;}
        bool charge=Mouse.current!=null && Mouse.current.rightButton.isPressed || Keyboard.current!=null && Keyboard.current.fKey.isPressed;
        if(charge){chargingThrow=true;ThrowCharge01=Mathf.Clamp01(ThrowCharge01+Time.deltaTime/Mathf.Max(.1f,throwChargeDuration));}
        else if(chargingThrow)
        {
            var item=firstInteractableSelected as ReportGrabbable;
            if(item!=null){item.RequestThrow(transform.forward*Mathf.Lerp(minimumThrowSpeed,maximumThrowSpeed,ThrowCharge01));ReleaseUntilButtonUp();}
            chargingThrow=false;ThrowCharge01=0;
        }
    }
    public override void GetValidTargets(List<IXRInteractable> targets)
    {
        targets.Clear();Candidate=null;
        if(!Active)return;
        if(hasSelection){targets.Add((IXRInteractable)firstInteractableSelected);Candidate=firstInteractableSelected as ReportGrabbable;return;}
        if(!desktopOnly && nearRadius>0)
        {
            float closest=float.PositiveInfinity;
            foreach(var collider in Physics.OverlapSphere(transform.position,nearRadius,physicsLayers,QueryTriggerInteraction.Ignore))
            {
                var grab=collider.GetComponentInParent<ReportGrabbable>();
                if(grab==null || !grab.isActiveAndEnabled || grab.isSelected)continue;
                float distance=(collider.ClosestPoint(transform.position)-transform.position).sqrMagnitude;
                if(distance<closest){closest=distance;Candidate=grab;}
            }
        }
        if(Candidate==null)
        {
            var hits=Physics.RaycastAll(transform.position,transform.forward,rayDistance,physicsLayers,QueryTriggerInteraction.Collide);
            System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
            foreach(var hit in hits)
            {
                if(hit.collider.GetComponentInParent<DesktopPlayer>()!=null)continue;
                if(hit.collider.isTrigger && hit.collider.GetComponentInParent<DemoStation>()==null)continue;
                var grab=hit.collider.GetComponentInParent<ReportGrabbable>();
                if(grab!=null && grab.isActiveAndEnabled && !grab.isSelected)Candidate=grab;
                break; // Solid surfaces occlude distant grabs.
            }
        }
        if(Candidate!=null)targets.Add(Candidate);
    }
    public void ReleaseUntilButtonUp()
    {
        blockedUntilRelease=true;
        while(hasSelection)interactionManager.SelectExit(this,firstInteractableSelected);
    }
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);blockedUntilRelease=true;
    }
}
