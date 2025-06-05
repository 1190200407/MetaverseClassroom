using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public enum NetworkMessageType
{
    StartActivity,
    EndActivity,
    StartActionTree,
    TaskComplete,
    ItemPickup,
    ItemDrop,
    ItemReset,
    ElementInteract
}

public struct NetworkMessageClassroom : NetworkMessage
{
    public NetworkMessageType messageType;
    // 以json格式传输数据
    public string messageJson;
}

[Serializable]
public struct StartActivityMessageData
{
    public int activityIndex;
}

[Serializable]
public struct TaskCompleteMessageData
{
    public int actionNodeId;
    public uint netId;
}

[Serializable]
public struct ItemPickMessageData
{
    public uint holderId;
    public string itemKey;
}

[Serializable]
public struct ItemDropMessageData
{
    public uint holderId;
    public Vector3 position;
}

[Serializable]
public struct ItemResetMessageData
{
    public uint holderId;
}

[Serializable]
public struct ElementInteractMessageData
{
    public int netId;
    public string elementId;
}

