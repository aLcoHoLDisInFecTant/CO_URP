using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToTargetState : PlayerState_Explore
{
    private Vector3 targetPosition;
    private float speed;

    public MoveToTargetState(PlayerStateMachine_Explore sm) : base(sm)
    {
        speed = data.Speed;
    }

    public void SetTarget(Vector3 target)
    {
        targetPosition = target;
    }

    public override void OnStateEnter()
    {

    }

    public override void OnStateExit()
    {

    }

    public override void Tick()
    {

    }

    public override void FixedTick()
    {
        //Debug.Log("ÒÆ¶¯");
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.1f)
        {
            stateMachine.SetState(stateMachine.PreviewState);
            return;
        }

        direction.Normalize();
        transform.forward = direction;
        stateMachine.Move(direction * speed * Time.fixedDeltaTime);
    }
}