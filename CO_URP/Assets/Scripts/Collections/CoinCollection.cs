using System.Collections.Generic;
using UnityEngine;

public class CoinCollection : MonoBehaviour, ICollection
{
    [SerializeField] private float coinScore = 5f;
    public bool isCollected = false;

    [System.Serializable]
    public class TriggerEvent
    {
        public string eventName;
        public string eventInput;
    }

    public List<TriggerEvent> triggerEvents = new List<TriggerEvent>();
    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        OnCollected(other.gameObject);
        Destroy(gameObject);
    }

    public void OnCollected(GameObject collector)
    {
        ScoreHUDBridge.Instance?.AddCoin(); // ���ӽ�Ҽ���
        ScoreManager.Instance?.AddScoredPoints(coinScore);
        foreach (var evt in triggerEvents)
        {
            if (!string.IsNullOrEmpty(evt.eventName))
            {
                EventManager.TriggerEvent(evt.eventName, evt.eventInput);
                Debug.Log($"[RegularCollection] �����¼���{evt.eventName}��{evt.eventInput}��");
            }
        }
        // ����չ��������Ч������
        EventManager.TriggerEvent("PlaySFX", "pickUpCoin");
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player");
    }
}
