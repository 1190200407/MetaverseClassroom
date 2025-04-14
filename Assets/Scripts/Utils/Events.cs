using Photon.Realtime;
using UnityEngine;

public struct StartLocalPlayerEvent {}

public struct ResumeEvent {}

public struct ReloadEvent {}

public struct ChangeSlideEvent
{
    public int changeNum;
}

public struct ChangeSceneEvent
{
    public int[] includePlayers;// 目前没用
    public string sceneName;
}

public struct RoomPropertyChangeEvent
{
    public string key;
    public string value;
}

public struct PlayerChangeDataEvent
{
    public PlayerData data;
}
public struct TransitionOpenEndEvent {}

public struct TransitionCloseEndEvent {}
