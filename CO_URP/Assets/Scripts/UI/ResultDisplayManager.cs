using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;
using Unity.VisualScripting;

public class ResultDisplayManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject result;
    public TMP_InputField coinText;
    public TMP_InputField scoreText;
    public TMP_InputField timeText;
    public TMP_InputField operationCountText;
    public TMP_InputField operationDurationText;

    [Header("External References")]
    public PlayerOperationLogger operationLogger;

    [Header("Star")]
    public GameObject star01;
    public GameObject star02;
    public GameObject star03;
    public GameObject fadeStar01;
    public GameObject fadeStar02;
    public GameObject fadeStar03;

    void Start()
    {
        EventManager.StartListening("settleResult", ShowResults);
        if (operationLogger == null)
            operationLogger = FindObjectOfType<PlayerOperationLogger>();
    }

    /// <summary>
    /// 外部调用该方法以显示结算信息
    /// </summary>
    public void ShowResults(object data)
    {
        result.SetActive(true);
        Time.timeScale = 0f;
        // 1. Coin & Score
        int coin = ScoreHUDBridge.Instance?.CoinCount ?? 0;
        int score = ScoreHUDBridge.Instance?.GetTotalScore() ?? 0;
        coinText.text = $"Coins: {coin}";
        scoreText.text = $"Score: {score}";

        // 2. Time
        string formattedTime = SceneTimer.Instance?.GetFormattedTime() ?? "00:00";
        timeText.text = $"Time: {formattedTime}";

        // 3. Star
        switch (data)
        {
            case 0:
                star01.SetActive(false);
                star02.SetActive(false);
                star03.SetActive(false);
                fadeStar01.SetActive(true);
                fadeStar02.SetActive(true);
                fadeStar03.SetActive(true);
                break;
            case 1:
                star01.SetActive(true);
                star02.SetActive(false);
                star03.SetActive(false);
                fadeStar01.SetActive(false);
                fadeStar02.SetActive(true);
                fadeStar03.SetActive(true);
                break;
            case 2:
                star01.SetActive(true);
                star02.SetActive(true);
                star03.SetActive(false);
                fadeStar01.SetActive(false);
                fadeStar02.SetActive(false);
                fadeStar03.SetActive(true);
                break;
            case 3:
                star01.SetActive(true);
                star02.SetActive(true);
                star03.SetActive (true);
                fadeStar01.SetActive(false);
                fadeStar02.SetActive(false);
                fadeStar03.SetActive(false);
                break; 
        }

        // 4. Operation Stats
        int opCount = 0;
        float totalDuration = 0f;

        if (operationLogger != null)
        {
            // 触发结算（写入 CSV）
            operationLogger.SettleAndSave();

            // 获取内部数据（直接遍历字段）
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

    public void OnApplicationQuit()
    {
        Debug.Log("quit");
    }
}
