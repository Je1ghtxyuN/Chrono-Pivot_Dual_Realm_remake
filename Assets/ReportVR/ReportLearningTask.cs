using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class ReportLearningRequirement
{
    public ReportLearningMeasure measure;
    public Vector2 acceptedRange=new Vector2(0,1);
    [Min(0)] public int freshOperations=1;
    public bool requireReleased=true;
    [NonSerialized] public int initialOperations;
}

public sealed class ReportLearningTask : MonoBehaviour
{
    public ReportLearningRequirement[] requirements=new ReportLearningRequirement[0];
    [Min(.05f)] public float stableDuration=.5f;
    public bool Complete {get;private set;}
    float stableTime;
    public void BeginGoal()
    {
        foreach(var condition in requirements)condition.initialOperations=condition.measure.Operations;
        stableTime=0;Complete=false;
    }
    public void Observe(bool placementsReady,float deltaTime)
    {
        bool ready=placementsReady;
        foreach(var condition in requirements)
        {
            var source=condition.measure;
            ready &= source.Reading>=condition.acceptedRange.x && source.Reading<=condition.acceptedRange.y;
            ready &= source.Operations-condition.initialOperations>=condition.freshOperations;
            ready &= !condition.requireReleased || !source.Held;
        }
        stableTime=ready?stableTime+deltaTime:0;Complete=stableTime>=stableDuration;
    }
    public string Summary=>string.Join("\n",requirements.Select(r=>r.measure.Summary));
    public void ResetProgress(){stableTime=0;Complete=false;}
}
