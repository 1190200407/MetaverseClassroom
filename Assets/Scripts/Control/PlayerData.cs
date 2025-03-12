using System;
using UnityEngine;

/// <summary>
/// 用户的一些基本参数
/// </summary>
[Serializable]
public class PlayerData
{
    [Range(0,100)]
    public float moveSpeed = 50; //移动速度
    
    [Range(0,100)]
    public float cameraSensitivity = 50; //相机灵敏度
    
    [Range(0,100)]
    public float volume = 50; //音量
    
}