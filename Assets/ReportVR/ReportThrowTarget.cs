using System.Collections.Generic;
using UnityEngine;

public sealed class ReportThrowTarget : MonoBehaviour
{
    [Tooltip("空数组允许任何可测量的投掷物；否则仅接受列出的样品。")]
    public ReportThrowTrack[] acceptedItems=new ReportThrowTrack[0];
    public Transform launchArea;
    [Min(.1f)] public float launchRadius=1.5f;
    [Min(0)] public float minimumFlightDistance=1.5f;
    [Min(0)] public float minimumFlightTime=.15f;
    public Vector2 acceptedSpeed=new Vector2(.5f,20);
    public Vector2 acceptedAngle=new Vector2(-90,90);
    public TextMesh readout;
    [Tooltip("指定时仅在该步骤进行中记录命中，避免提前投掷跳过任务。")]
    public DemoStation requiredStation;
    public int Hits {get;private set;}
    public int DifferentItems=>items.Count;
    readonly HashSet<ReportThrowTrack> items=new HashSet<ReportThrowTrack>();
    readonly Dictionary<ReportThrowTrack,int> serials=new Dictionary<ReportThrowTrack,int>();
    public bool HasHit(ReportThrowTrack item)=>items.Contains(item);
    void OnTriggerEnter(Collider other)
    {
        if(requiredStation!=null && requiredStation.level.Step!=requiredStation.index)return;
        var track=other.GetComponentInParent<ReportThrowTrack>();if(track==null || !track.HasThrown)return;
        var grab=track.GetComponent<ReportGrabbable>();if(grab!=null && grab.isSelected)return;
        if(acceptedItems.Length>0 && System.Array.IndexOf(acceptedItems,track)<0)return;
        if(serials.TryGetValue(track,out int serial) && serial==track.ThrowSerial)return;
        float distance=Vector3.ProjectOnPlane(track.transform.position-track.ReleasePosition,Vector3.up).magnitude;
        float speed=track.ReleaseVelocity.magnitude;
        if(distance<minimumFlightDistance || Time.time-track.ReleaseTime<minimumFlightTime)return;
        if(launchArea!=null && Vector3.ProjectOnPlane(track.ReleasePosition-launchArea.position,Vector3.up).magnitude>launchRadius)return;
        if(speed<acceptedSpeed.x || speed>acceptedSpeed.y || track.LaunchAngle<acceptedAngle.x || track.LaunchAngle>acceptedAngle.y)return;
        serials[track]=track.ThrowSerial;items.Add(track);Hits++;
        ReportAudio.Play(ReportSound.TargetHit,track.transform.position);
        if(readout!=null)readout.text="已落入目标区\n观察抛物线，再比较不同材料的落点";
    }
    // A wide landing zone can be entered before the minimum flight distance is reached.
    void OnTriggerStay(Collider other)=>OnTriggerEnter(other);
    public void ResetMeasurements(){Hits=0;items.Clear();serials.Clear();if(readout!=null)readout.text="等待投掷 · 近距离放入不计命中";}
}
