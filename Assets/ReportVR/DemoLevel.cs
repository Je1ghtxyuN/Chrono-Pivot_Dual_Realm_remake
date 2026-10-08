using UnityEngine;

public sealed class DemoLevel : MonoBehaviour
{
    [Header("关卡任务与告示板")]
    public string title;
    [Tooltip("任务顺序由数组顺序决定，不应只修改节点的 index。")]
    public DemoStation[] stations;
    [Tooltip("可拿取的物件；重置关卡时放回原位并取消抓取。")]
    public ReportGrabbable[] grabbables=new ReportGrabbable[0];
    public TextMesh board;
    [Min(1),Tooltip("任务反馈文字每行的字符数量。")]
    public int feedbackLineLength=26;
    [Header("出口门")]
    public Transform exitDoor;
    [Tooltip("完成关卡后门相对于原位置的位移，单位为米。")]
    public Vector3 exitDoorTravel=Vector3.right*2.5f;
    [Min(0),Tooltip("门的开启/关闭速度，单位为米/秒。")]
    public float exitDoorSpeed=1.2f;
    [Header("路线与出生位置")]
    [Tooltip("世界坐标路线检查点；不是自动寻路或移动轨迹。")]
    public Vector3[] route;
    public Transform player;
    public Vector3 spawn;
    public float spawnYaw;
    [TextArea] public string mission="";
    public int Step {get;private set;}
    public bool Completed=>stations!=null && Step>=stations.Length;
    public string Feedback {get;private set;}="";
    Vector3 closedDoor;
    public bool ExitReady=>Completed && (exitDoor==null || Vector3.Distance(exitDoor.localPosition,closedDoor)>=exitDoorTravel.magnitude-.15f);
    void OnValidate(){feedbackLineLength=Mathf.Max(1,feedbackLineLength);exitDoorSpeed=Mathf.Max(0,exitDoorSpeed);}
    void Awake(){if(exitDoor!=null)closedDoor=exitDoor.localPosition;Refresh();}
    public void Use(DemoStation station)
    {
        if(Completed){Feedback="演示完成，可前往出口。";Refresh();return;}
        if(station!=stations[Step]){Feedback="请先完成第 "+(Step+1)+" 步。";Refresh();return;}
        bool success=station.Activate();Feedback=station.Result;
        if(success){Step++;PlayProgress(station.transform.position);}
        Refresh();
    }
    public void ShowFeedback(string message){Feedback=message;Refresh();}
    ReportThrowTarget CurrentThrowTarget()
    {
        if(Completed)return null;
        var task=stations[Step].puzzle!=null?stations[Step].puzzle.learningTask:null;
        if(task!=null)foreach(var condition in task.requirements)
            if(condition.measure is ReportThrowMeasure measure)return measure.target;
        return null;
    }
    public bool CanRetrieveThrows
    {
        get
        {
            var target=CurrentThrowTarget();
            if(target!=null)foreach(var item in target.acceptedItems)
                if(item!=null && !target.HasHit(item))return true;
            return false;
        }
    }
    public void RetrieveThrows()
    {
        var target=CurrentThrowTarget();if(target==null)return;
        foreach(var item in target.acceptedItems)
            if(item!=null && !target.HasHit(item))item.GetComponent<ReportGrabbable>().ResetPose();
        ShowFeedback("未命中的投掷物已回到台上，可以继续尝试。");
    }
    public void ResolveStation(DemoStation station)
    {
        if(Completed || station!=stations[Step])return;
        Feedback=station.Result;Step++;PlayProgress(station.transform.position);Refresh();
    }
    void PlayProgress(Vector3 position)
    {
        ReportAudio.Play(Completed?ReportSound.LevelComplete:ReportSound.StepComplete,position,1,Completed);
        if(Completed && exitDoor!=null)ReportAudio.Play(ReportSound.DoorUnlock,exitDoor.position);
    }
    public void ResetLevel()
    {
        ReportAudio.SilenceWorld();ReportAudio.Play(ReportSound.Reset,transform.position,1,true);
        Step=0;Feedback="已重置，请从第 1 步开始。";
        foreach(var station in stations)station.ResetStation();
        foreach(var grab in grabbables)if(grab!=null)grab.ResetPose();
        foreach(var measurement in FindObjectsOfType<ReportLearningMeasure>())measurement.ResetMeasurements();
        foreach(var effect in FindObjectsOfType<ReportBoatParticles>())effect.ResetFeedback();
        foreach(var effect in FindObjectsOfType<ReportMeltParticles>())effect.ResetFeedback();
        foreach(var effect in FindObjectsOfType<ReportSnowParticles>())effect.ResetFeedback();
        if(exitDoor!=null)exitDoor.localPosition=closedDoor;
        if(player!=null)
        {
            var body=player.GetComponent<CharacterController>();if(body!=null)body.enabled=false;
            player.position=spawn;player.rotation=Quaternion.Euler(0,spawnYaw,0);if(body!=null)body.enabled=true;
            var desktop=player.GetComponent<DesktopPlayer>();if(desktop!=null)desktop.ResetView();
        }
        Refresh();
    }
    void Update()
    {
        if(exitDoor!=null)exitDoor.localPosition=Vector3.MoveTowards(exitDoor.localPosition,closedDoor+(Completed?exitDoorTravel:Vector3.zero),Time.deltaTime*exitDoorSpeed);
        if(board!=null && Completed)board.text=title+"\n"+(ExitReady?"沿步道前往出口":"出口正在打开");
    }
    public void Refresh()
    {
        if(board!=null){board.text=title+"\n"+(ExitReady?"沿步道前往出口":"出口正在打开");board.GetComponent<Renderer>().enabled=Completed;}
        if(stations==null)return;
        for(int i=0;i<stations.Length;i++)stations[i].SetState(i<Step,i==Step);
    }
    void OnGUI()
    {
        var ui=player!=null?player.GetComponent<ReportGameUI>():null;if(ui!=null && ui.enabled)return;
        GUILayout.BeginArea(new Rect(16,125,430,300),GUI.skin.box);
        GUILayout.Label(title);GUILayout.Label(Completed?"出口已解锁，请沿步道离开":(Step+1)+" / "+stations.Length+" "+stations[Step].instruction);
        if(!Completed && stations[Step].puzzle!=null)
        {
            var puzzle=stations[Step].puzzle;GUILayout.Label("目标："+puzzle.goal);GUILayout.Label("线索："+puzzle.clue);
        }
        GUILayout.Label(Feedback);
        if(GUILayout.Button("Reset / R"))ResetLevel();
        GUILayout.EndArea();
    }
}
