using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror;

/// <summary>
/// 所有可交互脚本的基类
/// </summary>
public class InteractionScript
{
    public SceneElement element;

    public virtual void OnEnable()
    {
    }

    public virtual void OnDisable()
    {
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    public virtual void Init(SceneElement element)
    {
        this.element = element;
    }

    /// <summary>
    /// 当射线射中时响应的函数
    /// </summary>
    public virtual void OnHoverEnter()
    {
    }

    /// <summary>
    /// 当射线离开时响应的函数
    /// </summary>
    public virtual void OnHoverExit()
    {
    }

    /// <summary>
    /// 当射线按下时响应的函数
    /// </summary>
    public virtual void OnSelectEnter()
    {
    }

    /// <summary>
    /// 当射线松开时响应的函数
    /// </summary>
    public virtual void OnSelectExit()
    {
    }
}