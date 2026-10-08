using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Count distinct samples only after an actual XRI selection, including desktop grabs.
public sealed class ReportPickupMeasure : ReportLearningMeasure
{
    public DemoStation station;
    public ReportGrabbable[] samples=new ReportGrabbable[0];
    readonly HashSet<ReportGrabbable> picked=new HashSet<ReportGrabbable>();
    public override float Reading=>picked.Count;
    public override int Operations=>picked.Count;
    public override bool Held=>samples.Any(s=>s!=null && s.isSelected);
    void OnEnable(){foreach(var sample in samples)if(sample!=null)sample.selectEntered.AddListener(RecordPickup);}
    void OnDisable(){foreach(var sample in samples)if(sample!=null)sample.selectEntered.RemoveListener(RecordPickup);}
    void RecordPickup(SelectEnterEventArgs args)
    {
        if(station.level.Step!=station.index)return;
        var sample=args.interactableObject as ReportGrabbable;
        if(sample!=null)picked.Add(sample);
    }
    public override void ResetMeasurements()=>picked.Clear();
}
