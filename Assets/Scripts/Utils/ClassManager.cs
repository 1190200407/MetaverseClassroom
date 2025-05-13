using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using Mirror;
using System;

public class ClassManager : NetworkSingleton<ClassManager>
{
    public bool isInClassroom = true;
    public string pptFilePath;//PPT文件的路径
    public string currentScene;
    public string nextScene;

    public BaseActivity currentActivity;
    // 可用的活动列表, 从服务器获取， 目前没有用
    public List<BaseActivity> availableActivities = new List<BaseActivity>();
    public bool isHavingActivity => currentActivity != null;

    #region 房间属性
    // 存储房间属性
    public Dictionary<string, string> roomProperties = new Dictionary<string, string>();
    public string propertyValue;
    public Whiteboard whiteboard;
    
    // 服务器端设置属性
    [Command(requiresAuthority = false)]
    public void CommandSetRoomProperty(string key, string value)
    {
        roomProperties[key] = value;
        RpcSetRoomProperty(key, value);
    }

    // 通知客户端属性变化
    [ClientRpc]
    private void RpcSetRoomProperty(string key, string value)
    {
        EventHandler.Trigger(new RoomPropertyChangeEvent() { key = key, value = value });
    }

    // 查询房间属性（Command）
    [Command(requiresAuthority = false)]
    public void CmdGetRoomProperty(string key, NetworkConnectionToClient conn = null)
    {
        if (isServer && conn != null)
        {
            // 查询房间属性
            if (roomProperties.ContainsKey(key))
            {
                // 只通过 TargetRpc 返回给调用者（特定客户端）
                TargetReplyRoomProperty(conn, key, roomProperties[key]);
                
            }
            else
            {
                // 如果没有找到属性，可以返回空值或者错误信息
                TargetReplyRoomProperty(conn, key, null);
            }
        }
    }

    [TargetRpc]
    private void TargetReplyRoomProperty(NetworkConnectionToClient conn, string key, string value)
    {
        propertyValue = value;
    }
    #endregion

    #region 选角
    // 角色列表, Key为角色Id，Value为角色名
    public Dictionary<string, string> roleList = new Dictionary<string, string>();
    // 角色占用情况, Key为角色Id，Value为玩家ID(0表示未占用， -1表示NPC)
    public Dictionary<string, int> roleOccupied = new Dictionary<string, int>();
    
    // 服务器端设置属性
    [Command(requiresAuthority = false)]
    public void CommandSetRoleOccupied(string roleId, int netId)
    {
        roleOccupied[roleId] = netId;
        RpcSetRoleOccupied(roleId, netId);
    }

    // 通知客户端属性变化
    [ClientRpc]
    private void RpcSetRoleOccupied(string roleId, int netId)
    {
        if (!roleList.ContainsKey(roleId))
        {
            Debug.LogError("没有找到角色 " + roleId);
            return;
        }
        
        roleOccupied[roleId] = netId;
        EventHandler.Trigger(new RoleOccupiedChangeEvent() { roleId = roleId, netId = netId});
    }
    #endregion

    #region 生命周期
    void Start()
    {
        StartCoroutine(StartCourseCoroutine());
    }

    public IEnumerator StartCourseCoroutine()
    {
        // 等待本地玩家初始化
        yield return new WaitUntil(() => PlayerManager.localPlayer != null);

        // TODO 暂时这些数据，之后需要从服务器获取
        roomProperties = new Dictionary<string, string>();
        pptFilePath = "餐馆点餐.ppt";

        // 初始化活动列表
        availableActivities = new List<BaseActivity>();
        availableActivities.Add(new ActingActivity("ActingActivity_Dining", "英语情景——餐馆点餐", true, "Cafe"));
        availableActivities.Add(new ActingActivity("ActingActivity_CheckIn", "英语情景——酒店入住", true, "Hotel"));

        // 显示加载界面
        UIManager.instance.Push(new LoadPanel(new UIType("Panels/LoadPanel", "LoadPanel")));
        
        // 加载教室场景
        SceneLoader.instance.LoadSceneFromXml("Classroom");
        yield return new WaitUntil(() => !SceneLoader.instance.isLoading);
        whiteboard = GameObject.FindObjectOfType<Whiteboard>();

        // 加载活动场景
        foreach (var activity in availableActivities)
        {
            SceneLoader.instance.LoadSceneFromXml(activity.sceneName, false);
            yield return new WaitUntil(() => !SceneLoader.instance.isLoading);
        }

        // 隐藏加载界面
        UIManager.instance.Pop(false);
        UIManager.instance.Push(new GamePanel(new UIType("Panels/GamePanel", "GamePanel")));

        // 触发场景加载完成事件
        PlayerManager.localPlayer.CurrentScene = "Classroom";
        isInClassroom = true;
        EventHandler.Trigger(new SceneLoadedEvent());
    }
    

