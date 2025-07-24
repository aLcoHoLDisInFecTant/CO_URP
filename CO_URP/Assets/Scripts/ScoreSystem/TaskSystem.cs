using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionTask
{
    public enum Axis { X, Y, Z }
    public Axis targetAxis;
    public float threshold = 20f;
    public int requiredCount = 10;
    public int completedCount = 0;
    public float rewardPerAction = 10f;

    public bool CheckCompletion(float input)
    {
        if (Mathf.Abs(input) > threshold && completedCount < requiredCount)
        {
            completedCount++;
            return true;
        }
        return false;
    }

    public float GetScore()
    {
        float score = completedCount * rewardPerAction;
        completedCount = 0;
        return score;
    }
}

public class TaskSystem : MonoBehaviour
{
    public List<ActionTask> tasks = new List<ActionTask>();
    [SerializeField] private EncoderInputBridgeV2 inputBridge;

    public float CheckAndGetTaskScore()
    {
        float score = 0f;
        foreach (var task in tasks)
        {
            float input = task.targetAxis switch
            {
                ActionTask.Axis.X => inputBridge.GetRelativeVertical(),
                ActionTask.Axis.Y => inputBridge.GetRelativeRotationY(),
                ActionTask.Axis.Z => inputBridge.GetRelativeHorizontal(),
                _ => 0f
            };

            if (task.CheckCompletion(input))
                score += task.rewardPerAction;
        }
        return score;
    }
}
