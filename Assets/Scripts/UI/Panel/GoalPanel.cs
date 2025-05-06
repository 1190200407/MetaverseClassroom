using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalPanel : BasePanel
{
    private int goalIndex; //任务目标的总条数
    private int currentIndex; //现在显示的条数
    
    private TextMeshProUGUI goalText;
    private Button confirmButton;
    
    public GoalPanel(UIType uiType) : base(uiType)
    {
    }

    public override void OnStart()
    {
        base.OnStart();
        //获取相关组件
        confirmButton = UIMethods.instance.GetOrAddComponentInChild<Button>(ActiveObj, "ConfirmButton");
        goalText = UIMethods.instance.GetOrAddComponentInChild<TextMeshProUGUI>(ActiveObj, "GoalText");
    }

    /// <summary>
    /// 设置任务目标
    /// </summary>
    /// <param name="text"></param>任务目标描述
    public void SetGoalText(string text)
    {
        goalText.text = "任务目标：" + text;
    }
    
}
