using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [Header("��������")]
    public string checkpointID;
    public string description;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"���� {checkpointID} �Ѵ���: {description}");
            CheckpointManager.Instance.SaveCheckpoint(this);
        }
    }
}
