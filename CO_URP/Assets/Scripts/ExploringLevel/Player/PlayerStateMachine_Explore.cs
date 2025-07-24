using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerStateMachine_Explore : StateMachine<Player_Explore>
{
    public Player_Explore player;

    //public PlayerState_Explore CurrentState { get; private set; }
    public PreviewState PreviewState { get; private set; }
    public MoveToTargetState MovingState { get; private set; }
    public RiddleControlState RiddleState { get; private set; }
    public GrappleAimState GrappleAimState { get; private set; }
    public SwingState SwingState { get; private set; }

    public BoomerangChargeState BoomerangState { get; private set; }

    public Vector3 VerticalVelocity;
    private Transform playerTransform;
    public Transform PlayerTransform { get { return playerTransform; } }
    public Transform DestinationTransform { get { return playerTransform; } }
    //public CharacterController Controller => player.CharacterController;

    
    public Transform Transform => player.transform;
    //public PlayerAnimator Animator => player.PlayerAnimator;
    public PlayerData_Explore Data => player.Data;
    public Queue<ECommand> inputQueue => player.inputQueue;

    public MovementPreviewController PreviewController => player.previewController;
    public PlayerStateMachine_Explore(Player_Explore player)
    {
        this.player = player;
        playerTransform = player.transform;
        InitState();
    }

    private void InitState() 
    {
        PreviewState = new PreviewState(this);
        MovingState = new MoveToTargetState(this);
        RiddleState = new RiddleControlState(this);
        //GrappleAimState = new GrappleAimState(this);
        SwingState = new SwingState(this);
        BoomerangState = new BoomerangChargeState(this);
        //player.SetControllable(true);
    }


    public void Move(Vector3 delta)
    {
        //Controller.Move(delta);
        player.Rb.MovePosition(player.Rb.position + delta);
    }
}
