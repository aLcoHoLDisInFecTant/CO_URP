using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ResultDisplayManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Text coinText;
    public Text scoreText;
    public Text timeText;
    public Text operationCountText;
    public Text operationDurationText;

    [Header("External References")]
    public PlayerOperationLogger operationLogger;

    void Start()
    {
        if (operationLogger == null)
            operationLogger = FindObjectOfType<PlayerOperationLogger>();
    }

    /// <summary>
    /// �ⲿ���ø÷�������ʾ������Ϣ
    /// </summary>
    public void ShowResults()
    {
        // 1. Coin & Score
        int coin = ScoreHUDBridge.Instance?.CoinCount ?? 0;
        int score = ScoreHUDBridge.Instance?.GetTotalScore() ?? 0;
        coinText.text = $"Coins: {coin}";
        scoreText.text = $"Score: {score}";

        // 2. Time
        string formattedTime = SceneTimer.Instance?.GetFormattedTime() ?? "00:00";
        timeText.text = $"Time: {formattedTime}";

        // 3. Operation Stats
        int opCount = 0;
        float totalDuration = 0f;

        if (operationLogger != null)
        {
            // �������㣨д�� CSV��
            operationLogger.SettleAndSave();

            // ��ȡ�ڲ����ݣ�ֱ�ӱ����ֶΣ�
            var field = typeof(PlayerOperationLogger).GetField("completedRecords",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                var records = field.GetValue(operationLogger) as System.Collections.IEnumerable;
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        var tuple = (ValueTuple<ECommand, float>)record;
                        totalDuration += tuple.Item2;
                        opCount++;
                    }
                }
            }
        }

        operationCountText.text = $"Effective Ops: {opCount}";
        operationDurationText.text = $"Op Time: {totalDuration:F2}s";
    }
}
