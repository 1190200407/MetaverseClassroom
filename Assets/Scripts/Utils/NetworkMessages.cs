using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public struct ChangeSceneMessage : NetworkMessage
{
    public string sceneName;

    public ChangeSceneMessage(string sceneName)
    {
        this.sceneName = sceneName;
    }
}
