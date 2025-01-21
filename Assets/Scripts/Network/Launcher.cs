using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    private PlayerController player;

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("DisConnected");
        NetworkManager.instance.OnDisconnected();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected");
        NetworkManager.instance.OnConnected();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        GameObject playerObject = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        StartCoroutine(WaitToJoinRoom(playerObject));

        Debug.Log("Joined");
    }

    IEnumerator WaitToJoinRoom(GameObject gameObject)
    {
        yield return new WaitForEndOfFrame();
        player = gameObject.GetComponent<PlayerController>();
        player.JoinRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        player.LeftRoom();
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting");
    }
}
