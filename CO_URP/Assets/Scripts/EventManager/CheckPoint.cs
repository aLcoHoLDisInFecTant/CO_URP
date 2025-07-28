using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [Header("检查点设置")]
    public string checkpointID;
    public string description;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"检查点 {checkpointID} 已触发: {description}");
            CheckpointManager.Instance.SaveCheckpoint(this);
        }
    }
}
