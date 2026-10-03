using System.Collections.Generic;
using UnityEngine;

// Measurements and trail come from the Rigidbody trajectory after a real release.
public sealed class ReportThrowTrack : MonoBehaviour
{
    [Min(0)] public float minimumThrowSpeed=.5f;
    [Min(0)] public float ignoreInitialContactSeconds=.12f;
    [Min(0)] public float trailLifetime=3;
    public Color trailColor=new Color(1,.73f,.28f);
    public bool InFlight {get;private set;}
    public bool HasThrown {get;private set;}
    public int ThrowSerial {get;private set;}
    public Vector3 ReleasePosition {get;private set;}
    public Vector3 ReleaseVelocity {get;private set;}
    public float ReleaseTime {get;private set;}
    public float LaunchAngle=>Mathf.Atan2(ReleaseVelocity.y,Vector3.ProjectOnPlane(ReleaseVelocity,Vector3.up).magnitude)*Mathf.Rad2Deg;
    public float HorizontalDistance {get;private set;}
    public float FlightTime {get;private set;}
    public string LastCollision {get;private set;}
    readonly List<Vector3> points=new List<Vector3>();
    LineRenderer line;
    Material material;
    float landedTime;
    public void RecordRelease(Vector3 velocity)
    {
        HasThrown=velocity.magnitude>=minimumThrowSpeed;InFlight=HasThrown;
        if(!HasThrown)return;
        ThrowSerial++;ReleasePosition=transform.position;ReleaseVelocity=velocity;ReleaseTime=Time.time;HorizontalDistance=FlightTime=0;LastCollision="";
        points.Clear();points.Add(ReleasePosition);
        if(line==null)
        {
            var root=new GameObject(name+" · 实测飞行轨迹");line=root.AddComponent<LineRenderer>();
            material=new Material(Shader.Find("Sprites/Default"));line.sharedMaterial=material;line.startWidth=line.endWidth=.014f;
            line.startColor=line.endColor=trailColor;line.useWorldSpace=true;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        line.enabled=true;line.positionCount=1;line.SetPosition(0,ReleasePosition);
    }
    void FixedUpdate()
    {
        if(!InFlight)return;
        FlightTime=Time.time-ReleaseTime;HorizontalDistance=Vector3.ProjectOnPlane(transform.position-ReleasePosition,Vector3.up).magnitude;
        if(points.Count<256 && Vector3.Distance(points[points.Count-1],transform.position)>.035f)
        {points.Add(transform.position);line.positionCount=points.Count;line.SetPositions(points.ToArray());}
        if(FlightTime>15){InFlight=false;landedTime=Time.time;}
    }
    void OnCollisionEnter(Collision collision)
    {
        if(!InFlight || Time.time-ReleaseTime<ignoreInitialContactSeconds)return;
        LastCollision=collision.collider.name;
        FlightTime=Time.time-ReleaseTime;HorizontalDistance=Vector3.ProjectOnPlane(transform.position-ReleasePosition,Vector3.up).magnitude;
        InFlight=false;landedTime=Time.time;
    }
    void Update(){if(line!=null && !InFlight && Time.time-landedTime>trailLifetime)line.enabled=false;}
    public void ResetMeasurements(){InFlight=HasThrown=false;HorizontalDistance=FlightTime=0;points.Clear();if(line!=null)line.enabled=false;}
    void OnDestroy(){if(line!=null)Destroy(line.gameObject);if(material!=null)Destroy(material);}
}
