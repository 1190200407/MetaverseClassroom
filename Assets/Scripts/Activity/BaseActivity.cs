using System;
using System.Collections;
using System.Collections.Generic;

// 教学活动基类, 之后扩展
public class BaseActivity
{
    public string activityName;
    public string activityDescription;

    public List<PlayerController> includedPlayers;

    public bool needToChangeScene;
    public string sceneName;

    // 活动的行为树
    public bool isActionTreeExecuting = false;
    public ActionTree actionTree;

    public BaseActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName)
    {
        this.activityName = activityName;
        this.activityDescription = activityDescription;
        this.needToChangeScene = needToChangeScene;
        this.sceneName = sceneName;
    }

    // 切换场景
    public virtual void GoToScene()
    {
        ClassManager.instance.ChangeScene(sceneName);
    }

    public virtual void Start()
    {
        GoToScene();
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void End()
    {

    }
}
