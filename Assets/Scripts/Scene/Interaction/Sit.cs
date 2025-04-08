using UnityEngine;
using Mirror;
using System.Collections.Generic;   

public class Sit : InteractionScript
{
    public static Sit currentSitting;
    private bool isOccupied = false;
    private string chairKey;

    // 使用 SyncVar 来同步椅子状态
    [SyncVar(hook = nameof(OnChairStateChanged))]
    private bool syncedIsOccupied;

    public override void Init(SceneElement element)
    {
        base.Init(element);
        chairKey = element.name;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // 初始化时更新椅子状态
        UpdateChairVisual();
    }

    private void OnChairStateChanged(bool oldValue, bool newValue)
    {
        isOccupied = newValue;
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
        
        // 如果是本地玩家，调用服务器命令来更新状态
        if (isLocalPlayer)
        {
            CmdSetChairOccupied(true);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSetChairOccupied(bool occupied)
    {
        syncedIsOccupied = occupied;
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
        
        // 如果是本地玩家，调用服务器命令来更新状态
        if (isLocalPlayer)
        {
            CmdSetChairOccupied(false);
        }
    }
}