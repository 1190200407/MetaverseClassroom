using UnityEngine;

public struct StartLocalPlayerEvent {}

public struct ResumeEvent {}

public struct ReloadEvent {}

public struct SceneLoadedEvent {}

public struct ChangeSlideEvent
{
    public int changeNum;
}

public struct ChangePPTPathEvent
{
    public string pptPath;
}

public struct PPTPathUpdatedEvent
{
    public string newPath;
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

#region 拾取物品相关
public struct ResetItemEvent
{
    public uint holderId;
    public string elementId;
}
public struct PickItemEvent
{
    public uint holderId;
    public string elementId;
    public string itemKey;
}   
public struct DropItemEvent
{
    public uint holderId;
    public string elementId;
    public Vector3 position;
}

public struct ResetItemCallback
{
    public uint holderId;
}
public struct PickItemCallback
{
    public uint holderId;
    public string itemKey;
}   
public struct DropItemCallback
{
    public uint holderId;
    public Vector3 position;
}

public struct UIChangeEvent
{
    public uint holderId;
    public string text;
}
#endregion

public struct InteractionEvent
{
    public string interactType;
    public string interactWay;
    public string elementId;
}

public struct NPCInteractionEvent
{
    public string npcName;
    public string interactionType;
    public string interactWay;
    public string elementId;
}