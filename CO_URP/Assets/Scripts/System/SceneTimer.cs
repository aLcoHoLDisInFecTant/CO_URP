using UnityEngine;
using System;

public class SceneTimer : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool isRunning = true;

    // 单例可选（便于其他脚本访问）
    public static SceneTimer Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
        }
        Debug.Log("当前游玩时间" + elapsedTime);
    }

    // 暂停计时
    public void PauseTimer()
    {
        isRunning = false;
    }

    // 继续计时
    public void ResumeTimer()
    {
        isRunning = true;
    }

    // 重置计时器
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }

    // 获取运行时间（秒）
    public float GetTime()
    {
        return elapsedTime;
    }

    // 获取格式化时间字符串（mm:ss）
    public string GetFormattedTime()
    {
        TimeSpan t = TimeSpan.FromSeconds(elapsedTime);
        return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
