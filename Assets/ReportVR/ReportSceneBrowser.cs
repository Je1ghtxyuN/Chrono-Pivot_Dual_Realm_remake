using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Free scene browsing; ordered experiments are handled by DemoLevel.
public sealed class ReportSceneBrowser : MonoBehaviour
{
    public InputActionReference nextScene;
    public InputActionReference previousScene;
    [Tooltip("调试时允许未通关自由切换；正式解谜流程默认关闭。")]
    public bool allowDebugNavigation;
    public DemoLevel level;
    bool loading;
    public AsyncOperation LoadingOperation=>ReportSceneTransition.LoadingOperation;
    bool navigationReady;
    bool showSceneList;
    Vector2 scroll;
    void Awake(){if(level==null)level=FindObjectOfType<DemoLevel>();}
    public bool CanBrowse=>allowDebugNavigation || level==null || level.Completed;

    void Update()
    {
        var ui=GetComponent<ReportGameUI>();if(ui!=null && ui.enabled && ui.MenuOpen)return;
        if(loading || ReportSceneTransition.IsTransitioning)return;
        // A held button must be released after loading before it can change scenes again.
        if(!navigationReady)
        {
            navigationReady=!nextScene.action.IsPressed() && !previousScene.action.IsPressed();
            return;
        }
        var keyboard=Keyboard.current;
        if(nextScene.action.WasPressedThisFrame() || (keyboard!=null && keyboard.pageDownKey.wasPressedThisFrame))ChangeScene(1);
        else if(previousScene.action.WasPressedThisFrame() || (keyboard!=null && keyboard.pageUpKey.wasPressedThisFrame))ChangeScene(-1);
        var desktop=GetComponent<DesktopPlayer>();
        if((ui==null || !ui.enabled) && keyboard!=null && keyboard.escapeKey.wasPressedThisFrame && (desktop==null || !desktop.IsDesktop))Application.Quit();
    }

    public static int AdjacentSceneIndex(int current,int offset,int count)
    {
        return ((current+offset)%count+count)%count;
    }

    public void ChangeScene(int offset)
    {
        if(!CanBrowse){level.ShowFeedback("出口尚未解锁，请完成当前关卡的三个目标。");return;}
        if(!loading)LoadScene(AdjacentSceneIndex(SceneManager.GetActiveScene().buildIndex,offset,SceneManager.sceneCountInBuildSettings));
    }

    void LoadScene(int index)
    {
        if(!CanBrowse)return;
        TryLoadScene(index);
    }
    public bool TryLoadScene(int index)
    {
        if(loading || ReportSceneTransition.IsTransitioning || index<0 || index>=SceneManager.sceneCountInBuildSettings)return false;
        var ui=GetComponent<ReportGameUI>();
        if(ui!=null && ui.enabled){if(!ui.IsSceneUnlocked(index))return false;}
        else if(!CanBrowse)return false;
        ReportAudio.SilenceWorld();ReportAudio.Play(ReportSound.SceneTravel,transform.position,1,true);
        if(!ReportSceneTransition.Begin(index))return false;
        loading=true;
        return true;
    }

    // Desktop companion controls; the HMD sees only the scene and wrist instructions.
    void OnGUI()
    {
        var ui=GetComponent<ReportGameUI>();if(ui!=null && ui.enabled)return;
        GUILayout.BeginArea(new Rect(16,16,390,showSceneList?480:100),GUI.skin.box);
        GUILayout.Label(level!=null?level.title:SceneManager.GetActiveScene().name);
        GUILayout.BeginHorizontal();
        GUI.enabled=!loading && !ReportSceneTransition.IsTransitioning && CanBrowse;
        if(GUILayout.Button("Previous / Page Up"))ChangeScene(-1);
        if(GUILayout.Button("Next / Page Down"))ChangeScene(1);
        if(GUILayout.Button("Scenes"))showSceneList=!showSceneList;
        GUILayout.EndHorizontal();
        GUI.enabled=true;
        if(showSceneList)
        {
            scroll=GUILayout.BeginScrollView(scroll);
            for(int i=0;i<SceneManager.sceneCountInBuildSettings;i++)
                if(GUILayout.Button(System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i))))LoadScene(i);
            GUILayout.EndScrollView();
        }
        GUILayout.EndArea();
    }
}
