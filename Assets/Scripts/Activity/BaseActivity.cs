using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;
using LitJson;

// 教学活动基类, 之后扩展
public class BaseActivity
{
    public string activityName;
    public string activityDescription;

    public List<PlayerController> includedPlayers;

    public bool needToChangeScene;
    public string sceneName;
    public string jsonFilePath;

    // 活动的行为树
    public bool isActionTreeExecuting = false;
    public ActionTree actionTree;

    public BaseActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName, string jsonFilePath)
    {
        this.activityName = activityName;
        this.activityDescription = activityDescription;
        this.needToChangeScene = needToChangeScene;
        this.sceneName = sceneName;
        this.jsonFilePath = jsonFilePath;
    }
    
    /// <summary>
    /// 转译Json文件获取有效信息
    /// </summary>
    protected void ConvertJson()
    {
        // 如果json文件不存在，则返回
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(jsonFilePath);
        if (jsonTextAsset == null)
        {
            Debug.LogError("Json文件不存在: " + jsonFilePath);
            return;
        }

        //识别Json文件中的Script块，提取角色并初始化行为树
        JsonData jsonData = JsonMapper.ToObject(jsonTextAsset.text);

        // 提取角色
        ClassManager.instance.roleList.Clear();
        foreach (JsonData role in jsonData["roles"])
        {
            string roleId = role["roleId"].ToString();
            string roleName = role["name"].ToString();
            string roleType = role["type"].ToString(); // 暂时没用

            // 初始化角色
            ClassManager.instance.roleList.Add(roleId, roleName);
        }
        
        // 提取动作
        Dictionary<int, ActionData> actionList = new Dictionary<int, ActionData>();
        foreach (JsonData action in jsonData["actions"])
        {
            string actionId = action["actionId"].ToString();
            string actionMethodName = action["actionMethod"].ToString();
            string delayTime = action["delayTime"].ToString();
            string roleId = action["roleId"].ToString();
            JsonData parameters = action["parameters"];
            JsonData errorHandling = action["errorHandling"];

            // 初始化动作
            ActionData actionData = new ActionData() {
                actionMethodName = actionMethodName,
                delayTime = int.Parse(delayTime),
                role = roleId,
                actionParams = new Dictionary<string, string>(),
                errorHandling = new Dictionary<string, string>()
            };

            // 初始化参数
            foreach (string key in parameters.Keys)
            {
                string parameterName = key;
                string parameterValue = parameters[key].ToString();
                actionData.actionParams.Add(parameterName, parameterValue);
            }

            // 初始化错误处理
            foreach (string key in errorHandling.Keys)
            {
                string errorHandlingName = key;
                string errorHandlingValue = errorHandling[key].ToString();
                actionData.errorHandling.Add(errorHandlingName, errorHandlingValue);
            }
            
            actionList.Add(int.Parse(actionId), actionData);
        }

        // 初始化行为树
        actionTree = new ActionTree();
        actionTree.leafNodes = new Dictionary<int, ActionTreeLeafNode>();
        Dictionary<int, ActionTreeCompositeNode> actionTreeNodes = new Dictionary<int, ActionTreeCompositeNode>();
        
        foreach (JsonData treeNode in jsonData["script"]["treeNodes"])
        {
            int nodeId = int.Parse(treeNode["nodeId"].ToString());
            int parentNodeId = int.Parse(treeNode["parentNodeId"].ToString());
            string isLeafNode = treeNode["isLeafNode"].ToString();

            IActionTreeNode actionTreeNode;
            
            // 初始化叶子节点
            if (isLeafNode == "true")
            {
                ActionTreeLeafNode actionTreeLeafNode = new ActionTreeLeafNode();
                actionTreeLeafNode.id = nodeId;

                string actionId = treeNode["actionId"].ToString();
                ActionData actionData = actionList[int.Parse(actionId)];
                actionTreeLeafNode.actionMethodName = actionData.actionMethodName;
                actionTreeLeafNode.delayTime = actionData.delayTime;
                actionTreeLeafNode.role = actionData.role;
                actionTreeLeafNode.actionParams = actionData.actionParams;
                actionTreeLeafNode.errorHandling = actionData.errorHandling;

                actionTreeNode = actionTreeLeafNode;
                actionTree.leafNodes.Add(nodeId, actionTreeLeafNode);
            }
            // 初始化组合节点
            else
            {
                ActionTreeCompositeNode actionTreeCompositeNode = new ActionTreeCompositeNode();
                actionTreeCompositeNode.id = nodeId;
                
                string compositeType = treeNode["compositeType"].ToString();
                actionTreeCompositeNode.type = (ActionTreeCompositeType)Enum.Parse(typeof(ActionTreeCompositeType), compositeType);
                actionTreeCompositeNode.children = new List<IActionTreeNode>();

                actionTreeNodes.Add(nodeId, actionTreeCompositeNode);
                actionTreeNode = actionTreeCompositeNode;
            }

            // 初始化父节点
            if (parentNodeId != -1)
            {
                actionTreeNodes[parentNodeId].children.Add(actionTreeNode);
            }
            else
            {
                actionTree.root = actionTreeNode;
            }
        }
    }

    // 切换场景
    public virtual void GoToScene()
    {
        ClassManager.instance.ChangeScene(sceneName);
    }

    public virtual void Start()
    {
        ConvertJson();
        GoToScene();
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void End()
    {

    }
}
