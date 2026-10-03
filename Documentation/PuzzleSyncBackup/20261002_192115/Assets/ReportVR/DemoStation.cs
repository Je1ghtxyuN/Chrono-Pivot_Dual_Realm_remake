using UnityEngine;

public enum DemoAction {Inspect, Buoyancy, Resonance, Compass, Mirror, Power, Valve, Repair, Coupling, Snowball, Domino, Sundial, RotateExhibit}

public sealed class DemoStation : MonoBehaviour
{
    [Header("任务定义")]
    public DemoLevel level;
    [Min(0)] public int index;
    public DemoAction action;
    public string instruction="观察器材";
    [Tooltip("建议观察位置，采用世界坐标，不控制玩家移动。")]
    public Vector3 viewingPoint;
    [Header("显示引用")]
    [Tooltip("文字朝向由文字物体的 ReportTextFacing 独立控制。")]
    public TextMesh label;
    public Renderer indicator;
    [Tooltip("完成任务的指示颜色。")]
    public Color completedColor=new Color(.4f,.7f,.48f);
    public Color currentColor=new Color(.96f,.71f,.3f);
    public Color pendingColor=new Color(.45f,.55f,.55f);
    [Header("动作部件与参数")]
    public Transform movingPart;
    [Tooltip("动作位移，采用部件父物体的局部坐标，单位为米。")]
    public Vector3 travel;
    public Transform secondaryPart;
    [Tooltip("当前动作的目标参数；观察任务不使用。单位随动作变化，例如 Hz 或角度。")]
    public float target=100;
    [Tooltip("每次交互依次采用的参数值；至少保留一个值。观察任务不使用。")]
    public float[] values=new float[]{60,80,100};
    public int Attempts {get;private set;}
    public string Result {get;private set;}="";
    Vector3 initialPosition,initialScale,secondaryPosition;
    Quaternion initialRotation;
    bool initialized,activated,done;
    float displayTime;
    public float CurrentValue=>values[Mathf.Min(Mathf.Max(Attempts-1,0),values.Length-1)];
    void OnValidate(){index=Mathf.Max(0,index);if(values==null || values.Length==0)values=new[]{target};}
    void Awake(){Initialize();}
    void Initialize()
    {
        if(initialized)return;initialized=true;
        if(movingPart!=null){initialPosition=movingPart.localPosition;initialRotation=movingPart.localRotation;initialScale=movingPart.localScale;}
        if(secondaryPart!=null)secondaryPosition=secondaryPart.localPosition;
    }
    public bool Activate()
    {
        Initialize();Attempts++;activated=true;displayTime=0;float value=CurrentValue;bool success=true;
        switch(action)
        {
            case DemoAction.Buoyancy:
                float submerged=DemoPhysics.SubmergedFraction(value,1000,.2f);
                if(movingPart!=null)movingPart.localPosition=initialPosition+Vector3.up*(.5f-submerged)*.35f;
                Result="总质量 "+value+" kg；浸没 "+(submerged*100).ToString("F0")+"%。";success=submerged<1 && submerged>=.3f;break;
            case DemoAction.Resonance:
            case DemoAction.Coupling:
                success=DemoPhysics.FrequencyMatches(value,target);Result="频率 "+value.ToString("F2")+" / "+target.ToString("F2")+" Hz；"+(success?"频率匹配。":"继续调节。" );break;
            case DemoAction.Compass:
                Result="司南指向南方；北方位于其反向。";break;
            case DemoAction.Mirror:
                success=Mathf.Abs(value-target)<2;Result="铜镜角度 "+value+"°；"+(success?"光路接通。":"继续调整，使光线指向目标。" );
                if(movingPart!=null)movingPart.localRotation=initialRotation*Quaternion.Euler(0,value-target,0);
                if(secondaryPart!=null)secondaryPart.gameObject.SetActive(success);break;
            case DemoAction.Power:
                float field=DemoPhysics.CoilField(value,400,.5f)*1000;
                success=field>=2;Result="电流 "+value+" A；理想螺线管磁场 "+field.ToString("F2")+" mT；"+(success?"铁芯吸合。":"继续提高电流。" );break;
            case DemoAction.Valve:
                Result="阀门开启，观察流向与水位。";
                if(movingPart!=null){movingPart.localPosition=initialPosition+travel;if(travel.sqrMagnitude<.01f)movingPart.localRotation=initialRotation*Quaternion.Euler(0,0,90);}break;
            case DemoAction.Repair:
                Result="更换密封圈，泄漏停止，压力恢复。";
                if(movingPart!=null)movingPart.gameObject.SetActive(false);
                if(secondaryPart!=null)secondaryPart.gameObject.SetActive(true);break;
            case DemoAction.Snowball:
                Result="抛出雪球，观察抛物线与目标反馈。";break;
            case DemoAction.Domino:
                Result="释放小球，演示多米诺传递。";break;
            case DemoAction.Sundial:
                if(movingPart!=null)movingPart.localRotation=initialRotation*Quaternion.Euler(0,value,0);
                Result="时角 "+value+"°；阴影方向随太阳位置变化。";break;
            case DemoAction.RotateExhibit:
                if(movingPart!=null)movingPart.localRotation=initialRotation*Quaternion.Euler(0,30,0);
                Result="展品旋转 30°，观察轮廓与结构。";break;
            default:Result="已观察："+instruction;break;
        }
        done=success;return success;
    }
    void Update()
    {
        if(!activated || movingPart==null)return;displayTime+=Time.deltaTime;
        if(action==DemoAction.Resonance || action==DemoAction.Coupling)
        {
            // Slow visual motion avoids aliasing of the audible 80–100Hz bell frequencies.
            float visualFrequency=action==DemoAction.Resonance && target>10?CurrentValue*.01f:CurrentValue;
            float angle=Mathf.Sin(displayTime*2*Mathf.PI*visualFrequency)*(done?12:3);
            movingPart.localRotation=initialRotation*Quaternion.Euler(0,0,angle);
        }
        else if(action==DemoAction.Snowball)
        {
            float t=Mathf.Clamp01(displayTime/1.4f);
            movingPart.localPosition=initialPosition+travel*t+Vector3.up*(4*t*(1-t)*2);
            if(t==1 && secondaryPart!=null)secondaryPart.localRotation=Quaternion.Euler(0,0,-12);
        }
        else if(action==DemoAction.Domino)
        {
            for(int i=0;i<movingPart.childCount;i++)
            {
                float t=Mathf.Clamp01((displayTime-i*.12f)/.2f);
                movingPart.GetChild(i).localRotation=Quaternion.Euler(0,0,-t*65);
            }
        }
        else if(action==DemoAction.Power)movingPart.localPosition=Vector3.MoveTowards(movingPart.localPosition,initialPosition+(done?travel:Vector3.zero),Time.deltaTime*.3f);
        else if(action==DemoAction.Compass)movingPart.localRotation=Quaternion.Slerp(movingPart.localRotation,initialRotation*Quaternion.Euler(0,180,0),Time.deltaTime*3);
    }
    public void ResetStation()
    {
        Initialize();Attempts=0;activated=false;done=false;displayTime=0;Result="";
        if(movingPart!=null)
        {
            movingPart.gameObject.SetActive(true);movingPart.localPosition=initialPosition;movingPart.localRotation=initialRotation;movingPart.localScale=initialScale;
            if(action==DemoAction.Domino)foreach(Transform child in movingPart)child.localRotation=Quaternion.identity;
        }
        if(secondaryPart!=null){secondaryPart.localPosition=secondaryPosition;secondaryPart.localRotation=Quaternion.identity;if(action==DemoAction.Mirror || action==DemoAction.Repair)secondaryPart.gameObject.SetActive(false);}
    }
    public void SetState(bool complete,bool current)
    {
        if(label!=null)label.text=(index+1)+"  "+instruction+"\n"+(complete?"完成 ✓":current?"握把键 / E 操作":"先完成前一步");
        if(indicator!=null)
        {
            var block=new MaterialPropertyBlock();block.SetColor("_Color",complete?completedColor:current?currentColor:pendingColor);indicator.SetPropertyBlock(block);
        }
    }
}
