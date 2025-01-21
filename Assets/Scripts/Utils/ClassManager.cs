using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class ClassManager : UnitySingleton<ClassManager>
{
    public List<PlayerController> players = new List<PlayerController>();
    public bool isInClassroom = true;
    public string currentScene;

    private void OnEnable()
    {
        EventHandler.Register<PlayerJoinRoomEvent>(OnPlayerJoinRoom);
        EventHandler.Register<PlayerLeftRoomEvent>(OnPlayerLeftRoom);
    }

    private void OnDisable()
    {
        EventHandler.Unregister<PlayerJoinRoomEvent>(OnPlayerJoinRoom);
        EventHandler.Unregister<PlayerLeftRoomEvent>(OnPlayerLeftRoom);
    }

    private void OnPlayerJoinRoom(PlayerJoinRoomEvent @event)
    {
        players.Add(@event.player);
    }

    private void OnPlayerLeftRoom(PlayerLeftRoomEvent @event)
    {
        players.Remove(@event.player);
    }

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
