using UnityEngine;
using System;

public class BoomerangProjectile : MonoBehaviour
{
    // Public enum to define the boomerang's behavior
    public enum BoomerangMovementMode
    {
        None,
        Forward, // Straight throw, waits at the end
        Circle   // Circles a central point
    }

    // State variables
    private BoomerangMovementMode movementMode = BoomerangMovementMode.None;
    private bool isReturning = false;
    private Transform playerTransform;
    private Action onReturnCallback;

    // Movement parameters
    private float launchSpeed;
    private float returnSpeed;

    // Forward mode parameters
    private Vector3 targetPosition;
    private bool hasReachedTarget = false;

    // Circle mode parameters
    private Vector3 circleCenter;
    private float circleRadius;
    private float circleAngularSpeed;
    private bool clockwise;
    private float currentAngle = 0f;
    private float totalAngleTraveled = 0f;
    private const float FULL_CIRCLE_DEGREES = 360f;

    [Header("Visuals")]
    public Transform visualModel;
    public float rotationSpeed = 720f; // Degrees per second for visual spin

    void Awake()
    {
        // Auto-assign visual model if not set
        if (visualModel == null && transform.childCount > 0)
        {
            visualModel = transform.GetChild(0);
        }
    }

    // --- INITIALIZATION METHODS ---

    /// <summary>
    /// Initializes the boomerang for a forward throw.
    /// </summary>
    public void InitializeForward(Vector3 start, Vector3 target, float launchSpd, float returnSpd, Action returnCallback)
    {
        playerTransform = FindObjectOfType<Player_Explore>()?.transform;
        transform.position = start;

        movementMode = BoomerangMovementMode.Forward;
        targetPosition = target;
        launchSpeed = launchSpd;
        returnSpeed = returnSpd;
        onReturnCallback = returnCallback;

        isReturning = false;
        hasReachedTarget = false;
    }

    public void InitializeForward(Transform player, Vector3 start, Vector3 target, float launchSpd, float returnSpd, Action returnCallback)
    {
        playerTransform = player;
        transform.position = start;

        movementMode = BoomerangMovementMode.Forward;
        targetPosition = target;
        launchSpeed = launchSpd;
        returnSpeed = returnSpd;
        onReturnCallback = returnCallback;

        isReturning = false;
        hasReachedTarget = false;
    }

    /// <summary>
    /// Initializes the boomerang for a circular throw.
    /// </summary>
    public void InitializeCircle(Transform player, Vector3 center, float radius, float angularSpeed, bool isClockwise, Action returnCallback)
    {
        playerTransform = player;
        transform.position = player.position + Vector3.up * 1.5f;

        movementMode = BoomerangMovementMode.Circle;
        circleCenter = center;
        circleRadius = radius;
        circleAngularSpeed = angularSpeed;
        clockwise = isClockwise;
        onReturnCallback = returnCallback;

        Vector3 initialDirection = (transform.position - (center + new Vector3(0, 1.5f, 0))).normalized;
        currentAngle = Mathf.Atan2(initialDirection.z, initialDirection.x) * Mathf.Rad2Deg;

        isReturning = false;
        totalAngleTraveled = 0f;

        //Debug.Log($"��ʼ��ԲȦͶ�� - playerTransform: {(playerTransform != null ? "����" : "null")}, ��ʼ�Ƕ�: {currentAngle}, �뾶: {circleRadius}");
    }

    void Update()
    {
        // Constant visual rotation regardless of mode
        if (visualModel != null)
        {
            visualModel.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }

        //Debug.Log($"Update - isReturning: {isReturning}, movementMode: {movementMode}");

        // State machine for movement
        if (isReturning)
        {
            //Debug.Log("����ִ�з����߼�");
            ReturnToPlayer();
            return;
        }

        switch (movementMode)
        {
            case BoomerangMovementMode.Forward:
                UpdateForwardMovement();
                break;
            case BoomerangMovementMode.Circle:
                UpdateCircleMovement();
                break;
            case BoomerangMovementMode.None:
                //Debug.Log("movementMode Ϊ None��������Ӧ�þ�ֹ");
                break;
        }
    }

