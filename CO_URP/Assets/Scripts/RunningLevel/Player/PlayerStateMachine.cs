using MathNet.Numerics;
using System.Xml;
using UnityEngine;
//using UnityEngine.Animations.Rigging;
public class PlayerStateMachine : StateMachine<Player>
{
    private Player player;
    private PlayerData playerData;
    private Transform playerTransform;
    public Transform PlayerTransform { get { return playerTransform; } }
    public PlayerData PlayerData { get { return playerData; } }
    public PlayerStateMachine(Player player)
    {
        this.player = player;
        playerData = player.PlayerData;
        playerTransform = player.transform;
        InitStates();
    }

    public ECommand currentInput;

    
    #region States

    public AscendState PlayerAscendState { get; private set; }

    public DescendState PlayerDescendState { get; private set; }
    public NormalFlyingState PlayerFlyState { get; private set; }
    public RollingRightState RollingRightState { get; private set; }

    public InvincibleState PlayerInvincibleState { get; private set; }

    public RollingLeftState RollingLeftState { get; private set; }

    public MovingLeftState MovingLeftState { get; private set; }

    public MovingRightState MovingRightState { get; private set; }
    
    private void InitStates()
    {
        PlayerAscendState = new AscendState(this);
        PlayerDescendState = new DescendState(this);
        PlayerFlyState = new NormalFlyingState(this);
        RollingLeftState = new RollingLeftState(this);
        RollingRightState = new RollingRightState(this);
        MovingLeftState = new MovingLeftState(this);
        MovingRightState = new MovingRightState(this);
        PlayerInvincibleState = new InvincibleState(this);

    }
    #endregion
    
    #region Movement
    public Vector3 HorizontalDeltaPosition;
    public Vector3 VerticalDeltaPosition;


    public bool IsOnTargetLane(float position)
    {
        return player.LaneSystem.IsOnTargetLane(position);
    }

    public bool IsOnTargetLayer(float position)
    { 
        return player.LaneSystem.IsOnTargetLayer(position);
    }
    public float TargetPosition { get { return player.LaneSystem.TargetPosition; } }

    public float TargetLayerPosition { get { return player.LaneSystem.TargetLayerPosition; } }

    public float CalculateDistanceToTargetLane(float position)
    {
        return player.LaneSystem.CalculateDistanceToTargetLane(position);
    }

    public float CalculateDistanceToTargetLayer(float position) 
    {
        return player.LaneSystem.CalculateDistanceToTargetLayer(position);
    }

    public void IncreaseTargetLane(int amount = 1)
    {
        player.LaneSystem.IncreaseTargetLane(amount);
    }
    public void DecreaseTargetLane(int amount = 1)
    {
        player.LaneSystem.DecreaseTargetLane(amount);
    }

    public void IncreaseTargetLayer(int amount = 1) 
    {
        player.LaneSystem.IncreaseTargetLayer(amount);
    }

    public void DecreaseTargetLayer(int amount = 1) 
    {
        player.LaneSystem.DecreaseTargetLayer(amount);
    }

    public void Move(Vector3 deltaPosition)
    {
        // 【添加这句日志】 确认我们收到的数据是什么
        Debug.Log("PlayerStateMachine: Received deltaPosition = " + deltaPosition.ToString("F7"));
        player.CharacterController.Move(deltaPosition);
        //player.Move(deltaPosition);
    }
    #endregion
    #region Animation
    public void PlayUpAnimation()
    {
        player.PlayerAnimator.SetUpState();
    }
    public void PlayDownAnimation()
    {
        player.PlayerAnimator.SetDownState();
    }
    public void PlayLeftAnimation()
    {
        player.PlayerAnimator.SetMovingLeftState();
    }
    public void PlayRightAnimation() // NAMING
    {
        player.PlayerAnimator.SetMovingRightState();
    }
    public void PlayRollingLeftState(bool isSlideLeft)
    {
        player.PlayerAnimator.SetSlideLeftState(isSlideLeft);
    }

    public void RollingLeftToNorm(bool isSlideLeft) 
    {
        player.PlayerAnimator.SetSlideLefttoNorm(isSlideLeft);
    }

    public void PlayRollingRightState(bool isSlideRight)
    { 
        player.PlayerAnimator.SetSlideRightState(isSlideRight);
    }

    public void RollingRightToNorm(bool isSlideRight) 
    {
        player.PlayerAnimator.SetSlideRighttoNorm(isSlideRight);
    }

    public void PlayInvincibleAni(bool gotHit)
    {
        player.PlayerAnimator.SetInvincibleState(gotHit);
    }

    
    #endregion
    /*
    #region Rigging
    //public float RightHandRigWeight { get { return player.PlayerRigging.RightHandRig.weight; } }
    //public Rig RightHandRig { get { return player.PlayerRigging.RightHandRig; } }
    public void ChangeRigWeight(Rig rig, float from, float to, float timeToChange) //
    {
        rig.ChangeWeightOverTime(from, to, timeToChange);//ChaChangeRigWeight(rig,from, to, timeToChange);
    }
    public void ChangeRigWeight(Rig rig, float to)
    {
        rig.ChangeWeight(to);
    }
    #endregion
    */
    #region Statistics

    public void UpdateDistance(float amount)
    {
        //player.PlayerStatictics.UpdateDistance(amount);
    }
    //ADD CALCULATE SCORE
    #endregion
    #region Collider
    public float DefaultColliderHeight { get { return player.PlayerCollider.defaultHeight; } } // =>
    public Vector3 DefaultColliderCenter { get { return player.PlayerCollider.defaultCenter; } }
    public void ChangeColliderHeight(float newHeight)
    {
        player.PlayerCollider.ChangeColliderHeight(newHeight);
    }
    public void ChangeColliderCenter(Vector3 newCenter)
    {
        player.PlayerCollider.ChangeColliderCenter(newCenter);
    }
    public void ResetColliderToDefault()
    {
        player.PlayerCollider.ResetToDefault();
    }
    #endregion
}