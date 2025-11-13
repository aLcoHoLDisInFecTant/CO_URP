using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ��ǿ��·�˿����� - ��ϸ��Ѱ·ϵͳʵ��
public class EnhancedNPCController : MonoBehaviour
{
    [Header("Ѱ·����")]
    public float pathfindingUpdateRate = 0.2f;    // ·������Ƶ��
    public float stuckDetectionTime = 3f;         // ��ס���ʱ��
    public float minMoveDistance = 0.1f;          // ��С�ƶ�����
    public float obstacleAvoidanceRadius = 1f;    // �ϰ�����ð뾶
    public float dynamicObstacleDetectionRange = 5f; // ��̬�ϰ����ⷶΧ

    [Header("·���Ż�")]
    public bool usePathSmoothing = true;          // �Ƿ�ʹ��·��ƽ��
    public int pathSmoothingIterations = 3;       // ·��ƽ����������
    public float cornerSlowdownDistance = 2f;     // ת����پ���

    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private Queue<Vector3> pathHistory = new Queue<Vector3>();
    private bool isRecalculatingPath = false;

    // ��̬�ϰ�����
    private List<Collider> nearbyObstacles = new List<Collider>();
    private Coroutine pathfindingCoroutine;

    void Start()
    {
        InitializeNavMeshAgent();
        lastPosition = transform.position;

        // ����·�����Э��
        pathfindingCoroutine = StartCoroutine(PathfindingUpdateLoop());
    }

    void InitializeNavMeshAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        // NavMeshAgent��ϸ����
        agent.baseOffset = 0f;                    // ����ƫ��
        agent.speed = 1.5f;                       // �ƶ��ٶ�
        agent.angularSpeed = 120f;                // ת���ٶ�
        agent.acceleration = 8f;                  // ���ٶ�
        agent.stoppingDistance = 0.5f;            // ֹͣ����
        agent.autoBraking = true;                 // �Զ�ɲ��

        // ��������
        agent.avoidancePriority = Random.Range(10, 90); // �������ȼ��������
        agent.radius = 0.4f;                      // ��ײ�뾶
        agent.height = 1.8f;                      // �߶�

