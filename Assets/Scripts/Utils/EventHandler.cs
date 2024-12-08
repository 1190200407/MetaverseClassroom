using System;
using System.Collections.Generic;
public class EventHandler
{
    // 保存事件字典，key 是事件参数类型，value 是对应的事件回调列表
    private static Dictionary<Type, Delegate> eventDictionary = new Dictionary<Type, Delegate>();

    // 注册事件
    public static void Register<T>(Action<T> callback) where T : struct
    {
        Type eventType = typeof(T);

        if (eventDictionary.ContainsKey(eventType))
        {
            // 如果已经有该类型的事件，则追加回调
            eventDictionary[eventType] = Delegate.Combine(eventDictionary[eventType], callback);
        }
        else
        {
            // 如果没有该类型的事件，直接添加
            eventDictionary.Add(eventType, callback);
        }
    }

    // 注销事件
    public static void Unregister<T>(Action<T> callback) where T : struct
    {
        Type eventType = typeof(T);

        if (eventDictionary.ContainsKey(eventType))
        {
            Delegate currentDelegate = eventDictionary[eventType];
            currentDelegate = Delegate.Remove(currentDelegate, callback);

            if (currentDelegate == null)
            {
                // 如果没有更多的回调，移除该类型事件
                eventDictionary.Remove(eventType);
            }
            else
            {
                eventDictionary[eventType] = currentDelegate;
            }
        }
    }

    // 触发事件
    public static void Trigger<T>(T eventArgs) where T : struct
    {
        Type eventType = typeof(T);

        if (eventDictionary.ContainsKey(eventType))
        {
            Action<T> callback = eventDictionary[eventType] as Action<T>;

            if (callback != null)
            {
                callback.Invoke(eventArgs);
            }
        }
    }

    // 清空所有事件
    public static void ClearAll()
    {
        eventDictionary.Clear();
    }
}