    void Update()
    {
        if (currentActivity != null)
        {
            currentActivity.OnUpdate();
        }
    }

    void OnEnable()
    {
        EventHandler.Register<TaskCompleteEvent>(OnTaskComplete);
    }

    void OnDisable()
    {
        EventHandler.Unregister<TaskCompleteEvent>(OnTaskComplete);
    }

    private Delegate onEndActivity;
    private Delegate onStartActionTree;
    private Delegate onStartActivity;
    private Delegate onTaskComplete;

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("StartClient");

        onTaskComplete = new Action<TaskCompleteMessageData>(OnTaskCompleteMessage);
        onStartActivity = new Action<StartActivityMessageData>(OnStartActivity);
        onEndActivity = new Action(OnEndActivity);
        onStartActionTree = new Action(OnStartActionTree);

        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.TaskComplete, onTaskComplete);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.StartActivity, onStartActivity);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.EndActivity, onEndActivity);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.StartActionTree, onStartActionTree);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.TaskComplete, onTaskComplete);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.StartActivity, onStartActivity);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.EndActivity, onEndActivity);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.StartActionTree, onStartActionTree);
    }
    #endregion

    #region 活动
    public void OnStartActivity(StartActivityMessageData data)
    {
        StartActivity(availableActivities[data.activityIndex]);
    }

    public void OnEndActivity()
    {
        EndActivity();
    }

    public void OnStartActionTree()
    {
        if (currentActivity != null)
        {
            currentActivity.isActionTreeExecuting = true;
            StartCoroutine(currentActivity.actionTree.Execute());
        }
    }

    public void StartActivity(BaseActivity activity)
    {
        if (currentActivity != null)
        {
            EndActivity();
        }
        currentActivity = activity;
        currentActivity.Start();
    }

    public void EndActivity(bool backToClassroom = true)
    {
        if (currentActivity != null)
        {
            // 服务器取消选角
            if (NetworkServer.active)
            {
                foreach (var role in roleList)
                {
                    CommandSetRoleOccupied(role.Key, 0);
                }
            }

            currentActivity.End();
            currentActivity = null;

            if (backToClassroom)
            {
                // 向所有玩家发送切换场景的消息
                ChangeScene("Classroom");
            }
        }
    }
    
    public void ChangeScene(string sceneName)
    {
        // 弹出直到只剩游戏UI
        UIManager.instance.PopUntil(() => UIManager.instance.stack_ui.Peek().GetType() == typeof(GamePanel));
        UIManager.instance.Push(new TransitionPanel(new UIType("Panels/TransitionPanel", "TransitionPanel")));
        nextScene = sceneName;
        EventHandler.Trigger(new BeforeChangeSceneEvent() { sceneName = nextScene });
    }

    public void OnSceneTransitionEnd()
    {
        SceneLoader.instance.LoadSceneFromXml(nextScene);
        PlayerManager.localPlayer.CurrentScene = nextScene;
        EventHandler.Trigger(new ChangeSceneEvent() { sceneName = nextScene });
        isInClassroom = nextScene == "Classroom"; // 切换场景后，判断是否在教室,之后换成其他判断方式
    }

    // 在本地完成任务后，会触发本地的任务完成事件，然后会向客户端全体发送任务完成消息
    public void OnTaskComplete(TaskCompleteEvent @event)
    {
        if (currentActivity != null)
        {
            NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.TaskComplete, new TaskCompleteMessageData() { actionNodeId = @event.actionNodeId, netId = @event.netId });
        }
    }

    // 处理从网络中获得的任务完成消息，将任务完成状态设置为true
    public void OnTaskCompleteMessage(TaskCompleteMessageData data)
    {
        if (currentActivity != null && currentActivity.actionTree.leafNodes != null && currentActivity.actionTree.leafNodes.ContainsKey(@data.actionNodeId))
        {
            ActionTreeLeafNode node = currentActivity.actionTree.leafNodes[@data.actionNodeId];
            node.accomplished = true;
        }
    }
    #endregion
}
