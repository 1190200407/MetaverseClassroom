using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    private Button changeSceneButton;
    
    public PausePanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        changeSceneButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ChangeSceneButton");
        changeSceneButton.onClick.AddListener(ChangeScene);
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
        base.OnEnable();
        InteractionManager.instance.RaycastClosed = true;
        PlayerController.localPlayer.enabled = false;
    }

    public override void OnDisable() {
        base.OnDisable();
        InteractionManager.instance.RaycastClosed = false;
        PlayerController.localPlayer.enabled = true;
    }

    public void ChangeScene()
    {
        SceneLoader.instance.xmlPath = "FFK";
        SceneLoader.instance.LoadSceneFromXml();
    }
}
