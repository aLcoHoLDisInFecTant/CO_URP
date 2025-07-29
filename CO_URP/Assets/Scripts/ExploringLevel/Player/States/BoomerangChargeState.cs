using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// ��ҵĻ����������뷢��״̬��
/// �ڴ�״̬�£���ұ���ԭ�أ����Ը��ݺ���������������ڵ�Ͷ����ʽ��
/// </summary>
public class BoomerangChargeState : PlayerState_Explore
{
    private BoomerangLauncher boomerangLauncher;

    // ��ǻ������Ƿ����ڱ�״̬�ڱ�Ͷ��
    private bool isBoomerangThrown = false;

    // ��¼����Ͷ����ģʽ���������˳�״̬ʱ�����Ƿ���Ҫ��������ڷ���
    private BoomerangMode currentMode = BoomerangMode.None;

    // ��������ڵ�Ͷ��ģʽ
    private enum BoomerangMode
    {
        None,
        Forward, // ǰ��Ͷ������Ҫ�ֶ������
        Circle   // ��ȦͶ�������Զ�����
    }

    public BoomerangChargeState(PlayerStateMachine_Explore sm) : base(sm)
    {
        // �ڹ���ʱ��ȡ BoomerangLauncher ������
        boomerangLauncher = stateMachine.player.GetComponent<BoomerangLauncher>();
    }

    public override void OnStateEnter()
    {
        // --- ״̬����ʱ�ĳ�ʼ�� ---
        isBoomerangThrown = false;
        currentMode = BoomerangMode.None;

        // ��ʾ������ϵĻ�����ģ�ͣ���ʾ��������
        stateMachine.player.boomerangModel.gameObject.SetActive(true);

        // ������������
        if (stateMachine.player.Animator != null)
        {
            stateMachine.player.Animator.SetBool("BoomerangCharging", true);
        }

        Debug.Log("�������������״̬����ס E/Q ���� W/A/D ���䡣");
    }

    public override void OnStateExit()
    {
        // --- ״̬�˳�ʱ������ ---

        // �����ǰ��Ͷ��ģʽ�����һ��������ڷ����У������ɿ�����ʱ����������
        if (currentMode == BoomerangMode.Forward && stateMachine.player.isBoomerangFlying)
        {
            boomerangLauncher.RequestReturn();
            Debug.Log("������ǰ��Ͷ���Ļ����ڷ��ء�");
        }

        // ����������ϵĻ�����ģ��
        stateMachine.player.boomerangModel.gameObject.SetActive(false);

        // ������������
        if (stateMachine.player.Animator != null)
        {
            stateMachine.player.Animator.SetBool("BoomerangCharging", false);
        }

        // ������ҽű��е��������
        stateMachine.player.isBoomerangCharging = false;
    }

    public override void Tick()
    {
        // --- ÿ֡���µ��߼� ---

        // ����Ƿ����ڰ�ס������ (E/Q)
        bool stillCharging = stateMachine.inputQueue.Contains(ECommand.SLIDERIGHT) ||
                             stateMachine.inputQueue.Contains(ECommand.SLIDELEFT);

        // ����ɿ��������������߻������Ѿ��ɳ������أ�isBoomerangFlying��Ϊfalse�������˳���״̬
        if (!stillCharging || (isBoomerangThrown && !stateMachine.player.isBoomerangFlying))
        {
            stateMachine.SetState(stateMachine.PreviewState);
            return;
        }

        // ��������ڻ�δͶ��������Ͷ��ָ��
        if (!isBoomerangThrown)
        {
            // ���ǰ��Ͷ�� (W�� / ECommand.UP)
            if (stateMachine.inputQueue.Contains(ECommand.UP))
            {
                // ʹ���޸ĺ�� Launcher ����
                boomerangLauncher.LaunchForward(stateMachine.player.transform.forward);

                isBoomerangThrown = true;
                currentMode = BoomerangMode.Forward;
                ConsumeInputCommand(ECommand.UP); // ����ָ���ֹ�ظ�����
            }
            // ���������ȦͶ�� (D�� / ECommand.RIGHT)
            else if (stateMachine.inputQueue.Contains(ECommand.RIGHT))
            {
                // true ����˳ʱ��
                boomerangLauncher.LaunchCircle(true);

                isBoomerangThrown = true;
                currentMode = BoomerangMode.Circle;
                ConsumeInputCommand(ECommand.RIGHT);
            }
            // ���������ȦͶ�� (A�� / ECommand.LEFT)
            else if (stateMachine.inputQueue.Contains(ECommand.LEFT))
            {
                // false ������ʱ��
                boomerangLauncher.LaunchCircle(false);

                isBoomerangThrown = true;
                currentMode = BoomerangMode.Circle;
                ConsumeInputCommand(ECommand.LEFT);
            }
        }
    }

    /// <summary>
    /// ������������Ƴ�һ��ָ��Է�ֹ������һ֡���ظ�����
    /// </summary>
    /// <param name="command">Ҫ���ĵ�ָ��</param>
    private void ConsumeInputCommand(ECommand command)
    {
        // ʹ�� LINQ ����һ��������������ָ����¶���
        var list = stateMachine.inputQueue.ToList();
        list.Remove(command);
        stateMachine.player.inputQueue = new Queue<ECommand>(list);
    }
}