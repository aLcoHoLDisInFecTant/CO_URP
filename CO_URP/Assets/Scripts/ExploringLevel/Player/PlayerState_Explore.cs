using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerState_Explore : State<Player_Explore>
{
    protected PlayerStateMachine_Explore stateMachine;
    protected PlayerData_Explore data => stateMachine.Data;
    protected Transform transform => stateMachine.PlayerTransform;

    public PlayerState_Explore(PlayerStateMachine_Explore sm)
    {
        stateMachine = sm;
    }

    public override void OnStateEnter() { }
    public override void OnStateExit() { }
    public override void Tick() { }
    public override void FixedTick() { }
}
