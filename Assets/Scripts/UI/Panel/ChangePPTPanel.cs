using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangePPTPanel : BasePanel
{
    private TMP_Dropdown pptPathDropdown;
    private Button confirmButton;
    private Button restoreDefaultButton;
    private Button backButton;

    private string selectedPPTPath;
    private string defaultPPTPath; // 默认PPT路径，根据实际情况修改

    public ChangePPTPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        defaultPPTPath = ClassManager.instance.pptFilePath;
        // 绑定UI组件
        pptPathDropdown = UIMethods.instance.GetOrAddComponentInChild<TMP_Dropdown>(ActiveObj, "PPTPathDropdown");
        confirmButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ConfirmButton");
        restoreDefaultButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "RestoreDefaultButton");
        backButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "BackButton");

        // 添加事件监听
        pptPathDropdown.onValueChanged.AddListener(OnPPTPathChanged);
        confirmButton.onClick.AddListener(ConfirmSelection);
        restoreDefaultButton.onClick.AddListener(RestoreDefault);
        backButton.onClick.AddListener(GoBack);

        // 初始化下拉框选项
        LoadPPTOptions();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBack();
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        InteractionManager.instance.RaycastClosed = true;
        PlayerManager.localPlayer.playerController.enabled = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        InteractionManager.instance.RaycastClosed = false;
        PlayerManager.localPlayer.playerController.enabled = true;
    }

    private void LoadPPTOptions()
    {
        // 清空现有选项
        pptPathDropdown.ClearOptions();

        // 获取PPT文件路径列表（这里需要根据实际项目来实现）
        List<string> pptPaths = GetAvailablePPTPaths();

        // 添加到下拉框
        pptPathDropdown.AddOptions(pptPaths);

        // 设置初始选项
        if (pptPaths.Count > 0)
        {
            string currentPPTPath = GetCurrentPPTPath();
            int currentIndex = pptPaths.IndexOf(currentPPTPath);
            if (currentIndex >= 0)
            {
                pptPathDropdown.value = currentIndex;
            }
            selectedPPTPath = pptPaths[pptPathDropdown.value];
        }
    }

    private List<string> GetAvailablePPTPaths()
    {
        // 使用HashSet避免重复项
        HashSet<string> uniquePaths = new HashSet<string>();
        
        // 获取StreamingAssetsPath/PPTs文件夹下的所有PPT文件
        string pptDirectory = Path.Combine(Application.streamingAssetsPath, "PPTs");
        
        if (Directory.Exists(pptDirectory))
        {
            // 获取目录中的所有文件
            string[] allFiles = Directory.GetFiles(pptDirectory);
            Debug.Log("找到文件总数: " + allFiles.Length);
            
            // 筛选出PPT和PPTX文件
            foreach (string file in allFiles)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (extension == ".ppt" || extension == ".pptx")
                {
                    // 获取不含路径但包含扩展名的文件名
                    string fileName = Path.GetFileName(file);
                    Debug.Log("添加PPT文件: " + fileName);
                    uniquePaths.Add(fileName);
                }
            }
        }
        else
        {
            Debug.LogWarning("PPT目录不存在: " + pptDirectory);
            // 添加默认选项，以防目录不存在
            uniquePaths.Add(defaultPPTPath);
        }
        
        // 如果没有找到任何PPT文件，至少添加默认路径
        if (uniquePaths.Count == 0)
        {
            Debug.Log("未找到任何PPT文件，添加默认选项");
            uniquePaths.Add(defaultPPTPath);
        }

        // 转换为List并返回
        List<string> paths = new List<string>(uniquePaths);
        return paths;
    }

    private string GetCurrentPPTPath()
    {
        return ClassManager.instance.pptFilePath;
    }

    private void OnPPTPathChanged(int index)
    {
        List<string> options = new List<string>();
        foreach (TMP_Dropdown.OptionData option in pptPathDropdown.options)
        {
            options.Add(option.text);
        }

        if (index >= 0 && index < options.Count)
        {
            selectedPPTPath = options[index];
            Debug.Log("Selected PPT Path: " + selectedPPTPath);
        }
    }

    private void ConfirmSelection()
    {
        if (!string.IsNullOrEmpty(selectedPPTPath))
        {
            // 应用选择的PPT路径
            ApplyPPTPath(selectedPPTPath);

            // 关闭面板
            GoBack();
        }
        else
        {
            Debug.LogWarning("No PPT path selected!");
        }
    }

    private void ApplyPPTPath(string path)
    {
        // 触发PPT路径变更事件
        EventHandler.Trigger(new ChangePPTPathEvent() { pptPath = path });
        Debug.Log("Applying PPT Path: " + path);
    }

    private void RestoreDefault()
    {
        // 恢复默认PPT路径
        int defaultIndex = pptPathDropdown.options.FindIndex(option => option.text == GetCurrentPPTPath());
        if (defaultIndex >= 0)
        {
            pptPathDropdown.value = defaultIndex;
            selectedPPTPath = defaultPPTPath;
        }
    }

    private void GoBack()
    {
        UIManager.instance.Pop(false);
    }
}

