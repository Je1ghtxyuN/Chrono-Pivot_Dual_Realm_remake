using System.Collections.Generic;
using UnityEngine;

public class GravityBasic : MonoBehaviour
{
    // 说明：
    // 1) 本脚本挂载在中心天体上，对作用范围内刚体施加牛顿引力。
    // 2) 引力计算采用 F = G * M * m / r^2，并支持引力倍率调节。
    // 3) 使用 NonAlloc 查询和 Rigidbody 去重，减少 GC 与重复施力。

    // ===== 引力参数 =====
    [Header("引力参数")]
    [Tooltip("引力常数 G（游戏内单位，可按手感调节）。")]
    [SerializeField] private float gravityConstant = 0.6674f;
    [Tooltip("引力缩放倍率，用于整体调强/调弱引力。")]
    [SerializeField] private float gravityScale = 1f;
    [Tooltip("中心天体质量 M。")]
    [SerializeField] private float centerMass = 10000f;
    [Tooltip("最小计算距离下限，避免 r 过小导致力过大。")]
    [SerializeField] private float minDistance = 0.5f;

    // ===== 作用范围 =====
    [Header("作用范围")]
    [Tooltip("引力影响半径，只有半径内物体会被计算。")]
    [SerializeField] private float influenceRadius = 200f;
    [Tooltip("受影响层级过滤。")]
    [SerializeField] private LayerMask affectedLayers = ~0;
    [Tooltip("是否对 Kinematic 刚体也施加引力。")]
    [SerializeField] private bool includeKinematicBodies = false;

    // ===== 对外只读参数（供其他脚本读取） =====
    public float GravityConstant => gravityConstant;
    public float GravityScale => gravityScale;
    public float CenterMass => centerMass;
    public float EffectiveGravity => gravityConstant * gravityScale;

    // 运行时缓存：
    // overlapResults 用于 NonAlloc 查询结果缓存；
    // processedBodies 用于防止同一 Rigidbody（多个 Collider）被重复施力。
    private readonly Collider[] overlapResults = new Collider[256];
    private readonly HashSet<Rigidbody> processedBodies = new HashSet<Rigidbody>();
    private Rigidbody selfRigidbody;

    // 缓存自身刚体，后续用于排除“对自己施力”。
    private void Awake()
    {
        selfRigidbody = GetComponent<Rigidbody>();
    }

    // 物理帧中执行引力计算，保证与 Unity 物理系统节奏一致。
    private void FixedUpdate()
    {
        // 每帧先清空去重集合。
        processedBodies.Clear();

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            influenceRadius,
            overlapResults,
            affectedLayers,
            QueryTriggerInteraction.Ignore
        );

        float safeDistance = Mathf.Max(minDistance, 0.0001f);
        float minDistanceSqr = safeDistance * safeDistance;

        for (int i = 0; i < count; i++)
        {
            Collider hit = overlapResults[i];
            if (hit == null)
            {
                continue;
            }

            Rigidbody targetRigidbody = hit.attachedRigidbody;
            if (targetRigidbody == null)
            {
                continue;
            }

            if (targetRigidbody == selfRigidbody)
            {
                continue;
            }

            if (!processedBodies.Add(targetRigidbody))
            {
                continue;
            }

            if (!includeKinematicBodies && targetRigidbody.isKinematic)
            {
                continue;
            }

            // 牛顿万有引力：F = G * M * m / r^2。
            Vector3 direction = transform.position - targetRigidbody.worldCenterOfMass;
            float distanceSqr = Mathf.Max(direction.sqrMagnitude, minDistanceSqr);
            float forceMagnitude = (gravityConstant * gravityScale * centerMass * targetRigidbody.mass) / distanceSqr;
            Vector3 force = direction.normalized * forceMagnitude;
            targetRigidbody.AddForce(force, ForceMode.Force);
        }
    }

    // 在场景中选中对象时显示影响半径，便于调参。
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, influenceRadius);
    }
}
