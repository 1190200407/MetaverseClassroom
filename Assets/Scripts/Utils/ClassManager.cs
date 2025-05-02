using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using Mirror;

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

    void Start()
    {
        StartCourse();
    }

    public void StartCourse()
    {
        // TODO 暂时这些数据，之后需要从服务器获取
        roomProperties = new Dictionary<string, string>();
        pptFilePath = "餐馆点餐.ppt";
        SceneLoader.instance.LoadSceneFromXml("Classroom");
        whiteboard = GameObject.FindObjectOfType<Whiteboard>();
        currentScene = "Classroom";
        isInClassroom = true;

        availableActivities = new List<BaseActivity>();
        availableActivities.Add(new ActingActivity("ActingActivity_Dining", "英语情景——餐馆点餐", true, "Cafe"));
        availableActivities.Add(new ActingActivity("ActingActivity_CheckIn", "英语情景——酒店入住", true, "Hotel"));
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
            currentActivity.End();
            currentActivity = null;
            if (backToClassroom)
            {
                // 向所有玩家发送切换场景的消息
                foreach (var conn in NetworkServer.connections.Values)
                {
                    conn.Send(new ChangeSceneMessage("Classroom"));
                }
            }
        }
    }

    public void ChangeScene(string sceneName)
    {
        // 弹出直到只剩游戏UI
        UIManager.instance.PopUntil(() => UIManager.instance.stack_ui.Peek().GetType() == typeof(GamePanel));
        UIManager.instance.Push(new TransitionPanel(new UIType("Panels/TransitionPanel", "TransitionPanel")));
        nextScene = sceneName;
    }

    public void OnSceneTransitionEnd()
    {
        SceneLoader.instance.LoadSceneFromXml(nextScene);
        PlayerManager.localPlayer.CurrentScene = nextScene;
        EventHandler.Trigger(new ChangeSceneEvent() { sceneName = nextScene });
        isInClassroom = !isInClassroom; // 切换场景后，教室状态取反, 之后可能出现一教室多场景情况，需要切换成别的判断方式
    }
}
