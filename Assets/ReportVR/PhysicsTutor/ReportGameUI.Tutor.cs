using UnityEngine;
using UnityEngine.UI;

public sealed partial class ReportGameUI
{
    [Header("AI 物理导师")]
    [Tooltip("共用配置。模型路径记录本机权重；HTTP 服务需另行启动。")]
    public PhysicsTutorConnection tutorConnection;
    static string sessionTutorEndpoint;
    ReportPhysicsTutor tutor;
    InputField tutorQuestion;
    Text tutorStatus,tutorOutput,tutorVoiceLabel,tutorHintLabel;
    Button tutorSend,tutorVoice,tutorNew,tutorHint;
    ScrollRect tutorScroll;
    string displayedOutput;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetTutorEndpoint(){sessionTutorEndpoint=null;}
    void InitializeTutor()
    {
        tutor=GetComponent<ReportPhysicsTutor>();
        if(tutor==null)tutor=gameObject.AddComponent<ReportPhysicsTutor>();
        if(tutorConnection==null)tutorConnection=Resources.Load<PhysicsTutorConnection>("PhysicsTutorConnection");
        tutor.level=level;tutor.Client.endpoint=sessionTutorEndpoint??(tutorConnection!=null?tutorConnection.serviceEndpoint:"");
    }
    void BuildTutorPage()
    {
        Label(content,"当前实验："+Short(level.title+" · "+CurrentInstruction(),43),0,0,695,27,16,jade);
        tutorStatus=Label(content,tutor.Status,0,31,695,40,15,ink);
        tutorQuestion=TutorInput("UI_TutorQuestion",tutor.Question,"输入你的物理疑问（Unity Play 中可直接打字）",0,75,695,62,true);
        tutorQuestion.onValueChanged.AddListener(value=>tutor.Question=value);
        tutorVoice=Button(content,"UI_TutorVoice","语音输入",0,146,150,39,()=>{tutor.ToggleListening();RefreshTutor();},false);
        tutorVoiceLabel=tutorVoice.GetComponentInChildren<Text>();
        tutorSend=Button(content,"UI_TutorSend","发送提问",160,146,130,39,()=>{tutor.Send();RefreshTutor();},true);
        tutorHint=Button(content,"UI_TutorHint","",300,146,165,39,()=>{tutor.HintLevel=tutor.HintLevel%3+1;RefreshTutor();},false);
        tutorHintLabel=tutorHint.GetComponentInChildren<Text>();
        tutorNew=Button(content,"UI_TutorNew","新对话",475,146,100,39,()=>{tutor.NewConversation();tutorQuestion.SetTextWithoutNotify(tutor.Question);RefreshTutor();},false);
        Button(content,"UI_TutorUp","↑",585,146,50,39,()=>tutorScroll.verticalNormalizedPosition=Mathf.Clamp01(tutorScroll.verticalNormalizedPosition+.25f),false);
        Button(content,"UI_TutorDown","↓",645,146,50,39,()=>tutorScroll.verticalNormalizedPosition=Mathf.Clamp01(tutorScroll.verticalNormalizedPosition-.25f),false);
        var viewport=Rect("Tutor Reply",content,0,197,695,137);Image(viewport,new Color(jade.r,jade.g,jade.b,.06f)).raycastTarget=true;
        viewport.gameObject.AddComponent<RectMask2D>();
        tutorScroll=viewport.gameObject.AddComponent<ScrollRect>();tutorScroll.viewport=viewport;tutorScroll.horizontal=false;tutorScroll.movementType=ScrollRect.MovementType.Clamped;tutorScroll.scrollSensitivity=30;
        tutorOutput=Label(viewport,tutor.Output,12,8,671,121,17,ink);tutorOutput.verticalOverflow=VerticalWrapMode.Overflow;tutorOutput.supportRichText=false;
        tutorScroll.content=(RectTransform)tutorOutput.transform;displayedOutput=null;
        Label(content,"服务",0,350,53,25,14,gold);
        var endpoint=TutorInput("UI_TutorEndpoint",tutor.Client.endpoint,"例如 http://127.0.0.1:18080/api/tutor",55,344,640,35,false);
        endpoint.onValueChanged.AddListener(value=>{if(!tutor.Busy){tutor.Client.endpoint=value;sessionTutorEndpoint=value;}});
        RefreshTutor();
    }
    void RefreshTutor()
    {
        if(!MenuOpen || Page!=ReportUIPage.Tutor || tutorStatus==null)return;
        tutorStatus.text=tutor.Status;
        // Final recognition results appear in the same editable debug input.
        if(tutorQuestion.text!=tutor.Question)tutorQuestion.SetTextWithoutNotify(tutor.Question);
        tutorQuestion.interactable=!tutor.Busy && !tutor.Listening;
        tutorSend.interactable=tutorNew.interactable=tutorHint.interactable=!tutor.Busy && !tutor.Listening;
        tutorVoice.interactable=!tutor.Busy;tutorVoiceLabel.text=tutor.Listening?"停止听写":"语音输入";
        tutorHintLabel.text=new[]{"1 · 概念方向","2 · 关系与步骤","3 · 详细指导"}[tutor.HintLevel-1];
        var endpoint=content.Find("UI_TutorEndpoint").GetComponent<InputField>();endpoint.interactable=!tutor.Busy && !tutor.Listening;
        if(displayedOutput==tutor.Output)return;
        displayedOutput=tutor.Output;tutorOutput.text=displayedOutput;
        var rect=(RectTransform)tutorOutput.transform;
        rect.sizeDelta=new Vector2(671,Mathf.Max(121,tutorOutput.preferredHeight+12));
        tutorScroll.verticalNormalizedPosition=1;
    }
    InputField TutorInput(string name,string value,string placeholder,float x,float y,float width,float height,bool multiline)
    {
        var rect=Rect(name,content,x,y,width,height);var background=Image(rect,Color.white);background.raycastTarget=true;
        var field=rect.gameObject.AddComponent<InputField>();field.targetGraphic=background;
        var text=Label(rect,"",10,7,width-20,height-14,multiline?18:14,ink);text.supportRichText=false;
        var hint=Label(rect,placeholder,10,7,width-20,height-14,multiline?17:14,new Color(jade.r,jade.g,jade.b,.7f));
        field.textComponent=text;field.placeholder=hint;field.lineType=multiline?InputField.LineType.MultiLineNewline:InputField.LineType.SingleLine;
        field.characterLimit=multiline?4000:512;field.text=value;return field;
    }
}
