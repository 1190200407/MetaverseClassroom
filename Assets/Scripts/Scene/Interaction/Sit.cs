using UnityEngine;
using Mirror;
using System.Collections.Generic;   

public class Sit : InteractionScript
{
    public static Sit currentSitting;
    private bool isOccupied = false;
    private string chairKey;

    public override void Init(SceneElement element)
    {
        base.Init(element);
        chairKey = element.name;
        // 初始化时从ClassManager获取椅子状态
        object isOccupied = ClassManager.instance.GetRoomProperty(chairKey);
        if (isOccupied != null && isOccupied is bool)
        {
            this.isOccupied = (bool)isOccupied;
        }
        UpdateChairVisual();
    }

    public override void OnHoverEnter()
    {
        if (!isOccupied)
        {
            base.OnHoverEnter();
        }
    }

    public override void OnHoverExit()
    {
        if (!isOccupied)
        {
            base.OnHoverExit();
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
        Transform player = PlayerManager.localPlayer.playerController.transform;
        PlayerManager.localPlayer.IsSitting = true;
        player.position = element.transform.position + new Vector3(0, 0.5f, 0);

        // 更新椅子状态
        isOccupied = true;
        currentSitting = this;
        
        // 通过ClassManager更新房间属性
        ClassManager.instance.SetRoomProperty(chairKey, true);
        UpdateChairVisual();
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
        ClassManager.instance.SetRoomProperty(chairKey, false);
        UpdateChairVisual();
    }
}