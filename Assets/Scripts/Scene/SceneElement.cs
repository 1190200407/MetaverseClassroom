using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneElement : MonoBehaviourPunCallbacks
{
    [HideInInspector] public int id;
    public string path;

    public InteractionScript interactionScript;

    public override void OnEnable()
    {
        base.OnEnable();
        interactionScript?.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        interactionScript?.OnDisable();
    }

    public void LoadData(int id, string name, string path, int parent_id) {
        this.id = id;
        this.name = name;
        this.path = path;
        
        SceneLoader.instance.IdToElement.Add(id, this);

        if(SceneLoader.instance.IdToElement.ContainsKey(parent_id))
            transform.SetParent(SceneLoader.instance.IdToElement[parent_id].transform);
        else
            transform.SetParent(null);
    }

    public void SetInteactionType(string type, string content = "") {
        Type t = Type.GetType(type);
        if (t != null)
        {
            interactionScript =  Activator.CreateInstance(t) as InteractionScript;
            interactionScript.Init(this, type, content);

            // VR环境挂载XRSimpleInteractable
            if (GameSettings.instance.isVR)
            {
                XRSimpleInteractable interactable = gameObject.AddComponent<XRSimpleInteractable>();
                interactable.hoverEntered.AddListener((args) => { OnHoverEnter(); });
                interactable.hoverExited.AddListener((args) => { OnHoverExit(); });
                interactable.selectEntered.AddListener((args) => { OnSelectEnter(); });
                interactable.selectExited.AddListener((args) => { OnSelectExit(); });
            }
        }
    }

    public override void OnJoinedRoom()
    {
        interactionScript?.OnJoinRoom();
    }

    private void OnDestroy() {
        SceneLoader.instance.IdToElement.Remove(id);
    }


    /// <summary>
    /// 当射线射中时响应的函数
    /// </summary>
    public void OnHoverEnter()
    {
        interactionScript?.OnHoverEnter();
    }

    /// <summary>
    /// 当射线离开时响应的函数
    /// </summary>
    public void OnHoverExit()
    {
        interactionScript?.OnHoverExit();
    }

    /// <summary>
    /// 当射线按下时响应的函数
    /// </summary>
    public void OnSelectEnter()
    {
        interactionScript?.OnSelectEnter();
    }

    /// <summary>
    /// 当射线松开时响应的函数
    /// </summary>
    public void OnSelectExit()
    {
        interactionScript?.OnSelectExit();
    }

    public void OnEvent(EventData data)
    {
        interactionScript?.OnEvent(data);
    }
}
