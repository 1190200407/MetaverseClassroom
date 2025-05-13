using System;
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

    // 活动的行为树
    public bool isActionTreeExecuting = false;
    public ActionTree actionTree;
    
    private String jsonFilePath; //Json文件存储路径

    public BaseActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName)
    {
        this.activityName = activityName;
        this.activityDescription = activityDescription;
        this.needToChangeScene = needToChangeScene;
        this.sceneName = sceneName;
    }
    
    /// <summary>
    /// 转译Json文件获取有效信息
    /// </summary>
    /// <param name="JsonPath"></param>Json文件的路径
    /// <returns></returns>
    private void ConvertJson(String JsonPath)
    {
        //TODO：识别Json文件中的Event块，提取角色并初始化行为树
    }

    // 切换场景
    public virtual void GoToScene()
    {
        ClassManager.instance.ChangeScene(sceneName);
    }

    public virtual void Start()
    {
        ConvertJson(jsonFilePath);
        GoToScene();
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void End()
    {

    }
}
