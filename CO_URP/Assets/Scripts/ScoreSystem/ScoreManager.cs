using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; } = 0;
    public float FrequencyMultiplier => Mathf.Clamp01(multiplierTracker.Frequency);
    public float DurationMultiplier => Mathf.Clamp01(multiplierTracker.Duration);
    public float CombinedMultiplier => multiplierTracker.TotalMultiplier;

    [SerializeField] private Player_Explore player;
    private ScoreMultiplierTracker multiplierTracker;

    private void Awake()
    {
        Instance = this;
        multiplierTracker = new ScoreMultiplierTracker();
    }

    private void Update()
    {
        if (player == null) return;

        multiplierTracker.UpdateMultiplier(player.inputQueue);
        //Debug.Log("Multiplier" + CombinedMultiplier);
    }

    // 对外统一接口
    public void AddScoredPoints(float baseScore)
    {
        int earnedScore = Mathf.RoundToInt(baseScore * CombinedMultiplier);
        TotalScore += earnedScore;
        Debug.Log($"Add Score: Base = {baseScore}, Multiplier = {CombinedMultiplier}, Total = {TotalScore}");
    }
}
