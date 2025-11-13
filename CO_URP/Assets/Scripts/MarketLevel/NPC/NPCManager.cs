using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ·������ö��
public enum NPCType
{
    QuickShopper,      // ���ٹ�����
    BrowserShopper,    // �й乺����
    TargetShopper,     // Ŀ����ȷ������
    Employee,          // Ա��
    Wanderer          // ����й���
}

// ·��״̬ö��
public enum NPCState
{
    Idle,
    Moving,
    Shopping,
    Waiting,
    Leaving
}

// �����λ��Ϣ
[System.Serializable]
public class ShoppingPoint
{
    public Transform point;
    public string category;        // ��Ʒ�����"food", "drinks", "electronics"��
    public float priority;         // ���ȼ�
    public float shopTime;         // ����ʱ��
}

// ·�˹����� - �����ƽű�
public class NPCManager : MonoBehaviour
{
    [Header("·��Ԥ����")]
    public GameObject[] npcPrefabs;

    [Header("���ɵ�λ")]
    public Transform[] spawnPoints;
    public Transform[] exitPoints;

    [Header("�����λ")]
    public ShoppingPoint[] shoppingPoints;

    [Header("��������")]
    public int maxNPCCount = 20;
    public float spawnInterval = 3f;

    [Header("·������Ȩ��")]
    [Range(0f, 1f)] public float quickShopperWeight = 0.3f;
    [Range(0f, 1f)] public float browserShopperWeight = 0.25f;
    [Range(0f, 1f)] public float targetShopperWeight = 0.25f;
    [Range(0f, 1f)] public float employeeWeight = 0.1f;
    [Range(0f, 1f)] public float wandererWeight = 0.1f;

    private List<NPCController> activeNPCs = new List<NPCController>();
    private Coroutine spawnCoroutine;

    void Start()
    {
        // ��֤�����λ��NavMesh
        ValidateShoppingPoints();

        // ��ʼ����·��
        spawnCoroutine = StartCoroutine(SpawnNPCs());
    }

    void ValidateShoppingPoints()
    {
        foreach (var point in shoppingPoints)
        {
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(point.point.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"Shopping point {point.point.name} is not on NavMesh!");
            }
        }
    }

    IEnumerator SpawnNPCs()
    {
        while (true)
        {
            if (activeNPCs.Count < maxNPCCount)
            {
                SpawnRandomNPC();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomNPC()
    {
        if (spawnPoints.Length == 0 || npcPrefabs.Length == 0) return;

        // ���ѡ�����ɵ��Ԥ����
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];

        // ����Ȩ��ѡ��·������
        NPCType npcType = GetRandomNPCType();

        // ����·��
        GameObject npcObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        NPCController npcController = npcObj.GetComponent<NPCController>();

        if (npcController == null)
        {
            npcController = npcObj.AddComponent<NPCController>();
        }

        // ��ʼ��·��
        npcController.Initialize(this, npcType);
        activeNPCs.Add(npcController);
    }

    NPCType GetRandomNPCType()
    {
        float[] weights = { quickShopperWeight, browserShopperWeight, targetShopperWeight, employeeWeight, wandererWeight };
        float totalWeight = 0f;

        foreach (float weight in weights)
            totalWeight += weight;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
            {
                return (NPCType)i;
            }
        }

        return NPCType.Wanderer;
    }

    public void RemoveNPC(NPCController npc)
    {
        if (activeNPCs.Contains(npc))
        {
            activeNPCs.Remove(npc);
        }
    }

    public ShoppingPoint[] GetShoppingPointsByCategory(string category)
    {
        List<ShoppingPoint> points = new List<ShoppingPoint>();
        foreach (var point in shoppingPoints)
        {
            if (point.category == category)
            {
                points.Add(point);
            }
        }
        return points.ToArray();
    }

    public ShoppingPoint GetRandomShoppingPoint()
    {
        if (shoppingPoints.Length > 0)
            return shoppingPoints[Random.Range(0, shoppingPoints.Length)];
        return null;
    }

    public Transform GetRandomExitPoint()
    {
        if (exitPoints.Length > 0)
            return exitPoints[Random.Range(0, exitPoints.Length)];
        return null;
    }
}

// ·�˿����� - ����·�˵���Ϊ����
public class NPCController : MonoBehaviour
{
    [Header("·����Ϣ")]
    public NPCType npcType;
    public NPCState currentState;

    [Header("�ƶ�����")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float rotationSpeed = 120f;

    private NPCManager manager;
    private NavMeshAgent agent;
    private Animator animator;
    private NPCBehavior currentBehavior;

    // ��Ϊģʽ�ֵ�
    private Dictionary<NPCType, NPCBehavior> behaviors;

    public void Initialize(NPCManager npcManager, NPCType type)
    {
        manager = npcManager;
        npcType = type;

        // ��ȡ���
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();

        // ����NavMeshAgent
        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = 0.5f;

        // ��ʼ����Ϊģʽ
        InitializeBehaviors();

        // ���õ�ǰ��Ϊ
        SetBehavior(npcType);

        // ��ʼ��Ϊ
        currentState = NPCState.Idle;
        if (currentBehavior != null)
        {
            StartCoroutine(currentBehavior.ExecuteBehavior());
        }
    }

    void InitializeBehaviors()
    {
        behaviors = new Dictionary<NPCType, NPCBehavior>
        {
            { NPCType.QuickShopper, new QuickShopperBehavior(this, manager) },
            { NPCType.BrowserShopper, new BrowserShopperBehavior(this, manager) },
            { NPCType.TargetShopper, new TargetShopperBehavior(this, manager) },
            { NPCType.Employee, new EmployeeBehavior(this, manager) },
            { NPCType.Wanderer, new WandererBehavior(this, manager) }
        };
    }

    void SetBehavior(NPCType type)
    {
        if (behaviors.ContainsKey(type))
        {
            currentBehavior = behaviors[type];
        }
    }

    public void MoveTo(Vector3 destination)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(destination);
            currentState = NPCState.Moving;

            // ��������
            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
    }

