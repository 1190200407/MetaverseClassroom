using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class CourseManager : UnitySingleton<CourseManager>
{
    public bool isInClassroom = true;
    public string currentScene;

    public void StartCourse()
    {
        SceneLoader.instance.LoadSceneFromXml("Classroom");
        currentScene = "Classroom";
        isInClassroom = true;
    }

    public void StartSceneTransition()
    {
        UIManager.instance.Push(new TransitionPanel(new UIType("Panels/TransitionPanel", "TransitionPanel")));
    }

    public void ChangeScene()
    {
        if (isInClassroom)
        {
            SceneLoader.instance.LoadSceneFromXml("FFK");
            currentScene = "FFK";
            EventHandler.Trigger(new ChangeSceneEvent(){ sceneName = "FFK" });
            PlayerController.localPlayer.CurrentScene = "FFK";
            isInClassroom = false;
        }
        else
        {
            SceneLoader.instance.LoadSceneFromXml("Classroom");
            currentScene = "Classroom";
            EventHandler.Trigger(new ChangeSceneEvent(){ sceneName = "Classroom" });
            PlayerController.localPlayer.CurrentScene = "Classroom";
            isInClassroom = true;
        }
    }
}
