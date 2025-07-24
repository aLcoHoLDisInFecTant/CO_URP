using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingState : PlayerState
{
    private float speed; //SO
    private float laneSwitchSpeed; //SO
    private float layerSwitchSpeed;
    public float invincibilityTime => playerSM.PlayerData.InvincibilityTime;

    public float rollingDuration => playerSM.PlayerData.SlideRollDuration;
    public FlyingState(PlayerStateMachine playerStateMachine) : base(playerStateMachine)
    {
        this.playerSM = playerStateMachine;
        speed = playerData.FlyingSpeed;
        laneSwitchSpeed = playerData.SwitchSpeed;
        layerSwitchSpeed = playerData.SwitchSpeed;
    }
    public override void OnStateEnter() { }

    public override void OnStateExit() { }

    public override void Tick()
    {
        //HandleDirection();
        //playerSM.HorizontalDeltaPosition = speed * playerSM.PlayerTransform.forward * Time.deltaTime;
        Vector3 forwardMovement = speed * playerSM.PlayerTransform.forward * Time.deltaTime;
        // 【修复】在这里添加对垂直位移的重置
        playerSM.VerticalDeltaPosition = Vector3.zero;
        playerSM.HorizontalDeltaPosition = forwardMovement;
        playerSM.UpdateDistance(playerSM.HorizontalDeltaPosition.z);
        SwitchLane();
        SwitchLayer();
        Vector3 deltaPosition = new Vector3(playerSM.HorizontalDeltaPosition.x,
                                            playerSM.VerticalDeltaPosition.y,
                                            playerSM.HorizontalDeltaPosition.z);
        Debug.Log("FlyingState: Sending deltaPosition = " + deltaPosition.ToString("F7"));
        playerSM.Move(deltaPosition);
    }

    //private void HandleDirection()
    //{
    //    switch (playerSM.InputDirection)
    //    {
    //        case EInputDirection.RIGHT:
    //            playerSM.IncreaseTargetLane();
    //            break;
    //        case EInputDirection.LEFT:
    //            playerSM.DecreaseTargetLane();
    //            break;
    //        case EInputDirection.UP:
    //            playerSM.SetState(playerSM.PlayerJumpState);
    //            break;
    //        case EInputDirection.DOWN:
    //            playerSM.SetState(playerSM.PlayerSlideState);
    //            break;
    //        default:
    //            break;
    //    }
    //}

    public void SwitchLane()
    {
        float sidewaysPos = playerTransform.localPosition.x; // 考虑局部还是全局
        Vector3 playerDirection = playerTransform.right;

        float targetPos = playerSM.TargetPosition;
        if (playerSM.IsOnTargetLane(sidewaysPos))
        {
            return;
        }
        Vector3 diffX = (targetPos - sidewaysPos) * playerDirection;
        Debug.DrawLine(playerTransform.position, diffX, Color.green);
        Vector3 deltaX = diffX.normalized * laneSwitchSpeed * Time.deltaTime;
        Debug.DrawLine(playerTransform.position, deltaX, Color.red);
        if (deltaX.sqrMagnitude < diffX.sqrMagnitude)
        {
            playerSM.HorizontalDeltaPosition += playerSM.PlayerTransform.right * deltaX.x;
        }
        else
        {
            playerSM.HorizontalDeltaPosition += playerSM.PlayerTransform.right * diffX.x;
        }
    }


    public void SwitchLayer()
    {
        float currentY = playerTransform.localPosition.y;
        //Debug.Log("currentY" + currentY);
        float targetY = playerSM.TargetLayerPosition;
        //Debug.Log("targetY" + targetY);
        float diff = targetY - currentY;

        if (Mathf.Abs(diff) < 0.01f)
        {
            // 精确对齐以避免浮点误差
            playerTransform.localPosition = new Vector3(
                playerTransform.localPosition.x,
                targetY,
                playerTransform.localPosition.z
            );
            return;
        }

        float moveDelta = layerSwitchSpeed * Time.deltaTime;
        float moveY = Mathf.Clamp(diff, -moveDelta, moveDelta); // 防止超调

        playerSM.VerticalDeltaPosition += new Vector3(0, moveY, 0);
    }


}
