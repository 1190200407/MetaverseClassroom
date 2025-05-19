using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Actions;

public class ActionTree
{
    public IActionTreeNode root;

    // 记录所有叶子节点，用于在执行过程中查找
    public Dictionary<int, ActionTreeLeafNode> leafNodes;

    public IEnumerator Execute()
    {
        yield return root.ExecuteCoroutine();
    }
}

public interface IActionTreeNode
{
    public int id { get; set; }
    public bool isExecuting { get; set; }

    public IEnumerator ExecuteCoroutine();
}

// 组合节点，用于表示子节点是顺序或者并行执行的顺序
public class ActionTreeCompositeNode : IActionTreeNode
{
    public int id { get; set; }
    public List<IActionTreeNode> children;
    public ActionTreeCompositeType type;
    public bool isExecuting { get; set; } // 用于并行执行时，判断所有子节点是否执行完成

    public IEnumerator ExecuteCoroutine()
    {
        isExecuting = true;

        if (type == ActionTreeCompositeType.Sequence)
        {
            foreach (var child in children)
            {
                yield return child.ExecuteCoroutine();
            }
        }
        else if (type == ActionTreeCompositeType.Parallel)
        {
            // 启动所有子节点
            foreach (var child in children)
            {
                child.isExecuting = true;
                ClassManager.instance.StartCoroutine(ExecuteChildCoroutine(child));
            }

            // 等待所有子节点执行完成
            var childrenCopy = children;
            yield return new WaitUntil(() => childrenCopy.All(child => !child.isExecuting));
        }

        isExecuting = false;
    }

    private static IEnumerator ExecuteChildCoroutine(IActionTreeNode child)
    {
        yield return child.ExecuteCoroutine();
        child.isExecuting = false;
    }
}

public enum ActionTreeCompositeType
{
    Sequence,
    Parallel,
    Selector
}

// 叶子节点，用于表示一个具体的动作
public class ActionTreeLeafNode : IActionTreeNode
{
    public int id { get; set; }
    public bool isExecuting { get; set; }
    public string name;
    public string role;
    public float delayTime;
    public string actionMethodName;
    public Dictionary<string, string> actionParams;
    public Dictionary<string, string> errorHandling;

    // 用于同步行为树，指定角色执行完该节点后，通知其他角色该节点执行完成
    public bool accomplished;

    public IEnumerator ExecuteCoroutine()
    {
        isExecuting = true;

        // 获取actionMethodName对应的ActionMethod
        Type actionMethodType = Type.GetType("Actions." + actionMethodName);

        if (actionMethodType == null)
        {
            Debug.LogError($"Action method type {actionMethodName} not found");
            isExecuting = false;
            yield break;
        }

        if (!actionMethodType.IsSubclassOf(typeof(BaseActionMethod)))
        {
            Debug.LogError($"Action method type {actionMethodName} is not a subclass of BaseActionMethod");
            isExecuting = false;
            yield break;
        }

        // delayTime单位为ms
        yield return new WaitForSeconds(delayTime / 1000f);

        BaseActionMethod actionMethod = (BaseActionMethod)Activator.CreateInstance(actionMethodType);
        actionMethod.actionNode = this;
        actionMethod.Initialize();

        yield return actionMethod.ExecuteCoroutine();

        isExecuting = false;
    }
}

public struct ActionData
{
    public float delayTime;
    public string role;
    public string actionMethodName;
    public Dictionary<string, string> actionParams;
    public Dictionary<string, string> errorHandling;
}