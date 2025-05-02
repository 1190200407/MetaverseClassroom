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
    #endregion

    #region 玩家控制器
    public PlayerController playerController;
    public Animator animator;
    public Rigidbody rb;
    #endregion

    #region 玩家UI
    public NameText nameText;
    public RoleText roleText;
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
        nameText = GetComponentInChildren<NameText>();
        roleText = GetComponentInChildren<RoleText>();
        rb = GetComponent<Rigidbody>();
        animator = transform.Find("PlayerVisual").GetComponent<Animator>();

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
    
    private void OnDestroy()
    {
        allPlayers.Remove(this);
    }
    #endregion

    #region 玩家连接
    public override void OnStartLocalPlayer()
    {
        localPlayer = this;
        StartCoroutine(StartLocalPlayer());
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<ChangeSceneMessage>(OnChangeScene);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        NetworkClient.UnregisterHandler<ChangeSceneMessage>();
    }

    private IEnumerator StartLocalPlayer()
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        nameText.gameObject.SetActive(true);
        roleText.gameObject.SetActive(true);

        // 同步当前场景
        CurrentScene = ClassManager.instance.currentScene;
        // 同步名字
        PlayerName = PlayerPrefs.GetString("NickName");
        // 同步角色
        CharacterName = PlayerPrefs.GetString("ChosenCharacter", string.Empty);
        // 同步是否是学生
        IsStudent = PlayerPrefs.GetInt("IsStudent", 1) == 1;

        EventHandler.Trigger(new StartLocalPlayerEvent());

        // 初始化时隐藏红点
        isShowingRedDot = false;
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        DestroyRedDot();
    }
    #endregion

    #region 玩家权限
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
            // 创建红点逻辑
        }
    }

    private void DestroyRedDot()
    {
        if (redDot != null)
        {
            redDot = null;
        }
    }
    #endregion

    
    #region 同步属性
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName;
    public string PlayerName
    {
        get { return playerName; }
        set
        {
            if (isLocalPlayer)
            {
                CmdSetPlayerName(value);
            }
        }
    }
    [SyncVar(hook = nameof(OnRoleNameChanged))]
    private string roleName;

    public string RoleName
    {
        get { return roleName; }
        set
        {
            if (isLocalPlayer)
            {
                CmdSetRoleName(value);
            }
        }
    }
    
    [SyncVar(hook = nameof(OnCharacterNameChanged))]
    private string characterName;
    public string CharacterName
    {
        get { return characterName; }
        set
        {
            if (isLocalPlayer)
            {
                CmdSetCharacterName(value);
            }
        }
    }

    [SyncVar(hook = nameof(OnIsSittingChanged))]
    private bool isSitting = false;
    public bool IsSitting
    {
        get { return isSitting; }
        set
        {
            if (isLocalPlayer)
            {
                CmdSetIsSitting(value);
            }
        }
    }

    [SyncVar(hook = nameof(OnCurrentSceneChanged))]
    private string currentScene;
    public string CurrentScene
    {
        get { return currentScene; }
        set
        {
            if (isLocalPlayer)
            {
                if (value != currentScene)
                {
                    if (Sit.currentSitting != null)
                    {
                        Sit.currentSitting.ResetSeat();
                    }
                    CmdSetCurrentScene(value);
                }
            }
        }
    }

    [SyncVar(hook = nameof(OnIsStudentChanged))]
    private bool isStudent;
    public bool IsStudent
    {
        get { return isStudent; }
        set
        {
            if (isLocalPlayer)
            {
                OnIsStudentChanged(isStudent, value);
                CmdSetIsStudent(value);
            }
        }
    }

    // Command 方法
    [Command]
    private void CmdSetPlayerName(string value)
    {
        playerName = value;
    }
    [Command]
    private void CmdSetRoleName(string value)
    {
        roleName = value;
    }
    [Command]
    private void CmdSetCharacterName(string value)
    {
        characterName = value;
    }

    [Command]
    private void CmdSetIsSitting(bool value)
    {
        isSitting = value;
    }

    [Command]
    private void CmdSetCurrentScene(string value)
    {
        currentScene = value;
    }

    [Command]
    private void CmdSetIsStudent(bool value)
    {
        isStudent = value;
    }

    // 属性变更回调
    private void OnPlayerNameChanged(string oldValue, string newValue)
    {
        if (!isLocalPlayer)
        {
            nameText.SetName(newValue);
        }
    }

    private void OnRoleNameChanged(string oldValue, string newValue)
    {
        roleText.SetRole(newValue);
    }
    private void OnCharacterNameChanged(string oldValue, string newValue)
    {
        Transform playerVisual = transform.Find("PlayerVisual");
        for (int i = 0; i < playerVisual.childCount; i++)
        {
            Transform child = playerVisual.GetChild(i);

            // 如果是本地玩家，则不显示角色
            // 如果是其他玩家，则显示名字对应的角色
            if (!isLocalPlayer && child.name == newValue)
            {
                child.gameObject.SetActive(true);
                animator.avatar = child.GetComponent<Animator>().avatar;
                child.SetAsFirstSibling();
                animator.Rebind();
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private void OnIsSittingChanged(bool oldValue, bool newValue)
    {
        rb.isKinematic = newValue;
        animator.SetBool("IsSitting", newValue);
    }

    private void OnCurrentSceneChanged(string oldValue, string newValue)
    {
        if (newValue != ClassManager.instance.currentScene && isLocalPlayer)
        {
            ClassManager.instance.currentScene = newValue;
            playerController.ResetTransform(SceneLoader.instance.PathToSceneObject[newValue].transform);
            
            //切换场景后，更新player的状态
            foreach (var player in allPlayers)
            {
                if (player == this || player.currentScene == newValue)
                {
                    player.rb.isKinematic = isSitting;
                }
                else
                {
                    player.rb.isKinematic = true;
                }   
            }
        }
        else if (!isLocalPlayer)
        {
            if (newValue != ClassManager.instance.currentScene)
            {
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = isSitting;
            }
        }
    }

    private void OnIsStudentChanged(bool oldValue, bool newValue)
    {
        if (isStudent)
        {
            permissionHolder.SetPermission(Permission.Chat | Permission.RedDot);
        }
        else
        {
            permissionHolder.SetAllPermission();
        }
    }
    #endregion

    #region 网络消息
    public void OnChangeScene(ChangeSceneMessage message)
    {
        ClassManager.instance.ChangeScene(message.sceneName);
    }
    #endregion
}
