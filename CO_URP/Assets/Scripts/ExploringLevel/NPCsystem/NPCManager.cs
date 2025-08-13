using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 路人类型枚举
public enum NPCType
{
    QuickShopper,      // 快速购物者
    BrowserShopper,    // 闲逛购物者
    TargetShopper,     // 目标明确购物者
    Employee,          // 员工
    Wanderer          // 随机闲逛者
}

// 路人状态枚举
public enum NPCState
{
    Idle,
    Moving,
    Shopping,
    Waiting,
    Leaving
}

// 购物点位信息
[System.Serializable]
public class ShoppingPoint
{
    public Transform point;
    public string category;        // 商品类别（如"food", "drinks", "electronics"）
    public float priority;         // 优先级
    public float shopTime;         // 购物时间
}

// 路人管理器 - 主控制脚本
public class NPCManager : MonoBehaviour
{
    [Header("路人预制体")]
    public GameObject[] npcPrefabs;

    [Header("生成点位")]
    public Transform[] spawnPoints;
    public Transform[] exitPoints;

    [Header("购物点位")]
    public ShoppingPoint[] shoppingPoints;

    [Header("生成设置")]
    public int maxNPCCount = 20;
    public float spawnInterval = 3f;

    [Header("路人类型权重")]
    [Range(0f, 1f)] public float quickShopperWeight = 0.3f;
    [Range(0f, 1f)] public float browserShopperWeight = 0.25f;
    [Range(0f, 1f)] public float targetShopperWeight = 0.25f;
    [Range(0f, 1f)] public float employeeWeight = 0.1f;
    [Range(0f, 1f)] public float wandererWeight = 0.1f;

    private List<NPCController> activeNPCs = new List<NPCController>();
    private Coroutine spawnCoroutine;

    void Start()
    {
        // 验证购物点位的NavMesh
        ValidateShoppingPoints();

        // 开始生成路人
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

        // 随机选择生成点和预制体
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];

        // 根据权重选择路人类型
        NPCType npcType = GetRandomNPCType();

        // 生成路人
        GameObject npcObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        NPCController npcController = npcObj.GetComponent<NPCController>();

        if (npcController == null)
        {
            npcController = npcObj.AddComponent<NPCController>();
        }

        // 初始化路人
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

// 路人控制器 - 单个路人的行为控制
public class NPCController : MonoBehaviour
{
    [Header("路人信息")]
    public NPCType npcType;
    public NPCState currentState;

    [Header("移动设置")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float rotationSpeed = 120f;

    private NPCManager manager;
    private NavMeshAgent agent;
    private Animator animator;
    private NPCBehavior currentBehavior;

    // 行为模式字典
    private Dictionary<NPCType, NPCBehavior> behaviors;

    public void Initialize(NPCManager npcManager, NPCType type)
    {
        manager = npcManager;
        npcType = type;

        // 获取组件
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        animator = GetComponent<Animator>();

        // 配置NavMeshAgent
        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = 0.5f;

        // 初始化行为模式
        InitializeBehaviors();

        // 设置当前行为
        SetBehavior(npcType);

        // 开始行为
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

            // 动画控制
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
        // 检查是否到达目的地
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

// 路人行为基类
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

// 快速购物者行为
public class QuickShopperBehavior : NPCBehavior
{
    private int targetItemCount;
    private int currentItemCount = 0;

    public QuickShopperBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager)
    {
        targetItemCount = Random.Range(1, 4); // 购买1-3件物品
    }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.runSpeed * 0.8f); // 稍快的速度

        while (currentItemCount < targetItemCount)
        {
            // 选择购物点
            ShoppingPoint targetPoint = manager.GetRandomShoppingPoint();
            if (targetPoint != null)
            {
                // 前往购物点
                npc.MoveTo(targetPoint.point.position);

                // 等待到达
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // 快速购物
                npc.currentState = NPCState.Shopping;
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

                currentItemCount++;
            }
            else
            {
                break;
            }
        }

        // 前往出口
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}

// 闲逛购物者行为
public class BrowserShopperBehavior : NPCBehavior
{
    public BrowserShopperBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed * 0.7f); // 较慢的速度

        float totalTime = Random.Range(60f, 120f); // 总共停留60-120秒
        float startTime = Time.time;

        while (Time.time - startTime < totalTime)
        {
            // 随机选择购物点闲逛
            ShoppingPoint targetPoint = manager.GetRandomShoppingPoint();
            if (targetPoint != null)
            {
                npc.MoveTo(targetPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // 长时间浏览
                npc.currentState = NPCState.Shopping;
                yield return new WaitForSeconds(Random.Range(5f, 15f));

                // 随机决定是否购买
                if (Random.value < 0.3f) // 30%概率购买
                {
                    yield return new WaitForSeconds(Random.Range(2f, 5f));
                }
            }
        }

        // 离开
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}

// 目标明确购物者行为
public class TargetShopperBehavior : NPCBehavior
{
    private string[] targetCategories;
    private int currentCategoryIndex = 0;

    public TargetShopperBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager)
    {
        // 随机选择2-4个目标类别
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
            // 寻找特定类别的购物点
            ShoppingPoint[] categoryPoints = manager.GetShoppingPointsByCategory(category);

            if (categoryPoints.Length > 0)
            {
                ShoppingPoint targetPoint = categoryPoints[Random.Range(0, categoryPoints.Length)];

                // 直接前往目标区域
                npc.MoveTo(targetPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // 购物
                npc.currentState = NPCState.Shopping;
                yield return new WaitForSeconds(Random.Range(3f, 8f));
            }
        }

        // 前往出口
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}

// 员工行为
public class EmployeeBehavior : NPCBehavior
{
    private Transform[] workStations;

    public EmployeeBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed);

        while (true) // 员工不离开
        {
            // 在不同工作区域之间巡逻
            ShoppingPoint workPoint = manager.GetRandomShoppingPoint();
            if (workPoint != null)
            {
                npc.MoveTo(workPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // 工作（整理货架、清洁等）
                npc.currentState = NPCState.Shopping; // 重用状态表示工作
                yield return new WaitForSeconds(Random.Range(10f, 30f));

                // 偶尔停下来休息
                if (Random.value < 0.2f)
                {
                    npc.currentState = NPCState.Waiting;
                    yield return new WaitForSeconds(Random.Range(5f, 10f));
                }
            }
        }
    }
}

// 随机闲逛者行为
public class WandererBehavior : NPCBehavior
{
    public WandererBehavior(NPCController npcController, NPCManager npcManager)
        : base(npcController, npcManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        npc.SetSpeed(npc.walkSpeed * 0.6f); // 很慢的速度

        float wanderTime = Random.Range(30f, 90f);
        float startTime = Time.time;

        while (Time.time - startTime < wanderTime)
        {
            // 随机选择点位闲逛
            ShoppingPoint randomPoint = manager.GetRandomShoppingPoint();
            if (randomPoint != null)
            {
                npc.MoveTo(randomPoint.point.position);
                yield return new WaitUntil(() => npc.HasReachedDestination());

                // 长时间停留观察
                npc.currentState = NPCState.Waiting;
                yield return new WaitForSeconds(Random.Range(8f, 20f));
            }
        }

        // 离开
        Transform exit = manager.GetRandomExitPoint();
        if (exit != null)
        {
            npc.MoveTo(exit.position);
            yield return new WaitUntil(() => npc.HasReachedDestination());
        }

        npc.DestroyNPC();
    }
}