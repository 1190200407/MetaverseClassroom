using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class Grab : InteractionScript
{
    public static Grab currentGrabing;

    public PlayerManager holder;
    public bool isHeld => holder != null;
    public string grabKey;
    private OutlineObject outline;

    public Transform originalParent;
    public Vector3 originalPosition;
    public Quaternion originalRotation;

    public override void OnEnable()
    {
        base.OnEnable();
        EventHandler.Register<RoomPropertyChangeEvent>(OnRoomPropertyChange);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EventHandler.Unregister<RoomPropertyChangeEvent>(OnRoomPropertyChange);
    }

    public override void Init(SceneElement element)
    {
        base.Init(element);
        grabKey = string.Concat("grab_", ClassManager.instance.currentScene, "_", element.id);
        if (element.gameObject.GetComponent<OutlineObject>() == null)
        {
            outline = element.gameObject.AddComponent<OutlineObject>();
        }
        else
        {
            outline = element.gameObject.GetComponent<OutlineObject>();
        }
        outline.enabled = false;
    }

    public override void OnHoverEnter()
    {
        if (isHeld) return;
        if (currentGrabing != null && currentGrabing != this) return;
        outline.enabled = true;
    }
    
    public override void OnHoverExit()
    {
        outline.enabled = false;
    }

    public override void OnSelectEnter()
    {
        if (isHeld) return;

        if (currentGrabing != null && currentGrabing != this)   
        {
            return;
        }

        base.OnSelectEnter();
        currentGrabing = this;
        // 设置持有者
        ClassManager.instance.CommandSetRoomProperty(grabKey, PlayerManager.localPlayer.netId.ToString());

        // 监听放下事件
        EventHandler.Register<GrabResetEvent>(OnGrabReset);
        EventHandler.Register<BeforeChangeSceneEvent>(OnBeforeChangeScene);
    }

    public void StopGrab()
    {
        if (currentGrabing != this) return;
        currentGrabing = null;
        EventHandler.Unregister<GrabResetEvent>(OnGrabReset);
        EventHandler.Unregister<BeforeChangeSceneEvent>(OnBeforeChangeScene);
        
        ClassManager.instance.CommandSetRoomProperty(grabKey, "");
    }

    public void SetHolder(PlayerManager player)
    {
        holder = player;

        // 保存原始位置
        originalParent = element.transform.parent;
        originalPosition = element.transform.localPosition;
        originalRotation = element.transform.localRotation;

        // 设置为手部位置
        element.transform.SetParent(holder.handTransform);
        element.transform.localPosition = Vector3.zero;
        element.transform.localRotation = Quaternion.identity;
    }

    public void SetNoHolder()
    {
        holder = null;

        element.transform.SetParent(originalParent);
        element.transform.localPosition = originalPosition;
        element.transform.localRotation = originalRotation;
    }

    public void OnRoomPropertyChange(RoomPropertyChangeEvent @event)
    {
        if (@event.key == grabKey)
        {
            // value值为空串时，表示没有玩家持有
            if (@event.value == "")
            {
                SetNoHolder();
            }
            else
            {
                // 获取持有者
                var player = PlayerManager.allPlayers.FirstOrDefault(p => p.netId.ToString() == @event.value);
                if (player != null)
                {
                    SetHolder(player);                    
                }
            }
        }
    }

    public void OnBeforeChangeScene(BeforeChangeSceneEvent @event)
    {
        if (@event.sceneName != ClassManager.instance.currentScene)
        {
            StopGrab();
        }
    }

    public void OnGrabReset(GrabResetEvent @event)
    {
        StopGrab();
    }
}
