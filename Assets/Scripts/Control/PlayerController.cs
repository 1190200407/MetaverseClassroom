using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviourPunCallbacks
{
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
                photonView.RPC("SetCurrentScene", RpcTarget.AllBuffered, value);

                transform.position = SceneLoader.instance.PathToSceneObject[value].transform.position;
                //切换场景后，更新player的状态
                foreach (var player in ClassManager.instance.players)
                {
                    if (player != this && player.currentScene == value)
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

    protected GameObject redDot;
    protected bool isShowingRedDot = false;

    protected virtual void Start()
    {
        if (photonView.IsMine)
            localPlayer = this;
        //将自己的名字牌和模型隐藏
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }
    
    public void JoinRoom()
    {
        CurrentScene = ClassManager.instance.currentScene;
        EventHandler.Trigger(new PlayerJoinRoomEvent() { player = this });

        // 初始化时隐藏红点
        CreateRedDot();
        isShowingRedDot = false;
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

        if (sceneName != localPlayer.currentScene)
        {
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = isSitting;
        }
    }
}
