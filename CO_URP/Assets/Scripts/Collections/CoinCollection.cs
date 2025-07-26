using UnityEngine;

public class CoinCollection : MonoBehaviour, ICollection
{
    [SerializeField] private float coinScore = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other)) return;

        OnCollected(other.gameObject);
        Destroy(gameObject);
    }

    public void OnCollected(GameObject collector)
    {
        ScoreHUDBridge.Instance?.AddCoin(); // 增加金币计数
        ScoreManager.Instance?.AddScoredPoints(coinScore);
        // 可扩展：播放音效、动画
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player");
    }
}
