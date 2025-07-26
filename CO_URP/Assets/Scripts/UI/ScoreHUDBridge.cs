using UnityEngine;

public class ScoreHUDBridge : MonoBehaviour
{
    public static ScoreHUDBridge Instance { get; private set; }

    public int CoinCount { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public float GetTotalScore()
    {
        return ScoreManager.Instance?.TotalScore ?? 0f;
    }

    public int GetCombinedMultiplier()
    {
        return ScoreManager.Instance?.CombinedMultiplier ?? 1;
    }

    public void AddCoin()
    {
        CoinCount++;
    }
}