    public void Stop()
    {
        if (agent != null)
        {
            agent.ResetPath();
            currentState = NPCState.Idle;

            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
        }
    }

    public bool HasReachedDestination()
    {
        return agent != null && !agent.pathPending && agent.remainingDistance < 0.5f;
    }

    public void SetSpeed(float speed)
    {
        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    public void DestroyNPC()
    {
        manager.RemoveNPC(this);
        Destroy(gameObject, 0.1f);
    }

    void Update()
    {
        // ����Ƿ񵽴�Ŀ�ĵ�
        if (currentState == NPCState.Moving && HasReachedDestination())
        {
            currentState = NPCState.Idle;
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
        }
    }
}

// ·����Ϊ����
public abstract class NPCBehavior
{
    protected NPCController npc;
    protected NPCManager manager;

    public NPCBehavior(NPCController npcController, NPCManager npcManager)
    {
        npc = npcController;
        manager = npcManager;
    }

    public abstract IEnumerator ExecuteBehavior();
}

// ���ٹ�������Ϊ
public class QuickShopperBehavior : NPCBehavior
{
    private int targetItemCount;
    private int currentItemCount = 0;

    public QuickShopperBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager)
    {
        targetItemCount = Random.Range(1, 4); // ����1-3����Ʒ
    }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.runSpeed * 0.8f); // �Կ���ٶ�

        while (currentItemCount < targetItemCount)
        {
            // ѡ�����
            ShoppingPoint targetPoint = manager.GetRandomShoppingPoint();
            if (targetPoint != null)
            {
                // ǰ�������
                npc.MoveTo(targetPoint.point.position);

                // �ȴ�����
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // ���ٹ���
                npc.currentState = NPCState.Shopping;
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

                currentItemCount++;
            }
            else
            {
                break;
            }
        }

        // ǰ������
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}

// �й乺������Ϊ
public class BrowserShopperBehavior : NPCBehavior
{
    public BrowserShopperBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed * 0.7f); // �������ٶ�

        float totalTime = Random.Range(60f, 120f); // �ܹ�ͣ��60-120��
        float startTime = Time.time;

        while (Time.time - startTime < totalTime)
        {
            // ���ѡ������й�
            ShoppingPoint targetPoint = manager.GetRandomShoppingPoint();
            if (targetPoint != null)
            {
                npc.MoveTo(targetPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // ��ʱ�����
                npc.currentState = NPCState.Shopping;
                yield return new WaitForSeconds(Random.Range(5f, 15f));

                // ��������Ƿ���
                if (Random.value < 0.3f) // 30%���ʹ���
                {
                    yield return new WaitForSeconds(Random.Range(2f, 5f));
                }
            }
        }

        // �뿪
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}

// Ŀ����ȷ��������Ϊ
public class TargetShopperBehavior : NPCBehavior
{
    private string[] targetCategories;
    private int currentCategoryIndex = 0;

    public TargetShopperBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager)
    {
        // ���ѡ��2-4��Ŀ�����
        string[] allCategories = { "food", "drinks", "electronics", "clothing", "books" };
        int categoryCount = Random.Range(2, 5);
        targetCategories = new string[categoryCount];

        for (int i = 0; i < categoryCount; i++)
        {
            targetCategories[i] = allCategories[Random.Range(0, allCategories.Length)];
        }
    }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed);

        foreach (string category in targetCategories)
        {
            // Ѱ���ض����Ĺ����
            ShoppingPoint[] categoryPoints = manager.GetShoppingPointsByCategory(category);

            if (categoryPoints.Length > 0)
            {
                ShoppingPoint targetPoint = categoryPoints[Random.Range(0, categoryPoints.Length)];

                // ֱ��ǰ��Ŀ������
                npc.MoveTo(targetPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // ����
                npc.currentState = NPCState.Shopping;
                yield return new WaitForSeconds(Random.Range(3f, 8f));
            }
        }

        // ǰ������
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}

// Ա����Ϊ
public class EmployeeBehavior : NPCBehavior
{
    private Transform[] workStations;

    public EmployeeBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed);

        while (true) // Ա�����뿪
        {
            // �ڲ�ͬ��������֮��Ѳ��
            ShoppingPoint workPoint = manager.GetRandomShoppingPoint();
            if (workPoint != null)
            {
                npc.MoveTo(workPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // �������������ܡ����ȣ�
                npc.currentState = NPCState.Shopping; // ����״̬��ʾ����
                yield return new WaitForSeconds(Random.Range(10f, 30f));

                // ż��ͣ������Ϣ
                if (Random.value < 0.2f)
                {
                    npc.currentState = NPCState.Waiting;
                    yield return new WaitForSeconds(Random.Range(5f, 10f));
                }
            }
        }
    }
}

// ����й�����Ϊ
public class WandererBehavior : NPCBehavior
{
    public WandererBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed * 0.6f); // �������ٶ�

        float wanderTime = Random.Range(30f, 90f);
        float startTime = Time.time;

        while (Time.time - startTime < wanderTime)
        {
            // ���ѡ���λ�й�
            ShoppingPoint randomPoint = manager.GetRandomShoppingPoint();
            if (randomPoint != null)
            {
                npc.MoveTo(randomPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // ��ʱ��ͣ���۲�
                npc.currentState = NPCState.Waiting;
                yield return new WaitForSeconds(Random.Range(8f, 20f));
            }
        }

        // �뿪
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}
