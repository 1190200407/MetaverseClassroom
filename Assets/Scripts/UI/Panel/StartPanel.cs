using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using TMPro;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
    private GameObject menuPanel;
    private GameObject roomPanel;

    private Button startButton;
    private Button backButton;
    private Button enterButton;

    private Image studentFrame;
    private Button studentButton;
    private Image teacherFrame;
    private Button teacherButton;
    
    private TMP_InputField nameInput;

    private Dictionary<string, Image> nameIconDict;
    private string chosenName;
    private PlayerManager player;

    public StartPanel(UIType uiType) : base(uiType)
    {   
    }

    public override void OnStart()
    {
        base.OnStart();
        menuPanel = ActiveObj.transform.Find("Menu").gameObject;
        roomPanel = ActiveObj.transform.Find("Room").gameObject;
        startButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "StartButton");
        backButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "BackButton");
        enterButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "EnterButton");
        nameInput = UIMethods.instance.GetOrAddComponentInChild<TMP_InputField>(ActiveObj, "NameInputField");
        nameInput.text = PlayerPrefs.GetString("NickName", string.Empty);

        studentFrame = UIMethods.instance.GetOrAddComponentInChild<Image>(ActiveObj, "Student");
        teacherFrame = UIMethods.instance.GetOrAddComponentInChild<Image>(ActiveObj, "Teacher");
        studentButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "StudentButton");
        teacherButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "TeacherButton");
        studentButton.onClick.AddListener(() => ChangeStatus(true));
        teacherButton.onClick.AddListener(() => ChangeStatus(false));
        ChangeStatus(PlayerPrefs.GetInt("IsStudent", 1) == 1);

        nameIconDict = new Dictionary<string, Image>();
        Transform content = ActiveObj.transform.Find("Room/SelectPanel/Scroll View/Viewport/Content");
        for (int i = 0; i < content.childCount; i++)
        {
            Transform child = content.GetChild(i);
            nameIconDict.Add(child.name, child.GetComponent<Image>());
            child.GetComponent<Button>().onClick.AddListener(() =>
            {
                Choose(child.name);
            });
        }
        player = GameObject.Instantiate(Resources.Load<PlayerManager>("Prefabs/PlayerModel"), Vector3.one * 1000f, Quaternion.identity);
        Choose(PlayerPrefs.GetString("ChosenCharacter", content.GetChild(0).name));
        
        // 初始化界面状态
        Show(true);
    }

    public override void OnDestroy()
    {
        GameObject.Destroy(player.gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        startButton.onClick.AddListener(ShowRoom);
        backButton.onClick.AddListener(ShowMenu);
        enterButton.onClick.AddListener(EnterGame);

        if (PlayerController.localPlayer != null)
            PlayerController.localPlayer.enabled = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        startButton.onClick.RemoveListener(ShowRoom);
        backButton.onClick.RemoveListener(ShowMenu);
        enterButton.onClick.RemoveListener(EnterGame);

        if (PlayerController.localPlayer != null)
            PlayerController.localPlayer.enabled = true;
    }

    private void Show(bool showMenu)
    {
        menuPanel.SetActive(showMenu);
        roomPanel.SetActive(!showMenu);
    }

    private void ShowMenu()
    {
        Show(true);
    }

    private void ShowRoom()
    {
        Show(false);
    }

    public void EnterGame()
    {
        // 获取输入框中的名称
        string playerName = nameInput.text;
        if (playerName.Length < 2)
        {
            UIManager.instance.ShowMessage("名字必须不少于2个字符", 1.5f);
            return;
        }

        PhotonNetwork.NickName = playerName;
        ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable()
        {
            {"CharacterName", chosenName},
            {"IsStudent", PlayerPrefs.GetInt("IsStudent", 1) == 1}
        };
        PhotonNetwork.SetPlayerCustomProperties(hashtable);
        PlayerPrefs.SetString("NickName", playerName);
        NetworkManager.instance.Connect();

        // 开始异步连接
        UIManager.instance.StartCoroutine(ConnectToNetwork());
    }

    private IEnumerator ConnectToNetwork()
    {
        UIManager.instance.ShowMessage("连接中..", float.MaxValue);
        UIManager.instance.DisableInteraction();

        // 异步连接到网络
        yield return new WaitUntil(() => NetworkManager.instance.Connection != ConnectResult.Connecting);
        //yield return new WaitForSecondsRealtime(5f);

        if (NetworkManager.instance.Connection == ConnectResult.Connected)
        {
            // 连接成功后执行以下逻辑
            //加入房间
            PhotonNetwork.JoinOrCreateRoom("Classroom", new Photon.Realtime.RoomOptions() { MaxPlayers = 4 }, default);
            ClassManager.instance.StartCourse();
            
            UIManager.instance.ShowMessage("连接成功", 1f);
            UIManager.instance.Pop(false);
            if (!GameSettings.instance.isVR)
                UIManager.instance.Push(new GamePanel(new UIType("Panels/GamePanel", "GamePanel")));
        }
        else
        {
            UIManager.instance.ShowMessage("连接失败", 1f);
            UIManager.instance.EnableInteraction();
        }
    }

    public void Choose(string name)
    {
        if (chosenName != null && nameIconDict.ContainsKey(chosenName))
        {
            nameIconDict[chosenName].enabled = false;
        }

        chosenName = name;
        if (nameIconDict.ContainsKey(chosenName))
            nameIconDict[chosenName].enabled = true;
        player.CharacterName = chosenName;
        PlayerPrefs.SetString("ChosenCharacter", chosenName);
    }

    public void ChangeStatus(bool isStudent)
    {
        teacherFrame.enabled = !isStudent;
        studentFrame.enabled = isStudent;
        PlayerPrefs.SetInt("IsStudent", isStudent ? 1 : 0);
    }
}
