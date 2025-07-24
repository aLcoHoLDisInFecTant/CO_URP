using UnityEngine;

public class GrappleAimState : PlayerState_Explore
{
    private bool isFiring = false;
    private Vector3 fireDirection;
    private Vector3 startPoint;
    private Vector3 currentEndPoint;
    private float grappleSpeed = 60f;

    private LineRenderer lineRenderer;
    private float maxDistance = 50f;
    private LayerMask grappleLayer;
    private bool hasHit = false;
    private RaycastHit hitInfo;

    public GrappleAimState(PlayerStateMachine_Explore sm) : base(sm) { }

    public override void OnStateEnter()
    {
        isFiring = false;
        hasHit = false;

        lineRenderer = stateMachine.player.grappleLauncher.lr;
        lineRenderer.positionCount = 2;

        grappleLayer = stateMachine.player.grappleLauncher.whatIsGrappleable;

        stateMachine.player.child.gameObject.SetActive(true);
        stateMachine.player.Animator.SetBool("Aiming", true);
    }

    public override void Tick()
    {
        if (!isFiring && !stateMachine.inputQueue.Contains(ECommand.SLIDERIGHT) &&
                        !stateMachine.inputQueue.Contains(ECommand.SLIDELEFT)) 
        
        {
            stateMachine.SetState(stateMachine.PreviewState);
        }

        

        // 若还未发射，则继续读取输入方向
        Vector2 moveDir = Vector2.zero;
        foreach (var input in stateMachine.inputQueue)
        {
            if (input == ECommand.LEFT) moveDir.x -= 1;
            if (input == ECommand.RIGHT) moveDir.x += 1;
            if (input == ECommand.UP) moveDir.y += 1;
            if (input == ECommand.DOWN) moveDir.y -= 1;
        }

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Vector3 direction = new Vector3(moveDir.x, 0, moveDir.y).normalized;

            if (!stateMachine.PreviewController.IsPreviewing)
                stateMachine.PreviewController.StartPreview(direction);
            else
                stateMachine.PreviewController.UpdatePreview(direction);
        }
        else if (stateMachine.PreviewController.IsPreviewing)
        {
            Vector3 target = stateMachine.PreviewController.EndPreview();

            // ✅ 开始发射逻辑（延迟检测 + LineRender 显示）
            isFiring = true;
            startPoint = stateMachine.Transform.position;
            currentEndPoint = startPoint;
            fireDirection = (target - startPoint).normalized;
        }
    }

    public override void OnStateExit()
    {
        stateMachine.PreviewController.CancelPreview();
        if (lineRenderer != null) lineRenderer.positionCount = 0;

        stateMachine.player.child.gameObject.SetActive(false);
        stateMachine.player.Animator.SetBool("Aiming", false);
    }
}
