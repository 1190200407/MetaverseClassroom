using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class Drink : InteractionScript
{
    private OutlineObject outline;
    private AudioClip drinkAudioClip;
    private Action<ElementInteractMessageData> onElementInteract;
    private Action onDrinkCompleteCallback;

    public override void Init(SceneElement element)
    {
        base.Init(element);

        if (element.gameObject.GetComponent<OutlineObject>() == null)
        {
            outline = element.gameObject.AddComponent<OutlineObject>();
        }
        else
        {
            outline = element.gameObject.GetComponent<OutlineObject>();
        }
        outline.enabled = false;

        onElementInteract = OnElementInteract;
        NetworkMessageHandler.instance.RegisterHandler(NetworkMessageType.ElementInteract, onElementInteract);

        drinkAudioClip = Resources.Load<AudioClip>("AudioClips/Drink");
    }

    private void OnDestroy()
    {
        NetworkMessageHandler.instance.UnregisterHandler(NetworkMessageType.ElementInteract, onElementInteract);
    }

    public override void OnHoverEnter()
    {
        outline.enabled = true;
    }
    
    public override void OnHoverExit()
    {
        outline.enabled = false;
    }

    public override void OnSelectEnter()
    {
        // 广播交互事件
        NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.ElementInteract, 
            new ElementInteractMessageData() { netId = (int)PlayerManager.localPlayer.netId, elementId = element.id });

        // 设置回调函数
        onDrinkCompleteCallback = () => {
            base.OnSelectEnter();
        };
    }

    public void OnElementInteract(ElementInteractMessageData data)
    {
        if (element.id != data.elementId) return;

        // 通过netId找到玩家，获取玩家动画组件
        GameObject player = Mirror.Utils.GetSpawnedInServerOrClient((uint)data.netId).gameObject;
        if (player == null) return;

        StartDrinking(player.GetComponent<Animator>());
    }

    public override void OnNPCInteract(NPCManager npcManager, string interactWay)
    {
        if (interactWay != "SelectEnter") return;
        StartDrinking(npcManager.GetComponent<Animator>());

        // 设置回调函数
        onDrinkCompleteCallback = () => {
            base.OnNPCInteract(npcManager, interactWay);
        };
    }

    public void StartDrinking(Animator animator)
    {
        // TODO 播放动画和音效
        if (drinkAudioClip != null)
        {
            VoiceManager.instance.PlayAudioOneShot(drinkAudioClip);
        }

        // 几秒后消失
        element.GetComponent<Collider>().enabled = false;
        element.StartCoroutine(WaitForDrinkComplete());
    }

    private IEnumerator WaitForDrinkComplete()
    {
        yield return new WaitForSeconds(5f);
        OnDrinkComplete();
    }

    public void OnDrinkComplete()
    {
        element.gameObject.SetActive(false);
        onDrinkCompleteCallback?.Invoke();
    }
}
