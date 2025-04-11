using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GamePanel : BasePanel
{
    private Text networkStatusTxt;

    public GamePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        networkStatusTxt = UIMethods.instance.GetOrAddComponentInChild<Text>(ActiveObj, "NetworkStatusText");
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.instance.Push(new PausePanel(new UIType("Panels/PausePanel", "PausePanel")));
        }

        // 获取网络状态
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            networkStatusTxt.text = $"<b>Disconnected</b>";
        }
        else if (NetworkServer.active && NetworkClient.active)
        {
            // host mode
            networkStatusTxt.text = $"<b>Host</b>: running via {Transport.active}";
        }
        else if (NetworkServer.active)
        {
            // server only
            networkStatusTxt.text = $"<b>Server</b>: running via {Transport.active}";
        }
        else if (NetworkClient.isConnected)
        {
            // client only
            networkStatusTxt.text = $"<b>Client</b>: connected to {NetworkManagerClassroom.singleton.networkAddress} via {Transport.active}";
        }
    }
}
