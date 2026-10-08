using System;
using System.Collections.Generic;
using System.Linq;
using PhysicsTutorPrototype;
using UnityEngine;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

[RequireComponent(typeof(PhysicsTutorClient))]
public sealed class ReportPhysicsTutor : MonoBehaviour
{
    public DemoLevel level;
    public string Question {get;set;}="";
    public int HintLevel {get;set;}=1;
    public string Output {get;private set;}="可使用语音或文字输入提问，导师回答将在发送后显示。";
    public string Status {get;private set;}="输入问题，或点击语音输入；发送前请确认导师服务已启动。";
    public bool Busy=>client!=null && client.Busy;
    public bool Listening {get;private set;}
    public PhysicsTutorClient Client=>client;
    readonly List<TutorMessage> history=new List<TutorMessage>();
    PhysicsTutorClient client;
    string speechStart="",speechFinal="";
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    DictationRecognizer recognizer;
#endif
    void Awake(){client=GetComponent<PhysicsTutorClient>();}
    public void Send()
    {
        if(Busy)return;
        StopListening();
        if(string.IsNullOrWhiteSpace(Question)){Status="请先输入问题。";return;}
        if(Question.Trim().Length>4000){Status="问题最多 4000 字，请缩短后发送。";return;}
        if(string.IsNullOrWhiteSpace(client.endpoint))
        {
            Status="尚未配置模型服务，问题已保留。";
            Output="服务配置完成后，可将问题与当前实验状态发送给导师。";return;
        }
        var request=new TutorRequest{question=Question.Trim(),hint_level=Mathf.Clamp(HintLevel,1,3),student_state=CaptureState(),history=history.ToArray()};
        Status="导师正在思考…";
        StartCoroutine(client.Ask(request,response=>
        {
            Output=response.answer.reply;
            if(response.evidence!=null)foreach(var card in response.evidence)
                Output+="\n\n参考："+card.title+" · "+card.source_section+"\n"+card.source_url;
            Status="回答已收到 · 可继续提问";
            history.Add(new TutorMessage{role="user",content=request.question});
            history.Add(new TutorMessage{role="assistant",content=Limit(response.answer.reply,4000)});
            while(history.Count>20 || history.Sum(m=>m.content.Length)>10000)history.RemoveRange(0,2);
        },error=>{Status="调用失败："+error;Output="本次没有收到有效回答。\n"+error;}));
    }
    StudentState CaptureState()
    {
        var state=new StudentState();
        if(level==null)return state;
        var station=!level.Completed && level.stations.Length>0?level.stations[level.Step]:null;
        var puzzle=station!=null?station.puzzle:null;
        state.task=level.title+" · "+(level.Completed?"实验已完成":station!=null?station.instruction:level.mission);
        if(puzzle!=null)state.task+="\n目标："+puzzle.goal;
        state.observation="已完成 "+level.Step+" / "+level.stations.Length+"；反馈："+level.Feedback;
        if(puzzle!=null && puzzle.learningTask!=null)
            foreach(var requirement in puzzle.learningTask.requirements)
            {
                var measure=requirement.measure;if(measure==null)continue;
                state.observation+="\n读数："+measure.Summary+"；操作次数："+measure.Operations+"；正在拿取："+measure.Held;
            }
        if(puzzle!=null && puzzle.mode==ReportPuzzleMode.Choice && puzzle.SelectedChoice>=0)
            state.observation+="\n已选："+puzzle.choices[puzzle.SelectedChoice];
        if(puzzle!=null)foreach(var placement in puzzle.placements)state.observation+="\n"+placement.label+"归位："+placement.IsReady;
        state.task=Limit(state.task,2000);state.observation=Limit(state.observation,5000);
        return state;
    }
    public void NewConversation()
    {
        if(Busy)return;
        StopListening();history.Clear();Question="";Output="已开启新对话。";Status="输入新的问题，或点击语音输入。";
    }
    public void ToggleListening()
    {
        if(Listening){StopListening();return;}
        if(Busy)return;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        try
        {
            if(Microphone.devices.Length==0){Status="未找到麦克风，请检查头显或电脑的录音设备。";return;}
            speechStart=Question;speechFinal="";
            recognizer=new DictationRecognizer(ConfidenceLevel.Low);
            recognizer.InitialSilenceTimeoutSeconds=15;recognizer.AutoSilenceTimeoutSeconds=8;
            recognizer.DictationResult+=SpeechResult;
            recognizer.DictationHypothesis+=SpeechHypothesis;
            recognizer.DictationComplete+=SpeechComplete;
            recognizer.DictationError+=SpeechError;
            recognizer.Start();Listening=recognizer.Status==SpeechSystemStatus.Running;
            Status=Listening?"正在聆听…说完点击停止，再确认文字并发送。":"听写未启动，请检查 Windows 语音设置。";
            if(!Listening)DisposeRecognizer();
        }
        catch(Exception ex){DisposeRecognizer();Status="语音启动失败："+ex.Message+"。请检查 Windows 语音与麦克风权限。";}
#else
        Status="语音输入需要 Windows；可使用文字输入调试。";
#endif
    }
    public void StopListening()
    {
        bool wasListening=Listening;
        DisposeRecognizer();
        if(wasListening)Status="听写已停止，请确认文字后发送。";
    }
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    void SpeechResult(string text,ConfidenceLevel confidence)
    {
        speechFinal+=text+" ";Question=Limit((speechStart+" "+speechFinal).Trim(),4000);Status="正在聆听…";
    }
    void SpeechHypothesis(string text){Status="识别中："+text;}
    void SpeechComplete(DictationCompletionCause cause)
    {
        DisposeRecognizer();Status=cause==DictationCompletionCause.Complete || cause==DictationCompletionCause.TimeoutExceeded || cause==DictationCompletionCause.PauseLimitExceeded
            ?"听写已结束，请确认文字后发送。":"听写中断："+cause+"。请检查 Windows 在线语音识别与网络。";
    }
    void SpeechError(string message,int code){DisposeRecognizer();Status="语音识别失败："+message+" ("+code+")。请检查 Windows 语音和麦克风权限。";}
#endif
    void DisposeRecognizer()
    {
        Listening=false;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if(recognizer==null)return;
        var current=recognizer;recognizer=null;
        current.DictationResult-=SpeechResult;current.DictationHypothesis-=SpeechHypothesis;current.DictationComplete-=SpeechComplete;current.DictationError-=SpeechError;
        if(current.Status==SpeechSystemStatus.Running)current.Stop();current.Dispose();
#endif
    }
    void OnApplicationFocus(bool focus){if(!focus)StopListening();}
    void OnDisable(){StopListening();}
    void OnDestroy(){DisposeRecognizer();}
    static string Limit(string text,int max)=>string.IsNullOrEmpty(text)?"":text.Length<=max?text:text.Substring(0,max);
}
