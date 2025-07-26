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
        ScoreHUDBridge.Instance?.AddCoin(); // ���ӽ�Ҽ���
        ScoreManager.Instance?.AddScoredPoints(coinScore);
        // ����չ��������Ч������
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player");
    }
}
