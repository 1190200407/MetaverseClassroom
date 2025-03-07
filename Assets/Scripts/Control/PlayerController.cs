using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public static List<PlayerController> allPlayers = new List<PlayerController>();
    public static PlayerController localPlayer;

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
            localPlayer = this;
        allPlayers.Add(this);
            
        //将自己的名字牌和模型隐藏
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        isStudent = PlayerPrefs.GetInt("IsStudent", 1) == 1;
    }

    protected virtual void Start()
    {
    }
    
    private void OnDestroy()
    {
        allPlayers.Remove(this);
    }

    public void JoinRoom()
    {
        IsStudent = PlayerPrefs.GetInt("IsStudent", 1) == 1;
        CurrentScene = ClassManager.instance.currentScene;
        EventHandler.Trigger(new PlayerJoinRoomEvent() { player = this });

        // 初始化时隐藏红点
        CreateRedDot();
        isShowingRedDot = false;

        // 获取权限, 并同步给其他客户端
        GetPermission();
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

    public void LeftRoom()
    {
        EventHandler.Trigger(new PlayerLeftRoomEvent() { player = this });
        DestroyRedDot();
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
