using UnityEngine;

public class MoveToTargetState : PlayerState_Explore
{
    private Vector3 targetPosition;
    private float speed;

    // === 新增：按层拦截 ===
    private LayerMask stopMask;                       // 要拦截的层
    private const string STOP_LAYER_NAME = "NotWalkable"; // 默认层名（可在Unity里创建）

    // === 基础缓存 ===
    private Rigidbody rb;
    private const float kStopDist = 0.1f;  // 认为到达目标的距离
    private const float kSkin = 0.05f;     // 前探冗余距离
    private const float kRayHeight = 0.5f; // 射线高度（根据角色身高可调）

    public MoveToTargetState(PlayerStateMachine_Explore sm) : base(sm)
    {
        speed = data.Speed;
        // 默认用层名取 LayerMask；若你后续用 SetStopLayers 注入就会覆盖它
        stopMask = LayerMask.GetMask(STOP_LAYER_NAME);
    }

    // 可选：在创建状态后由外部注入层（例如在状态机构造里调用）
    public void SetStopLayers(LayerMask mask) => stopMask = mask;

    public void SetTarget(Vector3 target) => targetPosition = target;

    public override void OnStateEnter()
    {
        rb = stateMachine.player.Rb; // 你的玩家刚体
    }

    public override void OnStateExit()
    {
        if (rb) rb.velocity = Vector3.zero;
    }

    public override void Tick() { }

    public override void FixedTick()
    {
        // 1) 目标与朝向
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= kStopDist)
        {
            stateMachine.SetState(stateMachine.PreviewState);
            return;
        }

        Vector3 dir = toTarget.normalized;
        float step = speed * Time.fixedDeltaTime;

        // 2) 按层前探：命中 stopMask 则立即回默认
        Vector3 rayOrigin = transform.position + Vector3.up * kRayHeight;
        float rayDist = step + kSkin;

        // 注意：使用 Collide 以便触发器也能被拦截
        bool hitBlocked = Physics.Raycast(
            rayOrigin, dir, rayDist, stopMask, QueryTriggerInteraction.Collide
        );

        // 可视化（调试用）
        Debug.DrawRay(rayOrigin, dir * rayDist, hitBlocked ? Color.red : Color.green, Time.fixedDeltaTime);

        if (hitBlocked)
        {
            if (rb) rb.velocity = Vector3.zero;
            stateMachine.SetState(stateMachine.PreviewState);
            return;
        }

        // 3) 未命中则推进
        transform.forward = dir;
        stateMachine.Move(dir * step);
    }
}
