using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class NetworkManagerClassroom : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Player added to server");
    }
    
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Player disconnected from server");
    }

    public override void OnStartServer()
    {
        Debug.Log("Server started");
    }
    
    public override void OnStartHost()
    {
        Debug.Log("Host started");
    }
    
    public override void OnStartClient()
    {
        Debug.Log("Client started");
    }
}
