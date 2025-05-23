using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneElement : MonoBehaviour
{
    [HideInInspector] public string id;
    public string path;

    public InteractionScript interactionScript;

    public string interactType = string.Empty;
    public string interactionContent = string.Empty;

    protected virtual void OnEnable()
    {
        interactionScript?.OnEnable();
        EventHandler.Register<StartLocalPlayerEvent>(OnStartLocalPlayer);
    }

    protected virtual void OnDisable()
    {
        interactionScript?.OnDisable();
        EventHandler.Unregister<StartLocalPlayerEvent>(OnStartLocalPlayer);
    }

    public void LoadData(string id, string name, string path) {
        this.id = id;
        this.name = name;
        this.path = path;
        
        SceneLoader.instance.IdToElement.Add(id, this);
    }

    public void SetInteactionType(string type, string content = "") {
        // 如果当前有交互脚本，则先执行该脚本的OnDisable，然后切换新脚本
        interactionScript?.OnDisable();

        Type t = Type.GetType(type);
        if (t != null)
        {
            interactionScript = Activator.CreateInstance(t) as InteractionScript;
            interactType = type;
            interactionContent = content;
            interactionScript.Init(this);
            interactionScript.OnEnable();

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

    /// <summary>
    /// 当本地玩家开始时响应的函数
    /// </summary>
    private void OnStartLocalPlayer(StartLocalPlayerEvent @event)
    {
        interactionScript?.OnStartLocalPlayer();
    }
}
