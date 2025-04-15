using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

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
    private PlayerModel player;

    private TMP_InputField ipInput;
    private TMP_InputField portInput;
    private Button clientButton;

    public StartPanel(UIType uiType) : base(uiType)
    {   
    }

    public override void OnStart()
    {
        base.OnStart();
        // 获取控制UI
        menuPanel = ActiveObj.transform.Find("Menu").gameObject;
        roomPanel = ActiveObj.transform.Find("Room").gameObject;
        startButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "StartButton");
        backButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "BackButton");
        enterButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "EnterButton");
        nameInput = UIMethods.instance.GetOrAddComponentInChild<TMP_InputField>(ActiveObj, "NameInputField");
        nameInput.text = PlayerPrefs.GetString("NickName", string.Empty);

        // 获取角色选择UI
        studentFrame = UIMethods.instance.GetOrAddComponentInChild<Image>(ActiveObj, "Student");
        teacherFrame = UIMethods.instance.GetOrAddComponentInChild<Image>(ActiveObj, "Teacher");
        studentButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "StudentButton");
        teacherButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "TeacherButton");
        studentButton.onClick.AddListener(() => ChangeStatus(true));
        teacherButton.onClick.AddListener(() => ChangeStatus(false));
        ChangeStatus(PlayerPrefs.GetInt("IsStudent", 1) == 1);

        // 获取角色选择UI
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
        player = GameObject.Instantiate(Resources.Load<PlayerModel>("Prefabs/PlayerModel"), Vector3.one * 1000f, Quaternion.identity);
        Choose(PlayerPrefs.GetString("ChosenCharacter", content.GetChild(0).name));

        // 获取连接UI
        ipInput = UIMethods.instance.GetOrAddComponentInChild<TMP_InputField>(ActiveObj, "IPInputField");
        portInput = UIMethods.instance.GetOrAddComponentInChild<TMP_InputField>(ActiveObj, "PortInputField");
        clientButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ClientButton");
        
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
        // 添加事件
        startButton.onClick.AddListener(ShowRoom);
        backButton.onClick.AddListener(ShowMenu);
        enterButton.onClick.AddListener(StartHost);
        clientButton.onClick.AddListener(StartClient);

        if (PlayerManager.localPlayer != null)
            PlayerManager.localPlayer.playerController.enabled = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        // 移除事件
        startButton.onClick.RemoveListener(ShowRoom);
        backButton.onClick.RemoveListener(ShowMenu);
        enterButton.onClick.RemoveListener(StartHost);
        clientButton.onClick.RemoveListener(StartClient);

        if (PlayerManager.localPlayer != null)
            PlayerManager.localPlayer.playerController.enabled = true;
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

    public void StartHost()
    {
        // 获取输入框中的名称
        string playerName = nameInput.text;
        if (playerName.Length < 2)
        {
            UIManager.instance.ShowMessage("名字必须不少于2个字符", 1.5f);
            return;
        }

        // 设置玩家名称
        PlayerPrefs.SetString("NickName", playerName);
        
        // 开始课程
        NetworkManagerClassroom.singleton.StartHost();

        // 切换到游戏界面
        UIManager.instance.Pop(false);
        if (!GameSettings.instance.isVR)
            UIManager.instance.Push(new GamePanel(new UIType("Panels/GamePanel", "GamePanel")));

        // PhotonNetwork.NickName = playerName;
        // ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable()
        // {
        //     {"CharacterName", chosenName},
        //     {"IsStudent", PlayerPrefs.GetInt("IsStudent", 1) == 1}
        // };
        // PhotonNetwork.SetPlayerCustomProperties(hashtable);
        // PlayerPrefs.SetString("NickName", playerName);
        // NetworkManagerClassroom.singeleton.Connect();

        // // 开始异步连接
        // UIManager.instance.StartCoroutine(ConnectToNetwork());
    }

    public void StartClient()
    {
        string ip = ipInput.text;
        string port = portInput.text;
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
        {
            UIManager.instance.ShowMessage("请输入IP和端口", 1f);
            return;
        }

        // 设置IP和端口
        NetworkManagerClassroom.singleton.networkAddress = ip;
        if (ushort.TryParse(port, out ushort portNumber) && Transport.active is PortTransport portTransport)
            portTransport.Port = portNumber;

        // 开始课程
        NetworkManagerClassroom.singleton.StartClient();

        // 切换到游戏界面
        UIManager.instance.Pop(false);
        if (!GameSettings.instance.isVR)
            UIManager.instance.Push(new GamePanel(new UIType("Panels/GamePanel", "GamePanel")));
    }

    // private IEnumerator ConnectToNetwork()
    // {
    //     UIManager.instance.ShowMessage("连接中..", float.MaxValue);
    //     UIManager.instance.DisableInteraction();

    //     // 异步连接到网络
    //     yield return new WaitUntil(() => NetworkManagerClassroom.singeleton.Connection != ConnectResult.Connecting);
    //     //yield return new WaitForSecondsRealtime(5f);

    //     if (NetworkManagerClassroom.singeleton.Connection == ConnectResult.Connected)
    //     {
    //         // 连接成功后执行以下逻辑
    //         //加入房间
    //         PhotonNetwork.JoinOrCreateRoom("Classroom", new Photon.Realtime.RoomOptions() { MaxPlayers = 4 }, default);
    //         ClassManager.instance.StartCourse();
            
    //         UIManager.instance.ShowMessage("连接成功", 1f);
    //         UIManager.instance.Pop(false);
    //         if (!GameSettings.instance.isVR)
    //             UIManager.instance.Push(new GamePanel(new UIType("Panels/GamePanel", "GamePanel")));
    //     }
    //     else
    //     {
    //         UIManager.instance.ShowMessage("连接失败", 1f);
    //         UIManager.instance.EnableInteraction();
    //     }
    // }

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
