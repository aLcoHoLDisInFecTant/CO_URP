// RehabilitationPlan.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRehabPlan", menuName = "Rehabilitation/Rehabilitation Plan")]
public class RehabilitationPlan : ScriptableObject
{
    [Tooltip("�˿����ƻ������ƣ����磺�����һ��")]
    public string planName;

    [TextArea(3, 10)]
    [Tooltip("���ڴ˼ƻ�����ϸ����")]
    public string description;

    [Tooltip("�˼ƻ����������п�������")]
    public List<RehabStep> steps = new List<RehabStep>();
}