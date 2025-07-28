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

    // ע�����
    public void RegisterCheckpoint(CheckPoint checkpoint)
    {
        if (!checkpointDictionary.ContainsKey(checkpoint.checkpointID))
        {
            checkpointDictionary.Add(checkpoint.checkpointID, checkpoint);
            Debug.Log($"���� {checkpoint.checkpointID} ��ע�ᡣ");
        }
        else
        {
            Debug.LogWarning($"���� {checkpoint.checkpointID} �Ѿ����ڡ�");
        }
    }

    // �Ƴ�����
    public void UnregisterCheckpoint(CheckPoint checkpoint)
    {
        if (checkpointDictionary.ContainsKey(checkpoint.checkpointID))
        {
            checkpointDictionary.Remove(checkpoint.checkpointID);
            Debug.Log($"���� {checkpoint.checkpointID} ���Ƴ���");
        }
    }

    // ���浱ǰ����
    public void SaveCheckpoint(CheckPoint checkpoint)
    {
        currentCheckpointID = checkpoint.checkpointID;

        CheckpointSaveData saveData = new CheckpointSaveData
        {
            currentCheckpointID = currentCheckpointID
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"�ѱ�����㣺{currentCheckpointID}");
    }

    // ���ؼ�������
    public void LoadCheckpoint()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("δ�ҵ�����浵�ļ���");
            return;
        }

        string json = File.ReadAllText(savePath);
        CheckpointSaveData saveData = JsonUtility.FromJson<CheckpointSaveData>(json);

        currentCheckpointID = saveData.currentCheckpointID;

        if (checkpointDictionary.TryGetValue(currentCheckpointID, out CheckPoint checkpoint))
        {
            Vector3 pos = checkpoint.transform.position;

            // �˴�Ӧʵ��ʵ�ʵ���Ҵ����߼������磺
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = pos;
                Debug.Log($"����Ѵ��͵����㣺{currentCheckpointID}��λ�ã�{pos}");
            }
            else
            {
                Debug.LogError("δ�ҵ���Ҷ�����ȷ�����������TagΪPlayer");
            }
        }
        else
        {
            Debug.LogError($"δ�ҵ�����ID��{currentCheckpointID}");
        }
    }
}
