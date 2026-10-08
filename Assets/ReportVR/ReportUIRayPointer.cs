using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A straight controller ray operates the same native widgets used by the mouse.
public sealed class ReportUIRayPointer : MonoBehaviour
{
    public ReportGameUI ui;
    public Transform source;
    public InputActionReference trigger;
    public bool left;
    public EventSystem events;
    Selectable hover,pressed;
    bool held,blockedUntilRelease=true;
    LineRenderer line;
    Material material;
    void Start()
    {
        line=gameObject.AddComponent<LineRenderer>();line.positionCount=2;line.startWidth=.004f;line.endWidth=.0015f;line.useWorldSpace=true;
        material=new Material(Shader.Find("Sprites/Default"));line.sharedMaterial=material;line.enabled=false;
    }
    void Update()
    {
        bool down=trigger!=null && trigger.action.IsPressed();
        bool active=ui.enabled && ui.MenuOpen && !ui.player.IsDesktop && ui.player.HeadTracked && ui.player.HandTracked(left);
        if(!down)blockedUntilRelease=false;
        if(!active){SetHover(null);pressed=null;held=down;blockedUntilRelease=down;line.enabled=false;return;}
        var ray=new Ray(source.position,source.forward);var canvas=(RectTransform)ui.MenuCanvas.transform;
        var plane=new Plane(canvas.forward,canvas.position);Selectable candidate=null;Vector3 point=ray.GetPoint(3);
        if(plane.Raycast(ray,out float distance) && distance>0 && distance<5)
        {
            point=ray.GetPoint(distance);
            foreach(var widget in ui.MenuCanvas.GetComponentsInChildren<Selectable>())
            {
                var rect=(RectTransform)widget.transform;
                if(widget.IsInteractable() && widget.gameObject.activeInHierarchy && rect.rect.Contains(rect.InverseTransformPoint(point)))candidate=widget;
            }
        }
        SetHover(candidate);line.enabled=true;line.SetPosition(0,source.position);line.SetPosition(1,point);line.startColor=line.endColor=candidate!=null?ui.gold:ui.jade;
        if(down && !held && !blockedUntilRelease){pressed=hover;if(pressed!=null)ExecuteEvents.Execute(pressed.gameObject,new PointerEventData(events),ExecuteEvents.pointerDownHandler);}
        if(down && pressed is Slider slider)
        {
            var rect=(RectTransform)slider.transform;var local=rect.InverseTransformPoint(point);slider.normalizedValue=Mathf.InverseLerp(rect.rect.xMin,rect.rect.xMax,local.x);
        }
        if(!down && held && pressed!=null)
        {
            var target=pressed;pressed=null;ExecuteEvents.Execute(target.gameObject,new PointerEventData(events),ExecuteEvents.pointerUpHandler);
            if(target==hover && target is Button)ExecuteEvents.Execute(target.gameObject,new PointerEventData(events),ExecuteEvents.pointerClickHandler);
        }
        held=down;
    }
    void SetHover(Selectable value)
    {
        if(hover==value)return;
        if(hover!=null)ExecuteEvents.Execute(hover.gameObject,new PointerEventData(events),ExecuteEvents.pointerExitHandler);
        hover=value;if(hover!=null)ExecuteEvents.Execute(hover.gameObject,new PointerEventData(events),ExecuteEvents.pointerEnterHandler);
    }
    void OnDestroy(){if(material!=null)Destroy(material);}
}
