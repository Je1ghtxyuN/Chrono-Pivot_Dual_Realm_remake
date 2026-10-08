using System;
using UnityEngine;

public enum ReportPuzzleMode {Choice, Placement, Physical}
[Serializable]
public sealed class PuzzlePlacement
{
    public string label;
    public ReportGrabbable item;
    public Transform socket;
    [Min(.05f)] public float tolerance=.4f;
    public bool IsReady=>item!=null && socket!=null && !item.isSelected && Vector3.Distance(item.transform.position,socket.position)<=tolerance;
}

public sealed class ReportPuzzle : MonoBehaviour
{
    public DemoStation station;
    public ReportPuzzleMode mode;
    [TextArea(2,4)] public string goal;
    [TextArea(2,6)] public string clue;
    [TextArea(2,4)] public string hint;
    public string successMessage;
    public string wrongMessage="尚未匹配，请对照线索重新选择。";
    public string[] choices=new string[0];
    [Min(0)] public int correctChoice;
    [Tooltip("物件必须释放并放入对应位置；选择题也可以要求保留前一步的装载。")]
    public PuzzlePlacement[] placements=new PuzzlePlacement[0];
    [Tooltip("这些物件不能出现在任何检验槽附近，例如额外的铜配重。")]
    public ReportGrabbable[] excludedItems=new ReportGrabbable[0];
    public ReportPuzzleButton[] buttons=new ReportPuzzleButton[0];
    public ReportLearningTask learningTask;
    bool observing;
    public int SelectedChoice {get;private set;}=-1;
    public bool Evaluate(out string message)
    {
        int ready=0;foreach(var p in placements)if(p.IsReady)ready++;
        if(ready!=placements.Length)
        {
            message=mode==ReportPuzzleMode.Physical?hint:"已归位 "+ready+" / "+placements.Length+"；请按槽位标记摆放并松手，再按验证。";return false;
        }
        foreach(var item in excludedItems)foreach(var placement in placements)
            if(item!=null && Vector3.Distance(item.transform.position,placement.socket.position)<.7f)
            {message="槽位旁还有多余的物件，请移走后重新验证。";return false;}
        if(mode==ReportPuzzleMode.Choice)
        {
            if(SelectedChoice<0){message="请先点选 A / B / C，再按验证。";return false;}
            if(SelectedChoice!=correctChoice){message=wrongMessage;return false;}
        }
        if(mode==ReportPuzzleMode.Physical && (learningTask==null || !learningTask.Complete))
        {message=hint;return false;}
        message=successMessage;return true;
    }
    void Update()
    {
        if(mode!=ReportPuzzleMode.Physical || learningTask==null)return;
        if(station.level.Step!=station.index){observing=false;return;}
        if(!observing){learningTask.BeginGoal();observing=true;}
        bool placed=true;foreach(var placement in placements)placed &= placement.IsReady;
        foreach(var item in excludedItems)foreach(var placement in placements)
            if(item!=null && Vector3.Distance(item.transform.position,placement.socket.position)<.7f)placed=false;
        learningTask.Observe(placed,Time.deltaTime);
        if(learningTask.Complete)station.level.Use(station);
    }
    public void SelectChoice(int index)
    {
        if(index<0 || index>=choices.Length)return;
        if(station.level.Step!=station.index){station.level.Use(station);return;}
        SelectedChoice=index;RefreshButtons();station.level.ShowFeedback("已选择 "+(char)('A'+index)+"："+choices[index]+"。请按验证。");
    }
    public void RequestHint()=>station.level.ShowFeedback("提示："+hint);
    public void ResetPuzzle(){SelectedChoice=-1;observing=false;if(learningTask!=null)learningTask.ResetProgress();RefreshButtons();}
    void RefreshButtons(){foreach(var button in buttons)if(button!=null)button.Refresh();}
}

