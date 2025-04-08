using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    private PlayerData playerData = new PlayerData(50,50,50);
    
    private Button activityButton;
    private Button lastPageButton;
    private Button nextPageButton;
    private Button saveButton;
    private Button restoreButton;
    
    //基本参数滚动条
    private Slider moveSpeedSlider;
    private Slider cameraSpeedSlider;
    private Slider volumeSlider;
    
    //基本参数数值
    private TMP_Text moveSpeedText;
    private TMP_Text cameraSpeedText;
    private TMP_Text volumeText;
    
    public PausePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        // 绑定按钮
        activityButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ActivityButton");
        activityButton.onClick.AddListener(StartActivity);
        saveButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "SaveButton");
        saveButton.onClick.AddListener(SaveData);
        restoreButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "RestoreButton");
        restoreButton.onClick.AddListener(RestoreDefaults);
        lastPageButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "LastPageButton");
        lastPageButton.onClick.AddListener(LastPage);
        nextPageButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "NextPageButton");
        nextPageButton.onClick.AddListener(NextPage);
        
        // 绑定 Slider
        moveSpeedSlider = UIMethods.instance.GetOrAddComponentInChild<Slider>(ActiveObj, "MoveSpeedSlider");
        cameraSpeedSlider = UIMethods.instance.GetOrAddComponentInChild<Slider>(ActiveObj, "CameraSpeedSlider");
        volumeSlider = UIMethods.instance.GetOrAddComponentInChild<Slider>(ActiveObj, "VolumeSlider");

        moveSpeedText = UIMethods.instance.GetOrAddComponentInChild<TMP_Text>(ActiveObj, "MoveSpeedText");
        cameraSpeedText = UIMethods.instance.GetOrAddComponentInChild<TMP_Text>(ActiveObj, "CameraSpeedText");
        volumeText = UIMethods.instance.GetOrAddComponentInChild<TMP_Text>(ActiveObj, "VolumeText");
        
        // 添加事件监听
        moveSpeedSlider.onValueChanged.AddListener(OnMoveSpeedChanged);
        cameraSpeedSlider.onValueChanged.AddListener(OnCameraSensitivityChanged);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape) || 
            (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()))
        {
            UIManager.instance.Pop(false);
        }
    }

    public override void OnEnable()
    {
        SetUi();
        base.OnEnable();
        InteractionManager.instance.RaycastClosed = true;
        PlayerManager.localPlayer.playerController.enabled = false;
    }

    public override void OnDisable() {
        base.OnDisable();
        InteractionManager.instance.RaycastClosed = false;
        PlayerManager.localPlayer.playerController.enabled = true;
    }
    private void SetUi()
    {
        PlayerData data = PlayerController.GetData();
        UpdateText(moveSpeedText,data.moveSpeed);
        UpdateText(cameraSpeedText,data.cameraSensitivity);
        UpdateText(volumeText,data.volume);
        moveSpeedSlider.value = data.moveSpeed;
        cameraSpeedSlider.value = data.cameraSensitivity;
        volumeSlider.value = data.volume;
    }
    
    private void OnMoveSpeedChanged(float value)
    {
        playerData.moveSpeed = value;
        UpdateText(moveSpeedText,value);
    }

    private void OnCameraSensitivityChanged(float value)
    {
        playerData.cameraSensitivity = value;
        UpdateText(cameraSpeedText,value);
    }

    private void OnVolumeChanged(float value)
    {
        playerData.volume = value;
        UpdateText(volumeText,value);
    }

    private void UpdateText(TMP_Text text,float value)
    {
        text.text = ((int)value).ToString();
    }

    private void SaveData()
    {
        EventHandler.Trigger(new PlayerChangeDataEvent() { data = playerData });
    }

    private void RestoreDefaults()
    {
        EventHandler.Trigger(new PlayerChangeDataEvent() { data = new PlayerData(50,50,50) });
        SetUi();
    }

    public void StartActivity()
    {
        if (PlayerManager.localPlayer.HavePermission(Permission.StartActivity))
        {
            UIManager.instance.Pop(false);
            ClassManager.instance.StartActivity("EnglishTalking");
        }
    }

    public void LastPage()
    {
        if (PlayerManager.localPlayer.HavePermission(Permission.SwitchPPT))
        {
            EventHandler.Trigger(new ChangeSlideEvent{ changeNum = -1 });
        }
    }

    public void NextPage()
    {
        if (PlayerManager.localPlayer.HavePermission(Permission.SwitchPPT))
        {
            EventHandler.Trigger(new ChangeSlideEvent{ changeNum = 1 });
        }
    }
}
