using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PausePanel : BasePanel
{
    public PausePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            UIManager.instance.Pop(false);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        InteractionManager.instance.RaycastClosed = true;
        PlayerController.localPlayer.enabled = false;
    }

    public override void OnDisable() {
        base.OnDisable();
        InteractionManager.instance.RaycastClosed = false;
        PlayerController.localPlayer.enabled = true;
    }
}
