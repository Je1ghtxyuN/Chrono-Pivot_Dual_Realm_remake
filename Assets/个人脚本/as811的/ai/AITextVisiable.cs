using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

[ExecuteAlways]
[DisallowMultipleComponent]
public class AITextVisiable : MonoBehaviour
{
    // 该脚本作用：
    // 1) 从 AIChatBasic 读取 OutputText
    // 2) 将文本显示到 3D TextMeshPro 文本框
    // 3) 支持“挂载物体前方显示”与“朝向相机 billboard”

    // ===== 数据源 =====
    [Header("数据源")]
    [Tooltip("AI 文本来源组件，通常绑定同物体上的 AIChatBasic。")]
    [FormerlySerializedAs("aiChatBasic")]
    [SerializeField] private AIChatBasic _aiChatBasic;

    // ===== 文本目标 =====
    [Header("文本目标（TextMeshPro）")]
    [Tooltip("用于显示 AI 输出的 TextMeshPro 组件。")]
    [FormerlySerializedAs("textBox")]
    [SerializeField] private TextMeshPro _textBox;
    [Tooltip("当 textBox 为空时是否自动创建一个默认文本框。")]
    [FormerlySerializedAs("autoCreateTextBoxIfMissing")]
    [SerializeField] private bool _autoCreateTextBoxIfMissing = true;

    // ===== 文本框空间变换（相对于当前挂载物体） =====
    [Header("变换设置（相对当前物体）")]
    [Tooltip("开启后每帧应用本地位置/旋转/缩放设置。")]
    [FormerlySerializedAs("followThisTransform")]
    [SerializeField] private bool _followThisTransform = true;
    [Tooltip("开启后文本始终朝向主相机（billboard）。")]
    [FormerlySerializedAs("billboardToPlayerCamera")]
    [SerializeField] private bool _billboardToPlayerCamera = false;
    [Tooltip("文本框相对当前物体的本地位置（默认在正前方）。")]
    [FormerlySerializedAs("localPosition")]
    [SerializeField] private Vector3 _localPosition = new Vector3(0f, 0.2f, 0.8f);
    [Tooltip("文本框相对当前物体的本地欧拉角旋转。")]
    [FormerlySerializedAs("localEulerAngles")]
    [SerializeField] private Vector3 _localEulerAngles = Vector3.zero;
    [Tooltip("文本框本地缩放（TextMeshPro 常用较小值）。")]
    [FormerlySerializedAs("localScale")]
    [SerializeField] private Vector3 _localScale = Vector3.one * 0.01f;

    // ===== 文本框布局 =====
    [Header("文本框布局")]
    [Tooltip("文本框宽度（对应 rectTransform.sizeDelta.x）。")]
    [FormerlySerializedAs("boxWidth")]
    [SerializeField] private float _boxWidth = 6f;
    [Tooltip("文本框高度（对应 rectTransform.sizeDelta.y）。")]
    [FormerlySerializedAs("boxHeight")]
    [SerializeField] private float _boxHeight = 3f;
    [Tooltip("是否启用自动换行。")]
    [FormerlySerializedAs("enableWordWrap")]
    [SerializeField] private bool _enableWordWrap = true;
    [Tooltip("文本对齐方式。")]
    [FormerlySerializedAs("alignment")]
    [SerializeField] private TextAlignmentOptions _alignment = TextAlignmentOptions.TopLeft;
    [Tooltip("文本溢出处理方式。")]
    [FormerlySerializedAs("overflow")]
    [SerializeField] private TextOverflowModes _overflow = TextOverflowModes.Overflow;

    // ===== 文本样式 =====
    [Header("文本样式")]
    [Tooltip("字体资源，不填写则使用 TextMeshPro 默认字体。")]
    [FormerlySerializedAs("fontAsset")]
    [SerializeField] private TMP_FontAsset _fontAsset;
    [Tooltip("字体大小。")]
    [FormerlySerializedAs("fontSize")]
    [SerializeField] private float _fontSize = 2.4f;
    [Tooltip("字体颜色。")]
    [FormerlySerializedAs("fontColor")]
    [SerializeField] private Color _fontColor = Color.white;
    [Tooltip("字体样式（Normal/Bold/Italic 等）。")]
    [FormerlySerializedAs("fontStyle")]
    [SerializeField] private FontStyles _fontStyle = FontStyles.Normal;
    [Tooltip("是否启用自动字号。")]
    [FormerlySerializedAs("enableAutoSizing")]
    [SerializeField] private bool _enableAutoSizing = false;
    [Tooltip("自动字号的最小值。")]
    [FormerlySerializedAs("minAutoSize")]
    [SerializeField] private float _minAutoSize = 1.2f;
    [Tooltip("自动字号的最大值。")]
    [FormerlySerializedAs("maxAutoSize")]
    [SerializeField] private float _maxAutoSize = 4f;

    // ===== 刷新策略 =====
    [Header("刷新策略")]
    [Tooltip("Play 模式下是否每帧刷新文本与布局。")]
    [FormerlySerializedAs("updateInPlayMode")]
    [SerializeField] private bool _updateInPlayMode = true;
    [Tooltip("Edit 模式下是否刷新（配合 ExecuteAlways）。")]
    [FormerlySerializedAs("updateInEditMode")]
    [SerializeField] private bool _updateInEditMode = true;

    // 缓存上次文本，避免每帧重复赋值造成不必要开销。
    private string _lastText = string.Empty;

