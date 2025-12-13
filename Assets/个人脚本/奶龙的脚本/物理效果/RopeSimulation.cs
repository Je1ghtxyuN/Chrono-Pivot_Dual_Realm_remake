using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeSimulation : MonoBehaviour
{
    [Header("Rope Settings")]
    public int segmentCount = 15;
    public float segmentLength = 0.1f;
    public Transform startPoint;
    public Transform endPoint;
    public bool fixStart = true;
    public bool fixEnd = true;

    [Header("Physics Settings")]
    public float gravity = -9.81f;
    public float stiffness = 0.8f;
    public float damping = 0.98f;
    public float airResistance = 0.99f;
    public int constraintIterations = 5;

    [Header("Rendering")]
    public float ropeWidth = 0.05f;
    public Material ropeMaterial;
    public Color ropeColor = Color.white;
    public float textureTiling = 5f;

    private LineRenderer lineRenderer;
    private Vector3[] positions;
    private Vector3[] prevPositions;

    void Start()
    {
        InitializeRope();
    }

    void InitializeRope()
    {
        // 初始化线段渲染器
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        lineRenderer.material = ropeMaterial;
        lineRenderer.material.color = ropeColor;
        lineRenderer.material.mainTextureScale = new Vector2(textureTiling, 1);

        // 初始化位置数组
        positions = new Vector3[segmentCount];
        prevPositions = new Vector3[segmentCount];

        // 设置初始位置
        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 startPos = startPoint ? startPoint.position : transform.position;
            Vector3 endPos = endPoint ? endPoint.position : transform.position + Vector3.right * segmentLength * (segmentCount - 1);

            positions[i] = Vector3.Lerp(startPos, endPos, t);
            prevPositions[i] = positions[i];
        }
    }

    void Update()
    {
        if (startPoint && fixStart) positions[0] = startPoint.position;
        if (endPoint && fixEnd) positions[segmentCount - 1] = endPoint.position;

        SimulatePhysics();
        ApplyConstraints();
        RenderRope();
    }

    void SimulatePhysics()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            // 跳过固定点
            if ((i == 0 && fixStart) || (i == segmentCount - 1 && fixEnd)) continue;

            // Verlet积分物理模拟
            Vector3 velocity = (positions[i] - prevPositions[i]) * damping;
            prevPositions[i] = positions[i];
            positions[i] += velocity;
            positions[i].y += gravity * Time.deltaTime * Time.deltaTime;
            positions[i] *= airResistance;
        }
    }

    void ApplyConstraints()
    {
        // 多次迭代以获得更稳定的约束
        for (int iteration = 0; iteration < constraintIterations; iteration++)
        {
            // 保持线段长度约束
            for (int i = 0; i < segmentCount - 1; i++)
            {
                Vector3 segment = positions[i + 1] - positions[i];
                float currentLength = segment.magnitude;
                float lengthDifference = (currentLength - segmentLength) / currentLength;

                // 调整位置以保持长度
                if (!(i == 0 && fixStart))
                    positions[i] += segment * lengthDifference * stiffness * 0.5f;

                if (!(i == segmentCount - 2 && fixEnd))
                    positions[i + 1] -= segment * lengthDifference * stiffness * 0.5f;
            }

            // 碰撞约束
            for (int i = 0; i < segmentCount; i++)
            {
                // 跳过固定点
                if ((i == 0 && fixStart) || (i == segmentCount - 1 && fixEnd)) continue;

                // 简单的地面碰撞
                if (positions[i].y < 0)
                {
                    positions[i].y = 0;
                }
            }
        }
    }

    void RenderRope()
    {
        lineRenderer.SetPositions(positions);
    }

    // 在编辑器中可视化绳索点
    void OnDrawGizmos()
    {
        if (positions == null) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < segmentCount; i++)
        {
            Gizmos.DrawSphere(positions[i], ropeWidth * 0.5f);
        }
    }
}