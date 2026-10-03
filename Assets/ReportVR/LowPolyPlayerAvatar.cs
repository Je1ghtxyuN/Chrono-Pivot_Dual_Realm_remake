using UnityEngine;
using UnityEngine.Rendering;

// A small articulated avatar driven by the same head and hand targets as the rig.
public sealed class LowPolyPlayerAvatar : MonoBehaviour
{
    [Header("跟踪目标与模型引用")]
    public Transform headTarget,leftTarget,rightTarget;
    public Transform torso,face,leftUpperArm,leftForearm,rightUpperArm,rightForearm,leftHand,rightHand,leftLeg,rightLeg;
    public Renderer[] firstPersonHidden;
    [Tooltip("第一人称中隐藏头部与衣身，只保留投影；在开始播放时应用。")]
    public bool firstPerson;
    [Header("模型姿态校准（米）")]
    public Vector2 trackedHeightRange=new Vector2(1.1f,2.05f);
    public float torsoDrop=.65f;
    public Vector3 headOffset=new Vector3(0,-.035f,-.075f);
    [Min(.02f)] public float armSegmentLength=.29f;
    [Min(0)] public float shoulderHalfWidth=.205f;
    public float shoulderDrop=.37f;
    public float wristBackOffset=.035f;
    [Header("桌面双手位置（相机局部坐标，米）")]
    public Vector3 desktopLeftHandOffset=new Vector3(-.26f,-.42f,.32f);
    public Vector3 desktopRightHandOffset=new Vector3(.26f,-.42f,.32f);
    [Header("行走表现")]
    [Min(0),Tooltip("每走一米累计的摆腿相位，单位为弧度/米。")]
    public float gaitRate=9;
    [Range(0,45),Tooltip("最大摆腿角度，单位为度。")]
    public float gaitAngle=15;
    [Min(.01f),Tooltip("单帧超过此距离视为传送，不累计步态。")]
    public float teleportDistance=.5f;
    [Header("FBX 蒙皮角色")]
    public bool skinnedRig;
    public Renderer[] firstPersonVisible=new Renderer[0];
    Transform[] bindBones;
    Vector3[] bindPositions;
    Quaternion[] bindRotations;
    Quaternion headCalibration,leftHandCalibration,rightHandCalibration;
    float bindEyeHeight;
    Vector3 previousPosition;
    float gait;
    DesktopPlayer desktop;
    void Start()
    {
        desktop=GetComponentInParent<DesktopPlayer>();previousPosition=transform.position;
        if(skinnedRig)
        {
            bindBones=torso.GetComponentsInChildren<Transform>();
            bindPositions=new Vector3[bindBones.Length];bindRotations=new Quaternion[bindBones.Length];
            for(int i=0;i<bindBones.Length;i++){bindPositions[i]=bindBones[i].localPosition;bindRotations[i]=bindBones[i].localRotation;}
            bindEyeHeight=transform.InverseTransformPoint(face.position).y-headOffset.y;
            headCalibration=Quaternion.Inverse(transform.rotation)*face.rotation;
            leftHandCalibration=Quaternion.Inverse(transform.rotation)*Quaternion.FromToRotation(-transform.right,transform.forward)*leftHand.rotation;
            rightHandCalibration=Quaternion.Inverse(transform.rotation)*Quaternion.FromToRotation(transform.right,transform.forward)*rightHand.rotation;
            foreach(var renderer in firstPersonVisible)renderer.enabled=firstPerson;
        }
        if(firstPerson)foreach(var renderer in firstPersonHidden)renderer.shadowCastingMode=ShadowCastingMode.ShadowsOnly;
    }
    void LateUpdate()
    {
        if(headTarget==null)return;
        if(skinnedRig){PoseSkinned();return;}
        Vector3 localHead=transform.parent.InverseTransformPoint(headTarget.position);
        float yaw=headTarget.eulerAngles.y-transform.parent.eulerAngles.y;
        transform.localPosition=new Vector3(localHead.x,0,localHead.z);
        transform.localRotation=Quaternion.Euler(0,yaw,0);
        float height=Mathf.Clamp(localHead.y,trackedHeightRange.x,trackedHeightRange.y);
        torso.localPosition=new Vector3(0,height-torsoDrop,0);
        face.position=headTarget.TransformPoint(headOffset);
        face.rotation=headTarget.rotation;
        bool desk=desktop!=null && desktop.IsDesktop;
        Vector3 left=desk?headTarget.TransformPoint(desktopLeftHandOffset):leftTarget.position;
        Vector3 right=desk?headTarget.TransformPoint(desktopRightHandOffset):rightTarget.position;
        PoseArm(-1,height,left,desk?headTarget.rotation:leftTarget.rotation,leftUpperArm,leftForearm,leftHand);
        PoseArm(1,height,right,desk?headTarget.rotation:rightTarget.rotation,rightUpperArm,rightForearm,rightHand);
        float distance=Vector3.Distance(new Vector3(transform.position.x,0,transform.position.z),new Vector3(previousPosition.x,0,previousPosition.z));
        previousPosition=transform.position;
        if(distance<teleportDistance)gait+=distance*gaitRate;
        float swing=distance>.001f?Mathf.Sin(gait)*gaitAngle:0;
        leftLeg.localRotation=Quaternion.Euler(swing,0,0);rightLeg.localRotation=Quaternion.Euler(-swing,0,0);
    }
    void PoseSkinned()
    {
        for(int i=0;i<bindBones.Length;i++){bindBones[i].localPosition=bindPositions[i];bindBones[i].localRotation=bindRotations[i];}
        var localHead=transform.parent.InverseTransformPoint(headTarget.position);
        transform.localPosition=new Vector3(localHead.x,0,localHead.z);
        transform.localRotation=Quaternion.Euler(0,headTarget.eulerAngles.y-transform.parent.eulerAngles.y,0);
        float height=Mathf.Clamp(localHead.y,trackedHeightRange.x,trackedHeightRange.y);
        torso.position+=transform.up*(height-bindEyeHeight);
        face.rotation=headTarget.rotation*headCalibration;
        bool desk=desktop!=null && desktop.IsDesktop;
        BoneArm(desk?headTarget.TransformPoint(desktopLeftHandOffset):leftTarget.position,desk?headTarget.rotation:leftTarget.rotation,-1,leftUpperArm,leftForearm,leftHand,leftHandCalibration);
        BoneArm(desk?headTarget.TransformPoint(desktopRightHandOffset):rightTarget.position,desk?headTarget.rotation:rightTarget.rotation,1,rightUpperArm,rightForearm,rightHand,rightHandCalibration);
        float distance=Vector3.Distance(new Vector3(transform.position.x,0,transform.position.z),new Vector3(previousPosition.x,0,previousPosition.z));
        previousPosition=transform.position;if(distance<teleportDistance)gait+=distance*gaitRate;
        float swing=distance>.001f && distance<teleportDistance?Mathf.Sin(gait)*gaitAngle:0;
        leftLeg.rotation=Quaternion.AngleAxis(swing,transform.right)*leftLeg.rotation;
        rightLeg.rotation=Quaternion.AngleAxis(-swing,transform.right)*rightLeg.rotation;
    }
    void BoneArm(Vector3 target,Quaternion rotation,int side,Transform upper,Transform lower,Transform hand,Quaternion calibration)
    {
        Vector3 shoulder=upper.position;
        float a=Vector3.Distance(shoulder,lower.position),b=Vector3.Distance(lower.position,hand.position);
        Vector3 delta=target-shoulder;Vector3 direction=delta.sqrMagnitude>.000001f?delta.normalized:transform.forward;
        float distance=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.001f,a+b-.001f);
        float along=(a*a-b*b+distance*distance)/(2*distance);
        Vector3 bend=Vector3.ProjectOnPlane(transform.right*side*.5f-transform.up+transform.forward*.2f,direction).normalized;
        Vector3 elbow=shoulder+direction*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
        upper.rotation=Quaternion.FromToRotation(lower.position-shoulder,elbow-shoulder)*upper.rotation;
        lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
        hand.position=target;hand.rotation=rotation*calibration;
    }
    void PoseArm(int side,float height,Vector3 target,Quaternion rotation,Transform upper,Transform lower,Transform hand)
    {
        Vector3 shoulder=transform.TransformPoint(new Vector3(side*shoulderHalfWidth,height-shoulderDrop,0));
        Vector3 wrist=target-rotation*Vector3.forward*wristBackOffset;
        Vector3 direction=(wrist-shoulder).normalized;
        float distance=Mathf.Clamp(Vector3.Distance(shoulder,wrist),.03f,armSegmentLength*2-.01f);
        Vector3 bend=Vector3.ProjectOnPlane(transform.right*side*.5f-transform.up+transform.forward*.2f,direction).normalized;
        Vector3 elbow=shoulder+direction*distance*.5f+bend*Mathf.Sqrt(Mathf.Max(0,armSegmentLength*armSegmentLength-distance*distance*.25f));
        Segment(upper,shoulder,elbow);Segment(lower,elbow,wrist);
        hand.position=target;hand.rotation=rotation;
    }
    static void Segment(Transform bone,Vector3 a,Vector3 b)
    {
        bone.position=a;bone.rotation=Quaternion.FromToRotation(Vector3.up,b-a);bone.localScale=new Vector3(1,Vector3.Distance(a,b),1);
    }
    void OnValidate()
    {
        trackedHeightRange.x=Mathf.Max(.1f,trackedHeightRange.x);trackedHeightRange.y=Mathf.Max(trackedHeightRange.x,trackedHeightRange.y);
        armSegmentLength=Mathf.Max(.02f,armSegmentLength);shoulderHalfWidth=Mathf.Max(0,shoulderHalfWidth);
        gaitRate=Mathf.Max(0,gaitRate);gaitAngle=Mathf.Clamp(gaitAngle,0,45);teleportDistance=Mathf.Max(.01f,teleportDistance);
    }
}

