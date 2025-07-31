using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class PlayerOperationLogger : MonoBehaviour
{
    public Player_Explore playerExplore;

    private class OperationRecord
    {
        public float startTime;
        public float duration;
    }

    // ��ǰ����ִ�еĲ�����¼
    private Dictionary<ECommand, OperationRecord> activeCommands = new Dictionary<ECommand, OperationRecord>();

    // ������ɵĲ�����¼
    private List<(ECommand command, float duration)> completedRecords = new List<(ECommand, float)>();

    private float logStartTime;

    void Start()
    {
        if (playerExplore == null)
            playerExplore = FindObjectOfType<Player_Explore>();

        logStartTime = Time.time;
    }

    void Update()
    {
        if (playerExplore == null) return;

        var currentQueue = new List<ECommand>(playerExplore.inputQueue);

        // ����½�����е�
        foreach (var cmd in currentQueue)
        {
            if (!activeCommands.ContainsKey(cmd))
            {
                activeCommands[cmd] = new OperationRecord
                {
                    startTime = Time.time
                };
            }
        }

        // ������뿪���е�
        List<ECommand> toRemove = new List<ECommand>();
        foreach (var kvp in activeCommands)
        {
            if (!currentQueue.Contains(kvp.Key))
            {
                float duration = Time.time - kvp.Value.startTime;
                completedRecords.Add((kvp.Key, duration));
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var cmd in toRemove)
        {
            activeCommands.Remove(cmd);
        }
    }

    /// <summary>
    /// �ⲿ���ã�����������¼�Ľ��㲢����
    /// </summary>
    public void SettleAndSave()
    {
        // ��������ִ�еĲ���
        foreach (var kvp in activeCommands)
        {
            float duration = Time.time - kvp.Value.startTime;
            completedRecords.Add((kvp.Key, duration));
        }

        activeCommands.Clear();

        string log = GenerateLog();
        Debug.Log(log);

        // ��ѡ������Ϊ CSV �ļ�
        string path = Application.dataPath + "/OperationLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
        File.WriteAllText(path, log);
        Debug.Log("Log saved to: " + path);
    }

    private string GenerateLog()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Command,Duration");

        float totalDuration = 0f;
        int count = 0;

        foreach (var (command, duration) in completedRecords)
        {
            sb.AppendLine($"{command},{duration:F2}");
            totalDuration += duration;
            count++;
        }

        sb.AppendLine();
        sb.AppendLine($"Total Commands,{count}");
        sb.AppendLine($"Total Duration,{totalDuration:F2} seconds");
        return sb.ToString();
    }
}
