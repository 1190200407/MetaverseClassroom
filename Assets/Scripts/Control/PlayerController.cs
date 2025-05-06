using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mirror;
using System.IO;

public class PlayerController : MonoBehaviour
{
    public PlayerManager playerManager;
    public Camera playerCamera = null;
    public Animator animator => playerManager.animator;
    public Rigidbody rb;

    public float cameraSensitivity;

    public float moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        initialize();
    }

    protected virtual void initialize()
    {
    }

    public virtual void ResetTransform(Transform sceneTransform)
    {
        transform.position = sceneTransform.position;
        transform.rotation = Quaternion.identity;
    }
    
    #region 玩家数据
    
    protected static PlayerData playerData; //用户的基本参数
    private static string savePath => Path.Combine(Application.persistentDataPath, "playerData.json");

    public static PlayerData GetData()
    {
        return playerData;
    }

    /// <summary>
    /// 保存用户数据
    /// </summary>
    public static void SaveData()
    {
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(savePath, json);
        //Debug.Log("玩家数据已保存：" + savePath);
    }
    
    /// <summary>
    /// 加载用户数据
    /// </summary>
    /// <returns></returns>
    public static PlayerData LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            Debug.LogWarning("未找到存档文件，使用默认数据");
            return new PlayerData(50,50,50); // 返回默认数据
        }
    }
    
    /// <summary>
    /// 初始化用户的基本参数
    /// </summary>
    protected virtual void InitializeData()
    {
        if (playerManager.isLocalPlayer)
        {
            playerData = LoadData();
            //Debug.Log("玩家数据已加载：" + playerData.moveSpeed + " " + playerData.cameraSensitivity + " " + playerData.volume);
        }
    }

    
    /// <summary>
    /// 修改当前读取的玩家参数
    /// </summary>
    /// <param name="data"></param>
    protected void changePlayerData(PlayerChangeDataEvent @event)
    {
        changeData(@event.data);
    }

    protected virtual void changeData(PlayerData data)
    {
        if (playerManager.isLocalPlayer)
        {
            playerData = data;
            cameraSensitivity = data.cameraSensitivity;
            moveSpeed = data.moveSpeed;
            Debug.Log("玩家数据已保存：" + playerData.moveSpeed + " " + playerData.cameraSensitivity + " " + playerData.volume);
        }
    }
    #endregion
}
