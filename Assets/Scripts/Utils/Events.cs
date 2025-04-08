using Photon.Realtime;
using UnityEngine;

public struct ResumeEvent {}

public struct ReloadEvent {}

public struct ChangeSlideEvent
{
    public int changeNum;
}

public struct ChangeSceneEvent
{
    public int[] includePlayers;
    public string sceneName;
}

public struct PlayerJoinRoomEvent
{
    public PlayerManager player;
}

public struct PlayerLeftRoomEvent
{
    public PlayerManager player;
}

public struct PlayerChangeDataEvent
{
    public PlayerData data;
}
public struct TransitionOpenEndEvent {}

public struct TransitionCloseEndEvent {}
