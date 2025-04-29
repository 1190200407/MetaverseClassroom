using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GamePanel : BasePanel
{
    private Text networkStatusTxt;
    private Button muteBtn;
    private Button unmuteBtn;

    public GamePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        networkStatusTxt = UIMethods.instance.GetOrAddComponentInChild<Text>(ActiveObj, "NetworkStatusText");
        muteBtn = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "MuteButton");
        unmuteBtn = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "UnmuteButton");
        muteBtn.onClick.AddListener(OnMuteBtnClick);
        unmuteBtn.onClick.AddListener(OnUnmuteBtnClick);
        UpdateMuteState();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.instance.Push(new PausePanel(new UIType("Panels/PausePanel", "PausePanel")));
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UIManager.instance.Push(new PPTPanel(new UIType("Panels/PPTPanel", "PPTPanel")));
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            VoiceManager.instance.IsSelfMute = !VoiceManager.instance.IsSelfMute;
            UpdateMuteState();
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

    private void OnMuteBtnClick()
    {
        VoiceManager.instance.IsSelfMute = true;
        UpdateMuteState();
    }

    private void OnUnmuteBtnClick()
    {
        VoiceManager.instance.IsSelfMute = false;
        UpdateMuteState();
    }

    public void UpdateMuteState()
    {
        if (VoiceManager.instance.IsMute)
        {
            muteBtn.gameObject.SetActive(false);
            unmuteBtn.gameObject.SetActive(true);
        }
        else
        {
            muteBtn.gameObject.SetActive(true);
            unmuteBtn.gameObject.SetActive(false);
        }
    }
}
