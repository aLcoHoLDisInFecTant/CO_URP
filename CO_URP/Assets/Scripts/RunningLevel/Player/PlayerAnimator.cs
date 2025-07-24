using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator
{
    private Animator animator;
    private int upHash = Animator.StringToHash("MovingUp");
    private int downHash = Animator.StringToHash("MovingDown");
    private int leftHash = Animator.StringToHash("MovingLeft");
    private int rightHash = Animator.StringToHash("MovingRight");
    private int slideRightHash = Animator.StringToHash("SlideRight");
    private int slideLeftHash = Animator.StringToHash("SlideLeft");
    private int gotHitHash = Animator.StringToHash("GotHit");
    private int LefttoNormHash = Animator.StringToHash("LeftToNorm");
    private int RighttoNormHash = Animator.StringToHash("RightToNorm");

    public PlayerAnimator(Animator animator)
    {
        if (animator)
            this.animator = animator;
    }

    public void SetNormalFlying(bool isFLying)
    {
    
    }

    public void SetMovingLeftState()
    {
        animator.SetTrigger(leftHash);
    }
    public void SetMovingRightState()
    {
        animator.SetTrigger(rightHash);
    }
    public void SetUpState()
    {
        animator.SetTrigger(upHash);
    }
    public void SetDownState()
    {
        animator.SetTrigger(downHash);
    }
    public void SetSlideLeftState(bool isSlideLeft)
    {
        animator.SetBool(slideLeftHash, isSlideLeft);
    }

    public void SetSlideLefttoNorm(bool isSlideOver) 
    {
        animator.SetBool(LefttoNormHash, isSlideOver);
    }

    public void SetSlideRightState(bool isSlideRight)
    { 
        animator.SetBool(slideRightHash, isSlideRight);
    }

    public void SetSlideRighttoNorm(bool isSlideOver)
    {
        animator.SetBool(RighttoNormHash, isSlideOver);
    }

    public void SetInvincibleState(bool gotHit)
    {
        animator.SetBool(gotHitHash, gotHit);
    }
}
