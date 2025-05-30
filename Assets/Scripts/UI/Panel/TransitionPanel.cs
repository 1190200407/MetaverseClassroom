using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPanel : BasePanel
{
    private Animator anim;

    public TransitionPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        anim = ActiveObj.GetComponent<Animator>();
        anim.Play("Open");
        PlayerManager.localPlayer.playerController.enabled = false;
        InteractionManager.instance.RaycastClosed = true;
    }

    public void OnCloseEnd(TransitionCloseEndEvent @event)
    {
        UIManager.instance.Pop(false);
        PlayerManager.localPlayer.playerController.enabled = true;
        InteractionManager.instance.RaycastClosed = false;
    }

    public void OnOpenEnd(TransitionOpenEndEvent @event)
    {
        //TODO开始换场景
        anim.Play("Close");
        ClassManager.instance.OnSceneTransitionEnd();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        EventHandler.Register<TransitionCloseEndEvent>(OnCloseEnd);
        EventHandler.Register<TransitionOpenEndEvent>(OnOpenEnd);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EventHandler.Unregister<TransitionCloseEndEvent>(OnCloseEnd);
        EventHandler.Unregister<TransitionOpenEndEvent>(OnOpenEnd);
    }
}
