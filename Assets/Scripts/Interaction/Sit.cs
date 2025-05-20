using UnityEngine;
using Mirror;
using System.Collections.Generic;   

public class Sit : InteractionScript
{
    public static Sit currentSitting;
    private OutlineObject outline;
    private string chairKey;
    private bool isOccupied = false;

    public override void Init(SceneElement element)
    {
        base.Init(element);
        chairKey = string.Concat("isOccupied_", ClassManager.instance.currentScene, "_", element.id);
        outline = element.gameObject.AddComponent<OutlineObject>();
        outline.enabled = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        EventHandler.Register<RoomPropertyChangeEvent>(OnRoomPropertyChange);
        EventHandler.Register<BeforeChangeSceneEvent>(OnBeforeChangeScene);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EventHandler.Unregister<RoomPropertyChangeEvent>(OnRoomPropertyChange);
        EventHandler.Unregister<BeforeChangeSceneEvent>(OnBeforeChangeScene);
    }

    private bool CheckOccupied()
    {
        string isOccupied = null;
        
        ClassManager.instance.CmdGetRoomProperty(chairKey, PlayerManager.localPlayer.connectionToClient);
        if (ClassManager.instance.roomProperties.ContainsKey(chairKey))
        {
            isOccupied = ClassManager.instance.roomProperties[chairKey];
        }

        if (isOccupied != null && isOccupied == "true")
        {
            return true;
        }
        return false;
    }

    public override void OnStartLocalPlayer()
    {
        isOccupied = CheckOccupied();
        UpdateChairVisual();
    }

    public override void OnHoverEnter()
    {
        if (!isOccupied)
        {
            base.OnHoverEnter();
            outline.enabled = true;
        }
    }

    public override void OnHoverExit()
    {
        if (!isOccupied)
        {
            base.OnHoverExit();
            outline.enabled = false;
        }
    }

    public override void OnSelectEnter()
    {
        if (isOccupied) return;

        if (currentSitting != null && currentSitting != this)
        {
            currentSitting.ResetSeat();
        }
        
        // 获取玩家 Transform 并锁定位置
        Transform player = PlayerManager.localPlayer.transform;
        PlayerManager.localPlayer.IsSitting = true;
        player.position = element.transform.position + new Vector3(0, 0.1f, 0);

        // 更新椅子状态
        currentSitting = this;
        
        // 通过ClassManager更新房间属性
        ClassManager.instance.CommandSetRoomProperty(chairKey, "true");

        // 触发事件
        EventHandler.Trigger(new InteractionEvent() { interactionType = "sit", elementId = element.id });
    }

    private void UpdateChairVisual()
    {
        // 更新椅子的外观状态，比如颜色或材质，用于表示已被占用
        // 例如：
        // GetComponent<Renderer>().material.color = isOccupied ? Color.red : Color.white;
    }
    
    public void ResetSeat()
    {
        isOccupied = false;
        currentSitting = null;
        
        PlayerManager.localPlayer.IsSitting = false;
        
        // 通过ClassManager更新房间属性
        ClassManager.instance.CommandSetRoomProperty(chairKey, "false");
    }

    public void OnRoomPropertyChange(RoomPropertyChangeEvent @event)
    {
        if (@event.key == chairKey)
        {
            isOccupied = @event.value == "true";
            //Debug.Log("OnRoomPropertyChange: " + isOccupied);
        }
        UpdateChairVisual();
    }

    public void OnBeforeChangeScene(BeforeChangeSceneEvent @event)
    {
        if (currentSitting != this && @event.sceneName != ClassManager.instance.currentScene)
        {
            ResetSeat();
        }
    }
}