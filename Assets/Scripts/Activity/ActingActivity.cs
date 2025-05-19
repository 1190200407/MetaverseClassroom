using System.Collections.Generic;
using UnityEngine;
using Mirror;
// 表演活动，需要玩家进行表演，并根据表演的完成度进行评分
public class ActingActivity : BaseActivity
{
    public ActingActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName, string jsonFilePath) : base(activityName, activityDescription, needToChangeScene, sceneName, jsonFilePath)
    {
    }

    public override void Start()
    {
        Debug.Log("开始表演活动");
        base.Start();
    }

    public override void End()
    {
        isActionTreeExecuting = false;
        EventHandler.Trigger(new EndActivityEvent());
    }
}


