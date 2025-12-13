using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class XRUIToggle : MonoBehaviour
{
    [Header("必填参数")]
    [SerializeField] private InputActionReference menuAction; // 绑定菜单键
    [SerializeField] private GameObject uiCanvas; // UI预制体引用

    [Header("显示设置")]
    [SerializeField] private float displayDistance = 2f; // UI显示距离
    [SerializeField] private float heightOffset = -0.3f; // 高度偏移

    [Header("游戏状态")]
    [SerializeField] private bool pauseGameWhenOpen = true; // 是否在打开菜单时暂停游戏

    private Transform playerCamera;
    private bool isUIVisible;
    private float previousTimeScale; // 存储原来的时间流速

    void Start()
    {
        playerCamera = Camera.main.transform;
        menuAction.action.Enable();
        menuAction.action.performed += ToggleUI;
        menuAction.action.AddBinding("<XRController>{LeftHand}/menuButton");

        // 初始隐藏UI
        uiCanvas.SetActive(false);
        previousTimeScale = Time.timeScale; // 记录初始时间流速
    }

    private void ToggleUI(InputAction.CallbackContext ctx)
    {
        Debug.Log("打开菜单");
        isUIVisible = !isUIVisible;
        uiCanvas.SetActive(isUIVisible);

        if (isUIVisible)
        {
            // 计算UI位置：玩家前方 + 高度偏移
            Vector3 newPos = playerCamera.position +
                            playerCamera.forward * displayDistance +
                            Vector3.up * heightOffset;

            uiCanvas.transform.position = newPos;

            // 让UI始终面向玩家
            uiCanvas.transform.LookAt(playerCamera);
            uiCanvas.transform.Rotate(0, 180f, 0); // 翻转保证文字正向

            // 暂停游戏逻辑
            if (pauseGameWhenOpen)
            {
                previousTimeScale = Time.timeScale; // 备份当前时间流速
                Time.timeScale = 0f; // 完全暂停
                //AudioListener.pause = true; // 暂停音频
            }
        }
        else
        {
            // 恢复游戏逻辑
            if (pauseGameWhenOpen)
            {
                Time.timeScale = previousTimeScale; // 恢复原时间流速
                //AudioListener.pause = false; // 恢复音频
            }
        }
    }

    void OnDestroy()
    {
        // 确保游戏状态被正确恢复
        if (isUIVisible && pauseGameWhenOpen)
        {
            Time.timeScale = previousTimeScale;
            //AudioListener.pause = false;
        }

        menuAction.action.performed -= ToggleUI;
    }


}