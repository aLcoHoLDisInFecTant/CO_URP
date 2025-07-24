using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewState : PlayerState_Explore
{
    private float deadZone = 0.05f;

    public PreviewState(PlayerStateMachine_Explore sm) : base(sm) 
    {
        
        
    }

    public override void OnStateEnter()
    {
        //stateMachine.Animator.SetIdle(true);
    }

    public override void OnStateExit()
    {
        //stateMachine.Animator.SetIdle(false);
        //stateMachine.SetState(stateMachine.MovingState);
        //Debug.Log("preview exit");
    }

    public override void Tick()
    {
        Queue<ECommand> inputQueue = stateMachine.inputQueue;

        Vector2 moveDir = Vector2.zero;

        foreach (var input in inputQueue)
        {
            Debug.Log("read command" + input);
            switch (input)
            {
                
                case ECommand.LEFT:
                    moveDir.x -= 1f;
                    break;
                case ECommand.RIGHT:
                    moveDir.x += 1f;
                    break;
                case ECommand.UP:
                    moveDir.y += 1f;
                    break;
                case ECommand.DOWN:
                    moveDir.y -= 1f;
                    break;
            }
        }

        //Debug.Log("moveDir" + moveDir);
        if (moveDir.sqrMagnitude > 0.01f)
        {
            moveDir.Normalize();
            Vector3 direction = new Vector3(moveDir.x, 0, moveDir.y);

            if (!stateMachine.PreviewController.IsPreviewing)
            {
                stateMachine.PreviewController.StartPreview(direction);
            }
            else
            {
                stateMachine.PreviewController.UpdatePreview(direction);
            }
        }
        else if (stateMachine.PreviewController.IsPreviewing)
        {
            Vector3 target = stateMachine.PreviewController.EndPreview();
            stateMachine.MovingState.SetTarget(target);
            stateMachine.SetState(stateMachine.MovingState);
        }
    }
}