        // ·����������
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.areaMask = NavMesh.AllAreas;        // ����ͨ��������
    }

    // ��ҪѰ·����ѭ��
    IEnumerator PathfindingUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(pathfindingUpdateRate);

            // ����Ƿ�ס
            DetectStuckState();

            // ��⶯̬�ϰ���
            DetectDynamicObstacles();

            // ·���Ż�
            if (usePathSmoothing && agent.hasPath)
            {
                SmoothPath();
            }

            // ����ת�����
            HandleCornerSlowdown();
        }
    }

    // ��ס״̬���
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
                    Debug.Log($"{gameObject.name} ��⵽��ס״̬�����¼���·��");
                    HandleStuckState();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f; // ���ÿ�ס��ʱ��
            }
        }

        lastPosition = transform.position;
    }

    // ������ס״̬
    void HandleStuckState()
    {
        // ����1: �������¼��㵽ͬһĿ���·��
        if (agent.destination != Vector3.zero)
        {
            Vector3 originalDestination = agent.destination;
            agent.ResetPath();

            StartCoroutine(RecalculatePathWithOffset(originalDestination));
        }
    }

    // ��ƫ�Ƶ�·���ؼ���
    IEnumerator RecalculatePathWithOffset(Vector3 originalDestination)
    {
        isRecalculatingPath = true;

        // ���Զ��ƫ��λ��
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

            // ��NavMesh�ϲ�����Чλ��
            if (NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetPath(path);
                        Debug.Log($"�ɹ��ؼ���·����ʹ��ƫ��: {offset}");
                        break;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }

        isRecalculatingPath = false;
    }

    // ��̬�ϰ�����
    void DetectDynamicObstacles()
    {
        nearbyObstacles.Clear();

        // ��⸽������ײ�壨����NPC�����ﳵ�ȣ�
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            dynamicObstacleDetectionRange,
            LayerMask.GetMask("NPC", "DynamicObstacle")
        );

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject) // �ų��Լ�
            {
                nearbyObstacles.Add(col);
            }
        }

        // ���ǰ�����ϰ������·��
        if (nearbyObstacles.Count > 0 && !isRecalculatingPath)
        {
            HandleDynamicObstacles();
        }
    }

    // ������̬�ϰ���
    void HandleDynamicObstacles()
    {
        Vector3 avoidanceForce = Vector3.zero;

        foreach (Collider obstacle in nearbyObstacles)
        {
            Vector3 directionToObstacle = obstacle.transform.position - transform.position;
            float distance = directionToObstacle.magnitude;

            if (distance < obstacleAvoidanceRadius && distance > 0)
            {
                // ���������
                Vector3 avoidanceDirection = -directionToObstacle.normalized;
                float avoidanceStrength = (obstacleAvoidanceRadius - distance) / obstacleAvoidanceRadius;
                avoidanceForce += avoidanceDirection * avoidanceStrength;
            }
        }

        // Ӧ�ñ�����
        if (avoidanceForce.magnitude > 0.1f)
        {
            ApplyAvoidanceForce(avoidanceForce);
        }
    }

    // Ӧ�ñ�����
    void ApplyAvoidanceForce(Vector3 avoidanceForce)
    {
        Vector3 newDirection = (transform.forward + avoidanceForce.normalized * 0.5f).normalized;
        Vector3 newTarget = transform.position + newDirection * 2f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newTarget, out hit, 2f, NavMesh.AllAreas))
        {
            // ��ʱ�����µ��м�Ŀ���
            Vector3 originalDestination = agent.destination;
            agent.SetDestination(hit.position);

            // �ӳٻָ�ԭ·��
            StartCoroutine(RestoreOriginalPath(originalDestination, 1f));
        }
    }

    // �ָ�ԭʼ·��
    IEnumerator RestoreOriginalPath(Vector3 originalDestination, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Vector3.Distance(transform.position, originalDestination) > agent.stoppingDistance)
        {
            agent.SetDestination(originalDestination);
        }
    }

    // ·��ƽ������
    void SmoothPath()
    {
        if (!agent.hasPath || agent.path.corners.Length < 3) return;

        NavMeshPath smoothedPath = new NavMeshPath();
        List<Vector3> smoothedCorners = new List<Vector3>();

        // ������ʼ��
        smoothedCorners.Add(agent.path.corners[0]);

        // ���м�����ƽ������
        for (int i = 1; i < agent.path.corners.Length - 1; i++)
        {
            Vector3 prevCorner = agent.path.corners[i - 1];
            Vector3 currentCorner = agent.path.corners[i];
            Vector3 nextCorner = agent.path.corners[i + 1];

            // ʹ�����Բ�ֵ����ƽ��
            Vector3 smoothedCorner = Vector3.Lerp(
                Vector3.Lerp(prevCorner, currentCorner, 0.5f),
                Vector3.Lerp(currentCorner, nextCorner, 0.5f),
                0.5f
            );

            // ȷ��ƽ����ĵ���NavMesh��
            NavMeshHit hit;
            if (NavMesh.SamplePosition(smoothedCorner, out hit, 1f, NavMesh.AllAreas))
            {
                smoothedCorners.Add(hit.position);
            }
            else
            {
                smoothedCorners.Add(currentCorner); // �������NavMesh�ϣ�ʹ��ԭʼ��
            }
        }

        // �����յ�
        smoothedCorners.Add(agent.path.corners[agent.path.corners.Length - 1]);

        // �����µ�ƽ��·��
        if (NavMesh.CalculatePath(transform.position, smoothedCorners[smoothedCorners.Count - 1], NavMesh.AllAreas, smoothedPath))
        {
            agent.SetPath(smoothedPath);
        }
    }

    // ת����ٴ���
    void HandleCornerSlowdown()
    {
        if (!agent.hasPath || agent.path.corners.Length < 2) return;

        // �ҵ���һ��ת���
        Vector3 nextCorner = agent.path.corners[1];
        float distanceToCorner = Vector3.Distance(transform.position, nextCorner);

        if (distanceToCorner <= cornerSlowdownDistance)
        {
            // ����ת��Ƕ�
            Vector3 directionToCorner = (nextCorner - transform.position).normalized;
            Vector3 currentDirection = transform.forward;
            float angle = Vector3.Angle(currentDirection, directionToCorner);

            // ���ݽǶȵ����ٶ�
            float speedMultiplier = Mathf.Lerp(0.3f, 1f, 1f - (angle / 180f));
            agent.speed = agent.speed * speedMultiplier;
        }
    }

    // �߼�Ѱ·���� - ��·����֤
    public bool MoveToWithValidation(Vector3 destination)
    {
        // ������֤Ŀ��λ���Ƿ���Ч
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(destination, out hit, 5f, NavMesh.AllAreas))
        {
            Debug.LogWarning($"Ŀ��λ�� {destination} ����NavMesh��");
            return false;
        }

        // ����·��
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
        {
            Debug.LogWarning("�޷����㵽Ŀ��λ�õ�·��");
            return false;
        }

        // ���·��״̬
        switch (path.status)
        {
            case NavMeshPathStatus.PathComplete:
                agent.SetPath(path);
                LogPathInfo(path);
                return true;

            case NavMeshPathStatus.PathPartial:
                Debug.LogWarning("ֻ���ҵ�����·��");
                agent.SetPath(path); // ��Ȼ���Գ����߲���·��
                return true;

            case NavMeshPathStatus.PathInvalid:
                Debug.LogError("·����Ч");
                return false;

            default:
                return false;
        }
    }

    // ·����Ϣ��־
    void LogPathInfo(NavMeshPath path)
    {
        float pathLength = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            pathLength += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        Debug.Log($"·������ɹ�: ����={pathLength:F2}m, ת�����={path.corners.Length}");
    }

    // �����鷽��
    public bool IsPositionWalkable(Vector3 position, float radius = 1f)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, radius, NavMesh.AllAreas);
    }

    // ��ȡ����Ŀ�����λ��
    public Vector3 GetNearestWalkablePosition(Vector3 position, float searchRadius = 10f)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, searchRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position; // ����Ҳ��������ص�ǰλ��
    }

    // ·�����ȼ���
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

    // ֹͣѰ·
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

    // Debug���ӻ�
    void OnDrawGizmosSelected()
    {
        // ���Ƽ�ⷶΧ
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dynamicObstacleDetectionRange);

        // ���Ʊ��ð뾶
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRadius);

        // ����·��
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = agent.path.corners;
            for (int i = 1; i < corners.Length; i++)
            {
                Gizmos.DrawLine(corners[i - 1], corners[i]);
            }
        }

        // ���Ƽ�⵽���ϰ���
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

