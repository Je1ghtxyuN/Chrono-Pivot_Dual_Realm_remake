using UnityEngine;

public sealed class ReportTaskFeedback : MonoBehaviour
{
    public DemoStation station;
    public Renderer lamp;
    public Transform progress;
    MaterialPropertyBlock properties;
    void Update()
    {
        int step=station.level.Step;bool complete=step>station.index;bool active=step==station.index;
        var task=station.puzzle.learningTask;
        if(properties==null)properties=new MaterialPropertyBlock();
        properties.SetColor("_Color",complete || active && task.Ready?new Color(.3f,1,.62f):active?new Color(1,.68f,.2f):new Color(.24f,.35f,.4f));lamp.SetPropertyBlock(properties);
        progress.localScale=new Vector3(complete?1:active?Mathf.Max(.001f,task.StabilityProgress):.001f,1,1);
    }
}
