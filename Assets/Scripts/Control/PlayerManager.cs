using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    #region 玩家列表
    public static List<PlayerManager> allPlayers = new List<PlayerManager>();
    public static PlayerManager localPlayer;
    public new bool isLocalPlayer => base.isLocalPlayer;
    public string playerName; //用户的名称（ID）
    #endregion

    #region 玩家控制器
    public PlayerController playerController;
    public Animator animator;
    public Rigidbody rb;
    #endregion

    #region 玩家UI
    public NameText nameText;

    private string characterName;
    public string CharacterName
    {
        get 
        {
            return characterName;
        }
        set
        {
            characterName = value;
            Transform playerVisual = transform.Find("PlayerVisual");
            for (int i = 0; i < playerVisual.childCount; i++)
            {
                Transform child = playerVisual.GetChild(i);
                if (child.name == characterName)
                {
                    child.gameObject.SetActive(true);
                    child.SetAsFirstSibling();
                    animator.Rebind();
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region 玩家状态
    protected bool isSitting = false;
    public bool IsSitting
    {
        get { return isSitting; }
        set
        {
            if (isLocalPlayer)
            {
                CmdSetSittingState(value);
            }
        }
    }

    protected string currentScene;
    public string CurrentScene
    {
        get { return currentScene; }
        set
        {
            if (isLocalPlayer)
            {
                //其他客户端同步状态
                CmdSetCurrentScene(value);

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
    #endregion

    #region 玩家权限
    protected PermissionHolder permissionHolder = new PermissionHolder();
    #endregion

    #region 玩家红点
    public GameObject redDot;
    public bool isShowingRedDot = false;
    #endregion

    #region 生命周期
    void Awake()
    {
        animator = transform.Find("PlayerVisual").GetComponent<Animator>();
        nameText = GetComponentInChildren<NameText>();
        rb = GetComponent<Rigidbody>();

        if (GameSettings.instance.isVR)
        {
            playerController =  gameObject.AddComponent<VRPlayerController>();
        }
        else 
        {
            playerController = gameObject.AddComponent<WinPlayerController>();
        }
        playerController.playerManager = this;
        allPlayers.Add(this);
    }

    void Start()
    {
        nameText = GetComponentInChildren<NameText>();

        if (!isLocalPlayer) 
        { 
            playerName = "待定";
            isStudent = false;
            // nameText.SetName(photonView.Owner.NickName);
            // CharacterName = photonView.Owner.CustomProperties["CharacterName"] as string;
            return; 
        }
        else
        {
            localPlayer = this;
            playerName = PlayerPrefs.GetString("NickName"); //标识用户姓名
            isStudent = PlayerPrefs.GetInt("IsStudent", 1) == 1;
            nameText.gameObject.SetActive(false);
            CharacterName = string.Empty;
        }
    }
    
    
    private void OnDestroy()
    {
        allPlayers.Remove(this);
        // 取消注册事件处理器
        //PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }
    #endregion

    #region 玩家连接
    public void JoinRoom()
    {
        CurrentScene = ClassManager.instance.currentScene;
        EventHandler.Trigger(new PlayerJoinRoomEvent() { player = this });

        // 初始化时隐藏红点
        //CreateRedDot();
        isShowingRedDot = false;

        // 获取权限, 并同步给其他客户端
        GetPermission();
        
        // 注册事件处理器
        //PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void LeftRoom()
    {
        EventHandler.Trigger(new PlayerLeftRoomEvent() { player = this });
        
        DestroyRedDot();

        // 注销事件处理器
        //PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    // private void OnEvent(EventData photonEvent)
    // {
    //     // 收到其他客户端的场景切换请求
    //     if (photonEvent.Code == EventCodes.ChangeSceneEventCode)
    //     {
    //         object[] data = (object[])photonEvent.CustomData;
    //         int[] targetViewIDs = (int[])data[0];
    //         string sceneName = (string)data[1];
    //         EventHandler.Trigger(new ChangeSceneEvent(){ includePlayers = targetViewIDs, sceneName = sceneName });
    //         Debug.Log("收到场景切换请求，场景名：" + sceneName);

    //         // 检查当前玩家是否在目标列表中
    //         if (targetViewIDs.Contains(photonView.ViewID))
    //         {
    //             // 在当前客户端上主动调用ChangeScene
    //             ChangeScene(sceneName);
    //         }
    //         // 如果不在目标列表中，也要加载另一边场景
    //         else
    //         {
    //             SceneLoader.instance.LoadSceneFromXml(sceneName, false);
    //         }
    //     }
    // }
    #endregion

    #region 玩家权限
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
        CmdSetPermission(permissionHolder.GetPermission());
    }

    public bool HavePermission(Permission permission)
    {
        return permissionHolder.HasPermission(permission);
    }
    #endregion

    #region 玩家红点

    private void CreateRedDot()
    {
        if (isLocalPlayer)
        {
            // // 使用 PhotonNetwork.Instantiate 来实例化红点
            // redDot = PhotonNetwork.Instantiate("RedDot", Vector3.down * 10000f, Quaternion.identity);
        }
    }

    private void DestroyRedDot()
    {
        if (redDot != null)
        {
            // PhotonNetwork.Destroy(redDot);
            redDot = null;
        }
    }
    #endregion

    #region 玩家状态

    [Command]
    public void CmdSetSittingState(bool sitting)
    {
        isSitting = sitting;

        rb.isKinematic = sitting;
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", 0);
    }

    public void ChangeScene(string sceneName)
    {
        // 如果是本地玩家，直接调用场景切换
        if (isLocalPlayer)
        {
            ClassManager.instance.StartSceneTransition(sceneName);
        }
    }

    [Command]
    public void CmdSetCurrentScene(string sceneName)
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

    [Command]
    public void CmdSetPermission(int permission)
    {
        permissionHolder.SetPermission((Permission)permission);
    }
    #endregion
}
