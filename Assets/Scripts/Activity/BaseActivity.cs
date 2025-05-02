using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Scene.Action;

// 教学活动基类, 之后扩展
public class BaseActivity
{
    public string activityName;
    public string activityDescription;

    public List<PlayerController> includedPlayers;

    public bool needToChangeScene;
    public string sceneName;
    public List<BaseExercise> exercises = new List<BaseExercise>(); //课程片段中包含的所有活动
    
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
        //TODO：识别Json文件中的Exercise块，提取角色并初始化行为树
    }
    
    /// <summary>
    /// 初始化场景中所有的Exercise空间
    /// </summary>
    private void InitializeExerciseZone()
    {
        //TODO：根据exercises列表，在场景中创建物体挂载Exercise脚本，并初始化相关参数
        exercises.Add(new BaseExercise());
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

    public virtual void Start()
    {
        ConvertJson(jsonFilePath);
        InitializeExerciseZone();
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void End()
    {

    }
}
