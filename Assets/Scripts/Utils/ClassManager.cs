using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Mirror;

public class ClassManager : NetworkSingleton<ClassManager>
{
    public bool isInClassroom = true;
    public string pptFilePath;//PPT文件的路径
    public string currentScene;
    public string nextScene;

    public BaseActivity currentActivity;

    #region 房间属性
    // 存储房间属性
    private Dictionary<object, object> roomProperties = new Dictionary<object, object>();

    // 服务器端设置属性
    [Server]
    public void SetRoomProperty(object key, object value)
    {
        roomProperties[key] = value;
        RpcUpdateRoomProperty(key, value);
    }

    // 客户端获取属性
    public object GetRoomProperty(object key)
    {
        if (roomProperties.TryGetValue(key, out object value))
        {
            return value;
        }
        return null;
    }

    // 服务器广播属性更新
    [ClientRpc]
    private void RpcUpdateRoomProperty(object key, object value)
    {
        roomProperties[key] = value;
    }
    #endregion

    public void StartCourse()
    {
        SceneLoader.instance.LoadSceneFromXml("Classroom");
        currentScene = "Classroom";
        isInClassroom = true;
    }

    public void StartActivity(string activityName)
    {
        // 暂时以这个为例子
        currentActivity = new ActingActivity(activityName, "英语情景", true, "Cafe");
        currentActivity.Start();
    }

    public void ChangeScene(string sceneName)
    {
        UIManager.instance.Push(new TransitionPanel(new UIType("Panels/TransitionPanel", "TransitionPanel")));
        nextScene = sceneName;
    }

    public void OnSceneTransitionEnd()
    {
        SceneLoader.instance.LoadSceneFromXml(nextScene);
        currentScene = nextScene;
        PlayerManager.localPlayer.CurrentScene = nextScene;
        isInClassroom = !isInClassroom; // 切换场景后，教室状态取反, 之后可能出现一教室多场景情况，需要切换成别的判断方式
    }
}
