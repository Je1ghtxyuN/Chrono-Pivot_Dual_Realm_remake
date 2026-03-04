using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhysicsController : MonoBehaviour
{
    private enum SupportedValueType
    {
        Unsupported = 0,
        Bool = 1,
        Int = 2,
        Float = 3
    }

    [Serializable]
    private class ControlBinding
    {
        [Tooltip("控制项在面板里的显示名称。")]
        public string displayName = "新控制项";

        [Tooltip("要控制的脚本组件（可来自任意物体）。")]
        public MonoBehaviour targetScript;

        [Tooltip("变量名：支持字段名或属性名（必须可读写）。")]
        public string memberName;

        [Tooltip("float/int 类型最小值。")]
        public float minValue = 0f;

        [Tooltip("float/int 类型最大值。")]
        public float maxValue = 1f;

        [Tooltip("仅 float 生效。步进值，0 表示不强制步进。")]
        public float floatStep = 0f;

        [Tooltip("是否覆盖全局控制项尺寸。")]
        public bool overrideItemSize;

        [Tooltip("当前控制项尺寸（宽, 高）。")]
        public Vector2 customItemSize = new Vector2(300f, 90f);

        public Vector2 ResolveItemSize(Vector2 defaultSize)
        {
            Vector2 result = overrideItemSize ? customItemSize : defaultSize;
            result.x = Mathf.Max(120f, result.x);
            result.y = Mathf.Max(50f, result.y);
            return result;
        }
    }

    private struct MemberAccessor
    {
        public MonoBehaviour target;
        public FieldInfo field;
        public PropertyInfo property;
        public SupportedValueType valueType;
        public string fullName;

        public object GetValue()
        {
            if (target == null)
            {
                return null;
            }

            if (field != null)
            {
                return field.GetValue(target);
            }

            if (property != null)
            {
                return property.GetValue(target);
            }

            return null;
        }

        public void SetValue(object value)
        {
            if (target == null)
            {
                return;
            }

            if (field != null)
            {
                field.SetValue(target, value);
                return;
            }

            if (property != null)
            {
                property.SetValue(target, value);
            }
        }
    }

    private class RuntimeControl
    {
        public ControlBinding binding;
        public MemberAccessor accessor;
        public SupportedValueType valueType;
        public Toggle toggle;
        public Slider slider;
        public Text valueText;
    }

    [Header("面板生成")]
    [Tooltip("开始运行时自动生成面板。")]
    [SerializeField] private bool buildOnStart = true;

    [Tooltip("场景中没有 EventSystem 时是否自动创建。")]
    [SerializeField] private bool autoCreateEventSystem = true;

    [Tooltip("运行时面板对象名称。")]
    [SerializeField] private string panelObjectName = "PhysicsRuntimePanel";

    [Tooltip("面板标题文本。")]
    [SerializeField] private string panelTitle = "物理参数面板";

    [Tooltip("面板初始尺寸（宽, 高）。")]
    [SerializeField] private Vector2 panelSize = new Vector2(700f, 520f);

    [Tooltip("面板相对挂载物体的本地位置。")]
    [SerializeField] private Vector3 panelLocalPosition = new Vector3(0f, 0f, 0.45f);

    [Tooltip("面板相对挂载物体的本地旋转。")]
    [SerializeField] private Vector3 panelLocalEulerAngles = Vector3.zero;

    [Tooltip("世界空间 Canvas 的本地缩放。")]
    [SerializeField] private Vector3 panelLocalScale = new Vector3(0.001f, 0.001f, 0.001f);

    [Header("布局设置")]
    [Tooltip("列数。1 表示纵向列表；大于 1 时按左上角开始逐个填充。")]
    [SerializeField, Min(1)] private int columnCount = 1;

    [Tooltip("默认控制项尺寸（宽, 高）。")]
    [SerializeField] private Vector2 defaultItemSize = new Vector2(300f, 90f);

    [Tooltip("控制项间距（x, y）。")]
    [SerializeField] private Vector2 itemSpacing = new Vector2(12f, 12f);

    [Tooltip("内容区内边距（左, 右, 上, 下）。")]
    [SerializeField] private RectOffset contentPadding = new RectOffset(16, 16, 16, 16);

    [Tooltip("是否根据控制项数量自动增大面板高度。")]
    [SerializeField] private bool autoExpandPanelHeight = true;

    [Header("视觉设置")]
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color itemBackgroundColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private Color sliderBackgroundColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color sliderFillColor = new Color(0.25f, 0.65f, 1f, 0.95f);
    [SerializeField] private Color sliderHandleColor = Color.white;
    [SerializeField] private Color warningTextColor = new Color(1f, 0.45f, 0.45f, 1f);

    [Header("同步设置")]
    [Tooltip("是否每帧从目标变量回读到 UI，避免外部脚本改值后 UI 不一致。")]
    [SerializeField] private bool syncUiFromTargetEveryFrame = true;

    [Tooltip("float 数值显示的小数位数。")]
    [SerializeField, Range(0, 6)] private int floatDisplayDecimals = 3;

    [Header("控制项列表")]
    [Tooltip("每一项代表一个可控变量。")]
    [SerializeField] private List<ControlBinding> controls = new List<ControlBinding>();

    private const float TITLE_HEIGHT = 42f;
    private readonly List<RuntimeControl> runtimeControls = new List<RuntimeControl>();

    private Font uiFont;
    private RectTransform panelRect;
    private bool isBuilding;

    private void Start()
    {
        if (buildOnStart)
        {
            RebuildPanel();
        }
    }

    private void LateUpdate()
    {
        if (!syncUiFromTargetEveryFrame || isBuilding)
        {
            return;
        }

        RefreshControlsFromTarget();
    }

    private void OnValidate()
    {
        columnCount = Mathf.Max(1, columnCount);
        panelSize.x = Mathf.Max(240f, panelSize.x);
        panelSize.y = Mathf.Max(140f, panelSize.y);
        defaultItemSize.x = Mathf.Max(120f, defaultItemSize.x);
        defaultItemSize.y = Mathf.Max(50f, defaultItemSize.y);
        itemSpacing.x = Mathf.Max(0f, itemSpacing.x);
        itemSpacing.y = Mathf.Max(0f, itemSpacing.y);

        if (controls == null)
        {
            return;
        }

        for (int i = 0; i < controls.Count; i++)
        {
            ControlBinding binding = controls[i];
            if (binding == null)
            {
                continue;
            }

            if (binding.maxValue < binding.minValue)
            {
                binding.maxValue = binding.minValue;
            }

            if (binding.floatStep < 0f)
            {
                binding.floatStep = 0f;
            }

            binding.customItemSize.x = Mathf.Max(120f, binding.customItemSize.x);
            binding.customItemSize.y = Mathf.Max(50f, binding.customItemSize.y);
        }
    }

    [ContextMenu("重建物理面板")]
    public void RebuildPanel()
    {
        isBuilding = true;
        runtimeControls.Clear();

        EnsureEventSystem();
        EnsureFont();
        DestroyExistingPanel();

        GameObject panelObject = new GameObject(
            panelObjectName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(Image)
        );

        panelObject.transform.SetParent(transform, false);
        panelObject.transform.localPosition = panelLocalPosition;
        panelObject.transform.localEulerAngles = panelLocalEulerAngles;
        panelObject.transform.localScale = panelLocalScale;

        Canvas canvas = panelObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 200;
        canvas.worldCamera = Camera.main;

        CanvasScaler scaler = panelObject.GetComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        scaler.referencePixelsPerUnit = 100f;

        panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = panelColor;

        BuildTitle(panelRect);

        RectTransform contentRect = CreateRectTransform("Content", panelRect);
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = new Vector2(0f, -TITLE_HEIGHT);

        BuildItems(contentRect);
        RefreshControlsFromTarget();
        isBuilding = false;
    }

    [ContextMenu("清理物理面板")]
    public void ClearPanel()
    {
        runtimeControls.Clear();
        DestroyExistingPanel();
    }

    private void BuildTitle(RectTransform parent)
    {
        RectTransform titleRect = CreateRectTransform("Title", parent);
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(0f, TITLE_HEIGHT);
        titleRect.anchoredPosition = Vector2.zero;

        Text titleText = CreateText("TitleText", titleRect, panelTitle, 22, FontStyle.Bold, TextAnchor.MiddleCenter, labelColor);
        RectTransform textRect = titleText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private void BuildItems(RectTransform contentRoot)
    {
        int safeColumns = Mathf.Max(1, columnCount);
        float cursorX = contentPadding.left;
        float cursorY = contentPadding.top;
        float rowMaxHeight = 0f;
        int columnIndex = 0;

        for (int i = 0; i < controls.Count; i++)
        {
            ControlBinding binding = controls[i];
            if (binding == null)
            {
                continue;
            }

            Vector2 size = binding.ResolveItemSize(defaultItemSize);

            if (columnIndex >= safeColumns)
            {
                cursorX = contentPadding.left;
                cursorY += rowMaxHeight + itemSpacing.y;
                rowMaxHeight = 0f;
                columnIndex = 0;
            }

            RectTransform itemRect = CreateRectTransform($"Item_{i}_{binding.displayName}", contentRoot);
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(0f, 1f);
            itemRect.pivot = new Vector2(0f, 1f);
            itemRect.sizeDelta = size;
            itemRect.anchoredPosition = new Vector2(cursorX, -cursorY);

            BuildSingleItem(itemRect, binding);

            cursorX += size.x + itemSpacing.x;
            rowMaxHeight = Mathf.Max(rowMaxHeight, size.y);
            columnIndex++;
        }

        if (columnIndex > 0)
        {
            cursorY += rowMaxHeight;
        }

        float requiredContentHeight = cursorY + contentPadding.bottom;
        if (autoExpandPanelHeight && panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, requiredContentHeight + TITLE_HEIGHT);
        }
    }

    private void BuildSingleItem(RectTransform itemRect, ControlBinding binding)
    {
        Image itemImage = itemRect.gameObject.AddComponent<Image>();
        itemImage.color = itemBackgroundColor;

        string displayName = string.IsNullOrWhiteSpace(binding.displayName)
            ? binding.memberName
            : binding.displayName;

        if (!TryResolveAccessor(binding, out MemberAccessor accessor, out string errorMessage))
        {
            CreateWarningItem(itemRect, string.IsNullOrWhiteSpace(displayName) ? "未命名控制项" : displayName, errorMessage);
            return;
        }

        switch (accessor.valueType)
        {
            case SupportedValueType.Bool:
                BuildBoolItem(itemRect, displayName, binding, accessor);
                break;
            case SupportedValueType.Int:
                BuildNumericItem(itemRect, displayName, binding, accessor, true);
                break;
            case SupportedValueType.Float:
                BuildNumericItem(itemRect, displayName, binding, accessor, false);
                break;
            default:
                CreateWarningItem(itemRect, displayName, "仅支持 bool / int / float。");
                break;
        }
    }

    private void BuildBoolItem(RectTransform itemRect, string displayName, ControlBinding binding, MemberAccessor accessor)
    {
        Text label = CreateText("Label", itemRect, displayName, 18, FontStyle.Normal, TextAnchor.MiddleLeft, labelColor);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10f, 8f);
        labelRect.offsetMax = new Vector2(-46f, -8f);

        Toggle toggle = CreateToggle(itemRect);

        bool initialValue = false;
        object raw = accessor.GetValue();
        if (raw is bool boolValue)
        {
            initialValue = boolValue;
        }

        toggle.SetIsOnWithoutNotify(initialValue);
        toggle.onValueChanged.AddListener(value => accessor.SetValue(value));

        runtimeControls.Add(new RuntimeControl
        {
            binding = binding,
            accessor = accessor,
            valueType = SupportedValueType.Bool,
            toggle = toggle
        });
    }

    private void BuildNumericItem(RectTransform itemRect, string displayName, ControlBinding binding, MemberAccessor accessor, bool isInteger)
    {
        Text label = CreateText("Label", itemRect, displayName, 16, FontStyle.Normal, TextAnchor.UpperLeft, labelColor);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.sizeDelta = new Vector2(0f, 24f);
        labelRect.anchoredPosition = new Vector2(0f, -6f);

        Text valueText = CreateText("Value", itemRect, string.Empty, 15, FontStyle.Bold, TextAnchor.MiddleRight, labelColor);
        RectTransform valueRect = valueText.rectTransform;
        valueRect.anchorMin = new Vector2(1f, 0f);
        valueRect.anchorMax = new Vector2(1f, 0f);
        valueRect.pivot = new Vector2(1f, 0f);
        valueRect.sizeDelta = new Vector2(74f, 26f);
        valueRect.anchoredPosition = new Vector2(-8f, 8f);

        Slider slider = CreateSlider(itemRect, binding.minValue, binding.maxValue, isInteger);

        if (isInteger)
        {
            int minInt = Mathf.RoundToInt(binding.minValue);
            int maxInt = Mathf.RoundToInt(binding.maxValue);
            slider.SetValueWithoutNotify(minInt);

            object raw = accessor.GetValue();
            int startValue = raw is int intValue ? intValue : minInt;
            startValue = Mathf.Clamp(startValue, minInt, maxInt);
            slider.SetValueWithoutNotify(startValue);
            valueText.text = startValue.ToString();

            slider.onValueChanged.AddListener(rawSliderValue =>
            {
                int value = Mathf.RoundToInt(rawSliderValue);
                value = Mathf.Clamp(value, minInt, maxInt);
                accessor.SetValue(value);
                valueText.text = value.ToString();
            });
        }
        else
        {
            float min = binding.minValue;
            float max = binding.maxValue;

            object raw = accessor.GetValue();
            float startValue = raw is float floatValue ? floatValue : min;
            startValue = Mathf.Clamp(startValue, min, max);
            startValue = QuantizeFloat(startValue, min, binding.floatStep);

            slider.SetValueWithoutNotify(startValue);
            valueText.text = FormatFloat(startValue);

            slider.onValueChanged.AddListener(rawSliderValue =>
            {
                float value = Mathf.Clamp(rawSliderValue, min, max);
                value = QuantizeFloat(value, min, binding.floatStep);

                if (!Mathf.Approximately(value, rawSliderValue))
                {
                    slider.SetValueWithoutNotify(value);
                }

                accessor.SetValue(value);
                valueText.text = FormatFloat(value);
            });
        }

        runtimeControls.Add(new RuntimeControl
        {
            binding = binding,
            accessor = accessor,
            valueType = accessor.valueType,
            slider = slider,
            valueText = valueText
        });
    }

    private void CreateWarningItem(RectTransform parent, string title, string message)
    {
        Text warning = CreateText("Warning", parent, $"{title}\n{message}", 14, FontStyle.Normal, TextAnchor.MiddleLeft, warningTextColor);
        RectTransform warningRect = warning.rectTransform;
        warningRect.anchorMin = Vector2.zero;
        warningRect.anchorMax = Vector2.one;
        warningRect.offsetMin = new Vector2(10f, 8f);
        warningRect.offsetMax = new Vector2(-10f, -8f);
    }

    private void RefreshControlsFromTarget()
    {
        for (int i = 0; i < runtimeControls.Count; i++)
        {
            RuntimeControl control = runtimeControls[i];
            if (control == null || control.accessor.target == null)
            {
                continue;
            }

            object rawValue = control.accessor.GetValue();
            switch (control.valueType)
            {
                case SupportedValueType.Bool:
                {
                    if (control.toggle == null)
                    {
                        break;
                    }

                    bool boolValue = rawValue is bool value && value;
                    control.toggle.SetIsOnWithoutNotify(boolValue);
                    break;
                }
                case SupportedValueType.Int:
                {
                    if (control.slider == null || control.valueText == null)
                    {
                        break;
                    }

                    int intValue = rawValue is int value ? value : Mathf.RoundToInt(control.slider.minValue);
                    intValue = Mathf.Clamp(intValue, Mathf.RoundToInt(control.slider.minValue), Mathf.RoundToInt(control.slider.maxValue));
                    control.slider.SetValueWithoutNotify(intValue);
                    control.valueText.text = intValue.ToString();
                    break;
                }
                case SupportedValueType.Float:
                {
                    if (control.slider == null || control.valueText == null)
                    {
                        break;
                    }

                    float floatValue = rawValue is float value ? value : control.slider.minValue;
                    floatValue = Mathf.Clamp(floatValue, control.slider.minValue, control.slider.maxValue);
                    float step = control.binding != null ? control.binding.floatStep : 0f;
                    floatValue = QuantizeFloat(floatValue, control.slider.minValue, step);

                    control.slider.SetValueWithoutNotify(floatValue);
                    control.valueText.text = FormatFloat(floatValue);
                    break;
                }
            }
        }
    }

    private bool TryResolveAccessor(ControlBinding binding, out MemberAccessor accessor, out string errorMessage)
    {
        accessor = default;
        errorMessage = string.Empty;

        if (binding == null)
        {
            errorMessage = "控制项为空。";
            return false;
        }

        if (binding.targetScript == null)
        {
            errorMessage = "未绑定目标脚本。";
            return false;
        }

        if (string.IsNullOrWhiteSpace(binding.memberName))
        {
            errorMessage = "memberName 不能为空。";
            return false;
        }

        Type targetType = binding.targetScript.GetType();

        FieldInfo field = FindFieldRecursive(targetType, binding.memberName);
        if (field != null)
        {
            SupportedValueType valueType = GetSupportedType(field.FieldType);
            if (valueType == SupportedValueType.Unsupported)
            {
                errorMessage = $"字段类型不支持: {field.FieldType.Name}";
                return false;
            }

            accessor = new MemberAccessor
            {
                target = binding.targetScript,
                field = field,
                property = null,
                valueType = valueType,
                fullName = $"{targetType.Name}.{field.Name}"
            };
            return true;
        }

        PropertyInfo property = FindPropertyRecursive(targetType, binding.memberName);
        if (property != null)
        {
            if (!property.CanRead || !property.CanWrite || property.GetSetMethod(true) == null)
            {
                errorMessage = "属性必须可读写。";
                return false;
            }

            SupportedValueType valueType = GetSupportedType(property.PropertyType);
            if (valueType == SupportedValueType.Unsupported)
            {
                errorMessage = $"属性类型不支持: {property.PropertyType.Name}";
                return false;
            }

            accessor = new MemberAccessor
            {
                target = binding.targetScript,
                field = null,
                property = property,
                valueType = valueType,
                fullName = $"{targetType.Name}.{property.Name}"
            };
            return true;
        }

        errorMessage = $"未找到字段/属性: {binding.memberName}";
        return false;
    }

    private static FieldInfo FindFieldRecursive(Type type, string memberName)
    {
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        Type current = type;
        while (current != null)
        {
            FieldInfo field = current.GetField(memberName, FLAGS);
            if (field != null)
            {
                return field;
            }

            current = current.BaseType;
        }

        return null;
    }

    private static PropertyInfo FindPropertyRecursive(Type type, string memberName)
    {
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        Type current = type;
        while (current != null)
        {
            PropertyInfo property = current.GetProperty(memberName, FLAGS);
            if (property != null)
            {
                return property;
            }

            current = current.BaseType;
        }

        return null;
    }

    private static SupportedValueType GetSupportedType(Type type)
    {
        if (type == typeof(bool))
        {
            return SupportedValueType.Bool;
        }

        if (type == typeof(int))
        {
            return SupportedValueType.Int;
        }

        if (type == typeof(float))
        {
            return SupportedValueType.Float;
        }

        return SupportedValueType.Unsupported;
    }

    private RectTransform CreateRectTransform(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
    {
        RectTransform rect = CreateRectTransform(name, parent);
        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = uiFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private Toggle CreateToggle(RectTransform parent)
    {
        RectTransform toggleRect = CreateRectTransform("Toggle", parent);
        toggleRect.anchorMin = new Vector2(1f, 0.5f);
        toggleRect.anchorMax = new Vector2(1f, 0.5f);
        toggleRect.pivot = new Vector2(1f, 0.5f);
        toggleRect.sizeDelta = new Vector2(28f, 28f);
        toggleRect.anchoredPosition = new Vector2(-8f, 0f);

        Toggle toggle = toggleRect.gameObject.AddComponent<Toggle>();

        RectTransform bgRect = CreateRectTransform("Background", toggleRect);
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bgRect.gameObject.AddComponent<Image>();
        bgImage.color = sliderBackgroundColor;

        RectTransform checkRect = CreateRectTransform("Checkmark", bgRect);
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.offsetMin = Vector2.zero;
        checkRect.offsetMax = Vector2.zero;
        Image checkImage = checkRect.gameObject.AddComponent<Image>();
        checkImage.color = sliderFillColor;

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        return toggle;
    }

    private Slider CreateSlider(RectTransform parent, float minValue, float maxValue, bool wholeNumbers)
    {
        RectTransform sliderRect = CreateRectTransform("Slider", parent);
        sliderRect.anchorMin = new Vector2(0f, 0f);
        sliderRect.anchorMax = new Vector2(1f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0f);
        sliderRect.offsetMin = new Vector2(10f, 8f);
        sliderRect.offsetMax = new Vector2(-88f, 32f);

        Image bgImage = sliderRect.gameObject.AddComponent<Image>();
        bgImage.color = sliderBackgroundColor;

        RectTransform fillArea = CreateRectTransform("FillArea", sliderRect);
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = new Vector2(6f, 6f);
        fillArea.offsetMax = new Vector2(-6f, -6f);

        RectTransform fillRect = CreateRectTransform("Fill", fillArea);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillRect.gameObject.AddComponent<Image>();
        fillImage.color = sliderFillColor;

        RectTransform handleArea = CreateRectTransform("HandleArea", sliderRect);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = new Vector2(6f, 0f);
        handleArea.offsetMax = new Vector2(-6f, 0f);

        RectTransform handleRect = CreateRectTransform("Handle", handleArea);
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(16f, 28f);
        Image handleImage = handleRect.gameObject.AddComponent<Image>();
        handleImage.color = sliderHandleColor;

        Slider slider = sliderRect.gameObject.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = wholeNumbers;
        slider.direction = Slider.Direction.LeftToRight;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.value = minValue;
        return slider;
    }

    private void EnsureFont()
    {
        if (uiFont != null)
        {
            return;
        }

        uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void EnsureEventSystem()
    {
        if (!autoCreateEventSystem)
        {
            return;
        }

        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
        Type inputModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModuleType != null)
        {
            eventSystemObject.AddComponent(inputModuleType);
        }
        else
        {
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    private void DestroyExistingPanel()
    {
        Transform existing = transform.Find(panelObjectName);
        if (existing == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(existing.gameObject);
        }
        else
        {
            DestroyImmediate(existing.gameObject);
        }
    }

    private float QuantizeFloat(float value, float minValue, float step)
    {
        if (step <= 0f)
        {
            return value;
        }

        float offset = value - minValue;
        float rounded = Mathf.Round(offset / step) * step + minValue;
        return rounded;
    }

    private string FormatFloat(float value)
    {
        int digits = Mathf.Clamp(floatDisplayDecimals, 0, 6);
        return value.ToString($"F{digits}");
    }
}
