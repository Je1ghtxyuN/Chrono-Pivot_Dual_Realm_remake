using UnityEngine;
using UnityEngine.InputSystem; // 引用新输入系统
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Jump;

[RequireComponent(typeof(JumpProvider))]
public class JumpFix : MonoBehaviour
{
    [Header("输入设置")]
    [Tooltip("请绑定右手的 Primary Button (A键)")]
    public InputActionProperty jumpInputSource;

    [Header("跳跃限制设置")]
    [Tooltip("是否允许空中跳跃（二段跳）")]
    [SerializeField] private bool allowDoubleJump = false;

    [Tooltip("跳跃冷却时间（秒）")]
    [SerializeField] private float jumpCooldown = 0.5f;

    private JumpProvider m_JumpProvider;
    private bool m_LastButtonState;
    private float m_LastJumpTime;

    private void Awake()
    {
        m_JumpProvider = GetComponent<JumpProvider>();
    }

    private void Start()
    {
        ConfigureJumpProvider();
    }

    private void ConfigureJumpProvider()
    {
        if (m_JumpProvider == null) return;

        // 1. 禁止 JumpProvider 自己的无限空跳
        m_JumpProvider.unlimitedInAirJumps = false;

        // 2. 设置空跳次数
        m_JumpProvider.inAirJumpCount = allowDoubleJump ? 1 : 0;
    }

    private void Update()
    {
        // 获取按键状态 (支持 float 类型的 trigger 或 bool 类型的 button)
        bool isPressed = IsJumpInputPressed();

        // 只有在按下的一瞬间触发 (Down)
        if (isPressed && !m_LastButtonState)
        {
            if (Time.time - m_LastJumpTime > jumpCooldown)
            {
                TryPerformJump();
            }
        }

        m_LastButtonState = isPressed;
    }

    private bool IsJumpInputPressed()
    {
        if (jumpInputSource.action == null) return false;

        // 读取按键值，兼容 Button 和 Value 类型
        return jumpInputSource.action.ReadValue<float>() > 0.5f;
    }

    private void TryPerformJump()
    {
        // 核心检查：JumpProvider 会负责检测是否在地面 (isGrounded)
        if (m_JumpProvider != null && m_JumpProvider.CanJump())
        {
            m_JumpProvider.Jump();
            m_LastJumpTime = Time.time;
            // Debug.Log("执行跳跃");
        }
        else
        {
            // 如果这里打印了，说明按键检测到了，但是没在地面上
            // Debug.LogWarning("跳跃失败：不在地面 (Not Grounded) 或次数耗尽");
        }
    }

    private void OnEnable()
    {
        jumpInputSource.action?.Enable();
    }

    private void OnDisable()
    {
        jumpInputSource.action?.Disable();
    }
}