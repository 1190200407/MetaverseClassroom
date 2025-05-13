using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GamePanel : BasePanel
{
    private Text networkStatusTxt;
    private Button muteBtn;
    private Button unmuteBtn;


    #region 任务相关
    private int actionNodeId; //任务目标的id
    private ActionTreeLeafNode actionNode;
    
    private TextMeshProUGUI goalText;
    private Button confirmButton;
    private GameObject goalPanel;
    public bool isGoalPanelOpen
    {
        get
        {
            return goalPanel.activeSelf;
        }
        set
        {
            goalPanel.SetActive(value);
        }
    }
    #endregion

    public GamePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        networkStatusTxt = UIMethods.instance.GetOrAddComponentInChild<Text>(ActiveObj, "NetworkStatusText");
        muteBtn = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "MuteButton");
        unmuteBtn = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "UnmuteButton");
        muteBtn.onClick.AddListener(OnMuteBtnClick);
        unmuteBtn.onClick.AddListener(OnUnmuteBtnClick);
        UpdateMuteState();
        
        goalPanel = ActiveObj.transform.Find("GoalPanel").gameObject;
        confirmButton = UIMethods.instance.GetOrAddComponentInChild<Button>(goalPanel, "ConfirmButton");
        goalText = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(goalPanel, "GoalText");
        CloseGoal();

        confirmButton.onClick.AddListener(ForceTaskComplete);
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        EventHandler.Register<NewTaskEvent>(OnNewTask);
        EventHandler.Register<TaskCompleteEvent>(OnTaskComplete);
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        EventHandler.Unregister<NewTaskEvent>(OnNewTask);
        EventHandler.Unregister<TaskCompleteEvent>(OnTaskComplete);
    }

    public override void OnUpdate()
    {
        // 暂停
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.instance.Push(new PausePanel(new UIType("Panels/PausePanel", "PausePanel")));
        }
        // 打开PPT
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.instance.Push(new PPTPanel(new UIType("Panels/PPTPanel", "PPTPanel")));
        }
        // 静音
        if (Input.GetKeyDown(KeyCode.M))
        {
            VoiceManager.instance.IsSelfMute = !VoiceManager.instance.IsSelfMute;
            UpdateMuteState();
        }

        // 强制完成任务
        if (actionNodeId != -1 && Input.GetKeyDown(KeyCode.E))
        {
            ForceTaskComplete();
        }

        // 获取网络状态
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            networkStatusTxt.text = $"<b>Disconnected</b>";
        }
        else if (NetworkServer.active && NetworkClient.active)
        {
            // host mode
            networkStatusTxt.text = $"<b>Host</b>: running via {Transport.active}";
        }
        else if (NetworkServer.active)
        {
            // server only
            networkStatusTxt.text = $"<b>Server</b>: running via {Transport.active}";
        }
        else if (NetworkClient.isConnected)
        {
            // client only
            networkStatusTxt.text = $"<b>Client</b>: connected to {NetworkManagerClassroom.singleton.networkAddress} via {Transport.active}";
        }
    }

    private void OnMuteBtnClick()
    {
        VoiceManager.instance.IsSelfMute = true;
        UpdateMuteState();
    }

    private void OnUnmuteBtnClick()
    {
        VoiceManager.instance.IsSelfMute = false;
        UpdateMuteState();
    }

    public void UpdateMuteState()
    {
        if (VoiceManager.instance.IsMute)
        {
            muteBtn.gameObject.SetActive(false);
            unmuteBtn.gameObject.SetActive(true);
        }
        else
        {
            muteBtn.gameObject.SetActive(true);
            unmuteBtn.gameObject.SetActive(false);
        }
    }

    #region 任务相关
    private void OnNewTask(NewTaskEvent @event)
    {
        if (@event.netId == PlayerManager.localPlayer.netId)
        {
            actionNodeId = @event.actionNodeId;
            actionNode = ClassManager.instance.currentActivity.actionTree.leafNodes[actionNodeId];
            SetGoal(@event.taskDescription);
        }
    }

    private void OnTaskComplete(TaskCompleteEvent @event)
    {
        if (@event.netId == PlayerManager.localPlayer.netId)
        {
            actionNodeId = -1;
            CloseGoal();
        }
    }
    private void ForceTaskComplete()
    {
        EventHandler.Trigger(new TaskCompleteEvent() { netId = PlayerManager.localPlayer.netId, actionNodeId = actionNodeId });
    }

    /// <summary>
    /// 设置任务目标
    /// </summary>
    /// <param name="text"></param>任务目标描述
    public void SetGoal(string text)
    {
        goalText.text = "任务目标：" + text;
        goalPanel.SetActive(true);
    }

    /// <summary>
    /// 关闭任务目标
    /// </summary>
    public void CloseGoal()
    {
        goalPanel.SetActive(false);
    }
    #endregion
}
