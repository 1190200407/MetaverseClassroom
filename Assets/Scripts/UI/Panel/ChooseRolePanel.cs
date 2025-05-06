using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class ChooseRolePanel : BasePanel
{
    private GameObject roleSlot; //选择项的预制件
    private String setterKey; //对应的角色设置器的key

    private Button exitButton;

    private GameObject content;

    public ChooseRolePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        roleSlot = Resources.Load<GameObject>("Panels/RoleSlot");
        if (roleSlot == null)
        {
            Debug.LogError("RoleSlot prefab not found in Resources/Panels/RoleSlot");
        }
        
        //绑定按钮
        exitButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ExitButton");
        exitButton.onClick.AddListener(Exit);
        
        //寻找content
        GameObject ScrollView = UIMethods.instance.FindObjectInChild(ActiveObj, "Scroll View");
        GameObject Viewport = UIMethods.instance.FindObjectInChild(ScrollView, "Viewport");
        content = UIMethods.instance.FindObjectInChild(Viewport, "Content");
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        InteractionManager.instance.RaycastClosed = true;
        PlayerManager.localPlayer.playerController.enabled = false;
    }

    public override void OnDisable() {
        base.OnDisable();
        InteractionManager.instance.RaycastClosed = false;
        PlayerManager.localPlayer.playerController.enabled = true;
    }
    
    /// <summary>
    /// 检查对应角色是否被选择
    /// </summary>
    /// <param name="RoleName"></param> 角色名
    /// <returns></returns> 被占用返回true，否则返回false
    private bool CheckRoleOccupied(String RoleName)
    {
        String roleKey = setterKey + RoleName; //角色的key等于setterKey+角色名
        string isOccupied = null;
        ClassManager.instance.CmdGetRoomProperty(roleKey, PlayerManager.localPlayer.connectionToClient);
        if (ClassManager.instance.roomProperties.ContainsKey(roleKey))
        {
            isOccupied = ClassManager.instance.roomProperties[roleKey];
        }
        if (isOccupied != null && isOccupied == "true")
        {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 返回
    /// </summary>
    private void Exit()
    {
        UIManager.instance.Pop(false);
        if (RoleSetter.currentPlayer != null)
        {
            RoleSetter.currentPlayer.ResetSetter();
        }
    }
    
    /// <summary>
    /// 当被勾选后，改变全局参数
    /// </summary>
    /// <param name="isOn"></param>是否勾选
    /// <param name="roleId"></param>勾选角色的Id
    void OnToggleValueChanged(bool isOn,String roleId)
    {
        if (isOn)
        {
            //若被选中，改变RoomProperty,相关key改为已占用
            ClassManager.instance.CommandSetRoomProperty(setterKey + roleId, "true");
            //设置当前玩家的角色名
            PlayerManager.localPlayer.RoleName = roleId;
        }
        else
        {
            //若取消选中，改变RoomProperty,相关key改为未占用
            ClassManager.instance.CommandSetRoomProperty(setterKey + roleId, "flase");
        }
    }
    
    /// <summary>
    /// 初始化角色的slots
    /// </summary>
    public void InitializeSlots(String setterKey)
    {
        this.setterKey = setterKey;
        // 先清空 content 下的所有子物体
        foreach (Transform child in content.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // 遍历所有玩家并创建 slot
        foreach (var role in ClassManager.instance.roleList)
        {
            // 实例化 slot 并设置为 content 的子物体
            GameObject newSlot = GameObject.Instantiate(roleSlot, content.transform);
            newSlot.name = role.Key;

            // 获取并设置玩家名称
            TextMeshProUGUI Name = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(newSlot, "Name");
            Name.text = role.Value;
            
            Toggle newToggle = newSlot.GetComponent<Toggle>();
            //设置ToggleGroup，保证同时只有一个toggle被勾选
            newToggle.group = content.GetComponent<ToggleGroup>();
            //Toggle值改变监听函数,当被勾选时，全局改变是否被勾选
            newToggle.onValueChanged.AddListener(isOn => OnToggleValueChanged(isOn, role.Key));
            
            //如果该角色已被占用
            if (CheckRoleOccupied(role.Key))
            {
                if (PlayerManager.localPlayer.RoleName != role.Key)
                {
                    newToggle.interactable = false;
                }
                else
                {
                    newToggle.isOn = true;
                }
            }
        }
    }
}
