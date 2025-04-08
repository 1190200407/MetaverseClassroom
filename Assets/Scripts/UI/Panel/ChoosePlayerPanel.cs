using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class ChoosePlayerPanel : BasePanel
{
    public GameObject PlayerSlot; //选择项的预制件
    private List<PlayerManager> ChosenPlayers = new List<PlayerManager>(); //被选中的用户
    private List<Toggle> PlayerToggles = new List<Toggle>();
    
    private Button saveButton;
    private Button cancelButton;
    private Button exitButton;

    private GameObject content;

    public ChoosePlayerPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        PlayerSlot = Resources.Load<GameObject>("Panels/PlayerSlot");
        if (PlayerSlot == null)
        {
            Debug.LogError("PlayerSlot prefab not found in Resources/Panels/PlayerSlot");
        }
        
        //绑定按钮
        saveButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "SaveButton");
        saveButton.onClick.AddListener(SaveChoice);
        cancelButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "CancelButton");
        cancelButton.onClick.AddListener(CancelChoice);
        exitButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ExitButton");
        exitButton.onClick.AddListener(Exit);
        
        //寻找content
        GameObject ScrollView = UIMethods.instance.FindObjectInChild(ActiveObj, "Scroll View");
        GameObject Viewport = UIMethods.instance.FindObjectInChild(ScrollView, "Viewport");
        content = UIMethods.instance.FindObjectInChild(Viewport, "Content");
        
        InitializeSlots();
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
    /// 保存自己的选择
    /// </summary>
    private void SaveChoice()
    {
        if (ChosenPlayers.Count == 0)
        {
            Debug.Log("没有选择任何学生");
            return;
        }
        
        // 收集所有选中玩家的ViewID
        // int[] targetViewIDs = new int[ChosenPlayers.Count];
        // for (int i = 0; i < ChosenPlayers.Count; i++)
        // {
        //     targetViewIDs[i] = ChosenPlayers[i].GetComponent<NetworkIdentity>().netId;
        // }

        // // 获取目标场景名
        // string sceneName = ClassManager.instance.isInClassroom ? 
        //     ClassManager.instance.currentActivity.sceneName : "Classroom";
        
        // // 创建事件内容，一次性发送所有信息
        // object[] content = new object[] { targetViewIDs, sceneName };
        // Debug.Log("发送场景切换请求，场景名：" + sceneName + "，目标ViewID：" + targetViewIDs[0]);
        // RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        // PhotonNetwork.RaiseEvent(EventCodes.ChangeSceneEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    
        // 选好学生后，开始活动
        ClassManager.instance.currentActivity.Start();

        // 关闭选择面板
        UIManager.instance.Pop(false);
    }

    /// <summary>
    /// 清空已选择学生
    /// </summary>
    private void CancelChoice()
    {
        foreach (var toggle in PlayerToggles)
        {
            toggle.isOn = false;
        }
    }

    /// <summary>
    /// 返回
    /// </summary>
    private void Exit()
    {
        UIManager.instance.Pop(false);
    }
    
    /// <summary>
    /// 当被勾选后的回调函数
    /// </summary>
    /// <param name="isOn"></param>是否勾选
    /// <param name="name"></param>勾选学生的名称
    void OnToggleValueChanged(bool isOn,PlayerManager player)
    {
        if (isOn)
        {
            ChosenPlayers.Add(player);
        }
        else
        {
            ChosenPlayers.Remove(player);
        }
    }
    
    /// <summary>
    /// 初始化学生的slots
    /// </summary>
    private void InitializeSlots()
    {
        // 先清空 content 下的所有子物体
        foreach (Transform child in content.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // 遍历所有玩家并创建 slot
        foreach (PlayerManager player in PlayerManager.allPlayers)
        {
            // 实例化 slot 并设置为 content 的子物体
            GameObject newSlot = GameObject.Instantiate(PlayerSlot, content.transform);
            newSlot.name = player.playerName;

            // 获取并设置玩家名称
            TextMeshProUGUI playerName = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(newSlot, "Name");
            playerName.text = player.playerName;
            
            //Toggle值改变监听函数,当被勾选时间，增加进列表中
            newSlot.GetComponent<Toggle>().onValueChanged.AddListener(isOn => OnToggleValueChanged(isOn, player));

            Toggle newToggle = newSlot.GetComponent<Toggle>();
            if (newToggle == null)
            {
                Debug.LogError($"Toggle component missing on {newSlot.name}");
            }
            PlayerToggles.Add(newToggle);
        }
    }
}
