using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

// Trigger uses a pointed station; grip remains reserved for physical manipulation.
public sealed class DemoInteractor : MonoBehaviour
{
    [Header("玩家与关卡引用")]
    [Tooltip("VR 射线的右手目标；键鼠模式使用 head。")]
    public Transform hand;
    public Camera head;
    public DemoLevel level;
    [Header("VR 输入")]
    public InputActionReference interact;
    public InputActionReference resetDemo;
    [Header("文字与射线")]
    [Tooltip("世界文字材质，需保留深度测试以避免文字穿透模型。")]
    public Material worldText;
    public LineRenderer ray;
    [Min(.1f),Tooltip("交互射线最大距离，单位为米。")]
    public float range=4;
    [Tooltip("有效交互目标的射线颜色。")]
    public Color targetRayColor=new Color(1,.73f,.28f);
    public Color idleRayColor=new Color(.3f,.75f,.7f,.45f);
    bool gripHeld,resetHeld;
    Font font;
    Material textMaterial;
    DesktopPlayer desktop;
    ReportGrabInteractor[] grabbers;
    void Start()
    {
        desktop=GetComponentInParent<DesktopPlayer>();
        grabbers=desktop.GetComponentsInChildren<ReportGrabInteractor>();
        font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},48);
        textMaterial=new Material(worldText);textMaterial.mainTexture=font.material.mainTexture;
        Font.textureRebuilt+=UpdateFontAtlas;
        foreach(var text in FindObjectsOfType<TextMesh>(true)){text.font=font;text.GetComponent<Renderer>().sharedMaterial=textMaterial;}
        level.Refresh();
    }
    void UpdateFontAtlas(Font rebuilt){if(rebuilt==font)textMaterial.mainTexture=font.material.mainTexture;}
    void Update()
    {
        if(desktop!=null && desktop.UIBlocked){if(ray!=null)ray.enabled=false;gripHeld=true;resetHeld=true;return;}
        bool desk=desktop!=null && desktop.IsDesktop;
        bool tracked=!desk && desktop.HeadTracked && desktop.HandTracked(false);
        Transform source=tracked?hand:head.transform;
        bool pressed=tracked && interact!=null && interact.action.IsPressed(),reset=!desk && desktop.HeadTracked && desktop.HandTracked(true) && resetDemo!=null && resetDemo.action.IsPressed();
        var keyboard=Keyboard.current;
        bool use=pressed&&!gripHeld || (!desk || desktop.CanInteract) && (keyboard!=null && keyboard.eKey.wasPressedThisFrame || desk && Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame);
        if(reset&&!resetHeld || keyboard!=null && keyboard.rKey.wasPressedThisFrame)level.ResetLevel();
        gripHeld=pressed;resetHeld=reset;
        var hits=Physics.RaycastAll(source.position,source.forward,range,~0,QueryTriggerInteraction.Collide);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));DemoStation selected=null;ReportGrabbable portable=null;ReportPuzzleButton puzzleButton=null;Vector3 end=source.position+source.forward*range;
        foreach(var hit in hits)
        {
            if(hit.collider.GetComponentInParent<DemoInteractor>()!=null)continue;
            if(hit.collider.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>()!=null)continue;
            end=hit.point;selected=hit.collider.GetComponentInParent<DemoStation>();portable=hit.collider.GetComponentInParent<ReportGrabbable>();puzzleButton=hit.collider.GetComponentInParent<ReportPuzzleButton>();break;
        }
        if(ray!=null){ray.SetPosition(0,source.position);ray.SetPosition(1,end);ray.startColor=ray.endColor=selected!=null || portable!=null?targetRayColor:idleRayColor;ray.enabled=tracked || selected!=null || portable!=null;}
        bool grabbing=grabbers.Any(g=>g.isSelectActive && (g.hasSelection || g.Candidate!=null));
        if(use && !grabbing){if(puzzleButton!=null)puzzleButton.Press();else if(selected!=null)level.Use(selected);}
    }
    void OnDestroy(){Font.textureRebuilt-=UpdateFontAtlas;if(textMaterial!=null)Destroy(textMaterial);if(font!=null)Destroy(font);}
}
