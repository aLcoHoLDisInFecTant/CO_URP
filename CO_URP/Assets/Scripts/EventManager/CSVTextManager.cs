using System.Collections.Generic;
using UnityEngine;

public class CSVTextManager : MonoBehaviour
{
    public static CSVTextManager Instance;

    private Dictionary<string, string> textData = new Dictionary<string, string>();

    public TextAsset csvFile; // ͨ��Inspector��קCSV�ļ�

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCSV();
    }

    void LoadCSV()
    {
        var lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++) // �ӵ�1�п�ʼ������ͷ
        {
            var parts = lines[i].Split(',');
            if (parts.Length >= 3)
                textData[parts[0].Trim()] = parts[2].Trim().Trim('"');
        }
    }

    public string GetTextByID(string id)
    {
        return textData.TryGetValue(id, out var text) ? text : $"[Missing Text: {id}]";
    }
}
