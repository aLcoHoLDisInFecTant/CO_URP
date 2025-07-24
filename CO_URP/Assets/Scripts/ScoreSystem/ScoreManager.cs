using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public float totalScore { get; private set; }

    [SerializeField] private ScoreCalculator calculator;
    [SerializeField] private TaskSystem taskSystem;

    void Awake() => Instance = this;

    void Update()
    {
        float actionScore = calculator.CalculateActionScore();
        float taskScore = taskSystem.CheckAndGetTaskScore();
        totalScore += actionScore + taskScore;
    }

    public void AddPickupScore(float amount) => totalScore += amount;
}
