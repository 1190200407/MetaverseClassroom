using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using Unity.VisualScripting;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static List<PlayerController> allPlayers = new List<PlayerController>();
    public static PlayerController localPlayer;

    public string playerName; //用户的名称（ID）
    
    protected static PlayerData playerData; //用户的基本参数
    
    public Camera playerCamera = null;
    public Animator anim;

    public Transform handTransform;

    public Rigidbody rb;
    protected bool isSitting = false;
    public bool IsSitting
    {
        get { return isSitting; }
        set
        {
            if (photonView.IsMine)
            {
                photonView.RPC("SetSittingState", RpcTarget.AllBuffered, value);
            }
        }
    }

    protected string currentScene;
    public string CurrentScene
    {
        get { return currentScene; }
        set
        {
            if (photonView.IsMine)
            {
                //其他客户端同步状态
                photonView.RPC("SetCurrentScene", RpcTarget.OthersBuffered, value);

                transform.position = SceneLoader.instance.PathToSceneObject[value].transform.position;
                //切换场景后，更新player的状态
                foreach (var player in allPlayers)
                {
                    if (player == this || player.currentScene == value)
                    {
                        player.rb.isKinematic = isSitting;
                    }
                    else
                    {
                        player.rb.isKinematic = true;
                    }
                }
            }
        }
    }
    
    private bool isStudent;
    
    public bool IsStudent
    {
        get { return isStudent; }
        set
        {
            isStudent = value;
        }
    }
    protected PermissionHolder permissionHolder = new PermissionHolder();

    protected GameObject redDot;
    protected bool isShowingRedDot = false;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            localPlayer = this;
            playerName = PlayerPrefs.GetString("NickName"); //标识用户姓名
        }
        else
        {
            playerName = photonView.Owner.NickName;
        }
        
        allPlayers.Add(this);
        
        //将自己的名字牌和模型隐藏
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        isStudent = PlayerPrefs.GetInt("IsStudent", 1) == 1;
    }

    protected virtual void Start()
    {
        initialize();
    }

    public static PlayerData GetData()
    {
        return playerData;
    }
    
    /// <summary>
    /// 初始化用户的基本参数
    /// </summary>
    protected virtual void initialize()
    {
        if (photonView.IsMine)
        {
            playerData = PlayerManager.LoadData();
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
        if (photonView.IsMine)
            playerData = data;
    }
    
    private void OnDestroy()
    {
        allPlayers.Remove(this);
        // 取消注册事件处理器
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void JoinRoom()
    {
        IsStudent = PlayerPrefs.GetInt("IsStudent", 1) == 1;
        CurrentScene = ClassManager.instance.currentScene;
        EventHandler.Register<PlayerChangeDataEvent>(changePlayerData); //注册用户修改基本参数事件
        EventHandler.Trigger(new PlayerJoinRoomEvent() { player = this });

        // 初始化时隐藏红点
        CreateRedDot();
        isShowingRedDot = false;

        // 获取权限, 并同步给其他客户端
        GetPermission();
        
        // 注册事件处理器
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void LeftRoom()
    {
        EventHandler.Unregister<PlayerChangeDataEvent>(changePlayerData); //注销用户修改基本参数事件
        EventHandler.Trigger(new PlayerLeftRoomEvent() { player = this });
        PlayerManager.SaveData(playerData);
        DestroyRedDot();

        // 注销事件处理器
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        // 收到其他客户端的场景切换请求
        if (photonEvent.Code == EventCodes.ChangeSceneEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            int[] targetViewIDs = (int[])data[0];
            string sceneName = (string)data[1];
            Debug.Log("收到场景切换请求，场景名：" + sceneName);

            // 检查当前玩家是否在目标列表中
            foreach (int viewID in targetViewIDs)
            {
                if (photonView.ViewID == viewID)
                {
                    // 在当前客户端上主动调用ChangeScene
                    ChangeScene(sceneName);
                    break;
                }
            }
        }
    }

    private void GetPermission()
    {
        // 学生有聊天和红点的权限
        if (isStudent)
        {
            permissionHolder.SetPermission(Permission.Chat | Permission.RedDot);
        }
        // 老师有所有权限
        else
        {
            permissionHolder.SetAllPermission();
        }
        // 同步权限给其他客户端
        photonView.RPC("SetPermission", RpcTarget.OthersBuffered, permissionHolder.GetPermission());
    }

    public bool HavePermission(Permission permission)
    {
        return permissionHolder.HasPermission(permission);
    }

    private void CreateRedDot()
    {
        if (photonView.IsMine)
        {
            // 使用 PhotonNetwork.Instantiate 来实例化红点
            redDot = PhotonNetwork.Instantiate("RedDot", Vector3.down * 10000f, Quaternion.identity);
        }
    }

    private void DestroyRedDot()
    {
        if (redDot != null)
        {
            PhotonNetwork.Destroy(redDot);
            redDot = null;
        }
    }

    [PunRPC]
    public void SetSittingState(bool sitting)
    {
        isSitting = sitting;

        rb.isKinematic = sitting;
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
    }

    public void ChangeScene(string sceneName)
    {
        // 如果是本地玩家，直接调用场景切换
        if (photonView.IsMine)
        {
            ClassManager.instance.StartSceneTransition(sceneName);
        }
    }

    [PunRPC]
    public void SetCurrentScene(string sceneName)
    {
        currentScene = sceneName;

        if (sceneName != ClassManager.instance.currentScene)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = isSitting;
        }
    }

    [PunRPC]
    public void SetPermission(int permission)
    {
        permissionHolder.SetPermission((Permission)permission);
    }
}
