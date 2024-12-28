using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Sit : InteractionScript
{
    public static Sit currentSitting;
    private bool isOccupied = false;
    private string chairKey;
    
    private const byte OccupyChairEventCode = 2; // 自定义事件代码

    public override void Init(SceneElement element)
    {
        base.Init(element);
        chairKey = element.name;
    }

    public override void OnJoinRoom()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(chairKey, out object state))
        {
            isOccupied = (bool)state;
            UpdateChairVisual(); // 更新椅子的外观
        }
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
        Transform player = PlayerController.localPlayer.transform;
        PlayerController.localPlayer.IsSitting = true;
        player.position = element.transform.position + new Vector3(0, 0.5f, 0); // 根据需求调整位置

        // 更新椅子状态
        isOccupied = true;
        currentSitting = this;
        SetChairOccupiedInRoomProperties(true);
        RaiseChairOccupiedEvent(true); // 广播事件
    }

    private void SetChairOccupiedInRoomProperties(bool occupied)
    {
        Hashtable currentProps = new Hashtable
        {
            { chairKey, occupied }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(currentProps);
    }

    private void RaiseChairOccupiedEvent(bool occupied)
    {
        // 使用 RaiseEvent 立即通知其他玩家椅子被占用状态
        object[] content = new object[] { chairKey, occupied };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(OccupyChairEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == OccupyChairEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string receivedChairKey = (string)data[0];
            bool occupied = (bool)data[1];

            if (receivedChairKey == chairKey)
            {
                isOccupied = occupied;
                UpdateChairVisual(); // 更新椅子外观
            }
        }
    }

    private void UpdateChairVisual()
    {
        // 更新椅子的外观状态，比如颜色或材质，用于表示已被占用
    }
    
    public void ResetSeat()
    {
        isOccupied = false;
        currentSitting = null;
        
        PlayerController.localPlayer.IsSitting = false;
        SetChairOccupiedInRoomProperties(false);
        RaiseChairOccupiedEvent(false);
    }
}