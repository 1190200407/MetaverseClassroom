using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
