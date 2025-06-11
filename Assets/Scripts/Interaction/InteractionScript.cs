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
    /// 当本地玩家开始时响应的函数
    /// </summary>
    public virtual void OnStartLocalPlayer()
    {
    }

    /// <summary>
    /// 当射线射中时响应的函数
    /// 默认会发送InteractionEvent事件, 如果需要忽略, 则不要加base.OnHoverEnter()
    /// </summary>
    public virtual void OnHoverEnter()
    {
        EventHandler.Trigger(new InteractionEvent() { interactType = element.interactType, interactWay = "HoverEnter", elementId = element.id });
    }

    /// <summary>
    /// 当射线离开时响应的函数
    /// 默认会发送InteractionEvent事件, 如果需要忽略, 则不要加base.OnHoverExit()
    /// </summary>
    public virtual void OnHoverExit()
    {
        EventHandler.Trigger(new InteractionEvent() { interactType = element.interactType, interactWay = "HoverExit", elementId = element.id });
    }

    /// <summary>
    /// 当射线按下时响应的函数
    /// 默认会发送InteractionEvent事件, 如果需要忽略, 则不要加base.OnSelectEnter()
    /// </summary>
    public virtual void OnSelectEnter()
    {
        EventHandler.Trigger(new InteractionEvent() { interactType = element.interactType, interactWay = "SelectEnter", elementId = element.id });
    }

    /// <summary>
    /// 当射线松开时响应的函数
    /// 默认会发送InteractionEvent事件, 如果需要忽略, 则不要加base.OnSelectExit()
    /// </summary>
    public virtual void OnSelectExit()
    {
        EventHandler.Trigger(new InteractionEvent() { interactType = element.interactType, interactWay = "SelectExit", elementId = element.id });
    }

    /// <summary>
    /// 操控NPC交互时响应的函数
    /// </summary>
    public virtual void OnNPCInteract(NPCManager npcManager, string interactWay)
    {
        EventHandler.Trigger(new NPCInteractionEvent()
        {
            npcName = npcManager.NPCName,
            interactionType = element.interactType,
            elementId = element.id
        });
    }
}