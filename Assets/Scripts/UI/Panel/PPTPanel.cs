using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PPTPanel : BasePanel
{
    public Image pptImage;

    public PPTPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        pptImage = UIMethods.instance.GetOrAddComponentInChild<Image>(ActiveObj, "PPTImage");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        pptImage.sprite = ClassManager.instance.whiteboard.screen.sprite;

        // 关闭面板
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.instance.Pop(false);
            UIManager.instance.Push(new PausePanel(new UIType("Panels/PausePanel", "PausePanel")));
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            UIManager.instance.Pop(false);
        }
    }
}
