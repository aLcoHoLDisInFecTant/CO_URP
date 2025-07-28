// RehabStep.cs
using UnityEngine;

[System.Serializable]
public class RehabStep
{
    [Tooltip("Ҫִ�еĿ�����������")]
    public RehabActionType actionType;

    [Tooltip("���鶯����Ҫ�ظ��Ĵ���")]
    public int repetitions = 10;

    [Tooltip("���ζ����Ľ������ʱ�䣨�룩")]
    public float durationPerRepetition = 2.0f;
}