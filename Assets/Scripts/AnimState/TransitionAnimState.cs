using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionAnimState : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animatorStateInfo.IsName("Close"))
            EventHandler.Trigger(new TransitionCloseEndEvent());
        if (animatorStateInfo.IsName("Open"))
            EventHandler.Trigger(new TransitionOpenEndEvent());
    }
}
