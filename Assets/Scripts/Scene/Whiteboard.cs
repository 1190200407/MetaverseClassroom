using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

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
    public int currentSlideIndex;

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
                    UpdateSlideTexture(GetCurrentSlideIndex());   
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

    private void OnEnable()
    {
        EventHandler.Register<RoomPropertyChangeEvent>(OnRoomPropertyChange);
        EventHandler.Register<ChangeSlideEvent>(OnChangeSlideEvent);
        EventHandler.Register<SceneLoadedEvent>(OnSceneLoaded);
    }

    private void OnDisable()
    {
        EventHandler.Unregister<RoomPropertyChangeEvent>(OnRoomPropertyChange);
        EventHandler.Unregister<ChangeSlideEvent>(OnChangeSlideEvent);
        EventHandler.Unregister<SceneLoadedEvent>(OnSceneLoaded);
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

    private void OnSceneLoaded(SceneLoadedEvent @event)
    {
        // 初始化PPT
        string pptFilePath = ClassManager.instance.pptFilePath;
        string pptFullPath = Path.Combine(Application.streamingAssetsPath, "PPTs/" + pptFilePath);

        // 如果pptFullPath不存在，则退出
        if (!File.Exists(pptFullPath))
        {
            Debug.LogError("PPT文件不存在: " + pptFullPath);
            return;
        }

        string outputDir = Path.Combine(Path.GetDirectoryName(pptFullPath), Path.GetFileNameWithoutExtension(pptFilePath) + "_Images");

        // 获取ppt图片
        pptSlides = PPTToImageConverter.instance.ConvertPPTToImage(pptFullPath, outputDir);

        if (pptSlides.Length == 0)
        {
            Debug.Log("ppt的页数不正确");
        }
        screen = GetComponentInChildren<Image>();
        UpdateSlideTexture(GetCurrentSlideIndex());
    }

    public int GetCurrentSlideIndex()
    {
        ClassManager.instance.CmdGetRoomProperty("CurrentSlideIndex", PlayerManager.localPlayer.connectionToClient);
        string value = ClassManager.instance.propertyValue;
        if (value != null && int.TryParse(value, out int index))
        {
            currentSlideIndex = index;
            return index;
        }
        return 0;
    }

    private void UpdateSlideTexture(int index)
    {
        if (pptSlides.Length > 0 && index < pptSlides.Length)
        {
            Texture2D texture = pptSlides[index];
            screen.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    public void OnChangeSlideEvent(ChangeSlideEvent @event)
    {
        ChangeSlide(@event.changeNum);
    }

    public void ChangeSlide(int num)
    {
        currentSlideIndex = (currentSlideIndex + num + pptSlides.Length) % pptSlides.Length;
        
        // 更新当前页码
        ClassManager.instance.CommandSetRoomProperty("CurrentSlideIndex", currentSlideIndex.ToString());
        
        // 更新显示
        UpdateSlideTexture(currentSlideIndex);
    }

    private void OnRoomPropertyChange(RoomPropertyChangeEvent @event)
    {
        if (@event.key == "CurrentSlideIndex")
        {
            currentSlideIndex = int.Parse(@event.value);
            UpdateSlideTexture(currentSlideIndex);
        }
    }

    private void OnChangeSceneEvent(ChangeSceneEvent @event)
    {
        if (!@event.includePlayers.Any(id => id == PlayerManager.localPlayer.netId))
        {
            if (@event.sceneName == "Classroom")
            {
                // 切换到白板模式
                CurrentMode = WhiteboardMode.PPT;
            }
            else
            {
                // 切换到监视器模式
                CurrentMode = WhiteboardMode.Monitor;
            }
        }
    }
}
