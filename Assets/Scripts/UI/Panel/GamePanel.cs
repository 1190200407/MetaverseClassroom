using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GamePanel : BasePanel
{
    private Text pingTxt;

    public GamePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        pingTxt = UIMethods.instance.GetOrAddComponentInChild<Text>(ActiveObj, "PingText");
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.instance.Push(new PausePanel(new UIType("Panels/PausePanel", "PausePanel")));
        }
        // 获取ping值
        pingTxt.text = $"Ping:{NetworkTime.rtt}ms";
    }
}
