using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingLeftState : FlyingState
{
    private float duration;
    private float timer = 0;

    public RollingLeftState(PlayerStateMachine sm) : base(sm)
    {
        duration = rollingDuration;
    }

    public override void OnStateEnter()
    {
        timer = 0;
        playerSM.PlayRollingLeftState(true);
    }

    public override void Tick()
    {
        base.Tick();
        timer += Time.deltaTime;
        if (timer >= duration)
            playerSM.SetState(playerSM.PlayerFlyState);
    }

    public override void OnStateExit()
    {
        playerSM.PlayRollingLeftState(false);
        playerSM.RollingLeftToNorm(true);
    }
}
