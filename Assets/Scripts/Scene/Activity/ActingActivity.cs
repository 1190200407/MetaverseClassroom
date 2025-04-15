using System.Collections.Generic;
using UnityEngine;
using Mirror;
// 表演活动，需要玩家进行表演，并根据表演的完成度进行评分
public class ActingActivity : BaseActivity
{
    public ActingActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName) : base(activityName, activityDescription, needToChangeScene, sceneName)
    {
    }

    public override void Start()
    {
        GoToScene();
    }

    public override void End()
    {
        BackToClassroom();
    }
}


