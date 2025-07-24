using UnityEngine;

public class SwingState : PlayerState_Explore
{
    public SwingState(PlayerStateMachine_Explore sm) : base(sm) { }

    public override void Tick()
    {
        Rigidbody rb = stateMachine.player.Rb;
        Transform orientation = stateMachine.Transform;
        float SwingForce = stateMachine.player.SwingForce;

        if (stateMachine.inputQueue.Contains(ECommand.RIGHT))
            rb.AddForce(orientation.right * SwingForce * Time.deltaTime);
        if (stateMachine.inputQueue.Contains(ECommand.LEFT))
            rb.AddForce(-orientation.right * SwingForce * Time.deltaTime);
        if (stateMachine.inputQueue.Contains(ECommand.UP))
            rb.AddForce(orientation.forward * SwingForce * Time.deltaTime);
        if (stateMachine.inputQueue.Contains(ECommand.DOWN))
            rb.AddForce(-orientation.forward * SwingForce * Time.deltaTime);

        // 离开 Swing 条件：滑键松开 或 落地
        if (!stateMachine.inputQueue.Contains(ECommand.SLIDERIGHT) && !stateMachine.inputQueue.Contains(ECommand.SLIDELEFT)
            || Physics.Raycast(stateMachine.Transform.position, Vector3.down, 0.6f, LayerMask.GetMask("Ground")))
        {
            stateMachine.player.grappleLauncher.StopSwing();
            stateMachine.SetState(stateMachine.PreviewState);
        }
    }
}
