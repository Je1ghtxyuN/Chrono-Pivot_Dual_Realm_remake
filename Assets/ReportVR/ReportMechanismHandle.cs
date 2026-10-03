using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public enum ReportHandleMotion {Rotation,Slide}

// A grabbed physical handle stays on its joint/rail instead of floating with the hand.
public sealed class ReportMechanismHandle : ReportGrabbable
{
    [Header("机关运动与单位")]
    public ReportHandleMotion motion;
    public Vector3 localAxis=Vector3.forward;
    public Vector2 valueRange=new Vector2(0,1);
    public float startingValue;
    public string unit="";
    public Vector2 angleRange=new Vector2(0,90);
    public Vector2 slideRange=new Vector2(0,.5f);
    [Tooltip("带偏心握点的转轮/太阳模型按绕轴拖动计算；普通旋钮按手腕转角计算。")]
    public bool rotationFromPosition;
    [Min(.001f)] public float desktopUnitsPerPixel=.005f;
    [Min(.01f)] public float vrTravelForFullRange=.5f;
    [Min(1)] public float vrDegreesForFullRange=90;
    public TextMesh readout;
    public float Value {get;private set;}
    public int Operations {get;private set;}
    Rigidbody body;
    Vector3 restPosition,worldAxis,previousHandPosition,previousDirection;
    Quaternion restRotation;
    Transform source;
    bool desktop,operationRecorded,referenceRight;
    bool mechanismInitialized;
    protected override void Awake()
    {
        base.Awake();InitializeMechanism();
    }
    void InitializeMechanism()
    {
        if(mechanismInitialized)return;mechanismInitialized=true;
        body=GetComponent<Rigidbody>();restPosition=transform.position;restRotation=transform.rotation;
        worldAxis=(restRotation*localAxis).normalized;Value=startingValue;ApplyPose();
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);source=args.interactorObject.GetAttachTransform(this);
        desktop=args.interactorObject is ReportGrabInteractor grabber && grabber.desktopOnly;
        previousHandPosition=source.position;referenceRight=Vector3.ProjectOnPlane(source.up,worldAxis).sqrMagnitude<.01f;
        previousDirection=GestureDirection();operationRecorded=false;
    }
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase phase)
    {
        base.ProcessInteractable(phase);
        if(phase==XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected && source!=null)
        {
            float old=Value,delta;
            if(desktop)delta=Mouse.current==null?0:(Mouse.current.delta.ReadValue().x-Mouse.current.delta.ReadValue().y)*desktopUnitsPerPixel;
            else if(motion==ReportHandleMotion.Slide)
                delta=Vector3.Dot(source.position-previousHandPosition,worldAxis)*(valueRange.y-valueRange.x)/vrTravelForFullRange;
            else
            {
                var direction=GestureDirection();
                delta=Vector3.SignedAngle(previousDirection,direction,worldAxis)*(valueRange.y-valueRange.x)/vrDegreesForFullRange;previousDirection=direction;
            }
            previousHandPosition=source.position;Value=Mathf.Clamp(Value+delta,valueRange.x,valueRange.y);
            if(!operationRecorded && Mathf.Abs(Value-old)>.001f){Operations++;operationRecorded=true;}
        }
        ApplyPose();
    }
    Vector3 GestureDirection()
    {
        var vector=rotationFromPosition?source.position-restPosition:referenceRight?source.right:source.up;
        return Vector3.ProjectOnPlane(vector,worldAxis).normalized;
    }
    public void SetValue(float value){Value=Mathf.Clamp(value,valueRange.x,valueRange.y);ApplyPose();}
    void ApplyPose()
    {
        float t=Mathf.InverseLerp(valueRange.x,valueRange.y,Value);
        var p=restPosition+(motion==ReportHandleMotion.Slide?worldAxis*Mathf.Lerp(slideRange.x,slideRange.y,t):Vector3.zero);
        var q=motion==ReportHandleMotion.Rotation?Quaternion.AngleAxis(Mathf.Lerp(angleRange.x,angleRange.y,t),worldAxis)*restRotation:restRotation;
        transform.SetPositionAndRotation(p,q);if(body!=null){body.position=p;body.rotation=q;}
        if(readout!=null)readout.text=Value.ToString("F2")+" "+unit+"\n握住模型后：转手 / 拖鼠标";
    }
    public override void ResetPose(){InitializeMechanism();base.ResetPose();Value=startingValue;Operations=0;ApplyPose();}
}
