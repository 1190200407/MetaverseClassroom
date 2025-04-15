using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

// 教学活动基类, 之后扩展
public class BaseActivity
{
    public string activityName;
    public string activityDescription;

    public List<PlayerController> includedPlayers;

    public bool needToChangeScene;
    public string sceneName;

    public BaseActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName)
    {
        this.activityName = activityName;
        this.activityDescription = activityDescription;
        this.needToChangeScene = needToChangeScene;
        this.sceneName = sceneName;
    }

    public void SetIncludedPlayers(List<PlayerController> players)
    {
        // 深拷贝
        includedPlayers = new List<PlayerController>(players);
    }

    // 切换场景
    public virtual void GoToScene()
    {
        // 向所有玩家发送切换场景的消息
        foreach (var conn in NetworkServer.connections.Values)
        {
            conn.Send(new ChangeSceneMessage(sceneName));
        }
    }

    // 返回教室
    public virtual void BackToClassroom()
    {
        // 向所有玩家发送切换场景的消息
        foreach (var conn in NetworkServer.connections.Values)
        {
            conn.Send(new ChangeSceneMessage("Classroom"));
        }
    }

    public virtual void Start()
    {

    }

    public virtual void OnUpdate()
    {

    }

    public virtual void End()
    {

    }
}
