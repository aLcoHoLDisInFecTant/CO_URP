// RehabilitationPlan.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewRehabPlan", menuName = "Rehabilitation/Rehabilitation Plan")]
public class RehabilitationPlan : ScriptableObject
{
    [Tooltip("此康复计划的名称，例如：手腕第一周")]
    public string planName;

    [TextArea(3, 10)]
    [Tooltip("关于此计划的详细描述")]
    public string description;

    [Tooltip("此计划包含的所有康复步骤")]
    public List<RehabStep> steps = new List<RehabStep>();
}