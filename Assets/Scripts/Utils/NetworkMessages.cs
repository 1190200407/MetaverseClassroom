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

