using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerController : MonoBehaviourPun
{
    public static PlayerController localPlayer;
    public GameObject playerCamera = null;
    public Animator anim;

    public Transform handTransform;

    public Rigidbody rb;
    protected bool isSitting = false;
    public bool IsSitting
    {
        set
        {
            if (photonView.IsMine)
            {
                photonView.RPC("SetSittingState", RpcTarget.All, value);
            }
        }
    }

    protected virtual void Start()
    {
        if (photonView.IsMine)
            localPlayer = this;
        //将自己的名字牌和模型隐藏
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    [PunRPC]
    public void SetSittingState(bool sitting)
    {
        isSitting = sitting;

        // 同步状态到其他客户端的对象
        rb.isKinematic = sitting;
        anim.SetFloat("MoveX", 0);
        anim.SetFloat("MoveY", 0);
    }
}
