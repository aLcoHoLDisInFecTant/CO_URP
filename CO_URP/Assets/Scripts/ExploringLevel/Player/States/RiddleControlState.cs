using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiddleControlState : PlayerState_Explore
{
    public RiddleControlState(PlayerStateMachine_Explore sm) : base(sm)
    {

    }
    public override void OnStateEnter()
    {
        //stateMachine.Animator.SetRun(true);
    }

    public override void OnStateExit()
    {
        //stateMachine.Animator.SetRun(false);
        //stateMachine.SetState(stateMachine.PreviewState);
    }
}
