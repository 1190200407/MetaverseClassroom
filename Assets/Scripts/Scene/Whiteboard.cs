using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.IO;
public class Whiteboard : MonoBehaviourPunCallbacks
{
    public Texture2D[] pptSlides;
    public Image screen;
    private const byte SlideChangeEventCode = 1; // 事件代码

    void Start()
    {
        
    }

    public override void OnJoinedRoom()
    {
        string pptFilePath = ClassManager.instance.pptFilePath;
        string pptFullPath = Path.Combine(Application.streamingAssetsPath, "PPTs/" + pptFilePath);
        string outputDir = Path.Combine(Path.GetDirectoryName(pptFullPath), Path.GetFileNameWithoutExtension(pptFilePath) + "_Images"); //一个ppt对应一个文件夹

        //获取ppt图片
        pptSlides = PPTToImageConverter.instance.ConvertPPTToImage(pptFullPath, outputDir);

        if (pptSlides.Length == 0)
        {
            Debug.Log("ppt的页数不正确");
        }
        screen = GetComponentInChildren<Image>();
        
        base.OnJoinedRoom();

        // 如果有已经存在的页码，更新到这个页码
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentSlideIndex"))
        {
            int currentIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentSlideIndex"];
            UpdateSlideTexture(currentIndex);
        }
        else
        {
            Hashtable props = new Hashtable { { "CurrentSlideIndex", 0 } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            UpdateSlideTexture(0); // 默认显示第一页
        }
    }

    public void OnChangeSlideEvent(ChangeSlideEvent @event)
    {
        ChangeSlide(@event.changeNum);
    }

    public void ChangeSlide(int num)
    {
        int currentIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentSlideIndex"];
        currentIndex = (currentIndex + num + pptSlides.Length) % pptSlides.Length;
        UpdateSlideTexture(currentIndex);

        // 更新房间属性，保存当前页码
        Hashtable props = new Hashtable { { "CurrentSlideIndex", currentIndex } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // 发送事件通知其他客户端
        RaiseSlideChangeEvent(currentIndex);
    }

    private void RaiseSlideChangeEvent(int newIndex)
    {
        // 创建事件内容
        object[] content = new object[] { newIndex };
        PhotonNetwork.RaiseEvent(SlideChangeEventCode, content, RaiseEventOptions.Default, SendOptions.SendReliable);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        // 注册事件处理器
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        EventHandler.Register<ChangeSlideEvent>(OnChangeSlideEvent);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        // 取消注册事件处理器
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        EventHandler.Unregister<ChangeSlideEvent>(OnChangeSlideEvent);
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SlideChangeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            int newIndex = (int)data[0];
            UpdateSlideTexture(newIndex);
        }
    }

    private void UpdateSlideTexture(int index)
    {
        if (pptSlides.Length > 0 && index < pptSlides.Length)
        {
            Texture2D texture = pptSlides[index];
            screen.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    private void UpdateSlideTexture()
    {
        UpdateSlideTexture((int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentSlideIndex"]);
    }
}
