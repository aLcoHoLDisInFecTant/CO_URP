using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescendState : FlyingState
{
    public DescendState(PlayerStateMachine sm) : base(sm) { }

    public override void OnStateEnter()
    {
        playerSM.DecreaseTargetLayer();
        playerSM.PlayDownAnimation();
    }

    public override void Tick()
    {
        base.Tick();
        //Debug.Log("currentLayerPosition" + playerTransform.localPosition.y + "targetLayerPosition" + playerSM.TargetLayerPosition);
        if (Mathf.Abs(playerTransform.localPosition.y - playerSM.TargetLayerPosition) < 0.01f)
        {
            playerSM.SetState(playerSM.PlayerFlyState);
        }

    }

    public override void OnStateExit()
    {

    }
}