// NavMesh�決�����ù�����
public class NavMeshBakeManager : MonoBehaviour
{
    [Header("NavMesh�決����")]
    public float agentRadius = 0.4f;
    public float agentHeight = 1.8f;
    public float maxSlope = 45f;
    public float stepHeight = 0.4f;

    [Header("��������")]
    public string[] walkableAreas = { "Walkable", "ShoppingArea" };
    public string[] nonWalkableAreas = { "Obstacle", "Staff Only" };

    void Start()
    {
        ValidateNavMeshSettings();
    }

    void ValidateNavMeshSettings()
    {
        NavMeshBuildSettings buildSettings = NavMesh.GetSettingsByID(0);

        Debug.Log($"��ǰNavMesh����:");
        Debug.Log($"- �����뾶: {buildSettings.agentRadius}");
        Debug.Log($"- �����߶�: {buildSettings.agentHeight}");
        Debug.Log($"- ����¶�: {buildSettings.agentSlope}");
        Debug.Log($"- ̨�׸߶�: {buildSettings.agentClimb}");
    }

    // ����ʱ��̬�����ϰ���
    public void AddDynamicObstacle(GameObject obstacle)
    {
        NavMeshObstacle navObstacle = obstacle.GetComponent<NavMeshObstacle>();
        if (navObstacle == null)
        {
            navObstacle = obstacle.AddComponent<NavMeshObstacle>();
        }

        navObstacle.carving = true; // ���õ�̹���
        navObstacle.carvingMoveThreshold = 0.1f;
        navObstacle.carvingTimeToStationary = 0.5f;
    }
}
