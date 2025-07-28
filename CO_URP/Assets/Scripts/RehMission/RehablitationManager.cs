using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Michsky.MUIP;

public class RehabilitationManager : MonoBehaviour
{
    [Header("康复计划")]
    public RehabilitationPlan currentPlan;

    [Header("UI 引用")]
    public ListView listView;

    [Header("设备输入桥")]
    public EncoderInputBridgeV2 inputBridge;

    private int currentStepIndex = 0;
    private int currentRepetition = 0;
    private Coroutine detectionRoutine = null;

    private List<ListView.ListItem> taskItems = new List<ListView.ListItem>();
    private bool actionInProgress = false;

    void Start()
    {
        if (currentPlan != null)
            PopulateTaskList();
    }

    public void StartRehabilitation()
    {
        if (currentPlan == null || currentPlan.steps.Count == 0)
        {
            Debug.LogError("康复计划未设置或为空！");
            return;
        }

        if (detectionRoutine != null)
            StopCoroutine(detectionRoutine);

        currentStepIndex = 0;
        currentRepetition = 0;
        ResetTaskProgressUI();
        if (inputBridge != null)
        {
            detectionRoutine = StartCoroutine(DetectAndAdvance());
        }
        else
        {
            Debug.LogWarning("未检测到 EncoderInputBridge，仅显示任务列表，未启用动作检测。");
        }

    }

    private IEnumerator DetectAndAdvance()
    {
        while (currentStepIndex < currentPlan.steps.Count)
        {
            var step = currentPlan.steps[currentStepIndex];
            UpdateListItemStatus(currentStepIndex, $"进行中 ({currentRepetition + 1}/{step.repetitions})");

            if (DetectAction(step.actionType))
            {
                if (!actionInProgress)
                {
                    actionInProgress = true;
                    currentRepetition++;

                    if (currentRepetition >= step.repetitions)
                    {
                        UpdateListItemStatus(currentStepIndex, "✅ 已完成");
                        currentStepIndex++;
                        currentRepetition = 0;
                    }

                    yield return new WaitForSeconds(step.durationPerRepetition);
                    actionInProgress = false;
                }
            }

            yield return null;
        }

        OnPlanComplete();
    }

    private bool DetectAction(RehabActionType action)
    {
        Vector3 relative = inputBridge.GetRelativeRotation();
        float threshold = inputBridge.threshold;

        switch (action)
        {
            case RehabActionType.Flexion:
                return relative.x < -threshold;
            case RehabActionType.Extension:
                return relative.x > threshold;
            case RehabActionType.Pronation:
                return relative.y < -threshold;
            case RehabActionType.Supination:
                return relative.y > threshold;
            case RehabActionType.UlnarDeviation:
                return relative.z > threshold;
            case RehabActionType.RadialDeviation:
                return relative.z < -threshold;
            default:
                return false;
        }
    }

    private void PopulateTaskList()
    {
        listView.listItems.Clear();

        foreach (var step in currentPlan.steps)
        {
            var item = new ListView.ListItem();
            item.row0 = new ListView.ListRow { rowText = GetActionName(step.actionType), iconScale = 0.8f };
            item.row1 = new ListView.ListRow { rowText = $"目标：{step.repetitions} 次" };
            item.row2 = new ListView.ListRow { rowText = "状态：未开始" };

            listView.listItems.Add(item);
        }

        listView.rowCount = ListView.RowCount.Three;
        listView.InitializeItems();
    }


    private void UpdateListItemStatus(int stepIndex, string statusText)
    {
        for (int i = 0; i < currentPlan.steps.Count; i++)
        {
            RehabStep step = currentPlan.steps[i];
            var item = new ListView.ListItem();

            item.row0 = new ListView.ListRow { rowText = GetActionName(step.actionType), iconScale = 0.8f };
            item.row1 = new ListView.ListRow { rowText = $"目标：{step.repetitions} 次" };

            if (i < stepIndex)
                item.row2 = new ListView.ListRow { rowText = "状态：✅ 已完成" };
            else if (i == stepIndex)
                item.row2 = new ListView.ListRow { rowText = $"状态：{statusText}" };
            else
                item.row2 = new ListView.ListRow { rowText = "状态：未开始" };

            listView.listItems[i] = item;
        }

        listView.InitializeItems();
    }



    private void ResetTaskProgressUI()
    {
        for (int i = 0; i < taskItems.Count; i++)
            UpdateListItemStatus(i, "未开始");
    }

    private void OnPlanComplete()
    {
        Debug.Log("康复计划已完成！");
    }

    private string GetActionName(RehabActionType actionType)
    {
        return actionType switch
        {
            RehabActionType.Flexion => "屈曲",
            RehabActionType.Extension => "伸展",
            RehabActionType.Supination => "旋后",
            RehabActionType.Pronation => "旋前",
            RehabActionType.UlnarDeviation => "尺偏",
            RehabActionType.RadialDeviation => "桡偏",
            _ => "未知"
        };
    }
}
