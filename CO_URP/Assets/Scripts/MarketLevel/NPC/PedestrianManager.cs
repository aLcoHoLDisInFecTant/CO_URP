using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// ��������ö��
public enum PedestrianType
{
    Walker,     // ������
    Stroller,   // ɢ����
    Pacer       // �첽��
}

// ����״̬ö��
public enum PedestrianState
{
    Walking,
    Waiting,
    SpecialAction
}

// ���˹����� - �����ƽű�
public class PedestrianManager : MonoBehaviour
{
    [Header("����Ԥ����")]
    public GameObject[] pedestrianPrefabs;

    [Header("���ɵ�λ")]
    public Transform[] spawnPoints;
    public Transform[] destinationPoints;

    [Header("��������")]
    public int maxPedestrianCount = 30;
    public float spawnInterval = 2f;

    [Header("��������Ȩ��")]
    [Range(0f, 1f)] public float walkerWeight = 0.5f;
    [Range(0f, 1f)] public float strollerWeight = 0.3f;
    [Range(0f, 1f)] public float pacerWeight = 0.2f;

    private List<PedestrianController> activePedestrians = new List<PedestrianController>();
    private Coroutine spawnCoroutine;

    void Start()
    {
        // ��ʼ��������
        spawnCoroutine = StartCoroutine(SpawnPedestrians());
    }

    IEnumerator SpawnPedestrians()
    {
        while (true)
        {
            if (activePedestrians.Count < maxPedestrianCount)
            {
                SpawnRandomPedestrian();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnRandomPedestrian()
    {
        if (spawnPoints.Length == 0 || pedestrianPrefabs.Length == 0) return;

        // ���ѡ�����ɵ��Ԥ����
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject prefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Length)];

        // ����Ȩ��ѡ����������
        PedestrianType pedestrianType = GetRandomPedestrianType();

        // ��������
        GameObject pedestrianObj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        PedestrianController pedestrianController = pedestrianObj.GetComponent<PedestrianController>();

        if (pedestrianController == null)
        {
            pedestrianController = pedestrianObj.AddComponent<PedestrianController>();
        }

        // ��ʼ������
        pedestrianController.Initialize(this, pedestrianType);
        activePedestrians.Add(pedestrianController);
    }

    PedestrianType GetRandomPedestrianType()
    {
        float[] weights = { walkerWeight, strollerWeight, pacerWeight };
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
                return (PedestrianType)i;
            }
        }

        return PedestrianType.Walker;
    }

    public void RemovePedestrian(PedestrianController pedestrian)
    {
        if (activePedestrians.Contains(pedestrian))
        {
            activePedestrians.Remove(pedestrian);
        }
    }

    public Transform GetRandomDestinationPoint()
    {
        if (destinationPoints.Length > 0)
            return destinationPoints[Random.Range(0, destinationPoints.Length)];
        return null;
    }
}

// ���˿����� - �������˵���Ϊ����
public class PedestrianController : MonoBehaviour
{
    [Header("������Ϣ")]
    public PedestrianType pedestrianType;
    public PedestrianState currentState;

    [Header("�ƶ�����")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 2.5f;
    public float rotationSpeed = 120f;

    private PedestrianManager manager;
    private NavMeshAgent agent;
    private Animator animator;
    private PedestrianBehavior currentBehavior;

    // ��Ϊģʽ�ֵ�
    private Dictionary<PedestrianType, PedestrianBehavior> behaviors;

    public void Initialize(PedestrianManager pedestrianManager, PedestrianType type)
    {
        manager = pedestrianManager;
        pedestrianType = type;

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
        SetBehavior(pedestrianType);

        // ��ʼ��Ϊ
        currentState = PedestrianState.Walking;
        if (currentBehavior != null)
        {
            StartCoroutine(currentBehavior.ExecuteBehavior());
        }
    }

    void InitializeBehaviors()
    {
        behaviors = new Dictionary<PedestrianType, PedestrianBehavior>
        {
            { PedestrianType.Walker, new WalkerBehavior(this, manager) },
            { PedestrianType.Stroller, new StrollerBehavior(this, manager) },
            { PedestrianType.Pacer, new PacerBehavior(this, manager) }
        };
    }

    void SetBehavior(PedestrianType type)
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
            currentState = PedestrianState.Walking;

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
            currentState = PedestrianState.Waiting;

            if (animator != null)
            {
                animator.SetBool("isWalking", false);
            }
        }
    }

    public bool HasReachedDestination()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    public void SetSpeed(float speed)
    {
        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    public void DestroyPedestrian()
    {
        manager.RemovePedestrian(this);
        Destroy(gameObject, 0.1f);
    }

    void Update()
    {
        // ����Ƿ񵽴�Ŀ�ĵ�
        if (currentState == PedestrianState.Walking && HasReachedDestination())
        {
            Stop();
        }
    }
}

