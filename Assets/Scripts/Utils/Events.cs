using UnityEngine;

public struct StartLocalPlayerEvent {}

public struct ResumeEvent {}

public struct ReloadEvent {}

public struct SceneLoadedEvent {}

public struct ChangeSlideEvent
{
    public int changeNum;
}

public struct BeforeChangeSceneEvent
{
    public string sceneName;
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

public struct RoleOccupiedChangeEvent
{
    public string roleId;
    public int netId;
}

public struct PlayerChangeDataEvent
{
    public PlayerData data;
}

public struct UIHighLightEvent
{
    public string id;
    public bool isHighlighted;
}
public struct TransitionOpenEndEvent {}

public struct TransitionCloseEndEvent {}

public struct GrabResetEvent {}

public struct EndActivityEvent {}

public struct NewTaskEvent
{
    public uint netId;
    public string taskDescription;
    public int actionNodeId;
}

public struct TaskCompleteEvent
{
    public uint netId;
    public int actionNodeId;
}

