using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalBodySurfaceGravity : MonoBehaviour
{
    // 注意：只能作用于球体之间。非球体可以为其制作一个近似的球形碰撞体来实现类似效果。
    // 说明：
    // 1) 挂载在“非中心天体”上，仅对指定 Layer 的物体施加局部引力。
    // 2) 可在靠近表面时自动“吸附”物体，吸附后保持其原有刚体类型（不改 Kinematic）。
    // 3) 通过 LayerMask 与吸附状态联合筛选，实现“只影响吸附在其上的物体”。

    // ===== 目标筛选 =====
    [Header("目标筛选")]
    [Tooltip("可被该天体局部引力影响的层级。")]
    [SerializeField] private LayerMask affectedLayers = ~0;
    [Tooltip("引力作用半径（以天体中心为球心）。")]
    [SerializeField] private float influenceRadius = 30f;
    [Tooltip("仅对已被本天体吸附的物体施加引力。")]
    [SerializeField] private bool onlyAffectAttachedBodies = true;
    [Tooltip("当启用自动吸附时，允许未吸附目标先受到牵引，帮助其进入吸附距离。")]
    [SerializeField] private bool allowPreAttachAttraction = true;
    [Tooltip("用于计算表面距离的碰撞体。为空时自动使用当前物体 Collider。")]
    [SerializeField] private Collider gravitySurfaceCollider;

    // ===== 吸附规则 =====
    [Header("吸附规则")]
    [Tooltip("是否在接近表面时自动吸附。")]
    [SerializeField] private bool autoAttach = true;
    [Tooltip("进入该距离后会被吸附。")]
    [SerializeField] private float attachDistance = 2.0f;
    [Tooltip("超过该距离后会自动脱离（建议 >= attachDistance）。")]
    [SerializeField] private float detachDistance = 4.0f;
    [Tooltip("吸附后是否关闭物体自身 useGravity。")]
    [SerializeField] private bool disableAttachedUseGravity = true;
    [Tooltip("吸附后是否设为该节点的子物体。")]
    [SerializeField] private bool reparentOnAttach = true;
    [Tooltip("吸附后的父节点。为空时默认使用当前天体 Transform。")]
    [SerializeField] private Transform attachRoot;
    [Tooltip("吸附后是否锁定在当前表面锚点，避免在天体运动时乱滑。")]
    [SerializeField] private bool stabilizeAttachedPosition = true;
    [Tooltip("吸附后是否清空线速度和角速度，快速消除残余漂移。")]
    [SerializeField] private bool zeroVelocityOnAttach = true;

    // ===== 局部引力参数 =====
    [Header("局部引力参数")]
    [Tooltip("局部引力加速度（ForceMode.Acceleration）。")]
    [SerializeField] private float gravityAcceleration = 16f;
    [Tooltip("是否将吸附/受力物体的 up 方向缓慢对齐到表面法线。")]
    [SerializeField] private bool alignUpToSurface = true;
    [Tooltip("仅对已吸附物体执行朝向对齐（Y+ 朝外法线）。")]
    [SerializeField] private bool alignOnlyAttachedBodies = true;
    [Tooltip("对齐速度。")]
    [SerializeField] private float alignSpeed = 8f;

    // ===== Layer 管理（可选） =====
    [Header("Layer 管理（可选）")]
    [Tooltip("吸附时是否自动切换物体 Layer。")]
    [SerializeField] private bool changeLayerOnAttach = false;
    [Tooltip("吸附后切换到的 Layer（-1 表示不切换）。")]
    [SerializeField] private int attachedLayer = -1;

    private readonly Collider[] overlapResults = new Collider[256];
    private readonly HashSet<Rigidbody> processedBodies = new HashSet<Rigidbody>();
    private readonly Dictionary<Rigidbody, AttachedBodyState> attachedBodies = new Dictionary<Rigidbody, AttachedBodyState>();

    private Rigidbody selfRigidbody;

    private struct AttachedBodyState
    {
        public Transform originalParent;
        public bool originalUseGravity;
        public int originalLayer;
        public Vector3 localAnchorPosition;
    }

    public int AttachedBodyCount => attachedBodies.Count;

    private void Awake()
    {
        selfRigidbody = GetComponent<Rigidbody>();

        if (gravitySurfaceCollider == null)
        {
            gravitySurfaceCollider = GetComponent<Collider>();
        }

        if (attachRoot == null)
        {
            attachRoot = transform;
        }

        if (detachDistance < attachDistance)
        {
            detachDistance = attachDistance;
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

        float detachThreshold = Mathf.Max(detachDistance, attachDistance + 0.01f);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = overlapResults[i];
            if (hit == null)
            {
                continue;
            }

            Rigidbody target = hit.attachedRigidbody;
            if (target == null)
            {
                continue;
            }

            if (target == selfRigidbody)
            {
                continue;
            }

            if (!processedBodies.Add(target))
            {
                continue;
            }

            float surfaceDistance = GetSurfaceDistance(target.worldCenterOfMass);
            bool isAttached = attachedBodies.ContainsKey(target);

            if (autoAttach && !isAttached && surfaceDistance <= attachDistance)
            {
                AttachBody(target);
                isAttached = true;
            }

            if (isAttached && surfaceDistance > detachThreshold)
            {
                DetachBody(target);
                isAttached = false;
            }

            bool canAffect = isAttached || !onlyAffectAttachedBodies || (autoAttach && allowPreAttachAttraction);
            if (!canAffect)
            {
                continue;
            }

            if (isAttached && stabilizeAttachedPosition)
            {
                StabilizeAttachedBody(target);
            }

            if (!target.isKinematic && !(isAttached && stabilizeAttachedPosition))
            {
                Vector3 direction = (transform.position - target.worldCenterOfMass).normalized;
                target.AddForce(direction * gravityAcceleration, ForceMode.Acceleration);
            }

            if (alignUpToSurface && (!alignOnlyAttachedBodies || isAttached))
            {
                AlignBodyUp(target.transform);
            }
        }
    }

    [ContextMenu("吸附范围内目标")]
    public void AttachBodiesInRange()
    {
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
            if (target == null || target == selfRigidbody)
            {
                continue;
            }

            float surfaceDistance = GetSurfaceDistance(target.worldCenterOfMass);
            if (surfaceDistance <= attachDistance)
            {
                AttachBody(target);
            }
        }
    }

    [ContextMenu("全部脱离")]
    public void DetachAllBodies()
    {
        List<Rigidbody> snapshot = new List<Rigidbody>(attachedBodies.Keys);
        for (int i = 0; i < snapshot.Count; i++)
        {
            Rigidbody body = snapshot[i];
            if (body != null)
            {
                DetachBody(body);
            }
        }

        attachedBodies.Clear();
    }

    public bool IsBodyAttached(Rigidbody target)
    {
        return target != null && attachedBodies.ContainsKey(target);
    }

    public void AttachBody(Rigidbody target)
    {
        if (target == null || target == selfRigidbody || attachedBodies.ContainsKey(target))
        {
            return;
        }

        AttachedBodyState state = new AttachedBodyState
        {
            originalParent = target.transform.parent,
            originalUseGravity = target.useGravity,
            originalLayer = target.gameObject.layer,
            localAnchorPosition = (attachRoot != null ? attachRoot : transform).InverseTransformPoint(target.worldCenterOfMass)
        };

        attachedBodies.Add(target, state);

        if (reparentOnAttach && attachRoot != null)
        {
            target.transform.SetParent(attachRoot, true);
        }

        if (disableAttachedUseGravity)
        {
            target.useGravity = false;
        }

        if (zeroVelocityOnAttach)
        {
            target.velocity = Vector3.zero;
            target.angularVelocity = Vector3.zero;
        }

        if (changeLayerOnAttach && attachedLayer >= 0 && attachedLayer <= 31)
        {
            target.gameObject.layer = attachedLayer;
        }
    }

    public void DetachBody(Rigidbody target)
    {
        if (target == null)
        {
            return;
        }

        if (!attachedBodies.TryGetValue(target, out AttachedBodyState state))
        {
            return;
        }

        target.transform.SetParent(state.originalParent, true);
        target.useGravity = state.originalUseGravity;
        target.gameObject.layer = state.originalLayer;

        attachedBodies.Remove(target);
    }

    private void AlignBodyUp(Transform target)
    {
        Vector3 surfaceUp = (target.position - transform.position).normalized;
        if (surfaceUp.sqrMagnitude < 0.000001f)
        {
            return;
        }

        Quaternion fromTo = Quaternion.FromToRotation(target.up, surfaceUp);
        Quaternion desired = fromTo * target.rotation;
        target.rotation = Quaternion.Slerp(target.rotation, desired, alignSpeed * Time.fixedDeltaTime);
    }

    private void StabilizeAttachedBody(Rigidbody target)
    {
        if (!attachedBodies.TryGetValue(target, out AttachedBodyState state))
        {
            return;
        }

        Transform root = attachRoot != null ? attachRoot : transform;
        Vector3 anchorWorld = root.TransformPoint(state.localAnchorPosition);

        if (target.isKinematic)
        {
            target.transform.position = anchorWorld;
            return;
        }

        target.MovePosition(anchorWorld);
        target.velocity = Vector3.zero;
        target.angularVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        DetachAllBodies();
    }

    private float GetSurfaceDistance(Vector3 worldPoint)
    {
        if (gravitySurfaceCollider == null)
        {
            return Vector3.Distance(transform.position, worldPoint);
        }

        Vector3 closestPoint = gravitySurfaceCollider.ClosestPoint(worldPoint);
        return Vector3.Distance(worldPoint, closestPoint);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 1f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, influenceRadius);

        Gizmos.color = new Color(1f, 0.7f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, attachDistance);

        Gizmos.color = new Color(1f, 0.35f, 0.2f, 1f);
        Gizmos.DrawWireSphere(transform.position, detachDistance);
    }
}
