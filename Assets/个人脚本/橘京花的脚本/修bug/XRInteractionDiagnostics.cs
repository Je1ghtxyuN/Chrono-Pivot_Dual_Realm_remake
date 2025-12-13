using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

[System.Serializable]
public class XRInteractionDiagnostics : MonoBehaviour
{
    [Header("诊断设置")]
    public bool autoDiagnoseOnStart = true;
    public bool logDetailedInfo = true;
    public GameObject targetUI; // 要诊断的UI对象

    [Header("诊断结果")]
    public bool hasEventSystem = false;
    public bool hasXRInteractionManager = false;
    public bool hasXRRayInteractors = false;
    public bool canvasHasGraphicRaycaster = false;
    public bool uiLayerCorrect = false;
    public bool xrInteractorsHaveUIInteraction = false;
    public bool canvasRenderModeCorrect = false;
    public bool canvasHasEventCamera = false;

    private StringBuilder diagnosticLog = new StringBuilder();

    void Start()
    {
        if (autoDiagnoseOnStart)
        {
            RunCompleteDiagnosis();
        }
    }

    [ContextMenu("运行完整诊断")]
    public void RunCompleteDiagnosis()
    {
        diagnosticLog.Clear();
        diagnosticLog.AppendLine("=== XR UI交互完整诊断 ===");
        diagnosticLog.AppendLine($"诊断时间: {System.DateTime.Now}");
        diagnosticLog.AppendLine($"场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        diagnosticLog.AppendLine();

        DiagnoseEventSystem();
        DiagnoseXRInteractionManager();
        DiagnoseXRRayInteractors();
        DiagnoseCanvasSettings();
        DiagnoseLayerSettings();
        DiagnoseInputSystem();
        DiagnosePhysicsSettings();
        DiagnoseOtherPotentialIssues();

        diagnosticLog.AppendLine("=== 诊断总结 ===");
        diagnosticLog.AppendLine($"事件系统: {(hasEventSystem ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"XR交互管理器: {(hasXRInteractionManager ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"XR射线交互器: {(hasXRRayInteractors ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"UI交互启用: {(xrInteractorsHaveUIInteraction ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"GraphicRaycaster: {(canvasHasGraphicRaycaster ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"UI图层正确: {(uiLayerCorrect ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"Canvas渲染模式: {(canvasRenderModeCorrect ? "✓" : "✗")}");
        diagnosticLog.AppendLine($"Canvas事件相机: {(canvasHasEventCamera ? "✓" : "✗")}");

        Debug.Log(diagnosticLog.ToString());
    }

    void DiagnoseEventSystem()
    {
        diagnosticLog.AppendLine("1. 事件系统诊断:");

        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        hasEventSystem = eventSystem != null;

        if (hasEventSystem)
        {
            diagnosticLog.AppendLine($"   ✓ 找到事件系统: {eventSystem.gameObject.name}");

            // 检查输入模块
            var inputModules = eventSystem.GetComponents<BaseInputModule>();
            foreach (var module in inputModules)
            {
                diagnosticLog.AppendLine($"   - 输入模块: {module.GetType().Name}");
            }

            // 检查XR UI输入模块（使用反射兼容不同版本）
            var xrModule = eventSystem.GetComponent(typeof(XRUIInputModule));
            if (xrModule != null)
            {
                diagnosticLog.AppendLine($"   - XR UI输入模块: 存在");
            }
        }
        else
        {
            diagnosticLog.AppendLine("   ✗ 未找到事件系统！");
        }
        diagnosticLog.AppendLine();
    }

    void DiagnoseXRInteractionManager()
    {
        diagnosticLog.AppendLine("2. XR交互管理器诊断:");

        XRInteractionManager interactionManager = FindObjectOfType<XRInteractionManager>();
        hasXRInteractionManager = interactionManager != null;

        if (hasXRInteractionManager)
        {
            diagnosticLog.AppendLine($"   ✓ 找到XR交互管理器: {interactionManager.gameObject.name}");

            // 使用反射获取注册的交互器（兼容不同版本）
            var interactorsField = typeof(XRInteractionManager).GetField("m_Interactors", BindingFlags.NonPublic | BindingFlags.Instance);
            if (interactorsField != null)
            {
                var interactors = interactorsField.GetValue(interactionManager) as List<IXRInteractor>;
                if (interactors != null)
                {
                    diagnosticLog.AppendLine($"   - 注册的交互器数量: {interactors.Count}");
                    foreach (var interactor in interactors)
                    {
                        if (interactor is MonoBehaviour behaviour)
                        {
                            diagnosticLog.AppendLine($"     > {behaviour.gameObject.name} ({interactor.GetType().Name})");
                        }
                    }
                }
            }
        }
        else
        {
            diagnosticLog.AppendLine("   ✗ 未找到XR交互管理器！");
        }
        diagnosticLog.AppendLine();
    }

    void DiagnoseXRRayInteractors()
    {
        diagnosticLog.AppendLine("3. XR射线交互器诊断:");

        XRRayInteractor[] rayInteractors = FindObjectsOfType<XRRayInteractor>();
        hasXRRayInteractors = rayInteractors.Length > 0;
        xrInteractorsHaveUIInteraction = false;

        if (hasXRRayInteractors)
        {
            diagnosticLog.AppendLine($"   ✓ 找到 {rayInteractors.Length} 个XR射线交互器");

            foreach (var interactor in rayInteractors)
            {
                diagnosticLog.AppendLine($"   - {interactor.gameObject.name}:");

                // 使用反射检查UI交互属性（兼容不同版本）
                var uiInteractionProperty = interactor.GetType().GetProperty("uiInteraction");
                if (uiInteractionProperty != null)
                {
                    bool uiInteraction = (bool)uiInteractionProperty.GetValue(interactor);
                    diagnosticLog.AppendLine($"     > UI交互: {uiInteraction}");
                    if (uiInteraction) xrInteractorsHaveUIInteraction = true;
                }
                else
                {
                    diagnosticLog.AppendLine($"     > UI交互: 属性不存在（版本兼容问题）");
                }

                // 检查交互层
                diagnosticLog.AppendLine($"     > 交互层: {GetInteractionLayersValue(interactor)}");

                // 检查射线遮罩
                diagnosticLog.AppendLine($"     > 射线遮罩: {interactor.raycastMask.value}");

                // 使用反射检查最大射线距离
                var maxDistanceField = interactor.GetType().GetField("m_MaxRaycastDistance", BindingFlags.NonPublic | BindingFlags.Instance);
                if (maxDistanceField != null)
                {
                    float maxDistance = (float)maxDistanceField.GetValue(interactor);
                    diagnosticLog.AppendLine($"     > 最大射线距离: {maxDistance}");
                }

                // 检查射线类型（使用反射）
                var raycastModeProperty = interactor.GetType().GetProperty("raycastMode");
                if (raycastModeProperty != null)
                {
                    object raycastMode = raycastModeProperty.GetValue(interactor);
                    diagnosticLog.AppendLine($"     > 射线类型: {raycastMode}");
                }

                // 检查附加的控制器
                XRBaseController controller = interactor.GetComponent<XRBaseController>();
                if (controller != null)
                {
                    diagnosticLog.AppendLine($"     > 控制器: {controller.gameObject.name}");
                }

                // 检查输入动作（ActionBasedController）
                var actionController = interactor.GetComponent<ActionBasedController>();
                if (actionController != null)
                {
                    diagnosticLog.AppendLine($"     > 位置动作: {GetInputActionName(actionController.positionAction)}");
                    diagnosticLog.AppendLine($"     > 旋转动作: {GetInputActionName(actionController.rotationAction)}");
                }
            }
        }
        else
        {
            diagnosticLog.AppendLine("   ✗ 未找到XR射线交互器！");
        }
        diagnosticLog.AppendLine();
    }

    // 辅助方法：获取交互层值
    private int GetInteractionLayersValue(XRRayInteractor interactor)
    {
        var layersProperty = interactor.GetType().GetProperty("interactionLayers");
        if (layersProperty != null)
        {
            object layersValue = layersProperty.GetValue(interactor);
            var valueProperty = layersValue.GetType().GetProperty("value");
            if (valueProperty != null)
            {
                return (int)valueProperty.GetValue(layersValue);
            }
        }
        return 0;
    }

    // 辅助方法：获取输入动作名称
    private string GetInputActionName(InputActionProperty property)
    {
        if (property.action != null)
        {
            return property.action.name;
        }
        return "无";
    }

    void DiagnoseCanvasSettings()
    {
        diagnosticLog.AppendLine("4. Canvas设置诊断:");

        if (targetUI == null)
        {
            diagnosticLog.AppendLine("   ⚠ 未指定目标UI，正在查找场景中的Canvas...");
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            diagnosticLog.AppendLine($"   - 找到 {allCanvases.Length} 个Canvas");

            foreach (var canvas in allCanvases)
            {
                AnalyzeCanvas(canvas.gameObject);
            }
        }
        else
        {
            AnalyzeCanvas(targetUI);
        }
        diagnosticLog.AppendLine();
    }

    void AnalyzeCanvas(GameObject canvasObject)
    {
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        if (canvas == null) return;

        diagnosticLog.AppendLine($"   - Canvas: {canvasObject.name}");
        diagnosticLog.AppendLine($"     > 渲染模式: {canvas.renderMode}");
        diagnosticLog.AppendLine($"     > 事件相机: {(canvas.worldCamera != null ? canvas.worldCamera.name : "无")}");
        diagnosticLog.AppendLine($"     > 排序层: {canvas.sortingLayerName}");
        diagnosticLog.AppendLine($"     > 排序顺序: {canvas.sortingOrder}");

        canvasRenderModeCorrect = canvas.renderMode == RenderMode.WorldSpace;
        canvasHasEventCamera = canvas.worldCamera != null;

        // 检查GraphicRaycaster
        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        canvasHasGraphicRaycaster = raycaster != null;

        if (canvasHasGraphicRaycaster)
        {
            diagnosticLog.AppendLine($"     > GraphicRaycaster: 存在");
            diagnosticLog.AppendLine($"       - 阻挡遮罩: {raycaster.blockingMask.value}");
            diagnosticLog.AppendLine($"       - 阻挡对象: {raycaster.blockingObjects}");
        }
        else
        {
            diagnosticLog.AppendLine($"     > GraphicRaycaster: 缺失！");
        }

        // 检查CanvasGroup
        CanvasGroup group = canvasObject.GetComponent<CanvasGroup>();
        if (group != null)
        {
            diagnosticLog.AppendLine($"     > CanvasGroup - 交互: {group.interactable}, 阻挡: {group.blocksRaycasts}");
        }
    }

    void DiagnoseLayerSettings()
    {
        diagnosticLog.AppendLine("5. 图层设置诊断:");

        int uiLayer = LayerMask.NameToLayer("UI");
        diagnosticLog.AppendLine($"   - UI图层索引: {uiLayer}");

        if (targetUI != null)
        {
            uiLayerCorrect = targetUI.layer == uiLayer;
            diagnosticLog.AppendLine($"   - 目标UI图层: {LayerMask.LayerToName(targetUI.layer)} ({targetUI.layer}) - {(uiLayerCorrect ? "✓" : "✗")}");
        }

        // 检查图层碰撞矩阵
        diagnosticLog.AppendLine("   - 图层碰撞矩阵检查:");
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                diagnosticLog.AppendLine($"     > {layerName} ({i}):");

                // 检查与UI层的碰撞
                if (uiLayer != -1)
                {
                    bool ignoresUI = Physics.GetIgnoreLayerCollision(i, uiLayer);
                    diagnosticLog.AppendLine($"       - 忽略UI层: {ignoresUI}");
                }
            }
        }
        diagnosticLog.AppendLine();
    }

    void DiagnoseInputSystem()
    {
        diagnosticLog.AppendLine("6. 输入系统诊断:");

        // 检查输入设备
        var devices = InputSystem.devices;
        diagnosticLog.AppendLine($"   - 输入设备数量: {devices.Count}");

        foreach (var device in devices)
        {
            diagnosticLog.AppendLine($"     > {device.name} ({device.layout}) - {(device.enabled ? "启用" : "禁用")}");

            // 检查XR控制器
            if (device.description.interfaceName.Contains("XR"))
            {
                diagnosticLog.AppendLine($"       - XR设备: {device.description.manufacturer} {device.description.product}");
            }
        }

        // 检查输入动作
        var inputActionAssets = FindObjectsOfType<InputActionAsset>();
        diagnosticLog.AppendLine($"   - 输入动作资源数量: {inputActionAssets.Length}");

        foreach (var asset in inputActionAssets)
        {
            diagnosticLog.AppendLine($"     > {asset.name}");
        }
        diagnosticLog.AppendLine();
    }

    void DiagnosePhysicsSettings()
    {
        diagnosticLog.AppendLine("7. 物理设置诊断:");

        diagnosticLog.AppendLine($"   - 重力: {Physics.gravity}");
        diagnosticLog.AppendLine($"   - 默认求解器迭代次数: {Physics.defaultSolverIterations}");
        diagnosticLog.AppendLine($"   - 射线命中触发器: {Physics.queriesHitTriggers}");
        diagnosticLog.AppendLine();
    }

    void DiagnoseOtherPotentialIssues()
    {
        diagnosticLog.AppendLine("8. 其他潜在问题诊断:");

        // 检查可能干扰的脚本
        var potentialConflicts = FindObjectsOfType<MonoBehaviour>();
        List<string> conflictScripts = new List<string>();

        foreach (var script in potentialConflicts)
        {
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Input") || scriptName.Contains("Ray") ||
                scriptName.Contains("UI") || scriptName.Contains("XR"))
            {
                if (!conflictScripts.Contains(scriptName))
                {
                    conflictScripts.Add(scriptName);
                }
            }
        }

        diagnosticLog.AppendLine($"   - 可能冲突的脚本类型: {conflictScripts.Count}");
        foreach (var script in conflictScripts)
        {
            diagnosticLog.AppendLine($"     > {script}");
        }

        // 检查时间缩放（可能影响输入）
        diagnosticLog.AppendLine($"   - 时间缩放: {Time.timeScale}");

        // 检查音频暂停状态
        diagnosticLog.AppendLine($"   - 音频暂停: {AudioListener.pause}");
        diagnosticLog.AppendLine();
    }

    [ContextMenu("尝试自动修复")]
    public void TryAutoFix()
    {
        diagnosticLog.Clear();
        diagnosticLog.AppendLine("=== 尝试自动修复 ===");

        // 1. 确保事件系统
        if (!hasEventSystem)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();

            // 尝试添加XR UI输入模块
            var xrModuleType = typeof(XRUIInputModule);
            if (xrModuleType != null)
            {
                eventSystem.AddComponent(xrModuleType);
            }
            diagnosticLog.AppendLine("✓ 创建了新的事件系统");
        }

        // 2. 确保UI交互启用
        XRRayInteractor[] interactors = FindObjectsOfType<XRRayInteractor>();
        foreach (var interactor in interactors)
        {
            // 使用反射设置UI交互属性
            var uiInteractionProperty = interactor.GetType().GetProperty("uiInteraction");
            if (uiInteractionProperty != null)
            {
                bool currentValue = (bool)uiInteractionProperty.GetValue(interactor);
                if (!currentValue)
                {
                    uiInteractionProperty.SetValue(interactor, true);
                    diagnosticLog.AppendLine($"✓ 启用了 {interactor.gameObject.name} 的UI交互");
                }
            }
        }

        // 3. 确保GraphicRaycaster
        if (targetUI != null && !canvasHasGraphicRaycaster)
        {
            if (targetUI.GetComponent<GraphicRaycaster>() == null)
            {
                targetUI.AddComponent<GraphicRaycaster>();
                diagnosticLog.AppendLine("✓ 添加了GraphicRaycaster到UI");
            }
        }

        // 4. 确保UI图层正确
        int uiLayer = LayerMask.NameToLayer("UI");
        if (targetUI != null && targetUI.layer != uiLayer && uiLayer != -1)
        {
            targetUI.layer = uiLayer;
            diagnosticLog.AppendLine("✓ 修正了UI图层");
        }

        Debug.Log(diagnosticLog.ToString());
        RunCompleteDiagnosis(); // 重新诊断
    }

    // 在Inspector中显示诊断结果
    void OnValidate()
    {
        if (targetUI == null)
        {
            // 尝试自动查找UI
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                targetUI = canvas.gameObject;
            }
        }
    }
}