    // --- MOVEMENT LOGIC ---

    private void UpdateForwardMovement()
    {
        if (hasReachedTarget)
        {
            // Reached destination, now just hover and wait for the return command.
            // The visual rotation in Update() gives it a "hovering" effect.
            return;
        }

        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, launchSpeed * Time.deltaTime);

        // Check if the target has been reached
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            hasReachedTarget = true;
        }
    }

    private void UpdateCircleMovement()
    {
        // Calculate angle increment based on speed and direction
        float angleIncrement = circleAngularSpeed * Time.deltaTime;
        totalAngleTraveled += angleIncrement;

        if (!clockwise)
        {
            currentAngle += angleIncrement;
        }
        else
        {
            currentAngle -= angleIncrement;
        }

        // Calculate new position in the circle
        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), 0, Mathf.Sin(radians)) * circleRadius;
        // Keep boomerang at player's height + offset
        Vector3 newPosition = new Vector3(circleCenter.x, 0, circleCenter.z) + offset;
        newPosition.y = playerTransform.position.y + 1.5f; // Follow player's Y position

        transform.position = newPosition;

        // After a full circle, automatically start returning
        //Debug.Log($"���ƽ���: {totalAngleTraveled:F1}/{FULL_CIRCLE_DEGREES} ��");
        if (totalAngleTraveled >= FULL_CIRCLE_DEGREES)
        {
            //Debug.Log("������ɣ���ʼ����");
            StartReturn();
        }
    }

    private void ReturnToPlayer()
    {
        if (playerTransform == null)
        {
            ReturnComplete(); // Failsafe if player is destroyed
            return;
        }

        Vector3 returnTarget = playerTransform.position + Vector3.up * 1.5f; // Target player's chest
        Vector3 oldPosition = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, returnTarget, returnSpeed * Time.deltaTime);

        // ���ӵ�����Ϣ
        float distanceToPlayer = Vector3.Distance(transform.position, returnTarget);
        //Debug.Log($"�����ڷ����� - �������: {distanceToPlayer:F2}, ��ǰλ��: {transform.position}, Ŀ��λ��: {returnTarget}");

        // Check for arrival - �����ⷶΧ�����Ӷ��ؼ������
        if (distanceToPlayer < 1.0f || Vector3.Distance(oldPosition, transform.position) < 0.01f)
        {
            //Debug.Log("��ɷ��� - ��������: " + (distanceToPlayer < 1.0f ? "������" : "�ƶ�ֹͣ"));
            ReturnComplete();
        }
    }

    /// <summary>
    /// Public method to trigger the return flight.
    /// </summary>
    public void StartReturn()
    {
        //Debug.Log($"StartReturn ������ - isReturning: {isReturning}, playerTransform: {(playerTransform != null ? "����" : "null")}");

        if (!isReturning)
        {
            isReturning = true;
            movementMode = BoomerangMovementMode.None; // ��ֹ����ִ�л����߼�
            //Debug.Log("�����ڿ�ʼ������� - ״̬������");

            // ��������һ�η����߼�
            if (playerTransform != null)
            {
                Vector3 returnTarget = playerTransform.position + Vector3.up * 1.5f;
                //Debug.Log($"����Ŀ��λ��: {returnTarget}, ��ǰλ��: {transform.position}, ����: {Vector3.Distance(transform.position, returnTarget)}");
            }
            else
            {
                //Debug.LogError("playerTransform Ϊ null���޷����أ�");
            }
        }
        else
        {
            //Debug.Log("StartReturn �����ã����Ѿ��ڷ���״̬��");
        }
    }

    private void ReturnComplete()
    {
        onReturnCallback?.Invoke();
        Destroy(gameObject); // Destroy self after callback
    }

    /// <summary>
    /// Handles collision with the environment.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // Ignore player and triggers
        if (other.CompareTag("Player") || other.isTrigger)
        {
            return;
        }

        // If boomerang hits an obstacle during forward or circle phase, it starts returning
        if (!isReturning)
        {
            //Debug.Log($"Boomerang hit obstacle: {other.name}, returning.");
            StartReturn();
        }
    }
}
