using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ReportGrabbable : XRGrabInteractable
{
    [Tooltip("桌面默认平稳放下；开启后才从相机移动估算抛掷速度。")]
    public bool throwInDesktop;
    [Min(0),Tooltip("VR 或开启桌面抛掷时的释放速度上限，单位为米/秒。")]
    public float maximumReleaseSpeed=8;
    [Min(0),Tooltip("释放角速度上限，单位为弧度/秒。")]
    public float maximumReleaseAngularSpeed=15;
    Rigidbody body;
    Vector3 initialPosition;
    Quaternion initialRotation;
    bool initialized,resetPending,desktopSelection;
    bool requestedThrow;
    Vector3 requestedVelocity;
    public void RequestThrow(Vector3 velocity){requestedVelocity=velocity;requestedThrow=true;}
    protected override void Awake(){base.Awake();CapturePose();}
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);GetComponent<ReportThrowTrack>()?.ResetMeasurements();desktopSelection=args.interactorObject is ReportGrabInteractor grabber && grabber.desktopOnly;
    }
    protected override void Detach()
    {
        base.Detach();
        if(body.isKinematic)return;
        body.velocity=requestedThrow?Vector3.ClampMagnitude(requestedVelocity,maximumReleaseSpeed):desktopSelection && !throwInDesktop?Vector3.zero:Vector3.ClampMagnitude(body.velocity,maximumReleaseSpeed);
        body.angularVelocity=desktopSelection && !throwInDesktop?Vector3.zero:Vector3.ClampMagnitude(body.angularVelocity,maximumReleaseAngularSpeed);
        requestedThrow=false;GetComponent<ReportThrowTrack>()?.RecordRelease(body.velocity);
    }
    void CapturePose()
    {
        if(initialized)return;initialized=true;
        body=GetComponent<Rigidbody>();initialPosition=transform.localPosition;initialRotation=transform.localRotation;
    }
    public virtual void ResetPose()
    {
        CapturePose();
        while(isSelected)
        {
            var interactor=interactorsSelecting[0];
            if(interactor is ReportGrabInteractor grabber)grabber.ReleaseUntilButtonUp();
            else interactionManager.SelectExit(interactor,this);
        }
        RestorePose();resetPending=Application.isPlaying;
    }
    void RestorePose()
    {
        requestedThrow=false;GetComponent<ReportThrowTrack>()?.ResetMeasurements();
        if(!body.isKinematic){body.velocity=Vector3.zero;body.angularVelocity=Vector3.zero;}
        transform.localPosition=initialPosition;transform.localRotation=initialRotation;
        body.position=transform.position;body.rotation=transform.rotation;
        body.Sleep();
    }
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        base.ProcessInteractable(phase);
        // XRI applies throw velocity at end of frame; reset must run after that.
        if(resetPending && phase==XRInteractionUpdateOrder.UpdatePhase.Late){RestorePose();resetPending=false;}
    }
}
