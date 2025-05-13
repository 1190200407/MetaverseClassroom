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
        GameObject player = Instantiate(playerPrefab, new Vector3(0, -1000, 0), Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log("Player added to server");
    }

    public override void OnStartServer()
    {
        // 服务器生成一个ClassManager用于全客户端同步
        GameObject classManager = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "ClassManager"));
        NetworkServer.Spawn(classManager);

        // 服务器生成一个NetworkMessageHandler用于全客户端同步
        GameObject networkMessageHandler = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "NetworkMessageHandler"));
        NetworkServer.Spawn(networkMessageHandler);

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
