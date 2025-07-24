using UnityEngine;
using System.Collections.Generic;

public class BoomerangChargeState : PlayerState_Explore
{
    private Vector3 aimDirection;
    private bool isCharging = false;
    private BoomerangLauncher boomerangLauncher;

    [Header("Aiming Parameters")]
    private float maxRange = 30f;
    private Vector3 currentTargetPosition;

    public BoomerangChargeState(PlayerStateMachine_Explore sm) : base(sm)
    {
        boomerangLauncher = stateMachine.player.GetComponent<BoomerangLauncher>();
    }

    public override void OnStateEnter()
    {
        isCharging = true;
        stateMachine.player.boomerangModel.gameObject.SetActive(true);
        aimDirection = Vector3.forward; // Default direction
        currentTargetPosition = transform.position + aimDirection * maxRange;

        // Set animation if available
        if (stateMachine.player.Animator != null)
        {
            stateMachine.player.Animator.SetBool("BoomerangCharging", true);
        }

        Debug.Log("Entered BoomerangChargeState - Ready to aim with WASD");
    }

    public override void OnStateExit()
    {
        isCharging = false;
        stateMachine.player.boomerangModel.gameObject.SetActive(false);
        // Cancel preview when exiting state
        if (stateMachine.PreviewController.IsPreviewing)
        {
            currentTargetPosition = stateMachine.PreviewController.EndPreview();
        }

        // Hide trajectory preview from boomerang launcher
        if (boomerangLauncher != null)
        {
            boomerangLauncher.HideTrajectoryPreview();
        }

        // Reset animation if available
        if (stateMachine.player.Animator != null)
        {
            stateMachine.player.Animator.SetBool("BoomerangCharging", false);
        }

        Debug.Log("Exited BoomerangChargeState");
    }

    public override void Tick()
    {
        // Check if still holding E or Q (SLIDERIGHT or SLIDELEFT)
        bool stillCharging = stateMachine.inputQueue.Contains(ECommand.SLIDERIGHT) ||
                           stateMachine.inputQueue.Contains(ECommand.SLIDELEFT);

        if (!stillCharging)
        {
            // Released E/Q - Launch boomerang and return to preview state
            LaunchBoomerang();
            stateMachine.player.isBoomerangCharging = false;
            stateMachine.SetState(stateMachine.PreviewState);
            return;
        }

        // Read WASD input for aiming
        UpdateAimDirection();

        // Use PreviewController similar to PreviewState for visual guidance
        Vector2 moveDir = Vector2.zero;
        foreach (var input in stateMachine.inputQueue)
        {
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
            // Keep preview active but don't end it while charging
            // The preview shows where the boomerang will go
        }
    }

    private void UpdateAimDirection()
    {
        Vector2 inputDir = Vector2.zero;

        // Process input queue for WASD
        foreach (var input in stateMachine.inputQueue)
        {
            switch (input)
            {
                case ECommand.LEFT:
                    inputDir.x -= 1f;
                    break;
                case ECommand.RIGHT:
                    inputDir.x += 1f;
                    break;
                case ECommand.UP:
                    inputDir.y += 1f;
                    break;
                case ECommand.DOWN:
                    inputDir.y -= 1f;
                    break;
            }
        }

        // Convert to 3D direction relative to camera
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 worldDirection = GetCameraRelativeDirection(new Vector3(inputDir.x, 0, inputDir.y));
            aimDirection = worldDirection.normalized;
        }

        // Calculate target position
        currentTargetPosition = transform.position + aimDirection * maxRange;

        // Optional: Raycast to find actual target position if hitting something
        RaycastHit hit;
        if (Physics.Raycast(transform.position, aimDirection, out hit, maxRange))
        {
            currentTargetPosition = hit.point;
        }
    }

    private Vector3 GetCameraRelativeDirection(Vector3 inputDir)
    {
        Transform cameraTransform = stateMachine.player.cameraTransform;
        if (cameraTransform == null) return inputDir;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = inputDir.z * camForward + inputDir.x * camRight;
        return moveDir.normalized;
    }

    private void LaunchBoomerang()
    {
        if (boomerangLauncher != null && !boomerangLauncher.IsBoomerangActive)
        {
            // Use the target position from preview controller if available
            Vector3 targetPos = currentTargetPosition;
            if (stateMachine.PreviewController.IsPreviewing)
            {
                targetPos = stateMachine.PreviewController.EndPreview();
            }

            boomerangLauncher.LaunchBoomerang(targetPos);
            Debug.Log($"Boomerang launched towards: {targetPos}");
        }
        else
        {
            Debug.LogWarning("Cannot launch boomerang - launcher not available or boomerang already active");
        }
    }
}