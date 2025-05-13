using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkMessageHandler : NetworkSingleton<NetworkMessageHandler>
{
    // 类型到事件的映射
    private Dictionary<NetworkMessageType, Delegate> handlers = new();

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<NetworkMessageClassroom>(OnNetworkMessage);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        NetworkClient.UnregisterHandler<NetworkMessageClassroom>();
    }

    public void RegisterHandler(NetworkMessageType messageType, Delegate handler)
    {
        if (handlers.TryGetValue(messageType, out var existingHandler))
        {
            // 如果已存在处理程序，则合并
            handlers[messageType] = Delegate.Combine(existingHandler, handler);
        }
        else
        {
            // 如果不存在处理程序，则直接添加
            handlers[messageType] = handler;
        }
    }

    public void UnregisterHandler(NetworkMessageType messageType, Delegate handler)
    {
        if (handlers.TryGetValue(messageType, out var existingHandler))
        {
            // 移除指定的处理程序
            var removed = Delegate.Remove(existingHandler, handler);
            handlers[messageType] = removed;
            if (handlers[messageType] == null)
            {
                handlers.Remove(messageType);
            }
        }
    }

    public void UnregisterAllHandlers(NetworkMessageType messageType)
    {
        handlers.Remove(messageType);
    }

    
    public void BroadcastMessage(NetworkMessageType messageType)
    {
        CmdBroadcastMessage(messageType, string.Empty);
    }

    public void BroadcastMessage<T>(NetworkMessageType messageType, T messageData) where T : struct
    {
        CmdBroadcastMessage(messageType, JsonUtility.ToJson(messageData));
    }

    [Command(requiresAuthority = false)]
    private void CmdBroadcastMessage(NetworkMessageType messageType, string messageJson)
    {
        foreach (var conn in NetworkServer.connections.Values)
        {
            conn.Send(new NetworkMessageClassroom() { messageType = messageType, messageJson = messageJson });
        }
    }

    private void OnNetworkMessage(NetworkMessageClassroom message)
    {
        if (handlers.TryGetValue(message.messageType, out var handler))
        {
            try
            {
                Type[] handlerTypes = handler.GetType().GetGenericArguments();
                
                if (handlerTypes.Length == 0)
                {
                    // 没有参数
                    handler.DynamicInvoke();
                }
                else
                {
                    // 获取消息处理方法的参数类型
                    Type handlerType = handler.GetType().GetGenericArguments()[0];
                    // 将消息json转换为参数类型
                    object messageData = JsonUtility.FromJson(message.messageJson, handlerType);
                    // 调用消息处理方法
                    handler.DynamicInvoke(messageData);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error invoking handler for message type {message.messageType}: {e}");
            }
        }
    }
}
