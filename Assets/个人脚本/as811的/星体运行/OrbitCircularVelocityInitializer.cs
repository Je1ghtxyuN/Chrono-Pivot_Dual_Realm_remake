using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class OrbitCircularVelocityInitializer : MonoBehaviour
{
    // 使用时别忘了取消useGravity，否则初速度会被重力叠加，导致轨道不稳定。
    // 说明：
    // 1) 本脚本用于给卫星刚体设置“近圆轨道”初速度。
    // 2) 若绑定 centerGravity，将自动读取中心天体的引力参数。
    // 3) 计算公式：v = sqrt(mu / r)，其中 mu = G * scale * M。

    // ===== 中心天体引用 =====
    [Header("中心天体")]
    [Tooltip("中心天体 Transform。若已设置 centerGravity，可自动同步。")]
    [SerializeField] private Transform centerBody;
    [Tooltip("中心天体上的 GravityBasic 组件（优先读取其参数）。")]
    [SerializeField] private GravityBasic centerGravity;

    // ===== 引力参数（当未绑定 centerGravity 时使用） =====
    [Header("引力参数(未绑定中心脚本时使用)")]
    [Tooltip("引力常数 G（仅在未绑定 centerGravity 时生效）。")]
    [SerializeField] private float gravityConstant = 0.6674f;
    [Tooltip("引力缩放倍率（仅在未绑定 centerGravity 时生效）。")]
    [SerializeField] private float gravityScale = 1f;
    [Tooltip("中心天体质量 M（仅在未绑定 centerGravity 时生效）。")]
    [SerializeField] private float centerMass = 10000f;

    // ===== 轨道参数 =====
    [Header("轨道参数")]
    [Tooltip("轨道平面法线。常用 (0,1,0) 表示在 XZ 平面绕行。")]
    [SerializeField] private Vector3 orbitNormal = Vector3.up;
    [Tooltip("是否按顺时针方向设置切向速度。")]
    [SerializeField] private bool clockwise = true;
    [Tooltip("速度倍率。1 为理论近圆轨道，<1 更易下坠，>1 更易外逸。")]
    [SerializeField] private float speedMultiplier = 1f;
    [Tooltip("是否叠加中心天体当前速度。中心在运动时建议开启。")]
    [SerializeField] private bool inheritCenterVelocity = true;
    [Tooltip("是否在 Start 自动应用一次初速度。")]
    [SerializeField] private bool applyOnStart = true;

    private Rigidbody selfRigidbody;

    // 缓存自身刚体。
    private void Awake()
    {
        selfRigidbody = GetComponent<Rigidbody>();
    }

    // 在组件重置时同步缓存。
    private void Reset()
    {
        selfRigidbody = GetComponent<Rigidbody>();
    }

    // 开局自动应用初速度（可在 Inspector 关闭）。
    private void Start()
    {
        if (applyOnStart)
        {
            ApplyInitialVelocity();
        }
    }

    // 右键菜单手动触发：应用近圆轨道初速度。
    [ContextMenu("应用近圆轨道初速度")]
    public void ApplyInitialVelocity()
    {
        if (selfRigidbody == null)
        {
            selfRigidbody = GetComponent<Rigidbody>();
        }

        if (centerGravity != null)
        {
            centerBody = centerGravity.transform;
        }

        if (centerBody == null)
        {
            Debug.LogWarning($"[{nameof(OrbitCircularVelocityInitializer)}] 未设置中心天体。", this);
            return;
        }

        Vector3 radial = transform.position - centerBody.position;
        float radius = radial.magnitude;
        if (radius < 0.0001f)
        {
            Debug.LogWarning($"[{nameof(OrbitCircularVelocityInitializer)}] 卫星与中心天体距离过小，无法计算轨道速度。", this);
            return;
        }

        float g = centerGravity != null ? centerGravity.GravityConstant : gravityConstant;
        float scale = centerGravity != null ? centerGravity.GravityScale : gravityScale;
        float mass = centerGravity != null ? centerGravity.CenterMass : centerMass;

        // 近圆轨道速度：v = sqrt(mu / r)。
        float mu = Mathf.Max(0f, g * scale * mass);
        float speed = Mathf.Sqrt(mu / radius) * Mathf.Max(0f, speedMultiplier);

        Vector3 radialDir = radial / radius;
        Vector3 normal = orbitNormal.sqrMagnitude > 0.000001f ? orbitNormal.normalized : Vector3.up;
        Vector3 tangent = Vector3.Cross(normal, radialDir);

        if (tangent.sqrMagnitude <= 0.000001f)
        {
            Vector3 fallbackAxis = Mathf.Abs(Vector3.Dot(radialDir, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
            tangent = Vector3.Cross(fallbackAxis, radialDir);
        }

        tangent.Normalize();
        if (!clockwise)
        {
            tangent = -tangent;
        }

        Vector3 orbitVelocity = tangent * speed;

        if (inheritCenterVelocity)
        {
            Rigidbody centerRb = centerBody.GetComponent<Rigidbody>();
            if (centerRb != null)
            {
                orbitVelocity += centerRb.velocity;
            }
        }

        selfRigidbody.velocity = orbitVelocity;
    }
}
