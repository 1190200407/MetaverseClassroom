using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class ClassManager : UnitySingleton<ClassManager>
{
    public bool isInClassroom = true;
    public string pptFilePath;//PPT文件的路径
    public string currentScene;
    public string nextScene;

    public BaseActivity currentActivity;

    public void StartCourse()
    {
        SceneLoader.instance.LoadSceneFromXml("Classroom");
        currentScene = "Classroom";
        isInClassroom = true;
    }

    public void StartActivity(string activityName)
    {
        // 暂时以这个为例子
        currentActivity = new ActingActivity(activityName, "英语情景", true, "FFK");
        if (currentActivity.needToChangeScene)
        {
            // 需要选择学生
            UIManager.instance.Push(new ChoosePlayerPanel(new UIType("Panels/ChoosePlayerPanel", "ChoosePlayerPanel")));
        }
        else
        {
            currentActivity.Start();
        }
    }

    public void StartSceneTransition(string sceneName)
    {
        UIManager.instance.Push(new TransitionPanel(new UIType("Panels/TransitionPanel", "TransitionPanel")));
        nextScene = sceneName;
    }

    public void ChangeScene()
    {
        SceneLoader.instance.LoadSceneFromXml(nextScene);
        currentScene = nextScene;
        PlayerController.localPlayer.CurrentScene = nextScene;
        isInClassroom = !isInClassroom; // 切换场景后，教室状态取反, 之后可能出现一教室多场景情况，需要切换成别的判断方式
    }
}
