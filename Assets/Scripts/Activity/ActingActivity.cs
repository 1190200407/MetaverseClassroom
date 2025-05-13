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
        base.Start();

        //TODO 读取JSON
        Debug.Log("开始表演活动");
        ClassManager.instance.roleList.Clear();
        ClassManager.instance.roleList.Add("role1", "服务员");
        ClassManager.instance.roleList.Add("role2", "顾客1");
        ClassManager.instance.roleList.Add("role3", "顾客2");
        ClassManager.instance.roleOccupied.Clear();

        actionTree = new ActionTree();

        ActionTreeCompositeNode node1 = new ActionTreeCompositeNode();
        node1.id = 1;
        node1.type = ActionTreeCompositeType.Sequence;
        node1.children = new List<IActionTreeNode>();

        ActionTreeCompositeNode node2 = new ActionTreeCompositeNode();
        node2.id = 2;
        node2.type = ActionTreeCompositeType.Parallel;
        node2.children = new List<IActionTreeNode>();

        ActionTreeCompositeNode node3 = new ActionTreeCompositeNode();
        node3.id = 3;
        node3.type = ActionTreeCompositeType.Sequence;
        node3.children = new List<IActionTreeNode>();

        ActionTreeLeafNode node4 = new ActionTreeLeafNode();
        node4.id = 4;
        node4.actionMethodName = "MoveTo";
        node4.role = "role1";
        node4.actionParams = new Dictionary<string, object>
        {
            { "position", new Vector3(0, 0, 0) }
        };

        ActionTreeLeafNode node5 = new ActionTreeLeafNode();
        node5.id = 5;
        node5.actionMethodName = "MoveTo";
        node5.role = "role2";
        node5.actionParams = new Dictionary<string, object>
        {
            { "position", new Vector3(0, 0, 0) }
        };

        ActionTreeLeafNode node6 = new ActionTreeLeafNode();
        node6.id = 6;
        node6.actionMethodName = "Speak";
        node6.role = "role2";
        node6.actionParams = new Dictionary<string, object>
        {
            { "text", "服务员,我要一个汉堡" }
        };

        ActionTreeLeafNode node7 = new ActionTreeLeafNode();
        node7.id = 7;
        node7.actionMethodName = "Speak";
        node7.role = "role1";
        node7.actionParams = new Dictionary<string, object>
        {
            { "text", "好的,请稍等" }
        };
        
        node2.children.Add(node4);
        node2.children.Add(node5);
        node3.children.Add(node6);
        node3.children.Add(node7);

        node1.children.Add(node2);
        node1.children.Add(node3);

        actionTree.root = node1;
        actionTree.leafNodes = new Dictionary<int, ActionTreeLeafNode>
        {
            { 4, node4 },
            { 5, node5 },
            { 6, node6 },
            { 7, node7 }
        };
    }

    public override void End()
    {
        isActionTreeExecuting = false;
        EventHandler.Trigger(new EndActivityEvent());
    }
}


