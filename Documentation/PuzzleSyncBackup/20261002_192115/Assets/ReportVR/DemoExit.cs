using UnityEngine;

public sealed class DemoExit : MonoBehaviour
{
    [Tooltip("必须完成其任务才能通过该出口。")]
    public DemoLevel level;
    [Tooltip("完成任务后是否允许出口触发自动切关。可在调试时关闭，保留手动浏览。")]
    public bool autoAdvance=true;
    bool changing;
    void OnTriggerEnter(Collider other)
    {
        if(changing || !autoAdvance || !level.Completed)return;
        var browser=other.GetComponentInParent<ReportSceneBrowser>();if(browser==null)return;
        changing=true;browser.ChangeScene(1);
    }
}