    // 组件首次添加时调用：自动补齐引用与默认显示状态。
    private void Reset()
    {
        TryResolveDependencies();
        EnsureTextBoxExists();
        ApplyVisualSettings();
        ApplyTextStyle();
    }

    // 运行时初始化。
    private void Awake()
    {
        TryResolveDependencies();
        EnsureTextBoxExists();
        ApplyVisualSettings();
        ApplyTextStyle();
        RefreshFromSource(force: true);
    }

    // 组件启用时确保显示状态正确。
    private void OnEnable()
    {
        TryResolveDependencies();
        EnsureTextBoxExists();
        ApplyVisualSettings();
        ApplyTextStyle();
        RefreshFromSource(force: true);
    }

    // Inspector 参数变化时（编辑器）立即应用。
    private void OnValidate()
    {
        if (_textBox == null)
        {
            EnsureTextBoxExists();
        }

        ApplyVisualSettings();
        ApplyTextStyle();
        RefreshFromSource(force: true);
    }

    // 每帧刷新：按配置决定是否更新布局、是否 billboard、是否同步文本。
    private void Update()
    {
        bool canRunInEditor = !Application.isPlaying && _updateInEditMode;
        bool canRunInPlayMode = Application.isPlaying && _updateInPlayMode;

        if (!canRunInEditor && !canRunInPlayMode)
        {
            return;
        }

        if (_followThisTransform)
        {
            ApplyVisualSettings();
        }

        if (_billboardToPlayerCamera)
        {
            // 保持文本正对相机，提升可读性。
            ApplyBillboardToCamera();
        }

        RefreshFromSource(force: false);
    }

    // 自动找依赖：优先当前物体上的 AIChatBasic，文本框找子物体 TextMeshPro。
    private void TryResolveDependencies()
    {
        if (_aiChatBasic == null)
        {
            _aiChatBasic = GetComponent<AIChatBasic>();
        }

        if (_textBox == null)
        {
            _textBox = GetComponentInChildren<TextMeshPro>(true);
        }
    }

    // 若未指定 TextMeshPro，则按开关自动创建一个默认子物体。
    private void EnsureTextBoxExists()
    {
        if (_textBox != null || !_autoCreateTextBoxIfMissing)
        {
            return;
        }

        Transform existingChild = transform.Find("AITextBox_TMP");
        if (existingChild != null)
        {
            _textBox = existingChild.GetComponent<TextMeshPro>();
            if (_textBox != null)
            {
                return;
            }
        }

        GameObject go = new GameObject("AITextBox_TMP");
        go.transform.SetParent(transform, false);
        _textBox = go.AddComponent<TextMeshPro>();
    }

    // 应用位置、旋转、缩放与文本框几何属性。
    private void ApplyVisualSettings()
    {
        if (_textBox == null)
        {
            return;
        }

        Transform t = _textBox.transform;
        t.localPosition = _localPosition;
        t.localEulerAngles = _localEulerAngles;
        t.localScale = _localScale;

        _textBox.rectTransform.sizeDelta = new Vector2(_boxWidth, _boxHeight);
        _textBox.enableWordWrapping = _enableWordWrap;
        _textBox.alignment = _alignment;
        _textBox.overflowMode = _overflow;
    }

    // 应用字体、字号、颜色、样式等文本渲染参数。
    private void ApplyTextStyle()
    {
        if (_textBox == null)
        {
            return;
        }

        if (_fontAsset != null)
        {
            _textBox.font = _fontAsset;
        }

        _textBox.fontSize = _fontSize;
        _textBox.color = _fontColor;
        _textBox.fontStyle = _fontStyle;
        _textBox.enableAutoSizing = _enableAutoSizing;
        _textBox.fontSizeMin = _minAutoSize;
        _textBox.fontSizeMax = _maxAutoSize;
    }

    // 从 AIChatBasic 拉取文本；当文本变化时更新到 TextMeshPro。
    private void RefreshFromSource(bool force)
    {
        if (_textBox == null || _aiChatBasic == null)
        {
            return;
        }

        string current = _aiChatBasic.OutputText ?? string.Empty;
        if (!force && current == _lastText)
        {
            return;
        }

        _lastText = current;
        _textBox.text = current;
    }

    // Billboard：让文本朝向主相机。
    private void ApplyBillboardToCamera()
    {
        if (_textBox == null)
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Transform textTransform = _textBox.transform;
        Vector3 toCamera = cam.transform.position - textTransform.position;
        if (toCamera.sqrMagnitude < 0.000001f)
        {
            return;
        }

        textTransform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }

    // 选中物体时绘制可视化辅助线框，便于在 Scene 中调节文本框位置与尺寸。
    private void OnDrawGizmosSelected()
    {
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
        Gizmos.DrawWireCube(_localPosition, new Vector3(0.06f, 0.06f, 0.06f));

        Quaternion boxRot = Quaternion.Euler(_localEulerAngles);
        Matrix4x4 boxMatrix = Matrix4x4.TRS(_localPosition, boxRot, _localScale);
        Gizmos.matrix = transform.localToWorldMatrix * boxMatrix;
        Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(_boxWidth, _boxHeight, 0.001f));

        Gizmos.matrix = old;
    }

    // 手动同步：用于编辑器下快速刷新一次显示。
    [ContextMenu("Sync Once")]
    public void SyncOnce()
    {
        TryResolveDependencies();
        EnsureTextBoxExists();
        ApplyVisualSettings();
        ApplyTextStyle();
        RefreshFromSource(force: true);
    }
}
