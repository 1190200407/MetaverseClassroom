using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class NetworkManager : UnitySingleton<NetworkManager>
{
    private Launcher launcher;
    public ConnectResult Connection { get; set; }

    public override void Awake()
    {
        base.Awake();
        launcher = GetComponent<Launcher>();
    }

    public void Connect()
    {
        launcher.Connect();
        Connection = ConnectResult.Connecting;
    }

    public void OnConnected()
    {
        Connection = ConnectResult.Connected;
    }

    public void OnDisconnected()
    {
        Connection = ConnectResult.Disconnected;
    }
}

public enum ConnectResult
{
    Connecting,
    Connected,
    Disconnected
}
