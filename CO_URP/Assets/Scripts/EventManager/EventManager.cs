using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static Dictionary<string, Action<object>> eventDict = new();

    public static void StartListening(string eventName, Action<object> listener)
    {
        if (eventDict.ContainsKey(eventName))
            eventDict[eventName] += listener;
        else
            eventDict.Add(eventName, listener);
    }

    public static void StopListening(string eventName, Action<object> listener)
    {
        if (eventDict.ContainsKey(eventName))
        {
            eventDict[eventName] -= listener;
            if (eventDict[eventName] == null)
                eventDict.Remove(eventName);
        }
    }

    public static void TriggerEvent(string eventName, object param = null)
    {
        if (eventDict.TryGetValue(eventName, out var callback))
            callback?.Invoke(param);
    }
}
