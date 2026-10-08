using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Both the desktop pointer and the XR UI ray use the standard EventSystem event.
public sealed class ReportUIAudio : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        var button=GetComponent<Button>();
        if(button!=null && button.IsInteractable())ReportAudio.Play(ReportSound.UIHover,transform.position,1,true);
    }
}
