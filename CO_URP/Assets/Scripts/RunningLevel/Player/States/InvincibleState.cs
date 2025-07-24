using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibleState : FlyingState
{
    public InvincibleState(PlayerStateMachine sm) : base(sm) { }

    public override void OnStateEnter()
    {
        playerSM.PlayInvincibleAni(true);
    }

    public override void Tick()
    {
        base.Tick();
        Debug.Log("Got hit, enter invincible mode");
        playerSM.SetState(playerSM.PlayerFlyState);
    }

    public override void OnStateExit()
    {
        playerSM.PlayInvincibleAni(false);
    }
}
