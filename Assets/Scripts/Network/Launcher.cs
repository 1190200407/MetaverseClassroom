using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
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

        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        Debug.Log("Joined");
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting");
    }
}
