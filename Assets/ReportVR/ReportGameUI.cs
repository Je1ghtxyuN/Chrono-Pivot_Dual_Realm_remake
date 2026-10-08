using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public enum ReportUIPage { Home,Tasks,Scenes,Settings,Controls,Completed,ConfirmReset,ConfirmQuit,Loading,Tutor }

[DefaultExecutionOrder(-200)]
public sealed partial class ReportGameUI : MonoBehaviour
{
    [Header("关卡和输入")]
    public DesktopPlayer player;
    public DemoLevel level;
    public ReportSceneBrowser browser;
    public InputActionReference menuInput,leftTrigger,rightTrigger;
    public string[] levelTitles=new string[0];
    [Header("界面展示")]
    public bool openOnFirstStart=true;
    public bool showHUD=true;
    [Min(.5f)] public float vrMenuDistance=1.7f;
    [Min(.0005f)] public float vrMenuScale=.00135f;
    public Vector3 wristPosition=new Vector3(0,.09f,.07f);
    public Vector3 wristRotation=new Vector3(65,0,0);
    public Color ink=new Color(.055f,.15f,.18f);
    public Color jade=new Color(.12f,.38f,.35f);
    public Color paper=new Color(.96f,.94f,.87f);
    public Color gold=new Color(.71f,.54f,.29f);
    [Header("保存")]
    public bool persistSettings=true;
    public bool saveProgress=true;
    public bool MenuOpen {get;private set;}
    public ReportUIPage Page {get;private set;}
    public Canvas MenuCanvas {get;private set;}
    public int HighestUnlocked {get;private set;}
    const string Key="ChronoPivot.UI.";
    static bool sessionStarted;
    static int sessionUnlocked;
    Font font;
    RectTransform frame,content,sidebar,footer;
    Canvas desktopHUD,wristHUD,promptHUD;
    Text desktopStatus,wristStatus,prompt,desktopPrompt,feedback,loadingStatus,desktopToast;
    Text pageTitle,pageCaption;
    Image[] reticleMarks;
    InputSystemUIInputModule mouseModule;
    EventSystem events;
    ReportUIRayPointer[] pointers;
    ContinuousMoveProvider move;
    SnapTurnProvider turn;
    float previousScale=1,nextRefresh;
    float feedbackUntil;
    string lastFeedback="";
    bool initialized,lastDesktop,completionShown;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSession(){sessionStarted=false;sessionUnlocked=0;}
    void Start()
    {
        Initialize();
        if(openOnFirstStart && !sessionStarted)ShowPage(ReportUIPage.Home);
        else{Resume();level.ShowFeedback(level.mission);}
    }
    void Initialize()
    {
        if(initialized)return;initialized=true;
        if(player==null)player=GetComponent<DesktopPlayer>();
        if(level==null)level=FindObjectOfType<DemoLevel>();
        if(browser==null)browser=GetComponent<ReportSceneBrowser>();
        InitializeTutor();
        move=GetComponent<ContinuousMoveProvider>();turn=GetComponent<SnapTurnProvider>();
        font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},28);
        HighestUnlocked=Mathf.Clamp(Mathf.Max(sessionUnlocked,saveProgress?PlayerPrefs.GetInt(Key+"Unlocked",0):0),0,Mathf.Max(0,levelTitles.Length-1));
        if(persistSettings)
        {
            if(PlayerPrefs.HasKey(Key+"Walk"))SetWalk(PlayerPrefs.GetFloat(Key+"Walk"));
            if(PlayerPrefs.HasKey(Key+"Turn"))turn.turnAmount=Mathf.Clamp(PlayerPrefs.GetFloat(Key+"Turn"),15,90);
            if(PlayerPrefs.HasKey(Key+"Mouse"))player.sensitivity=Mathf.Clamp(PlayerPrefs.GetFloat(Key+"Mouse"),.04f,.35f);
            if(PlayerPrefs.HasKey(Key+"Volume"))AudioListener.volume=Mathf.Clamp01(PlayerPrefs.GetFloat(Key+"Volume"));
        }
        events=FindObjectOfType<EventSystem>();
        if(events==null)events=new GameObject("Report UI Event System").AddComponent<EventSystem>();
        mouseModule=events.GetComponent<InputSystemUIInputModule>();
        if(mouseModule==null){mouseModule=events.gameObject.AddComponent<InputSystemUIInputModule>();mouseModule.AssignDefaultActions();}
        MenuCanvas=CreateCanvas("Report Menu Canvas",transform,RenderMode.ScreenSpaceOverlay,new Vector2(1280,800));
        var backdrop=Rect("Backdrop",MenuCanvas.transform,0,0,1280,800);Stretch(backdrop);Image(backdrop,new Color(ink.r,ink.g,ink.b,.65f));
        frame=Rect("Menu Frame",MenuCanvas.transform,0,0,1000,660);frame.anchorMin=frame.anchorMax=frame.pivot=new Vector2(.5f,.5f);frame.anchoredPosition=Vector2.zero;Image(frame,paper);
        sidebar=Rect("Sidebar",frame,0,0,220,660);Image(sidebar,ink);
        Dial(sidebar,new Vector2(140,16),56);
        Label(sidebar,"CHRONO PIVOT",20,25,160,24,13,gold);
        Label(sidebar,"晷枢\n双界",22,68,180,120,46,paper);
        Label(sidebar,"以双手理解物理",24,199,175,25,16,new Color(.65f,.76f,.70f));
        AddTab("UI_Home","01   探索首页",ReportUIPage.Home,258);
        AddTab("UI_Tasks","02   当前任务",ReportUIPage.Tasks,309);
        AddTab("UI_Scenes","03   关卡地图",ReportUIPage.Scenes,360);
        AddTab("UI_Settings","04   操作与设置",ReportUIPage.Settings,411);
        AddTab("UI_Controls","05   操作指南",ReportUIPage.Controls,462);
        AddTab("UI_Tutor","06   AI 物理导师",ReportUIPage.Tutor,513);
        Button(sidebar,"UI_Quit","退出演示",20,578,180,40,()=>ShowPage(ReportUIPage.ConfirmQuit),false,true);
        Label(sidebar,"WINDOWS / STEAMVR",24,630,180,20,11,gold);
        pageCaption=Label(frame,"物理探索 · 实验手册",250,28,680,22,13,gold);
        pageTitle=Label(frame,"探索首页",248,60,690,60,35,ink);
        Image(Rect("Header Rule",frame,250,123,700,2),new Color(gold.r,gold.g,gold.b,.45f));
        content=Rect("Page Content",frame,250,145,700,385);
        footer=Rect("Footer",frame,250,548,700,96);
        feedback=Label(footer,"",0,0,695,36,15,jade);
        Button(footer,"UI_Resume","开始探索  →",0,50,270,44,Resume,true);
        Button(footer,"UI_Reset","重新开始",290,50,180,44,()=>ShowPage(ReportUIPage.ConfirmReset),false);
        Button(footer,"UI_Retrieve","取回未命中物件",490,50,200,44,()=>{level.RetrieveThrows();Resume();},false);
        BuildHUD();
        var offset=GetComponent<Unity.XR.CoreUtils.XROrigin>().CameraFloorOffsetObject.transform;
        pointers=new ReportUIRayPointer[2];
        for(int i=0;i<2;i++)
        {
            var go=new GameObject(i==0?"Left UI pointer":"Right UI pointer");go.transform.SetParent(transform,false);
            var pointer=go.AddComponent<ReportUIRayPointer>();pointer.ui=this;pointer.source=offset.Find(i==0?"Left Controller":"Right Controller");pointer.trigger=i==0?leftTrigger:rightTrigger;pointer.left=i==0;pointer.events=events;pointers[i]=pointer;
        }
        lastDesktop=player.IsDesktop;UpdateMode();MenuCanvas.gameObject.SetActive(false);
    }
    void AddTab(string name,string text,ReportUIPage page,float y)=>Button(sidebar,name,text,16,y,188,43,()=>ShowPage(page),false,true);
    public void ShowPage(ReportUIPage page)
    {
        Initialize();
        if(!MenuOpen)
        {
            ReportAudio.Play(ReportSound.MenuOpen,transform.position,1,true);
            previousScale=Time.timeScale;MenuOpen=true;Time.timeScale=0;player.SetUIBlocked(true);
            foreach(var grab in player.GetComponentsInChildren<ReportGrabInteractor>())grab.ReleaseUntilButtonUp();
            MenuCanvas.gameObject.SetActive(true);UpdateMode();PlaceWorldMenu();
        }
        if(page!=ReportUIPage.Tutor)tutor.StopListening();
        Page=page;RebuildPage();
    }
    public void Resume()
    {
        if(!initialized)return;
        tutor.StopListening();
        if(MenuOpen)ReportAudio.Play(ReportSound.MenuClose,transform.position,1,true);
        MenuOpen=false;sessionStarted=true;Time.timeScale=previousScale;player.SetUIBlocked(ReportSceneTransition.IsTransitioning);
        MenuCanvas.gameObject.SetActive(false);player.ResumePointer();
        if(persistSettings || saveProgress)PlayerPrefs.Save();
    }
    public bool IsSceneUnlocked(int index)=>index>=0 && index<levelTitles.Length && (browser.allowDebugNavigation || index<=HighestUnlocked || index==SceneManager.GetActiveScene().buildIndex);
    void Update()
    {
        if(!initialized || ReportSceneTransition.IsTransitioning)return;
        if(lastDesktop!=player.IsDesktop){lastDesktop=player.IsDesktop;UpdateMode();if(MenuOpen){PlaceWorldMenu();RebuildPage();}else if(player.IsDesktop)player.ResumePointer();}
        var keyboard=Keyboard.current;
        bool typing=events.currentSelectedGameObject!=null && events.currentSelectedGameObject.GetComponent<InputField>()!=null;
        if(keyboard!=null && keyboard.tabKey.wasPressedThisFrame && !typing)ShowPage(level.Completed?ReportUIPage.Completed:ReportUIPage.Tasks);
        if(keyboard!=null && keyboard.escapeKey.wasPressedThisFrame || menuInput!=null && menuInput.action.WasPressedThisFrame())
        {
            if(MenuOpen)Resume();else ShowPage(ReportUIPage.Home);
        }
        if(Time.unscaledTime<nextRefresh)return;nextRefresh=Time.unscaledTime+.1f;
        if(level.Completed && !completionShown)
        {
            completionShown=true;HighestUnlocked=Mathf.Max(HighestUnlocked,Mathf.Min(SceneManager.GetActiveScene().buildIndex+1,levelTitles.Length-1));sessionUnlocked=HighestUnlocked;
            if(saveProgress){PlayerPrefs.SetInt(Key+"Unlocked",HighestUnlocked);PlayerPrefs.Save();}
        }
        else if(!level.Completed)completionShown=false;
        RefreshHUD();
        RefreshTutor();
    }
    void UpdateMode()
    {
        bool desk=player.IsDesktop;mouseModule.enabled=desk;
        MenuCanvas.renderMode=desk?RenderMode.ScreenSpaceOverlay:RenderMode.WorldSpace;
        var scaler=MenuCanvas.GetComponent<CanvasScaler>();scaler.enabled=desk;
        var rect=(RectTransform)MenuCanvas.transform;
        if(!desk){rect.sizeDelta=new Vector2(1000,660);rect.localScale=Vector3.one*vrMenuScale;}
        else rect.localScale=Vector3.one;
        desktopHUD.gameObject.SetActive(desk && showHUD && !MenuOpen && !ReportSceneTransition.IsTransitioning);
        wristHUD.gameObject.SetActive(!desk && showHUD && !MenuOpen && !ReportSceneTransition.IsTransitioning && player.HandTracked(true));
        promptHUD.gameObject.SetActive(!desk && showHUD && !MenuOpen && !ReportSceneTransition.IsTransitioning);
        var backdrop=MenuCanvas.transform.Find("Backdrop");backdrop.gameObject.SetActive(desk);
    }
    void PlaceWorldMenu()
    {
        if(player.IsDesktop)return;
        var forward=Vector3.ProjectOnPlane(player.head.transform.forward,Vector3.up).normalized;
        if(forward.sqrMagnitude<.01f)forward=transform.forward;
        MenuCanvas.transform.SetPositionAndRotation(player.head.transform.position+forward*vrMenuDistance,Quaternion.LookRotation(forward,Vector3.up));
    }
    void RebuildPage()
    {
        foreach(Transform child in content){child.gameObject.SetActive(false);Destroy(child.gameObject);}
        pageCaption.text="晷枢双界  /  "+level.title;
        string[] titles={"探索首页","当前任务","关卡地图","操作与设置","操作指南","实验完成","重新开始此关","退出演示","正在进入关卡","AI 物理导师"};pageTitle.text=titles[(int)Page];
        string[] tabs={"UI_Home","UI_Tasks","UI_Scenes","UI_Settings","UI_Controls","UI_Tutor"};
        int activeTab=Page==ReportUIPage.Tutor?5:(int)Page<5?(int)Page:Page==ReportUIPage.Completed?1:0;
        for(int i=0;i<tabs.Length;i++)sidebar.Find(tabs[i]).GetComponent<Image>().color=i==activeTab?jade:new Color(.13f,.25f,.27f);
        foreach(var button in sidebar.GetComponentsInChildren<Button>())button.interactable=Page!=ReportUIPage.Loading;
        footer.Find("UI_Retrieve").gameObject.SetActive(Page==ReportUIPage.Tasks && level.CanRetrieveThrows);
        foreach(var button in footer.GetComponentsInChildren<Button>())button.interactable=Page!=ReportUIPage.Loading;
        feedback.text=Short(level.Feedback,74);
        var resume=footer.Find("UI_Resume").GetComponentInChildren<Text>();resume.text=sessionStarted?"返回探索  →":"开始探索  →";
        switch(Page)
        {
            case ReportUIPage.Home:
                Label(content,"让知识在双手之间发生",0,0,690,48,29,ink);
                Label(content,Short(level.mission,110),0,60,690,70,18,jade);
                Card(0,148,216,92,"当前进度",level.Step+" / "+level.stations.Length+" 项实验");
                Card(234,148,216,92,"控制方式",player.IsDesktop?"键鼠调试":"SteamVR 双手操作");
                Card(468,148,216,92,"探索方法","拿取 · 摆放 · 观察");
                Label(content,"下一步",0,268,690,26,14,gold);
                Label(content,CurrentInstruction(),0,305,690,66,24,ink);break;
            case ReportUIPage.Tasks:
                var puzzle=CurrentPuzzle();
                Label(content,puzzle!=null?"目标  /  "+puzzle.goal:"全部目标已完成，可前往出口。",0,0,695,66,20,ink);
                Label(content,puzzle!=null?"规律  /  "+puzzle.clue:"沿场景步道继续探索，或选择下一关。",0,76,695,66,17,jade);
                for(int i=0;i<level.stations.Length;i++)
                {
                    var box=Rect("Task "+i,content,0,153+i*47,695,41);Image(box,new Color(jade.r,jade.g,jade.b,.075f));
                    Label(box,i<level.Step?"✓":(i+1).ToString("00"),12,7,44,28,19,i<level.Step?jade:gold);
                    Label(box,Short(level.stations[i].instruction,37),66,6,618,30,18,ink);
                }
                if(puzzle!=null)
                {
                    Label(content,Short(puzzle.hint,110),0,303,695,45,16,jade);
                    if(level.CanRetrieveThrows)Label(content,"投掷失败可取回，保留进度。",0,356,385,25,15,jade);
                }
                Button(content,"UI_AskTutor","向导师提问  →",405,351,290,34,()=>ShowPage(ReportUIPage.Tutor),true);
                break;
            case ReportUIPage.Tutor:BuildTutorPage();break;
            case ReportUIPage.Scenes:
                Label(content,"按实验顺序解锁；已解锁关卡可以重新探索。",0,0,695,26,17,jade);
                for(int i=0;i<levelTitles.Length;i++)
                {
                    int index=i;bool unlocked=IsSceneUnlocked(i);
                    var b=Button(content,"UI_Scene_"+i,(i+1).ToString("00")+"  "+Short(levelTitles[i],13)+(unlocked?"":"  · 锁定"),(i%3)*236,45+(i/3)*64,222,54,()=>SelectScene(index),false);
                    b.interactable=unlocked;b.GetComponentInChildren<Text>().fontSize=15;
                }
                break;
            case ReportUIPage.Settings:
                Setting("UI_Volume","音量",0,0,1,AudioListener.volume,v=>{AudioListener.volume=v;Save("Volume",v);},v=>(v*100).ToString("F0")+"%");
                Setting("UI_Walk","行走速度",86,0,3,player.IsDesktop?player.speed:move.moveSpeed,v=>{SetWalk(v);Save("Walk",v);},v=>v.ToString("F1")+" 米/秒");
                Setting("UI_Turn","分段转向",172,15,90,turn.turnAmount,v=>{turn.turnAmount=v;Save("Turn",v);},v=>v.ToString("F0")+"°");
                Setting("UI_Mouse","鼠标灵敏度",258,.04f,.35f,player.sensitivity,v=>{player.sensitivity=v;Save("Mouse",v);},v=>v.ToString("F2")+" 度/像素");
                Button(content,"UI_HUD","任务面板："+(showHUD?"开启":"关闭"),0,347,260,38,()=>{showHUD=!showHUD;RebuildPage();},false);
                Label(content,"建议先用低速行走与小角度转向。",285,354,410,25,14,jade);break;
            case ReportUIPage.Controls:
                string[] controls=player.IsDesktop?new[]{"移动与观察|WASD 行走；移动鼠标观察","抓取与操作|按住左键 / E 拿取；滚轮调远近；拖机关时 Shift 微调","抛掷|拿住物件后按 F / 右键蓄力，松开抛出","菜单与重置|Esc 打开菜单；R 重置；PgUp / PgDn 切关"}:new[]{"移动与观察|头显观察；左摇杆行走；右摇杆分段转向","抓取与操作|握把抓取；实际移动、摆放或旋转器材","抛掷|挥动手柄并松握把，物件按手部速度抛出","菜单与重置|右 B / 菜单键打开 UI；左 B / 菜单键重置"};
                for(int i=0;i<controls.Length;i++){var parts=controls[i].Split('|');Label(content,parts[0],0,i*88,695,30,21,ink);Label(content,parts[1],0,i*88+37,695,41,17,jade);}
                break;
            case ReportUIPage.Completed:
                Dial(content,new Vector2(560,8),115);
                Label(content,"你已完成本关的全部实验",0,20,545,55,28,ink);
                Label(content,level.title,0,87,530,48,21,jade);
                Label(content,"观察结果、理解原理，再把经验带入下一段探索。\n出口已解锁，可以继续探索或进入下一关。",0,157,690,82,19,jade);
                Button(content,"UI_Next","进入下一关  →",0,270,280,54,()=>SelectScene((SceneManager.GetActiveScene().buildIndex+1)%levelTitles.Length),true);break;
            case ReportUIPage.ConfirmReset:
                Label(content,"物件与机关将恢复到初始状态，\n本关任务进度也会清空。",0,35,690,105,25,ink);
                Button(content,"UI_ConfirmReset","确认重新开始",0,202,280,52,()=>{Resume();level.ResetLevel();},true);
                Button(content,"UI_CancelReset","保留当前进度",300,202,250,52,()=>ShowPage(ReportUIPage.Home),false);break;
            case ReportUIPage.ConfirmQuit:
                Label(content,"结束本次探索？\n已解锁关卡与设置会被保留。",0,35,690,105,25,ink);
                Button(content,"UI_ConfirmQuit","退出演示",0,202,280,52,()=>{Resume();Application.Quit();},true);
                Button(content,"UI_CancelQuit","继续探索",300,202,250,52,Resume,false);break;
            case ReportUIPage.Loading:
                Label(content,"新的实验正在准备",0,50,690,60,28,ink);
                loadingStatus=Label(content,"正在载入场景…",0,145,690,50,20,jade);break;
        }
    }
    void SelectScene(int index){if(!IsSceneUnlocked(index))return;Resume();browser.TryLoadScene(index);}
    void SetWalk(float value){value=Mathf.Clamp(value,0,3);player.speed=value;move.moveSpeed=value;}
    void Save(string name,float value){if(persistSettings)PlayerPrefs.SetFloat(Key+name,value);}
    ReportPuzzle CurrentPuzzle()=>!level.Completed && level.stations.Length>0?level.stations[level.Step].puzzle:null;
    string CurrentInstruction()=>level.Completed?"出口已解锁，沿步道继续探索。":level.stations[level.Step].instruction;
    static string Short(string value,int length)=>string.IsNullOrEmpty(value)?"":value.Length<=length?value:value.Substring(0,length-1)+"…";
    void Card(float x,float y,float w,float h,string title,string value)
    {
        var card=Rect(title,content,x,y,w,h);Image(card,new Color(jade.r,jade.g,jade.b,.07f));Label(card,title,14,14,w-28,24,14,gold);Label(card,value,14,49,w-28,31,19,ink);
    }
    void Setting(string name,string title,float y,float min,float max,float value,Action<float> change,Func<float,string> display)
    {
        Label(content,title,0,y,440,29,20,ink);var reading=Label(content,display(value),475,y,220,29,17,jade);reading.alignment=TextAnchor.MiddleRight;
        var rect=Rect(name,content,0,y+44,695,28);Image(rect,Color.clear).raycastTarget=true;var slider=rect.gameObject.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;
        var track=Rect("Track",rect,0,10,695,6);Image(track,new Color(jade.r,jade.g,jade.b,.15f));
        var fillArea=Rect("Fill Area",rect,0,10,695,6);var fill=Rect("Fill",fillArea,0,0,695,6);Stretch(fill);Image(fill,jade);slider.fillRect=fill;
        var area=Rect("Handle Area",rect,8,0,679,28);var handle=Rect("Handle",area,0,0,20,28);handle.sizeDelta=new Vector2(20,0);handle.anchoredPosition=Vector2.zero;handle.pivot=new Vector2(.5f,.5f);Image(handle,gold);slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();
        slider.value=value;slider.onValueChanged.AddListener(v=>{change(v);reading.text=display(v);ReportAudio.Play(ReportSound.UIAdjust,transform.position,1,true);});
    }
    void BuildHUD()
    {
        desktopHUD=CreateCanvas("Desktop Task HUD",transform,RenderMode.ScreenSpaceOverlay,new Vector2(1280,800));
        var plate=Rect("Task Plaque",desktopHUD.transform,0,0,346,180);plate.anchorMin=plate.anchorMax=plate.pivot=Vector2.one;plate.anchoredPosition=new Vector2(-22,-22);Image(plate,new Color(paper.r,paper.g,paper.b,.94f));
        Label(plate,"实验手册",18,13,250,26,14,gold);desktopStatus=Label(plate,"",18,45,310,85,17,ink);
        Button(plate,"UI_HUDMenu","菜单 / Esc",18,135,150,30,()=>ShowPage(ReportUIPage.Home),false);
        Button(plate,"UI_HUDTasks","任务详情",178,135,150,30,()=>ShowPage(level.Completed?ReportUIPage.Completed:ReportUIPage.Tasks),true);
        var reticle=Rect("Desktop Reticle",desktopHUD.transform,0,0,14,14);reticle.anchorMin=reticle.anchorMax=reticle.pivot=new Vector2(.5f,.5f);reticle.anchoredPosition=Vector2.zero;
        reticleMarks=new[]{Image(Rect("Horizontal",reticle,0,6,14,2),paper),Image(Rect("Vertical",reticle,6,0,2,14),paper)};
        var tip=Rect("Desktop Interaction",desktopHUD.transform,0,0,600,62);tip.anchorMin=tip.anchorMax=tip.pivot=new Vector2(.5f,0);tip.anchoredPosition=new Vector2(0,25);Image(tip,new Color(ink.r,ink.g,ink.b,.87f));
        desktopPrompt=Label(tip,"",12,6,576,50,18,paper);desktopPrompt.alignment=TextAnchor.MiddleCenter;
        var toast=Rect("Experiment Feedback",desktopHUD.transform,0,0,600,68);toast.anchorMin=toast.anchorMax=toast.pivot=new Vector2(.5f,0);toast.anchoredPosition=new Vector2(0,107);Image(toast,new Color(paper.r,paper.g,paper.b,.95f));
        desktopToast=Label(toast,"",16,9,568,50,17,jade);desktopToast.alignment=TextAnchor.MiddleCenter;toast.gameObject.SetActive(false);
        var offset=GetComponent<Unity.XR.CoreUtils.XROrigin>().CameraFloorOffsetObject.transform;
        wristHUD=CreateCanvas("VR Wrist Task HUD",offset.Find("Left Controller"),RenderMode.WorldSpace,new Vector2(560,235));
        wristHUD.transform.localPosition=wristPosition;wristHUD.transform.localRotation=Quaternion.Euler(wristRotation);wristHUD.transform.localScale=Vector3.one*.00072f;
        var wrist=Rect("Wrist Plaque",wristHUD.transform,0,0,560,235);Stretch(wrist);Image(wrist,new Color(ink.r,ink.g,ink.b,.94f));
        Label(wrist,"晷枢双界 · 当前实验",20,16,520,28,19,gold);wristStatus=Label(wrist,"",20,58,520,159,22,paper);
        promptHUD=CreateCanvas("VR Interaction Prompt",player.head.transform,RenderMode.WorldSpace,new Vector2(620,72));
        promptHUD.transform.localPosition=new Vector3(0,-.28f,.8f);promptHUD.transform.localScale=Vector3.one*.0008f;
        prompt=Label(promptHUD.transform,"",0,0,620,72,22,paper);prompt.alignment=TextAnchor.MiddleCenter;
    }
    void RefreshHUD()
    {
        UpdateMode();
        string state=level.title+"\n"+(level.Completed?(level.ExitReady?"✓ 全部完成 · 沿步道前往出口":"✓ 全部完成 · 出口正在打开"):(level.Step+1)+" / "+level.stations.Length+"  "+Short(CurrentInstruction(),46));
        desktopStatus.text=state;wristStatus.text=state+"\n"+Short(level.Feedback,23)+"\n右 B / 菜单键：任务与设置";
        if(level.Feedback!=lastFeedback){lastFeedback=level.Feedback;feedbackUntil=Time.unscaledTime+4;desktopToast.text=Short(lastFeedback,64);}
        desktopToast.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(lastFeedback) && Time.unscaledTime<feedbackUntil);
        var grab=player.GetComponentsInChildren<ReportGrabInteractor>().FirstOrDefault(g=>g.hasSelection)??player.GetComponentsInChildren<ReportGrabInteractor>().FirstOrDefault(g=>g.Candidate!=null);
        var item=grab==null?null:grab.Candidate;
        foreach(var mark in reticleMarks)mark.color=item!=null?gold:paper;
        var handle=item as ReportMechanismHandle;string name=item!=null?item.DisplayName+" · ":"";
        prompt.text=grab==null?"":name+(grab.hasSelection?(handle!=null?(handle.motion==ReportHandleMotion.Slide?"沿滑轨拖动，松手观察":handle.rotationFromPosition?"沿圆周拖动，松手观察":"转动手腕，松手观察"):"松手放下 · 挥手松握把抛掷"):"握把拿取");
        desktopPrompt.text=grab==null?"WASD 移动 · 鼠标观察 · Tab 任务 · Esc 菜单":name+(grab.hasSelection?(handle!=null?"拖鼠标调整 · Shift 微调 · 松开左键 / E 放开":grab.ThrowCharge01>0?"蓄力中 · 松开 F / 右键抛出":"滚轮调整远近 · 松开左键 / E 放下 · F / 右键蓄力抛出"):"左键 / E 拿取");
        if(MenuOpen && Page==ReportUIPage.Loading && loadingStatus!=null && browser.LoadingOperation!=null)loadingStatus.text="载入进度  "+Mathf.Clamp01(browser.LoadingOperation.progress/.9f).ToString("P0");
    }
    public bool PointerOverDesktopUI(Vector2 screen)
    {
        if(MenuOpen)return true;
        if(!desktopHUD.gameObject.activeInHierarchy)return false;
        return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)desktopHUD.transform.Find("Task Plaque"),screen,null);
    }
    void OnDisable()
    {
        if(!initialized)return;
        tutor.StopListening();
        if(MenuOpen){Time.timeScale=previousScale;MenuOpen=false;player.SetUIBlocked(false);}
        foreach(var canvas in new[]{MenuCanvas,desktopHUD,wristHUD,promptHUD})if(canvas!=null)canvas.gameObject.SetActive(false);
    }
    void OnDestroy(){if(font!=null)Destroy(font);}
    Canvas CreateCanvas(string name,Transform parent,RenderMode mode,Vector2 size)
    {
        var rect=Rect(name,parent,0,0,size.x,size.y);rect.pivot=new Vector2(.5f,.5f);var canvas=rect.gameObject.AddComponent<Canvas>();canvas.renderMode=mode;canvas.sortingOrder=100;
        var scaler=rect.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=size;scaler.matchWidthOrHeight=.5f;
        rect.gameObject.AddComponent<GraphicRaycaster>();return canvas;
    }
    static RectTransform Rect(string name,Transform parent,float x,float y,float w,float h)
    {
        var go=new GameObject(name,typeof(RectTransform));var rect=(RectTransform)go.transform;rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.sizeDelta=new Vector2(w,h);rect.anchoredPosition=new Vector2(x,-y);return rect;
    }
    static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
    static Image Image(RectTransform rect,Color color){var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
    Text Label(Transform parent,string value,float x,float y,float w,float h,int size,Color color)
    {
        var rect=Rect("Text",parent,x,y,w,h);var text=rect.gameObject.AddComponent<Text>();text.font=font;text.text=value;text.fontSize=size;text.color=color;text.alignment=TextAnchor.UpperLeft;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;text.raycastTarget=false;return text;
    }
    Button Button(Transform parent,string name,string value,float x,float y,float w,float h,Action action,bool primary,bool dark=false)
    {
        var rect=Rect(name,parent,x,y,w,h);var image=Image(rect,primary?jade:dark?new Color(.13f,.25f,.27f):new Color(jade.r,jade.g,jade.b,.08f));image.raycastTarget=true;
        var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;var colors=button.colors;colors.highlightedColor=new Color(1.13f,1.13f,1.06f);colors.pressedColor=new Color(.78f,.85f,.81f);colors.disabledColor=new Color(.7f,.7f,.7f,.45f);button.colors=colors;button.navigation=new Navigation{mode=Navigation.Mode.None};
        var text=Label(rect,value,12,0,w-24,h,18,primary||dark?paper:ink);text.alignment=TextAnchor.MiddleCenter;button.gameObject.AddComponent<ReportUIAudio>();button.onClick.AddListener(()=>{ReportAudio.Play(ReportSound.UIClick,transform.position,1,true);action();});return button;
    }
    void Dial(Transform parent,Vector2 position,float size)
    {
        var dial=Rect("Sundial Emblem",parent,position.x,position.y,size,size);dial.gameObject.AddComponent<ReportUIDial>().color=gold;
        var needle=Rect("Gnomon",dial,size*.47f,size*.18f,size*.05f,size*.47f);Image(needle,gold);needle.localRotation=Quaternion.Euler(0,0,-32);
    }
}
