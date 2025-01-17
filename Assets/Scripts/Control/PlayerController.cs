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
                transform.position = SceneLoader.instance.PathToSceneObject[value].transform.position;
            }
            // if (photonView.IsMine)
            // {
            //     photonView.RPC("SetCurrentScene", RpcTarget.AllBuffered, value);
            // }
        }
    }

    protected virtual void Start()
    {
        if (photonView.IsMine)
            localPlayer = this;
        //将自己的名字牌和模型隐藏
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        CurrentScene = CourseManager.instance.currentScene;
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

        if (photonView.IsMine)
        {
            transform.SetParent(SceneLoader.instance.PathToSceneObject[sceneName].transform);
            transform.localPosition = Vector3.zero;
        }
    }
}
