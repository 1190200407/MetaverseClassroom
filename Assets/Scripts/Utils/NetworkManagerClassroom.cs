using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class NetworkManagerClassroom : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // 生成玩家
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        player.GetComponent<PlayerManager>().JoinRoom();
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log("Player added to server");
    }
    
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // 获取玩家
        if (conn != null && conn.identity != null)
        {
            PlayerManager player = conn.identity.GetComponent<PlayerManager>();
            player.LeftRoom();
        }
        NetworkServer.DestroyPlayerForConnection(conn);

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
