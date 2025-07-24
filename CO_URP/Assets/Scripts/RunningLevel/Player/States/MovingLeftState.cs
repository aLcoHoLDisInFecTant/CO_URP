using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingLeftState : FlyingState
{
    public MovingLeftState(PlayerStateMachine sm) : base(sm) { }

    public override void OnStateEnter()
    {
        playerSM.DecreaseTargetLane(); // ÐÞ¸ÄÄ¿±êX
        playerSM.PlayLeftAnimation();
    }

    public override void Tick()
    {
        base.Tick();
        //Debug.Log("currentLanePosition" + playerTransform.localPosition.x + "targetLanePosition" + playerSM.TargetPosition);
        if (playerSM.IsOnTargetLane(playerTransform.localPosition.x))
        {
            playerSM.SetState(playerSM.PlayerFlyState);
        }
    }

    public override void OnStateExit()
    {

    }
}
