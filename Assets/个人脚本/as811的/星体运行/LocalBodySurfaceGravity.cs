using System.Collections.Generic;
using UnityEngine;

public class LocalBodySurfaceGravity : MonoBehaviour
{
    // 说明：
    // 1) 挂在“非中心天体”上，仅影响指定 Layer 的目标。
    // 2) 目标靠近表面后自动吸附，吸附后可沿表面走动。
    // 3) 通过径向纠偏实现贴地，切向速度保留，避免“钉死不动”。

    [Header("目标筛选")]
    [Tooltip("可被该天体局部引力影响的层级。")]
    [SerializeField] private LayerMask affectedLayers = ~0;
    [Tooltip("引力检测半径（以天体中心为球心）。")]
    [SerializeField] private float influenceRadius = 30f;
    [Tooltip("用于表面距离与法线计算的碰撞体；为空时默认使用当前物体Collider。")]
    [SerializeField] private Collider gravitySurfaceCollider;

    [Header("吸附逻辑")]
    [Tooltip("是否自动吸附接近表面的目标。")]
    [SerializeField] private bool autoAttach = true;
    [Tooltip("接触判定容差。距离小于等于该值视为碰到天体表面。")]
    [SerializeField] private float contactEpsilon = 0.02f;
    [Tooltip("已吸附目标离开表面超过该距离后脱离。")]
    [SerializeField] private float detachDistance = 0.5f;
    [Tooltip("仅对已吸附目标施加影响。")]
    [SerializeField] private bool onlyAffectAttachedBodies = true;
    [Tooltip("仅影响已吸附目标时，是否允许未吸附目标先被牵引靠近。")]
    [SerializeField] private bool allowPreAttachAttraction = true;
    [Tooltip("吸附后是否关闭目标自身的Unity重力。")]
    [SerializeField] private bool disableUseGravityOnAttach = true;
    [Tooltip("吸附后是否忽略目标与天体碰撞，减少顶开/抖动。")]
    [SerializeField] private bool ignoreCollisionWithBody = true;
    [Tooltip("吸附后与表面的目标间距偏移。0表示尽量贴地。")]
    [SerializeField] private float surfaceOffset = 0f;

    [Header("移动与贴地")]
    [Tooltip("未吸附目标的牵引加速度（ForceMode.Acceleration）。")]
    [SerializeField] private float gravityAcceleration = 16f;
    [Tooltip("吸附后径向纠偏速度，越大越快贴回表面。")]
    [SerializeField] private float radialSnapSpeed = 18f;
    [Tooltip("径向速度阻尼。1表示完全移除径向速度，保留切向速度。")]
    [SerializeField, Range(0f, 1f)] private float radialVelocityDamping = 1f;
    [Tooltip("吸附后是否将目标Y+对齐到表面外法线。")]
    [SerializeField] private bool alignUpToNormal = true;
    [Tooltip("朝向对齐速度。")]
    [SerializeField] private float alignSpeed = 10f;

    [Header("调试")]
    [Tooltip("是否在Scene视图绘制目标到表面的调试线。")]
    [SerializeField] private bool drawDebugRays = true;
    [Tooltip("是否输出吸附/脱离日志。")]
    [SerializeField] private bool logAttachEvents = false;

    private readonly Collider[] overlapResults = new Collider[256];
    private readonly HashSet<Rigidbody> processedBodies = new HashSet<Rigidbody>();
    private readonly Dictionary<Rigidbody, AttachState> attachedBodies = new Dictionary<Rigidbody, AttachState>();
    private readonly Dictionary<Rigidbody, Collider[]> colliderCache = new Dictionary<Rigidbody, Collider[]>();

    private Rigidbody selfRigidbody;
    private Collider[] bodyColliders;

    private struct AttachState
    {
        // 吸附前状态缓存：用于脱离时恢复。
        public bool originalUseGravity;
        public float desiredRadius;
        public Collider[] targetColliders;
    }

