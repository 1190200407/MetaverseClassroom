using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseActivityPanel : BasePanel
{
    public List<Button> activityButtons;
    public List<Image> activityFrames;
    public int selectedIndex;
    public Button confirmButton;
    public Button exitButton;

    public ChooseActivityPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        confirmButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ConfirmButton");
        confirmButton.onClick.AddListener(OnConfirm);

        exitButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ExitButton");
        exitButton.onClick.AddListener(OnExit);

        activityButtons = new List<Button>();
        activityFrames = new List<Image>();
        GameObject activities = UIMethods.instance.FindObjectInChild(ActiveObj, "Content");
        for (int i = 0; i < activities.transform.childCount; i++)
        {
            Button button = activities.transform.GetChild(i).GetComponent<Button>();
            activityButtons.Add(button);
            int index = i;
            button.onClick.AddListener(() => OnActivityButtonClick(index));

            Image frame = activities.transform.GetChild(i).GetChild(0).GetComponent<Image>();
            activityFrames.Add(frame);
            frame.gameObject.SetActive(false);
        }

        selectedIndex = 0;
        activityFrames[selectedIndex].gameObject.SetActive(true);

        // TODO 获取活动列表

        // TODO 初始化活动列表
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

    private void OnActivityButtonClick(int index)
    {
        activityFrames[selectedIndex].gameObject.SetActive(false);
        selectedIndex = index;
        activityFrames[selectedIndex].gameObject.SetActive(true);
    }

    private void OnConfirm()
    {
        if (selectedIndex == 0)
        {
            NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.EndActivity);
        }
        else
        {
            NetworkMessageHandler.instance.BroadcastMessage(NetworkMessageType.StartActivity, new StartActivityMessageData() { activityIndex = selectedIndex - 1 });
        }
    }

    private void OnExit()
    {
        UIManager.instance.Pop(false);
    }
}
