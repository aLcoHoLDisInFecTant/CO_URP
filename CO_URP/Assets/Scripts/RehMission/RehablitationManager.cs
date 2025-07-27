// RehabilitationManager.cs
using UnityEngine;
using UnityEngine.UI; // ����UI�����ռ�
using System.Collections;

public class RehabilitationManager : MonoBehaviour
{
    [Header("�����ƻ�")]
    [Tooltip("����Ҫִ�еĿ����ƻ� Scriptable Object �ʲ�")]
    public RehabilitationPlan currentPlan;

    [Header("UI ����")]
    [Tooltip("������ʾ��ǰ״̬���ı������磺���ڽ�������")]
    public Text statusText;
    [Tooltip("������ʾ��ǰ���������򵹼�ʱ���ı�")]
    public Text progressText;
    [Tooltip("������ʾ������ȵ��ı�")]
    public Text overallProgressText;

    // �ڲ�״̬����
    private int currentStepIndex = 0;
    private int currentRepetition = 0;
    private Coroutine currentRoutine = null;

    // �������������ڴ��ⲿ������UI��ť�����������ƻ�
    public void StartRehabilitation()
    {
        if (currentPlan == null || currentPlan.steps.Count == 0)
        {
            Debug.LogError("�����ƻ�δ���û�Ϊ�գ�");
            statusText.text = "���󣺼ƻ�Ϊ��";
            return;
        }

        // ֹͣ�����������еľ�����
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        // �����µĿ�������
        currentStepIndex = 0;
        currentRoutine = StartCoroutine(ExecutePlan());
    }

    private IEnumerator ExecutePlan()
    {
        // �����ƻ��е�ÿһ��
        while (currentStepIndex < currentPlan.steps.Count)
        {
            RehabStep currentStep = currentPlan.steps[currentStepIndex];
            currentRepetition = 1;

            // �����������UI
            UpdateOverallProgressUI();

            // ִ�е�ǰ����������ظ�����
            while (currentRepetition <= currentStep.repetitions)
            {
                // 1. ִ�ж����׶�
                statusText.text = $"���ڽ���: {GetActionName(currentStep.actionType)}";
                progressText.text = $"�� {currentRepetition} / {currentStep.repetitions} ��";

                // �ȴ����ζ����ĳ���ʱ��
                yield return new WaitForSeconds(currentStep.durationPerRepetition);

                currentRepetition++;
            }

            // 2. ��Ϣ�׶�
            // ����������һ�����裬�������Ϣ
            if (currentStepIndex < currentPlan.steps.Count - 1)
            {
                float restTimer = currentStep.restAfterStep;
                statusText.text = "����Ϣ";
                while (restTimer > 0)
                {
                    progressText.text = $"�����Ϣ: {Mathf.CeilToInt(restTimer)} ��";
                    yield return new WaitForSeconds(1.0f);
                    restTimer -= 1.0f;
                }
            }

            currentStepIndex++;
        }

        // 3. ��ɽ׶�
        OnPlanComplete();
    }

    private void OnPlanComplete()
    {
        statusText.text = "������ɣ�";
        progressText.text = "��ϲ��";
        overallProgressText.text = $"�������: {currentPlan.steps.Count}/{currentPlan.steps.Count}";
        Debug.Log("�����ƻ�����ɣ�");
    }

    private void UpdateOverallProgressUI()
    {
        if (overallProgressText != null)
        {
            overallProgressText.text = $"�������: {currentStepIndex + 1}/{currentPlan.steps.Count}";
        }
    }

    // ������������ö��ת��Ϊ�����ַ���������UI��ʾ��
    private string GetActionName(RehabActionType actionType)
    {
        switch (actionType)
        {
            case RehabActionType.Flexion: return "����";
            case RehabActionType.Extension: return "��չ";
            case RehabActionType.Supination: return "����";
            case RehabActionType.Pronation: return "��ǰ";
            case RehabActionType.UlnarDeviation: return "��ƫ";
            case RehabActionType.RadialDeviation: return "��ƫ";
            default: return "δ֪����";
        }
    }
}