using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleSetter : InteractionScript
{
    
    public static RoleSetter currentPlayer;
    
    public List<string> roleList; //exercise包含的所有角色
    
    private OutlineObject outline;
    private string setterKey;
    private bool isOccupied = false;

    public override void Init(SceneElement element)
    {
        base.Init(element);
        setterKey = "RoleSetter_" + ClassManager.instance.currentScene + "_" + element.id;
        outline = element.gameObject.AddComponent<OutlineObject>();
        outline.enabled = false;
        Debug.Log("Setter 成功");
    }

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

    private bool CheckOccupied()
    {
        string isOccupied = null;
        ClassManager.instance.CmdGetRoomProperty(setterKey, PlayerManager.localPlayer.connectionToClient);
        if (ClassManager.instance.roomProperties.ContainsKey(setterKey))
        {
            isOccupied = ClassManager.instance.roomProperties[setterKey];
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
        UpdateSetterVisual();
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
    /// <summary>
    /// 确保角色设置器每次只被一个玩家使用
    /// </summary>
    public override void OnSelectEnter()
    {
        if (isOccupied) return;

        if (currentPlayer != null && currentPlayer != this)
        {
            currentPlayer.ResetSetter();
        }
        
        currentPlayer = this;
        //弹出选角UI
        ChooseRolePanel panel = (ChooseRolePanel)UIManager.instance.Push(new ChooseRolePanel(new UIType("Panels/ChooseRolePanel", "ChooseRolePanel")));
        //初始化Panel的角色列表
        panel.InitializeSlots(setterKey);
        
        // 更新角色选择器状态
        currentPlayer = this;
        UpdateSetterVisual();
        
        // 通过ClassManager更新房间属性
        ClassManager.instance.CommandSetRoomProperty(setterKey, "true");
    }

    private void UpdateSetterVisual()
    {
        // 更新角色选择器的外观状态，比如颜色或材质，用于表示已被占用
        // 例如：
        // GetComponent<Renderer>().material.color = isOccupied ? Color.red : Color.white;
    }
    
    public void ResetSetter()
    {
        isOccupied = false;
        currentPlayer = null;
        
        // 通过ClassManager更新房间属性
        ClassManager.instance.CommandSetRoomProperty(setterKey, "false");
        UpdateSetterVisual();
    }

    public void OnRoomPropertyChange(RoomPropertyChangeEvent @event)
    {
        if (@event.key == setterKey)
        {
            isOccupied = @event.value == "true";
            Debug.Log("OnRoomPropertyChange: " + isOccupied);
        }
        UpdateSetterVisual();
    }
}
