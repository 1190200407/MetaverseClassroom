using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPanel : BasePanel
{
    private Slider loadingSlider;
    private TMP_Text loadingText;
    private TMP_Text loadingProgress;

    public LoadPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        loadingSlider = UIMethods.instance.GetOrAddComponentInChild<Slider>(ActiveObj, "LoadingSlider");
        loadingText = UIMethods.instance.GetOrAddComponentInChild<TMP_Text>(ActiveObj, "LoadingText");
        loadingProgress = UIMethods.instance.GetOrAddComponentInChild<TMP_Text>(ActiveObj, "LoadingProgress");
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UpdateLoadingProgress(0);
        PlayerManager.localPlayer.playerController.enabled = false;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PlayerManager.localPlayer.playerController.enabled = true;
    }

    public override void OnUpdate()
    {
        UpdateLoadingProgress(SceneLoader.instance.loadingProgress);
    }
    
    public void UpdateLoadingProgress(float progress)
    {
        loadingSlider.value = progress;
        loadingProgress.text = $"{progress * 100:F0}%";
    }
}
