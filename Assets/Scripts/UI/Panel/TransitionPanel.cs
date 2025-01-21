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
        PlayerController.localPlayer.enabled = false;
    }

    public void OnCloseEnd(TransitionCloseEndEvent @event)
    {
        UIManager.instance.Pop(false);
        PlayerController.localPlayer.enabled = true;
    }

    public void OnOpenEnd(TransitionOpenEndEvent @event)
    {
        //TODO开始换场景
        anim.Play("Close");
        ClassManager.instance.ChangeScene();
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
