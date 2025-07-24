using GuidanceLine;
using UnityEngine;

public class MovingRightState : FlyingState
{
    public MovingRightState(PlayerStateMachine sm) : base(sm) { }

    public override void OnStateEnter()
    {
        playerSM.IncreaseTargetLane(); // ÐÞ¸ÄÄ¿±êX
        playerSM.PlayRightAnimation();
    }

    public override void Tick()
    {
        base.Tick();
        Debug.Log("currentLanePosition" + playerTransform.localPosition.x + "targetLanePosition" + playerSM.TargetPosition);
        if (playerSM.IsOnTargetLane(playerTransform.localPosition.x))
        {
            playerSM.SetState(playerSM.PlayerFlyState);
        }
    }

    public override void OnStateExit()
    {

    }
}
