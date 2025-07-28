using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private Dictionary<string, System.Action> eventTable = new Dictionary<string, System.Action>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterEvent(string eventID, System.Action callback)
    {
        if (!eventTable.ContainsKey(eventID))
            eventTable[eventID] = callback;
        else
            eventTable[eventID] += callback;
    }

    public void UnregisterEvent(string eventID, System.Action callback)
    {
        if (eventTable.ContainsKey(eventID))
            eventTable[eventID] -= callback;
    }

    public void TriggerEvent(string eventID)
    {
        if (eventTable.TryGetValue(eventID, out var callback))
        {
            callback?.Invoke();
            Debug.Log($"[EventManager] �¼� {eventID} �Ѵ���");
        }
        else
        {
            Debug.LogWarning($"[EventManager] δ�ҵ��¼���{eventID}");
        }
    }
}
