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
    public int Step {get;private set;}
    public bool Completed=>stations!=null && Step>=stations.Length;
    public string Feedback {get;private set;}="";
    Vector3 closedDoor;
    void OnValidate(){feedbackLineLength=Mathf.Max(1,feedbackLineLength);exitDoorSpeed=Mathf.Max(0,exitDoorSpeed);}
    void Awake(){if(exitDoor!=null)closedDoor=exitDoor.localPosition;Refresh();}
    public void Use(DemoStation station)
    {
        if(Completed){Feedback="演示完成，可前往出口。";Refresh();return;}
        if(station!=stations[Step]){Feedback="请先完成第 "+(Step+1)+" 步。";Refresh();return;}
        bool success=station.Activate();Feedback=station.Result;
        if(success)Step++;
        Refresh();
    }
    public void ResetLevel()
    {
        Step=0;Feedback="已重置，请从第 1 步开始。";
        foreach(var station in stations)station.ResetStation();
        foreach(var grab in grabbables)if(grab!=null)grab.ResetPose();
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
    }
    public void Refresh()
    {
        if(board!=null)board.text=title+"\n\n"+(Completed?"全部完成 → 出口":(Step+1)+" / "+stations.Length+"  "+stations[Step].instruction)+"\n"+Wrap(Feedback,Mathf.Max(1,feedbackLineLength))+"\n握把键 / 左键 / E：交互    R：重置";
        if(stations==null)return;
        for(int i=0;i<stations.Length;i++)stations[i].SetState(i<Step,i==Step);
    }
    static string Wrap(string text,int count)
    {
        for(int i=text.Length/count;i>0;i--)if(i*count<text.Length)text=text.Insert(i*count,"\n");return text;
    }
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(16,125,430,170),GUI.skin.box);
        GUILayout.Label(title);GUILayout.Label(Completed?"Completed — proceed to exit":(Step+1)+" / "+stations.Length+" "+stations[Step].instruction);
        GUILayout.Label(Feedback);
        if(GUILayout.Button("Reset / R"))ResetLevel();
        GUILayout.EndArea();
    }
}
