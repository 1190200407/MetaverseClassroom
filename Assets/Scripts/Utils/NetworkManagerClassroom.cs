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
        NetworkServer.AddPlayerForConnection(conn, player);

        // 生成ClassManager
        GameObject classManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "ClassManager"));
        NetworkServer.Spawn(classManager);

        Debug.Log("Player added to server");
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
