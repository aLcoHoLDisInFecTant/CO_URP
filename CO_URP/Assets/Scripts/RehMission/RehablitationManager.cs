// RehabilitationManager.cs
using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间
using System.Collections;

public class RehabilitationManager : MonoBehaviour
{
    [Header("康复计划")]
    [Tooltip("拖入要执行的康复计划 Scriptable Object 资产")]
    public RehabilitationPlan currentPlan;

    [Header("UI 引用")]
    [Tooltip("用于显示当前状态的文本，例如：正在进行屈曲")]
    public Text statusText;
    [Tooltip("用于显示当前动作计数或倒计时的文本")]
    public Text progressText;
    [Tooltip("用于显示整体进度的文本")]
    public Text overallProgressText;

    // 内部状态变量
    private int currentStepIndex = 0;
    private int currentRepetition = 0;
    private Coroutine currentRoutine = null;

    // 公共方法，用于从外部（例如UI按钮）启动康复计划
    public void StartRehabilitation()
    {
        if (currentPlan == null || currentPlan.steps.Count == 0)
        {
            Debug.LogError("康复计划未设置或为空！");
            statusText.text = "错误：计划为空";
            return;
        }

        // 停止可能正在运行的旧例程
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        // 启动新的康复流程
        currentStepIndex = 0;
        currentRoutine = StartCoroutine(ExecutePlan());
    }

    private IEnumerator ExecutePlan()
    {
        // 遍历计划中的每一步
        while (currentStepIndex < currentPlan.steps.Count)
        {
            RehabStep currentStep = currentPlan.steps[currentStepIndex];
            currentRepetition = 1;

            // 更新整体进度UI
            UpdateOverallProgressUI();

            // 执行当前步骤的所有重复次数
            while (currentRepetition <= currentStep.repetitions)
            {
                // 1. 执行动作阶段
                statusText.text = $"正在进行: {GetActionName(currentStep.actionType)}";
                progressText.text = $"第 {currentRepetition} / {currentStep.repetitions} 次";

                // 等待单次动作的持续时间
                yield return new WaitForSeconds(currentStep.durationPerRepetition);

                currentRepetition++;
            }

            // 2. 休息阶段
            // 如果不是最后一个步骤，则进入休息
            if (currentStepIndex < currentPlan.steps.Count - 1)
            {
                float restTimer = currentStep.restAfterStep;
                statusText.text = "请休息";
                while (restTimer > 0)
                {
                    progressText.text = $"组间休息: {Mathf.CeilToInt(restTimer)} 秒";
                    yield return new WaitForSeconds(1.0f);
                    restTimer -= 1.0f;
                }
            }

            currentStepIndex++;
        }

        // 3. 完成阶段
        OnPlanComplete();
    }

    private void OnPlanComplete()
    {
        statusText.text = "任务完成！";
        progressText.text = "恭喜！";
        overallProgressText.text = $"整体进度: {currentPlan.steps.Count}/{currentPlan.steps.Count}";
        Debug.Log("康复计划已完成！");
    }

    private void UpdateOverallProgressUI()
    {
        if (overallProgressText != null)
        {
            overallProgressText.text = $"整体进度: {currentStepIndex + 1}/{currentPlan.steps.Count}";
        }
    }

    // 辅助方法，将枚举转换为中文字符串（用于UI显示）
    private string GetActionName(RehabActionType actionType)
    {
        switch (actionType)
        {
            case RehabActionType.Flexion: return "屈曲";
            case RehabActionType.Extension: return "伸展";
            case RehabActionType.Supination: return "旋后";
            case RehabActionType.Pronation: return "旋前";
            case RehabActionType.UlnarDeviation: return "尺偏";
            case RehabActionType.RadialDeviation: return "桡偏";
            default: return "未知动作";
        }
    }
}