// RehabStep.cs
using UnityEngine;

[System.Serializable]
public class RehabStep
{
    [Tooltip("要执行的康复动作类型")]
    public RehabActionType actionType;

    [Tooltip("此组动作需要重复的次数")]
    public int repetitions = 10;

    [Tooltip("单次动作的建议持续时间（秒）")]
    public float durationPerRepetition = 2.0f;
}