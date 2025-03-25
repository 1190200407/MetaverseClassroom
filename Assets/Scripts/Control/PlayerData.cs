using System;
using UnityEngine;

/// <summary>
/// 用户的一些基本参数
/// </summary>
[Serializable]
public struct PlayerData
{
    [Range(0,100)]
    public float moveSpeed; //移动速度
    
    [Range(0,100)]
    public float cameraSensitivity; //相机灵敏度
    
    [Range(0,100)]
    public float volume; //音量

    public PlayerData(float moveSpeed, float cameraSensitivity, float volume)
    {
        this.moveSpeed = moveSpeed;
        this.cameraSensitivity = cameraSensitivity;
        this.volume = volume;
    }
}