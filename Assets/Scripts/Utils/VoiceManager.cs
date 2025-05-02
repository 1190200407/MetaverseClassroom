using System;
using System.Collections;
using System.Collections.Generic;
using Dissonance;
using UnityEngine;

public class VoiceManager : UnitySingleton<VoiceManager>
{
    public DissonanceComms dissonanceComms;
    public VoiceBroadcastTrigger voiceBroadcastTrigger;
    public VoiceReceiptTrigger voiceReceiptTrigger;

    // 禁音
    private bool isSelfMute = false;
    private bool isForcelyMute = false;

    public bool IsMute => isSelfMute || isForcelyMute;
    public bool IsForcelyMute
    {
        get => isForcelyMute;
        set
        {
            isForcelyMute = value;
            OnMuteStateChange();
        }
    }

    public bool IsSelfMute
    {
        get => isSelfMute;
        set
        {
            isSelfMute = value;
            OnMuteStateChange();
        }
    }

    public void OnMuteStateChange()
    {
        if (IsMute)
        {
            dissonanceComms.IsMuted = true;
        }
        else
        {
            dissonanceComms.IsMuted = false;
        }
    }
}