// ������Ϊ����
public abstract class PedestrianBehavior
{
    protected PedestrianController pedestrian;
    protected PedestrianManager manager;

    public PedestrianBehavior(PedestrianController pedestrianController, PedestrianManager pedestrianManager)
    {
        pedestrian = pedestrianController;
        manager = pedestrianManager;
    }

    public abstract IEnumerator ExecuteBehavior();
}

// ��������Ϊ (Walker)
public class WalkerBehavior : PedestrianBehavior
{
    public WalkerBehavior(PedestrianController pedestrianController, PedestrianManager pedestrianManager)
        : base(pedestrianController, pedestrianManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        pedestrian.SetSpeed(pedestrian.walkSpeed);

        while (true)
        {
            // ѡ�����Ŀ�ĵ�
            Transform destination = manager.GetRandomDestinationPoint();
            if (destination != null)
            {
                pedestrian.MoveTo(destination.position);
                yield return new WaitUntil(() => pedestrian.HasReachedDestination());

                // ��Ŀ�ĵ�ͣ��
                pedestrian.currentState = PedestrianState.Waiting;
                yield return new WaitForSeconds(Random.Range(3f, 8f));
            }

            // ��������Ƿ�ִ�����⶯��
            if (Random.value < 0.1f) // 10%����
            {
                pedestrian.currentState = PedestrianState.SpecialAction;
                yield return new WaitForSeconds(Random.Range(2f, 5f));
            }
        }
    }
}

// ɢ������Ϊ (Stroller)
public class StrollerBehavior : PedestrianBehavior
{
    public StrollerBehavior(PedestrianController pedestrianController, PedestrianManager pedestrianManager)
        : base(pedestrianController, pedestrianManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        pedestrian.SetSpeed(pedestrian.walkSpeed * 0.7f); // �������ٶ�

        while (true)
        {
            // ѡ�����Ŀ�ĵ�
            Transform destination = manager.GetRandomDestinationPoint();
            if (destination != null)
            {
                pedestrian.MoveTo(destination.position);
                yield return new WaitUntil(() => pedestrian.HasReachedDestination());

                // ��Ŀ�ĵس�ʱ��ͣ��
                pedestrian.currentState = PedestrianState.Waiting;
                yield return new WaitForSeconds(Random.Range(5f, 12f));

                // ��������Ƿ�ִ�����⶯��
                if (Random.value < 0.2f) // 20%����
                {
                    pedestrian.currentState = PedestrianState.SpecialAction;
                    yield return new WaitForSeconds(Random.Range(3f, 6f));
                }
            }
        }
    }
}

// �첽����Ϊ (Pacer)
public class PacerBehavior : PedestrianBehavior
{
    public PacerBehavior(PedestrianController pedestrianController, PedestrianManager pedestrianManager)
        : base(pedestrianController, pedestrianManager) { }

    public override IEnumerator ExecuteBehavior()
    {
        pedestrian.SetSpeed(pedestrian.runSpeed); // ������ٶ�

        while (true)
        {
            // ѡ�����Ŀ�ĵ�
            Transform destination = manager.GetRandomDestinationPoint();
            if (destination != null)
            {
                pedestrian.MoveTo(destination.position);
                yield return new WaitUntil(() => pedestrian.HasReachedDestination());

                // ͣ��ʱ��ܶ�
                pedestrian.currentState = PedestrianState.Waiting;
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }

            // ������ִ�����⶯��
            if (Random.value < 0.05f) // 5%����
            {
                pedestrian.currentState = PedestrianState.SpecialAction;
                yield return new WaitForSeconds(Random.Range(1f, 2f));
            }
        }
    }
}
