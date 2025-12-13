using UnityEngine;

public class PortalRotator : MonoBehaviour
{
    [Header("旋转设置")]
    [SerializeField] private float rotationSpeed = 30f;  // 旋转速度（度/秒）
    [SerializeField] private bool clockwise = true;     // 是否顺时针旋转

    [Header("旋转轴设置")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;  // 旋转轴（默认为X轴）
    [SerializeField] private Space rotationSpace = Space.Self;       // 旋转空间（局部坐标）

    [Header("旋转中心设置")]
    [SerializeField] private Transform rotationCenterObject;         // 旋转中心空物体
    [SerializeField] private bool useCustomCenter = false;           // 是否使用自定义中心点

    private Vector3 actualRotationCenter;

    void Update()
    {
        // 计算当前帧的旋转角度
        float rotationAmount = rotationSpeed * Time.deltaTime;

        // 根据旋转方向调整角度（顺时针为正，逆时针为负）
        if (!clockwise)
        {
            rotationAmount = -rotationAmount;
        }

        // 应用旋转
        if (useCustomCenter && rotationCenterObject != null)
        {
            // 绕指定的中心点旋转
            RotateAroundCustomCenter(rotationAmount);
        }
        else
        {
            // 使用原来的轴心旋转
            transform.Rotate(rotationAxis, rotationAmount, rotationSpace);
        }
    }

    private void RotateAroundCustomCenter(float rotationAmount)
    {
        // 使用Unity内置的RotateAround方法绕指定点旋转
        Vector3 worldCenter = rotationCenterObject.position;
        Vector3 worldAxis = GetWorldRotationAxis();

        transform.RotateAround(worldCenter, worldAxis, rotationAmount);
    }

    private Vector3 GetWorldRotationAxis()
    {
        // 根据旋转空间设置返回正确的旋转轴
        if (rotationSpace == Space.Self)
        {
            return transform.TransformDirection(rotationAxis);
        }
        else
        {
            return rotationAxis;
        }
    }

    // 公共方法：设置旋转速度
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    // 公共方法：设置旋转方向
    public void SetRotationDirection(bool isClockwise)
    {
        clockwise = isClockwise;
    }

    // 公共方法：切换旋转方向
    public void ToggleRotationDirection()
    {
        clockwise = !clockwise;
    }

    // 公共方法：设置旋转中心物体
    public void SetRotationCenterObject(Transform centerObject)
    {
        rotationCenterObject = centerObject;
        useCustomCenter = (centerObject != null);
    }

    // 公共方法：启用自定义中心旋转
    public void EnableCustomCenterRotation()
    {
        useCustomCenter = true;
    }

    // 公共方法：禁用自定义中心旋转（使用轴心）
    public void DisableCustomCenterRotation()
    {
        useCustomCenter = false;
    }

    // 公共方法：开始旋转
    public void StartRotation()
    {
        enabled = true;
    }

    // 公共方法：停止旋转
    public void StopRotation()
    {
        enabled = false;
    }

    // 在Scene视图中显示旋转中心
    void OnDrawGizmosSelected()
    {
        if (useCustomCenter && rotationCenterObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(rotationCenterObject.position, 0.1f);

            // 绘制从中心到物体的线
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rotationCenterObject.position, transform.position);

            // 绘制旋转轴
            Gizmos.color = Color.blue;
            Vector3 axisEnd = rotationCenterObject.position + GetWorldRotationAxis() * 0.5f;
            Gizmos.DrawLine(rotationCenterObject.position, axisEnd);
        }
    }
}