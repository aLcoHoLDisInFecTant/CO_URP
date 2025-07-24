using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalFlyingState : FlyingState
{
    public NormalFlyingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
    { }
    public override void OnStateEnter()
    {
        
    }

    public override void OnStateExit()
    {
        
    }

    public override void Tick()
    {
        // �����������־
        Debug.Log("NormalFlyingState is Ticking! Speed: " + playerData.FlyingSpeed);
        base.Tick();
        //Debug.Log("currentLayerPosition" + playerTransform.localPosition.y + "targetLayerPosition" + playerSM.TargetLayerPosition);
    }
}
