using UnityEngine;

public enum ReportTextFacingMode
{
    [InspectorName("固定在世界/告示板上")] WorldFixed,
    [InspectorName("朝向玩家")] FacePlayer,
    [InspectorName("跟随父物体（手腕等）")] FollowParent
}

public sealed class ReportTextFacing : MonoBehaviour
{
    [Tooltip("告示板应使用固定朝向；仅悬浮提示使用朝向玩家。固定模式尊重场景中手动设置的旋转。")]
    public ReportTextFacingMode mode=ReportTextFacingMode.WorldFixed;
    [Tooltip("朝向玩家模式的视角目标；固定模式不使用该引用。")]
    public Transform viewer;
    [Tooltip("勾选后只绕世界竖直轴朝向玩家，避免文字俯仰。")]
    public bool yawOnly;
    void LateUpdate()
    {
        if(mode!=ReportTextFacingMode.FacePlayer || viewer==null)return;
        Vector3 direction=transform.position-viewer.position;
        if(yawOnly)direction=Vector3.ProjectOnPlane(direction,Vector3.up);
        if(direction.sqrMagnitude>.000001f)transform.rotation=Quaternion.LookRotation(direction);
    }
}
