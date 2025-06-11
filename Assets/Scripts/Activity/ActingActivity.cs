using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using LitJson;

// 表演活动，需要玩家进行表演，并根据表演的完成度进行评分
public class ActingActivity : BaseActivity
{
    private Delegate onStartActionTree;
    public string jsonFilePath;

    public ActingActivity(string activityName, string activityDescription, bool needToChangeScene, string sceneName, string jsonFilePath) : base(activityName, activityDescription, needToChangeScene, sceneName)
    {
        this.jsonFilePath = jsonFilePath;
    }

    public override void Start()
    {
        Debug.Log("开始表演活动");
        base.Start();

        // 转换Json文件， 获取剧本信息
        ConvertJson();

        onStartActionTree = new Action(OnStartActionTree);
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.StartActionTree, onStartActionTree);
    }

    public override void End()
    {
        // 服务器取消选角
        if (NetworkServer.active)
        {
            foreach (var role in ClassManager.instance.roleList)
            {
                ClassManager.instance.CommandSetRoleOccupied(role.Key, 0);
            }
        }
        // 移除所有NPC
        ClassManager.instance.RemoveAllNPC();

        isActionTreeExecuting = false;
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.StartActionTree, onStartActionTree);
    }

    public void OnStartActionTree()
    {
        // 添加NPC 
        foreach (var role in ClassManager.instance.roleList)
        {
            if (ClassManager.instance.roleOccupied[role.Key] == -1)
                ClassManager.instance.AddNPC(role.Key);
        }

        // 开始执行行为树
        isActionTreeExecuting = true;
        ClassManager.instance.StartCoroutine(actionTree.Execute());
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
}


