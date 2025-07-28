using UnityEngine;
using System;

public class BoomerangProjectile : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 currentTargetPosition;
    private float launchSpeed;
    private float returnSpeed;
    private Action onReturnCallback;

    private bool isReturning = false;
    private bool hasReachedTarget = false;
    private Transform playerTransform;

    [Header("Visual")]
    public Transform visualModel;  // 子对象用于自转

    [Header("Rotation")]
    public float rotationSpeed = 720f; // degrees per second

    [Header("Trail")]
    public TrailRenderer trailRenderer;

    void Awake()
    {
        // 如果子对象 visualModel 未设置，尝试自动查找
        if (visualModel == null && transform.childCount > 0)
        {
            visualModel = transform.GetChild(0); // 默认第一个子对象为模型
        }

        // 添加 TrailRenderer（如未设定）
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.5f;
            trailRenderer.startWidth = 0.2f;
            trailRenderer.endWidth = 0.05f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    public void Initialize(Vector3 start, Vector3 target, float launchSpd, float returnSpd, Action returnCallback)
    {
        startPosition = start;
        targetPosition = target;
        currentTargetPosition = target;
        launchSpeed = launchSpd;
        returnSpeed = returnSpd;
        onReturnCallback = returnCallback;

        transform.position = start;
        isReturning = false;
        hasReachedTarget = false;

        // 寻找玩家
        Player_Explore player = FindObjectOfType<Player_Explore>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // 仅在飞行中执行模型自转
        if (visualModel != null)
        {
            visualModel.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }

        if (!isReturning && !hasReachedTarget)
        {
            MoveTowardsTarget();
        }
        else if (isReturning)
        {
            ReturnToPlayer();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance < 0.5f)
        {
            hasReachedTarget = true;
            isReturning = true;
            return;
        }

        transform.position += direction * launchSpeed * Time.deltaTime;
        ApplyRotation(direction);
    }

    private void ReturnToPlayer()
    {
        if (playerTransform == null)
        {
            ReturnComplete();
            return;
        }

        currentTargetPosition = playerTransform.position;

        Vector3 direction = (currentTargetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, currentTargetPosition);

        if (distance < 0.8f)
        {
            ReturnComplete();
            return;
        }

        transform.position += direction * returnSpeed * Time.deltaTime;
        ApplyRotation(direction);
    }

    private void ApplyRotation(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion facing = Quaternion.LookRotation(direction);
            transform.rotation = facing;
        }
    }

    private void ReturnComplete()
    {
        onReturnCallback?.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SurroundingCollectable"))
        {
            Debug.Log($"Boomerang passed through collectable: {other.name}");
            return;
        }

        if (!isReturning && !other.CompareTag("Player") && !other.isTrigger)
        {
            hasReachedTarget = true;
            isReturning = true;
        }
    }
}
