using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 增强版路人控制器 - 详细的寻路系统实现
public class EnhancedNPCController : MonoBehaviour
{
    [Header("寻路配置")]
    public float pathfindingUpdateRate = 0.2f;    // 路径更新频率
    public float stuckDetectionTime = 3f;         // 卡住检测时间
    public float minMoveDistance = 0.1f;          // 最小移动距离
    public float obstacleAvoidanceRadius = 1f;    // 障碍物避让半径
    public float dynamicObstacleDetectionRange = 5f; // 动态障碍物检测范围

    [Header("路径优化")]
    public bool usePathSmoothing = true;          // 是否使用路径平滑
    public int pathSmoothingIterations = 3;       // 路径平滑迭代次数
    public float cornerSlowdownDistance = 2f;     // 转弯减速距离

    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private Queue<Vector3> pathHistory = new Queue<Vector3>();
    private bool isRecalculatingPath = false;

    // 动态障碍物检测
    private List<Collider> nearbyObstacles = new List<Collider>();
    private Coroutine pathfindingCoroutine;

    void Start()
    {
        InitializeNavMeshAgent();
        lastPosition = transform.position;

        // 启动路径检测协程
        pathfindingCoroutine = StartCoroutine(PathfindingUpdateLoop());
    }

    void InitializeNavMeshAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // NavMeshAgent详细配置
        agent.baseOffset = 0f;                    // 基础偏移
        agent.speed = 1.5f;                       // 移动速度
        agent.angularSpeed = 120f;                // 转向速度
        agent.acceleration = 8f;                  // 加速度
        agent.stoppingDistance = 0.5f;            // 停止距离
        agent.autoBraking = true;                 // 自动刹车

        // 避让配置
        agent.avoidancePriority = Random.Range(10, 90); // 避让优先级（随机）
        agent.radius = 0.4f;                      // 碰撞半径
        agent.height = 1.8f;                      // 高度

