using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CheckpointSaveData
{
    public string currentCheckpointID;
}

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance;

    private Dictionary<string, CheckPoint> checkpointDictionary = new Dictionary<string, CheckPoint>();

    private string savePath;
    private string currentCheckpointID;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        savePath = Path.Combine(Application.persistentDataPath, "checkpoint_save.json");
    }

    // 注册检查点
    public void RegisterCheckpoint(CheckPoint checkpoint)
    {
        if (!checkpointDictionary.ContainsKey(checkpoint.checkpointID))
        {
            checkpointDictionary.Add(checkpoint.checkpointID, checkpoint);
            Debug.Log($"检查点 {checkpoint.checkpointID} 已注册。");
        }
        else
        {
            Debug.LogWarning($"检查点 {checkpoint.checkpointID} 已经存在。");
        }
    }

    // 移除检查点
    public void UnregisterCheckpoint(CheckPoint checkpoint)
    {
        if (checkpointDictionary.ContainsKey(checkpoint.checkpointID))
        {
            checkpointDictionary.Remove(checkpoint.checkpointID);
            Debug.Log($"检查点 {checkpoint.checkpointID} 已移除。");
        }
    }

    // 保存当前检查点
    public void SaveCheckpoint(CheckPoint checkpoint)
    {
        currentCheckpointID = checkpoint.checkpointID;

        CheckpointSaveData saveData = new CheckpointSaveData
        {
            currentCheckpointID = currentCheckpointID
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"已保存检查点：{currentCheckpointID}");
    }

    // 加载检查点数据
    public void LoadCheckpoint()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("未找到检查点存档文件！");
            return;
        }

        string json = File.ReadAllText(savePath);
        CheckpointSaveData saveData = JsonUtility.FromJson<CheckpointSaveData>(json);

        currentCheckpointID = saveData.currentCheckpointID;

        if (checkpointDictionary.TryGetValue(currentCheckpointID, out CheckPoint checkpoint))
        {
            Vector3 pos = checkpoint.transform.position;

            // 此处应实现实际的玩家传送逻辑，比如：
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = pos;
                Debug.Log($"玩家已传送到检查点：{currentCheckpointID}，位置：{pos}");
            }
            else
            {
                Debug.LogError("未找到玩家对象，请确认玩家已设置Tag为Player");
            }
        }
        else
        {
            Debug.LogError($"未找到检查点ID：{currentCheckpointID}");
        }
    }
}
