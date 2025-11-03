using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using Mirror;
using System;

public class ClassManager : NetworkSingleton<ClassManager>
{
    public bool isInClassroom = true;
    [SyncVar(hook = nameof(OnPPTFilePathChanged))]
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
    public Dictionary<string, GameObject> NPCList = new Dictionary<string, GameObject>();
    
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
        availableActivities.Add(new ActingActivity("ActingActivity_Dining", "英语情景——餐馆点餐", true, "Cafe", "CourseJsons/EnglishActing2"));
        availableActivities.Add(new ActingActivity("ActingActivity_CheckIn", "英语情景——酒店入住", true, "Hotel", "CourseJsons/EnglishActing"));

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
        yield return new WaitForSeconds(0.1f);

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
        
        EventHandler.Register<PickItemEvent>(SendPickItemNetMessage);
        EventHandler.Register<ResetItemEvent>(SendResetItemNetMessage);
        EventHandler.Register<DropItemEvent>(SendDropItemNetMessage);
        
        // 注册PPT路径变更事件
        EventHandler.Register<ChangePPTPathEvent>(OnChangePPTPath);
    }

    void OnDisable()
    {
        EventHandler.Unregister<TaskCompleteEvent>(OnTaskComplete);
        
        EventHandler.Unregister<PickItemEvent>(SendPickItemNetMessage);
        EventHandler.Unregister<ResetItemEvent>(SendResetItemNetMessage);
        EventHandler.Unregister<DropItemEvent>(SendDropItemNetMessage);
        
        // 取消注册PPT路径变更事件
        EventHandler.Unregister<ChangePPTPathEvent>(OnChangePPTPath);
    }

    private Delegate onEndActivity;
    private Delegate onStartActivity;
    private Delegate onTaskComplete;
    
    private Delegate onItemPick;
    private Delegate onItemReset;
    private Delegate onItemDrop;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("StartClient");

        onTaskComplete = new Action<TaskCompleteMessageData>(OnTaskCompleteMessage);
        onStartActivity = new Action<StartActivityMessageData>(OnStartActivity);
        onEndActivity = new Action(OnEndActivity);    

        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.TaskComplete, onTaskComplete);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.StartActivity, onStartActivity);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.EndActivity, onEndActivity);
        
        #region 物品拾取 网络事件监听
        onItemPick = new Action<ItemPickMessageData>(SendPickItemCallback);
        onItemReset = new Action<ItemResetMessageData>(SendResetItemCallback);
        onItemDrop = new Action<ItemDropMessageData>(SendDropItemCallback);
        
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.ItemPickup, onItemPick);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.ItemReset, onItemReset);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.ItemDrop, onItemDrop);
        #endregion 
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.TaskComplete, onTaskComplete);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.StartActivity, onStartActivity);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.EndActivity, onEndActivity);
        
        #region 物品拾取 网络事件取消监听
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.ItemPickup, onItemPick);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.ItemReset, onItemReset);
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.ItemDrop, onItemDrop);
        #endregion
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
        currentActivity.End();
        currentActivity = null;
        EventHandler.Trigger(new EndActivityEvent());

        if (backToClassroom)
        {
            // 向所有玩家发送切换场景的消息
            ChangeScene("Classroom");
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

    public void AddNPC(string roleId)
    {
        GameObject npc = Instantiate(Resources.Load<GameObject>("Prefabs/NPCModels/NPC"));
        NPCManager npcManager = npc.GetComponent<NPCManager>();
        //TODO 之后需要从脚本获取NPC的名称和角色
        string npcName = "NPC_" + roleList[roleId];
        npcManager.Init(npcName, "Dummy1", currentScene);
        npc.name = npcName;

        NPCList[roleId] = npc;
    }

    public void RemoveAllNPC()
    {
        foreach (var npc in NPCList)
        {
            Destroy(npc.Value);
        }
        NPCList.Clear();
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

    #region 拾取物品

    public void SendPickItemNetMessage(PickItemEvent @event)
    {
        NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.ItemPickup,new ItemPickMessageData(){holderId = @event.holderId,itemKey = @event.itemKey});
    }

    public void SendResetItemNetMessage(ResetItemEvent @event)
    {
        NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.ItemReset,new ItemResetMessageData(){holderId = @event.holderId});
    }

    public void SendDropItemNetMessage(DropItemEvent @event)
    {
        NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.ItemDrop,new ItemDropMessageData(){holderId = @event.holderId,position = @event.position});
    }
    
    public void SendPickItemCallback(ItemPickMessageData data)
    {
        EventHandler.Trigger(new PickItemCallback(){holderId = data.holderId,itemKey = data.itemKey});
    }
    public void SendResetItemCallback(ItemResetMessageData data)
    {
        EventHandler.Trigger(new ResetItemCallback(){holderId = data.holderId});
    }
    public void SendDropItemCallback(ItemDropMessageData data)
    {
        EventHandler.Trigger(new DropItemCallback(){holderId = data.holderId,position = data.position});
    }
    #endregion

    #region PPT路径同步
    // 处理PPT路径变更事件
    private void OnChangePPTPath(ChangePPTPathEvent @event)
    {
        // 调用Command命令来修改服务器上的PPT路径
        CommandChangePPTPath(@event.pptPath);
    }
    
    // SyncVar钩子函数，当pptFilePath变量被修改时调用
    private void OnPPTFilePathChanged(string oldPath, string newPath)
    {
        Debug.Log($"PPT路径已更新: {oldPath} -> {newPath}");
        
        // 触发PPT路径已更新事件，通知其他组件
        EventHandler.Trigger(new PPTPathUpdatedEvent { newPath = newPath });
    }
    
    // 服务器端修改PPT路径的Command
    [Command(requiresAuthority = false)]
    public void CommandChangePPTPath(string newPath)
    {
        // 在服务器上修改PPT路径
        pptFilePath = newPath;
        
        // 通过SyncVar自动同步到所有客户端
        // 无需额外的RPC调用，因为使用了SyncVar
        
        Debug.Log($"服务器PPT路径已更新为: {newPath}");
    }
    #endregion
}
