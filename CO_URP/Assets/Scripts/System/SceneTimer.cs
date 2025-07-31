using UnityEngine;
using System;

public class SceneTimer : MonoBehaviour
{
    private float elapsedTime = 0f;
    private bool isRunning = true;

    // ������ѡ�����������ű����ʣ�
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
        Debug.Log("��ǰ����ʱ��" + elapsedTime);
    }

    // ��ͣ��ʱ
    public void PauseTimer()
    {
        isRunning = false;
    }

    // ������ʱ
    public void ResumeTimer()
    {
        isRunning = true;
    }

    // ���ü�ʱ��
    public void ResetTimer()
    {
        elapsedTime = 0f;
    }

    // ��ȡ����ʱ�䣨�룩
    public float GetTime()
    {
        return elapsedTime;
    }

    // ��ȡ��ʽ��ʱ���ַ�����mm:ss��
    public string GetFormattedTime()
    {
        TimeSpan t = TimeSpan.FromSeconds(elapsedTime);
        return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