        // 路径质量设置
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.areaMask = NavMesh.AllAreas;        // 允许通过的区域
    }

    // 主要寻路更新循环
    IEnumerator PathfindingUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(pathfindingUpdateRate);

            // 检测是否卡住
            DetectStuckState();

            // 检测动态障碍物
            DetectDynamicObstacles();

            // 路径优化
            if (usePathSmoothing && agent.hasPath)
            {
                SmoothPath();
            }

            // 处理转弯减速
            HandleCornerSlowdown();
        }
    }

    // 卡住状态检测
    void DetectStuckState()
    {
        float moveDistance = Vector3.Distance(transform.position, lastPosition);

        if (agent.hasPath && agent.remainingDistance > 0.5f)
        {
            if (moveDistance < minMoveDistance)
            {
                stuckTimer += pathfindingUpdateRate;

                if (stuckTimer >= stuckDetectionTime)
                {
                    Debug.Log($"{gameObject.name} 检测到卡住状态，重新计算路径");
                    HandleStuckState();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f; // 重置卡住计时器
            }
        }

        lastPosition = transform.position;
    }

    // 处理卡住状态
    void HandleStuckState()
    {
        // 方法1: 尝试重新计算到同一目标的路径
        if (agent.destination != Vector3.zero)
        {
            Vector3 originalDestination = agent.destination;
            agent.ResetPath();

            StartCoroutine(RecalculatePathWithOffset(originalDestination));
        }
    }

    // 带偏移的路径重计算
    IEnumerator RecalculatePathWithOffset(Vector3 originalDestination)
    {
        isRecalculatingPath = true;

        // 尝试多个偏移位置
        Vector3[] offsets = {
            Vector3.zero,
            Vector3.right * 0.5f,
            Vector3.left * 0.5f,
            Vector3.forward * 0.5f,
            Vector3.back * 0.5f
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 targetPos = originalDestination + offset;
            NavMeshHit hit;

            // 在NavMesh上采样有效位置
            if (NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetPath(path);
                        Debug.Log($"成功重计算路径，使用偏移: {offset}");
                        break;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }

        isRecalculatingPath = false;
    }

    // 动态障碍物检测
    void DetectDynamicObstacles()
    {
        nearbyObstacles.Clear();

        // 检测附近的碰撞体（其他NPC、购物车等）
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            dynamicObstacleDetectionRange,
            LayerMask.GetMask("NPC", "DynamicObstacle")
        );

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject) // 排除自己
            {
                nearbyObstacles.Add(col);
            }
        }

        // 如果前方有障碍物，调整路径
        if (nearbyObstacles.Count > 0 && !isRecalculatingPath)
        {
            HandleDynamicObstacles();
        }
    }

    // 处理动态障碍物
    void HandleDynamicObstacles()
    {
        Vector3 avoidanceForce = Vector3.zero;

        foreach (Collider obstacle in nearbyObstacles)
        {
            Vector3 directionToObstacle = obstacle.transform.position - transform.position;
            float distance = directionToObstacle.magnitude;

            if (distance < obstacleAvoidanceRadius && distance > 0)
            {
                // 计算避让力
                Vector3 avoidanceDirection = -directionToObstacle.normalized;
                float avoidanceStrength = (obstacleAvoidanceRadius - distance) / obstacleAvoidanceRadius;
                avoidanceForce += avoidanceDirection * avoidanceStrength;
            }
        }

        // 应用避让力
        if (avoidanceForce.magnitude > 0.1f)
        {
            ApplyAvoidanceForce(avoidanceForce);
        }
    }

    // 应用避让力
    void ApplyAvoidanceForce(Vector3 avoidanceForce)
    {
        Vector3 newDirection = (transform.forward + avoidanceForce.normalized * 0.5f).normalized;
        Vector3 newTarget = transform.position + newDirection * 2f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newTarget, out hit, 2f, NavMesh.AllAreas))
        {
            // 临时设置新的中间目标点
            Vector3 originalDestination = agent.destination;
            agent.SetDestination(hit.position);

            // 延迟恢复原路径
            StartCoroutine(RestoreOriginalPath(originalDestination, 1f));
        }
    }

    // 恢复原始路径
    IEnumerator RestoreOriginalPath(Vector3 originalDestination, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Vector3.Distance(transform.position, originalDestination) > agent.stoppingDistance)
        {
            agent.SetDestination(originalDestination);
        }
    }

    // 路径平滑处理
    void SmoothPath()
    {
        if (!agent.hasPath || agent.path.corners.Length < 3) return;

        NavMeshPath smoothedPath = new NavMeshPath();
        List<Vector3> smoothedCorners = new List<Vector3>();

        // 添加起始点
        smoothedCorners.Add(agent.path.corners[0]);

        // 对中间点进行平滑处理
        for (int i = 1; i < agent.path.corners.Length - 1; i++)
        {
            Vector3 prevCorner = agent.path.corners[i - 1];
            Vector3 currentCorner = agent.path.corners[i];
            Vector3 nextCorner = agent.path.corners[i + 1];

            // 使用线性插值进行平滑
            Vector3 smoothedCorner = Vector3.Lerp(
                Vector3.Lerp(prevCorner, currentCorner, 0.5f),
                Vector3.Lerp(currentCorner, nextCorner, 0.5f),
                0.5f
            );

            // 确保平滑后的点在NavMesh上
            NavMeshHit hit;
            if (NavMesh.SamplePosition(smoothedCorner, out hit, 1f, NavMesh.AllAreas))
            {
                smoothedCorners.Add(hit.position);
            }
            else
            {
                smoothedCorners.Add(currentCorner); // 如果不在NavMesh上，使用原始点
            }
        }

        // 添加终点
        smoothedCorners.Add(agent.path.corners[agent.path.corners.Length - 1]);

        // 创建新的平滑路径
        if (NavMesh.CalculatePath(transform.position, smoothedCorners[smoothedCorners.Count - 1], NavMesh.AllAreas, smoothedPath))
        {
            agent.SetPath(smoothedPath);
        }
    }

    // 转弯减速处理
    void HandleCornerSlowdown()
    {
        if (!agent.hasPath || agent.path.corners.Length < 2) return;

        // 找到下一个转弯点
        Vector3 nextCorner = agent.path.corners[1];
        float distanceToCorner = Vector3.Distance(transform.position, nextCorner);

        if (distanceToCorner <= cornerSlowdownDistance)
        {
            // 计算转弯角度
            Vector3 directionToCorner = (nextCorner - transform.position).normalized;
            Vector3 currentDirection = transform.forward;
            float angle = Vector3.Angle(currentDirection, directionToCorner);

            // 根据角度调整速度
            float speedMultiplier = Mathf.Lerp(0.3f, 1f, 1f - (angle / 180f));
            agent.speed = agent.speed * speedMultiplier;
        }
    }

    // 高级寻路方法 - 带路径验证
    public bool MoveToWithValidation(Vector3 destination)
    {
        // 首先验证目标位置是否有效
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(destination, out hit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"目标位置 {destination} 不在NavMesh上");
            return false;
        }

        // 计算路径
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
        {
            Debug.LogWarning("无法计算到目标位置的路径");
            return false;
        }

        // 检查路径状态
        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                agent.SetPath(path);
                LogPathInfo(path);
                return true;

            case NavMeshPathStatus.PathPartial:
                Debug.LogWarning("只能找到部分路径");
                agent.SetPath(path); // 仍然可以尝试走部分路径
                return true;

            case NavMeshPathStatus.PathInvalid:
                Debug.LogError("路径无效");
                return false;

            default:
                return false;
        }
    }

    // 路径信息日志
    void LogPathInfo(NavMeshPath path)
    {
        float pathLength = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        Debug.Log($"路径计算成功: 长度={pathLength:F2}m, 转弯点数={path.corners.Length}");
    }

    // 区域检查方法
    public bool IsPositionWalkable(Vector3 position, float radius = 1f)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, radius, NavMesh.AllAreas);
    }

    // 获取最近的可行走位置
    public Vector3 GetNearestWalkablePosition(Vector3 position, float searchRadius = 10f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, searchRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position; // 如果找不到，返回当前位置
    }

    // 路径长度计算
    public float GetPathLength()
    {
        if (!agent.hasPath) return 0f;

        float length = 0f;
        Vector3[] corners = agent.path.corners;

        for (int i = 1; i < corners.Length; i++)
        {
            length += Vector3.Distance(corners[i - 1], corners[i]);
        }

        return length;
    }

    // 停止寻路
    public void StopNavigation()
    {
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        if (pathfindingCoroutine != null)
        {
            StopCoroutine(pathfindingCoroutine);
        }
    }

    void OnDestroy()
    {
        StopNavigation();
    }

    // Debug可视化
    void OnDrawGizmosSelected()
    {
        // 绘制检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dynamicObstacleDetectionRange);

        // 绘制避让半径
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);

        // 绘制路径
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = agent.path.corners;
            for (int i = 1; i < corners.Length; i++)
            {
                Gizmos.DrawLine(corners[i - 1], corners[i]);
            }
        }

        // 绘制检测到的障碍物
        Gizmos.color = Color.magenta;
        foreach (Collider obstacle in nearbyObstacles)
        {
            if (obstacle != null)
            {
                Gizmos.DrawLine(transform.position, obstacle.transform.position);
            }
        }
    }
}

