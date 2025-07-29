using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 玩家的回旋镖蓄力与发射状态。
/// 在此状态下，玩家保持原地，可以根据后续输入决定回旋镖的投掷方式。
/// </summary>
public class BoomerangChargeState : PlayerState_Explore
{
    private BoomerangLauncher boomerangLauncher;

    // 标记回旋镖是否已在本状态内被投出
    private bool isBoomerangThrown = false;

    // 记录本次投掷的模式，用于在退出状态时决定是否需要命令回旋镖返回
    private BoomerangMode currentMode = BoomerangMode.None;

    // 定义回旋镖的投掷模式
    private enum BoomerangMode
    {
        None,
        Forward, // 前向投掷，需要手动命令返回
        Circle   // 绕圈投掷，会自动返回
    }

    public BoomerangChargeState(PlayerStateMachine_Explore sm) : base(sm)
    {
        // 在构造时获取 BoomerangLauncher 的引用
        boomerangLauncher = stateMachine.player.GetComponent<BoomerangLauncher>();
    }

    public override void OnStateEnter()
    {
        // --- 状态进入时的初始化 ---
        isBoomerangThrown = false;
        currentMode = BoomerangMode.None;

        // 显示玩家身上的回旋镖模型，表示正在蓄力
        stateMachine.player.boomerangModel.gameObject.SetActive(true);

        // 触发蓄力动画
        if (stateMachine.player.Animator != null)
        {
            stateMachine.player.Animator.SetBool("BoomerangCharging", true);
        }

        Debug.Log("进入回旋镖蓄力状态。按住 E/Q 并按 W/A/D 发射。");
    }

    public override void OnStateExit()
    {
        // --- 状态退出时的清理 ---

        // 如果是前向投掷模式，并且回旋镖仍在飞行中，则在松开按键时命令它返回
        if (currentMode == BoomerangMode.Forward && stateMachine.player.isBoomerangFlying)
        {
            boomerangLauncher.RequestReturn();
            Debug.Log("已命令前向投掷的回旋镖返回。");
        }

        // 隐藏玩家身上的回旋镖模型
        stateMachine.player.boomerangModel.gameObject.SetActive(false);

        // 结束蓄力动画
        if (stateMachine.player.Animator != null)
        {
            stateMachine.player.Animator.SetBool("BoomerangCharging", false);
        }

        // 重置玩家脚本中的蓄力标记
        stateMachine.player.isBoomerangCharging = false;
    }

    public override void Tick()
    {
        // --- 每帧更新的逻辑 ---

        // 检查是否仍在按住蓄力键 (E/Q)
        bool stillCharging = stateMachine.inputQueue.Contains(ECommand.SLIDERIGHT) ||
                             stateMachine.inputQueue.Contains(ECommand.SLIDELEFT);

        // 如果松开了蓄力键，或者回旋镖已经飞出并返回（isBoomerangFlying变为false），则退出此状态
        if (!stillCharging || (isBoomerangThrown && !stateMachine.player.isBoomerangFlying))
        {
            stateMachine.SetState(stateMachine.PreviewState);
            return;
        }

        // 如果回旋镖还未投出，则检测投掷指令
        if (!isBoomerangThrown)
        {
            // 检测前向投掷 (W键 / ECommand.UP)
            if (stateMachine.inputQueue.Contains(ECommand.UP))
            {
                // 使用修改后的 Launcher 发射
                boomerangLauncher.LaunchForward(stateMachine.player.transform.forward);

                isBoomerangThrown = true;
                currentMode = BoomerangMode.Forward;
                ConsumeInputCommand(ECommand.UP); // 消耗指令，防止重复发射
            }
            // 检测向右绕圈投掷 (D键 / ECommand.RIGHT)
            else if (stateMachine.inputQueue.Contains(ECommand.RIGHT))
            {
                // true 代表顺时针
                boomerangLauncher.LaunchCircle(true);

                isBoomerangThrown = true;
                currentMode = BoomerangMode.Circle;
                ConsumeInputCommand(ECommand.RIGHT);
            }
            // 检测向左绕圈投掷 (A键 / ECommand.LEFT)
            else if (stateMachine.inputQueue.Contains(ECommand.LEFT))
            {
                // false 代表逆时针
                boomerangLauncher.LaunchCircle(false);

                isBoomerangThrown = true;
                currentMode = BoomerangMode.Circle;
                ConsumeInputCommand(ECommand.LEFT);
            }
        }
    }

    /// <summary>
    /// 从输入队列中移除一个指令，以防止其在下一帧被重复处理。
    /// </summary>
    /// <param name="command">要消耗的指令</param>
    private void ConsumeInputCommand(ECommand command)
    {
        // 使用 LINQ 创建一个不包含已消耗指令的新队列
        var list = stateMachine.inputQueue.ToList();
        list.Remove(command);
        stateMachine.player.inputQueue = new Queue<ECommand>(list);
    }
}