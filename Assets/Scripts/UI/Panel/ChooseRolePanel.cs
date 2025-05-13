using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class ChooseRolePanel : BasePanel
{
    private int count = 0;
    private GameObject roleSlot; //角色预制件
    private GameObject playerSlot; //用户预制件

    // TODO currentRold 改成 roleID    
    private String currentRoleID; //当下选择的角色
    private String setterKey; //对应的角色设置器的key

    private Button exitButton;
    private Button confirmButton;

    private GameObject content_A; //角色列表区域
    private GameObject content_B; //用户列表区域

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
        
        playerSlot = Resources.Load<GameObject>("Panels/PlayerSlot");
        if (playerSlot == null)
        {
            Debug.LogError("PlayerSlot prefab not found in Resources/Panels/PlayerSlot");
        }
        
        //绑定按钮
        exitButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ExitButton");
        exitButton.onClick.AddListener(Exit);
        confirmButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ConfirmButton");
        confirmButton.onClick.AddListener(Confirm);
        
        //寻找content
        GameObject ScrollView_A = UIMethods.instance.FindObjectInChild(ActiveObj, "Scroll View1");
        GameObject Viewport_A = UIMethods.instance.FindObjectInChild(ScrollView_A, "Viewport");
        content_A = UIMethods.instance.FindObjectInChild(Viewport_A, "Content");
        
        GameObject ScrollView_B = UIMethods.instance.FindObjectInChild(ActiveObj, "Scroll View2");
        GameObject Viewport_B = UIMethods.instance.FindObjectInChild(ScrollView_B, "Viewport");
        content_B = UIMethods.instance.FindObjectInChild(Viewport_B, "Content");
    }
    
    public override void OnEnable()
    {
        base.OnEnable();
        InteractionManager.instance.RaycastClosed = true;
        PlayerManager.localPlayer.playerController.enabled = false;
        count = 0;
    }
    public override void OnDisable() {
        base.OnDisable();
        InteractionManager.instance.RaycastClosed = false;
        PlayerManager.localPlayer.playerController.enabled = true;
    }
    
    /// <summary>
    /// 检查对应角色是否被选择
    /// </summary>
    /// <param name="RoleName"></param> 玩家名
    /// <returns></returns> 被占用返回true，否则返回false
    private bool CheckRoleOccupied(String playerName)
    {
        if (ClassManager.instance.roleOccupied.ContainsKey(currentRoleID))
        {
            if (playerName == "NPC")
            {
                return ClassManager.instance.roleOccupied[currentRoleID] == -1;
            }
            
            foreach (var player in PlayerManager.allPlayers) 
            {
                if (player.PlayerName == playerName)
                {
                    return ClassManager.instance.roleOccupied[currentRoleID] == (int)player.netId;
                }
            }
            
        }
        return false;
    }

    /// <summary>
    /// 是否完成选角
    /// </summary>
    /// <returns></returns>
    private bool IsDone()
    {
        return count == ClassManager.instance.roleList.Count;
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

    private void Confirm()
    {
        //关闭当前面板
        UIManager.instance.Pop(false);

        //通知所有玩家选角完成
        NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.StartActionTree);
    }    
    
    /// <summary>
    /// 当角色被勾选后，初始化用户列表
    /// </summary>
    /// <param name="isOn"></param>是否勾选
    /// <param name="roleId"></param>勾选角色名
    void OnRoleToggleValueChanged(bool isOn,String roleID)
    {
        if (isOn)
        {
            currentRoleID = roleID;
            InitializePlayerSlots();
        }
    }

    /// <summary>
    /// 当用户被勾选后，添加角色至已选列表中
    /// </summary>
    /// <param name="isOn"></param>是否勾选
    /// <param name="PlayerName"></param>勾选用户名
    void OnPlayerToggleValueChanged(bool isOn, string playerName)
    {
        EventHandler.Trigger(new UIHighLightEvent() { id = currentRoleID,isHighlighted = isOn});
        if (isOn)
        {
            if (playerName == "NPC")
            {
                ClassManager.instance.CommandSetRoleOccupied(currentRoleID, -1);
            }
            else
            {
                foreach (var player in PlayerManager.allPlayers)
                {
                    if (player.PlayerName == playerName)
                    {
                        // 修改角色占用情况, 玩家监听后会自动更新角色名
                        ClassManager.instance.CommandSetRoleOccupied(currentRoleID, (int)player.netId);
                    }
                }
            }
            count += 1;
        }
        else
        {
            ClassManager.instance.CommandSetRoleOccupied(currentRoleID, 0);
            count -= 1;
        }
        confirmButton.interactable = IsDone();
    }
    
    /// <summary>
    /// 初始化角色的slots
    /// </summary>
    public void InitializeRoleSlots(String setterKey)
    {
        this.setterKey = setterKey;
        // 先清空 content 下的所有子物体
        foreach (Transform child in content_A.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // 遍历所有角色并创建 slot
        foreach (var role in ClassManager.instance.roleList)
        {
            // 实例化 slot 并设置为 content 的子物体
            GameObject newSlot = GameObject.Instantiate(roleSlot, content_A.transform);
            //HighLight的id修改成角色id
            UIHighlight highlightComponent = newSlot.GetComponent<UIHighlight>();
            highlightComponent.id = role.Key;
            
            if (!ClassManager.instance.roleOccupied.ContainsKey(role.Key))
            {
                ClassManager.instance.roleOccupied.Add(role.Key, 0);
            }
            if (ClassManager.instance.roleOccupied[role.Key] != 0)
            {
                highlightComponent.IsHighlight = true;
                count += 1;
            }
            
            newSlot.name = role.Key;

            // 获取并设置玩家名称
            TextMeshProUGUI Name = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(newSlot, "Name");
            Name.text = role.Value;
            
            Toggle newToggle = newSlot.GetComponent<Toggle>();
            //设置ToggleGroup，保证同时只有一个toggle被勾选
            newToggle.group = content_A.GetComponent<ToggleGroup>();
            //Toggle值改变监听函数,当被勾选时，初始化选择用户的列表
            newToggle.onValueChanged.AddListener(isOn => OnRoleToggleValueChanged(isOn, role.Key));
        }

        confirmButton.interactable = IsDone();
    }

    /// <summary>
    /// 根据角色名称初始化选择用户的slots
    /// </summary>
    /// <param name="RoleName"></param>
    public void InitializePlayerSlots()
    {
        // 先清空 content 下的所有子物体
        foreach (Transform child in content_B.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // 遍历所有用户并创建 slot
        foreach (var player in PlayerManager.allPlayers)
        {
            string playerName = player.PlayerName;
            // 实例化 slot 并设置为 content 的子物体
            GameObject newSlot = GameObject.Instantiate(playerSlot, content_B.transform);
            newSlot.name = playerName;

            // 获取并设置玩家名称
            TextMeshProUGUI Name = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(newSlot, "Name");
            Name.text = playerName;

            Toggle newToggle = newSlot.GetComponent<Toggle>();
            //设置ToggleGroup，保证同时只有一个toggle被勾选
            newToggle.group = content_B.GetComponent<ToggleGroup>();
            //如果该角色已被占用
            if (CheckRoleOccupied(playerName))
            { 
                newToggle.isOn = true;
            }
            else if(!String.IsNullOrEmpty(player.RoleId))
            {
                newToggle.interactable = false;
            }
            //Toggle值改变监听函数,当被勾选时，初始化选择用户的列表
            newToggle.onValueChanged.AddListener(isOn => OnPlayerToggleValueChanged(isOn, playerName));
        }

        #region NPC
        // 实例化 slot 并设置为 content 的子物体
        GameObject npcSlot = GameObject.Instantiate(playerSlot, content_B.transform);
        npcSlot.name = "NPC";

        // 获取并设置玩家名称
        TextMeshProUGUI npcName = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(npcSlot, "Name");
        npcName.text = "NPC";

        Toggle npcToggle = npcSlot.GetComponent<Toggle>();
        //设置ToggleGroup，保证同时只有一个toggle被勾选
        npcToggle.group = content_B.GetComponent<ToggleGroup>();
        //如果该角色已被占用
        if (CheckRoleOccupied("NPC"))
        { 
            npcToggle.isOn = true;
        }
        //Toggle值改变监听函数,当被勾选时，初始化选择用户的列表
        npcToggle.onValueChanged.AddListener(isOn => OnPlayerToggleValueChanged(isOn, "NPC"));
        #endregion
        
    }
}