// NavMesh烘焙和配置管理器
public class NavMeshBakeManager : MonoBehaviour
{
    [Header("NavMesh烘焙设置")]
    public float agentRadius = 0.4f;
    public float agentHeight = 1.8f;
    public float maxSlope = 45f;
    public float stepHeight = 0.4f;

    [Header("区域设置")]
    public string[] walkableAreas = { "Walkable", "ShoppingArea" };
    public string[] nonWalkableAreas = { "Obstacle", "Staff Only" };

    void Start()
    {
        ValidateNavMeshSettings();
    }

    void ValidateNavMeshSettings()
    {
        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);

        Debug.Log($"当前NavMesh设置:");
        Debug.Log($"- 代理半径: {buildSettings.agentRadius}");
        Debug.Log($"- 代理高度: {buildSettings.agentHeight}");
        Debug.Log($"- 最大坡度: {buildSettings.agentSlope}");
        Debug.Log($"- 台阶高度: {buildSettings.agentClimb}");
    }

    // 运行时动态添加障碍物
    public void AddDynamicObstacle(GameObject obstacle)
    {
        NavMeshObstacle navObstacle = obstacle.GetComponent<NavMeshObstacle>();
        if (navObstacle == null)
        {
            navObstacle = obstacle.AddComponent<NavMeshObstacle>();
        }

        navObstacle.carving = true; // 启用雕刻功能
        navObstacle.carvingMoveThreshold = 0.1f;
        navObstacle.carvingTimeToStationary = 0.5f;
    }
}