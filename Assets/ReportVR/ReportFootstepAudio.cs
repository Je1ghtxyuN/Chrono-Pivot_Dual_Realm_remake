using UnityEngine;

public sealed class ReportFootstepAudio : MonoBehaviour
{
    [Min(.2f)] public float stride=.7f;
    [Range(0,1)] public float volume=.4f;
    Vector3 previous;
    float distance;
    Transform view;
    DemoLevel level;
    void Start(){var player=GetComponent<DesktopPlayer>();view=player!=null && player.head!=null?player.head.transform:transform;previous=view.position;level=FindObjectOfType<DemoLevel>();}
    void Update()
    {
        Vector3 position=view.position;float moved=Vector3.ProjectOnPlane(position-previous,Vector3.up).magnitude;previous=position;
        if(Time.timeScale==0 || moved>1.5f){distance=0;return;}
        if(moved<.005f || !Physics.Raycast(position,Vector3.down,out var floor,2.5f,~0,QueryTriggerInteraction.Ignore))return;
        distance+=moved;if(distance<stride)return;distance%=stride;
        string name=floor.collider.name;
        bool snow=name.Contains("Snow") || name.Contains("雪") || level!=null && level.title.Contains("冰雪");
        var cue=snow?ReportSound.FootSnow:name.Contains("Bridge") || name.Contains("木")?ReportSound.FootWood:ReportSound.FootStone;
        ReportAudio.Play(cue,floor.point,volume);
    }
}
