using UnityEngine;

public abstract class ReportLearningMeasure : MonoBehaviour
{
    public string label;
    public string unit;
    public abstract float Reading {get;}
    public abstract int Operations {get;}
    public virtual bool Held=>false;
    public virtual void ResetMeasurements(){}
    public string Summary=>label+" "+Reading.ToString("F2")+" "+unit;
}

