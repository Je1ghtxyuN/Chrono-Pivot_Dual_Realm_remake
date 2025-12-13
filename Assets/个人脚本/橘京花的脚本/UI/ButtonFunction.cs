using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunction : MonoBehaviour
{
    [Header("场景设置")]
    [SerializeField] private string sceneToSwitch;

    [Header("UI控制")]
    [SerializeField] private GameObject menuUI; // 拖入菜单UI的GameObject

    public void ClickDebug()
    {
        Debug.Log("Click!");
    }

    public void SwitchScene()
    {
        // 切换场景前确保时间流速恢复正常
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneToSwitch);

        // ⭐ 修改：已移除分数重置代码，因为现在是基于回合制的，
        // 重新加载场景会自动重置 GameRoundManager 的状态。
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    // 关闭菜单（继续游戏）
    public void CloseMenu()
    {
        if (menuUI != null)
        {
            menuUI.SetActive(false);

            // 恢复游戏时间（如果之前暂停了）
            Time.timeScale = 1f;

            Debug.Log("菜单已关闭，继续游戏");
        }
        else
        {
            Debug.LogWarning("未绑定菜单UI对象！");
        }
    }

    public void reLoadScene()
    {
        // 确保重开游戏时时间是流动的（防止在升级界面暂停时直接重开导致卡住）
        Time.timeScale = 1f;

        SceneManager.LoadScene(sceneToSwitch);

        // ⭐ 修改：已移除分数重置代码
    }
}