using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Mirror;

public enum WhiteboardMode
{
    PPT,
    Image,
    Video,
    Sound,
    Monitor,
}

public class Whiteboard : MonoBehaviour
{
    public Image screen;

    [Header("PPT")]
    public Texture2D[] pptSlides;

    [Header("Monitor")]
    public Camera monitorCamera;
    public RenderTexture monitorTexture;
    
    // 用于监视器模式的可重用资源
    private Texture2D monitorTexture2D;
    private Sprite monitorSprite;   
    
    private WhiteboardMode currentMode;
    public WhiteboardMode CurrentMode
    {
        get { return currentMode; }
        set
        {
            currentMode = value;
            switch (currentMode)
            {
                case WhiteboardMode.PPT:
                    //UpdateSlideTexture((int)NetworkRoomManager.CurrentRoom.CustomProperties["CurrentSlideIndex"]);
                    break;
                case WhiteboardMode.Monitor:
                    // 取消原监视器的RenderTexture
                    if (monitorCamera != null)
                    {
                        monitorCamera.targetTexture = null;
                        monitorCamera.enabled = false;
                    }

                    // 获取监视器
                    foreach (var player in PlayerManager.allPlayers)
                    {
                        if (!player.IsStudent)
                        {
                            monitorCamera = player.transform.Find("Monitor").GetComponent<Camera>();
                            break;
                        }
                    }

                    // 设置监视器的RenderTexture
                    monitorCamera.targetTexture = monitorTexture;
                    monitorCamera.enabled = true;
                    break;
            }

            if (currentMode != WhiteboardMode.Monitor && monitorCamera != null)
            {
                monitorCamera.targetTexture = null;
            }
        }
    }

    private void Update()
    {
        if (currentMode == WhiteboardMode.Monitor)
        {
            // 如果还没有创建Texture2D，创建一个
            if (monitorTexture2D == null || monitorTexture2D.width != monitorTexture.width || monitorTexture2D.height != monitorTexture.height)
            {
                if (monitorTexture2D != null)
                {
                    Destroy(monitorTexture2D);
                }
                monitorTexture2D = new Texture2D(monitorTexture.width, monitorTexture.height);
            }

            // 更新Texture2D
            RenderTexture.active = monitorTexture;
            monitorTexture2D.ReadPixels(new Rect(0, 0, monitorTexture.width, monitorTexture.height), 0, 0);
            monitorTexture2D.Apply();

            // 创建新的Sprite
            if (monitorSprite != null)
            {
                Destroy(monitorSprite);
            }
            monitorSprite = Sprite.Create(monitorTexture2D, new Rect(0, 0, monitorTexture2D.width, monitorTexture2D.height), new Vector2(0.5f, 0.5f));
            screen.sprite = monitorSprite;
        }
    }

    private void OnDestroy()
    {
        // 清理资源
        if (monitorTexture2D != null)
        {
            Destroy(monitorTexture2D);
        }
        if (monitorSprite != null)
        {
            Destroy(monitorSprite);
        }
    }

    public void OnJoinedRoom()
    {
        // string pptFilePath = ClassManager.instance.pptFilePath;
        // string pptFullPath = Path.Combine(Application.streamingAssetsPath, "PPTs/" + pptFilePath);
        // string outputDir = Path.Combine(Path.GetDirectoryName(pptFullPath), Path.GetFileNameWithoutExtension(pptFilePath) + "_Images"); //一个ppt对应一个文件夹

        // //获取ppt图片
        // pptSlides = PPTToImageConverter.instance.ConvertPPTToImage(pptFullPath, outputDir);

        // if (pptSlides.Length == 0)
        // {
        //     Debug.Log("ppt的页数不正确");
        // }
        // screen = GetComponentInChildren<Image>();
        
        // base.OnJoinedRoom();

        // // 如果有已经存在的页码，更新到这个页码
        // if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CurrentSlideIndex"))
        // {
        //     int currentIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentSlideIndex"];
        //     UpdateSlideTexture(currentIndex);
        // }
        // else
        // {
        //     Hashtable props = new Hashtable { { "CurrentSlideIndex", 0 } };
        //     PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        //     UpdateSlideTexture(0); // 默认显示第一页
        // }
    }

    public void OnEnable()
    {
        // base.OnEnable();
        // // 注册事件处理器
        // PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        // EventHandler.Register<ChangeSlideEvent>(OnChangeSlideEvent);
        // EventHandler.Register<ChangeSceneEvent>(OnChangeSceneEvent);
    }

    public void OnDisable()
    {
        // base.OnDisable();
        // // 取消注册事件处理器
        // PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
        // EventHandler.Unregister<ChangeSlideEvent>(OnChangeSlideEvent);
        // EventHandler.Unregister<ChangeSceneEvent>(OnChangeSceneEvent);
    }

    // private void OnEvent(EventData photonEvent)
    // {
    //     if (photonEvent.Code == EventCodes.SlideChangeEventCode)
    //     {
    //         object[] data = (object[])photonEvent.CustomData;
    //         int newIndex = (int)data[0];
    //         UpdateSlideTexture(newIndex);
    //     }
    // }

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
        // UpdateSlideTexture((int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentSlideIndex"]);
    }

    public void OnChangeSlideEvent(ChangeSlideEvent @event)
    {
        ChangeSlide(@event.changeNum);
    }

    public void ChangeSlide(int num)
    {
        // int currentIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["CurrentSlideIndex"];
        // currentIndex = (currentIndex + num + pptSlides.Length) % pptSlides.Length;
        // UpdateSlideTexture(currentIndex);

        // // 更新房间属性，保存当前页码
        // Hashtable props = new Hashtable { { "CurrentSlideIndex", currentIndex } };
        // PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        // // 发送事件通知其他客户端
        // RaiseSlideChangeEvent(currentIndex);
    }

    private void RaiseSlideChangeEvent(int newIndex)
    {
        // 创建事件内容
        // object[] content = new object[] { newIndex };
        // PhotonNetwork.RaiseEvent(EventCodes.SlideChangeEventCode, content, RaiseEventOptions.Default, SendOptions.SendReliable);
    }

    private void OnChangeSceneEvent(ChangeSceneEvent @event)
    {
        // if (!@event.includePlayers.Contains(PlayerController.localPlayer.GetComponent<NetworkIdentity>().netId))
        // {
        //     if (@event.sceneName == "Classroom")
        //     {
        //         // 切换到白板模式
        //         CurrentMode = WhiteboardMode.PPT;
        //     }
        //     else
        //     {
        //         // 切换到监视器模式
        //         CurrentMode = WhiteboardMode.Monitor;
        //     }
        // }
    }
}