    // 初始化：缓存自身组件并修正参数上下限。
    private void Awake()
    {
        selfRigidbody = GetComponent<Rigidbody>();
        bodyColliders = GetComponentsInChildren<Collider>(true);

        if (gravitySurfaceCollider == null)
        {
            gravitySurfaceCollider = GetComponent<Collider>();
        }
    }

    private void FixedUpdate()
    {
        processedBodies.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            influenceRadius,
            overlapResults,
            affectedLayers,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = overlapResults[i];
            if (hit == null)
            {
                continue;
            }

            Rigidbody target = hit.attachedRigidbody;
            if (target == null || target == selfRigidbody || !processedBodies.Add(target))
            {
                continue;
            }

            float surfaceDistance = GetSurfaceDistance(target);
            bool isAttached = attachedBodies.ContainsKey(target);
            bool isTouchingSurface = surfaceDistance <= Mathf.Max(0f, contactEpsilon);

            if (autoAttach && !isAttached && isTouchingSurface)
            {
                AttachBody(target);
                isAttached = true;
            }

            if (isAttached && surfaceDistance > detachDistance)
            {
                DetachBody(target);
                isAttached = false;
            }

            bool canAffect = isAttached || !onlyAffectAttachedBodies || (autoAttach && allowPreAttachAttraction);
            if (!canAffect)
            {
                continue;
            }

            Vector3 outwardNormal = GetOutwardNormal(target.worldCenterOfMass);

            if (isAttached)
            {
                StabilizeOnSurface(target, outwardNormal);
            }
            else if (!target.isKinematic)
            {
                target.AddForce(-outwardNormal * gravityAcceleration, ForceMode.Acceleration);
            }

            if (drawDebugRays)
            {
                Vector3 toSurface = gravitySurfaceCollider != null ? gravitySurfaceCollider.ClosestPoint(target.worldCenterOfMass) : transform.position;
                Debug.DrawLine(target.worldCenterOfMass, toSurface, isAttached ? Color.green : Color.yellow, Time.fixedDeltaTime, false);
            }
        }
    }

    public void AttachBody(Rigidbody target)
    {
        if (target == null || target == selfRigidbody || attachedBodies.ContainsKey(target))
        {
            return;
        }

        Collider[] targetColliders = GetTargetColliders(target);
        AttachState state = new AttachState
        {
            originalUseGravity = target.useGravity,
            desiredRadius = GetSurfaceRadius(target.worldCenterOfMass) + surfaceOffset,
            targetColliders = targetColliders
        };

        attachedBodies.Add(target, state);

        if (disableUseGravityOnAttach)
        {
            target.useGravity = false;
        }

        if (ignoreCollisionWithBody)
        {
            SetIgnoreCollision(state.targetColliders, true);
        }

        if (logAttachEvents)
        {
            Debug.Log($"[LocalBodySurfaceGravity] Attach: {target.name}", this);
        }
    }

    public void DetachBody(Rigidbody target)
    {
        if (target == null || !attachedBodies.TryGetValue(target, out AttachState state))
        {
            return;
        }

        target.useGravity = state.originalUseGravity;

        if (ignoreCollisionWithBody)
        {
            SetIgnoreCollision(state.targetColliders, false);
        }

        attachedBodies.Remove(target);

        if (logAttachEvents)
        {
            Debug.Log($"[LocalBodySurfaceGravity] Detach: {target.name}", this);
        }
    }

    [ContextMenu("全部脱离")]
    public void DetachAllBodies()
    {
        List<Rigidbody> snapshot = new List<Rigidbody>(attachedBodies.Keys);
        for (int i = 0; i < snapshot.Count; i++)
        {
            DetachBody(snapshot[i]);
        }
    }

    // 吸附后仅清除法线方向速度分量，不做其他位置/朝向修改。
    private void StabilizeOnSurface(Rigidbody target, Vector3 outwardNormal)
    {
        if (!attachedBodies.ContainsKey(target) || target.isKinematic)
        {
            return;
        }

        Vector3 velocity = target.velocity;
        float normalSpeed = Vector3.Dot(velocity, outwardNormal);
        target.velocity = velocity - outwardNormal * normalSpeed;
    }

    // 计算目标刚体到“指定表面碰撞体”的最短距离（按Collider最近点）。
    private float GetSurfaceDistance(Rigidbody target)
    {
        if (target == null)
        {
            return float.MaxValue;
        }

        if (gravitySurfaceCollider == null)
        {
            return Vector3.Distance(transform.position, target.worldCenterOfMass);
        }

        Collider[] targetColliders = GetTargetColliders(target);
        if (targetColliders == null || targetColliders.Length == 0)
        {
            return Vector3.Distance(transform.position, target.worldCenterOfMass);
        }

        float minDistance = float.MaxValue;
        for (int i = 0; i < targetColliders.Length; i++)
        {
            Collider targetCol = targetColliders[i];
            if (targetCol == null)
            {
                continue;
            }

            if (targetCol.transform.IsChildOf(transform))
            {
                continue;
            }

            Vector3 pOnSurface = gravitySurfaceCollider.ClosestPoint(targetCol.bounds.center);
            Vector3 pOnTarget = targetCol.ClosestPoint(pOnSurface);
            float d = Vector3.Distance(pOnSurface, pOnTarget);
            if (d < minDistance)
            {
                minDistance = d;
            }
        }

        if (minDistance == float.MaxValue)
        {
            return Vector3.Distance(transform.position, target.worldCenterOfMass);
        }

        return minDistance;
    }

    // 计算表面点到天体中心半径，用于吸附后目标半径。
    private float GetSurfaceRadius(Vector3 worldPoint)
    {
        if (gravitySurfaceCollider == null)
        {
            return Vector3.Distance(transform.position, worldPoint);
        }

        Vector3 closest = gravitySurfaceCollider.ClosestPoint(worldPoint);
        return Vector3.Distance(transform.position, closest);
    }

    // 获取表面外法线（由中心指向表面点）。
    private Vector3 GetOutwardNormal(Vector3 worldPoint)
    {
        if (gravitySurfaceCollider == null)
        {
            return (worldPoint - transform.position).normalized;
        }

        Vector3 closest = gravitySurfaceCollider.ClosestPoint(worldPoint);
        Vector3 normal = closest - transform.position;
        if (normal.sqrMagnitude < 0.000001f)
        {
            normal = worldPoint - transform.position;
        }

        return normal.normalized;
    }

    // 批量设置目标与天体碰撞忽略状态。
    private void SetIgnoreCollision(Collider[] targetColliders, bool ignore)
    {
        if (targetColliders == null || bodyColliders == null)
        {
            return;
        }

        for (int i = 0; i < targetColliders.Length; i++)
        {
            Collider targetCol = targetColliders[i];
            if (targetCol == null)
            {
                continue;
            }

            for (int j = 0; j < bodyColliders.Length; j++)
            {
                Collider bodyCol = bodyColliders[j];
                if (bodyCol == null || bodyCol == targetCol)
                {
                    continue;
                }

                Physics.IgnoreCollision(targetCol, bodyCol, ignore);
            }
        }
    }

    // 脚本禁用时恢复所有目标状态，避免残留。
    private void OnDisable()
    {
        DetachAllBodies();
        colliderCache.Clear();
    }

    private Collider[] GetTargetColliders(Rigidbody target)
    {
        if (target == null)
        {
            return null;
        }

        if (colliderCache.TryGetValue(target, out Collider[] cached) && cached != null)
        {
            return cached;
        }

        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        colliderCache[target] = colliders;
        return colliders;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 1f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, influenceRadius);
    }
}
