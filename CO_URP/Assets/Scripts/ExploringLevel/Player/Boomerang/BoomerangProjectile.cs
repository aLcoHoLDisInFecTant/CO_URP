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

    [Header("Visual Effects")]
    public float rotationSpeed = 720f; // degrees per second
    public TrailRenderer trailRenderer;

    [Header("Animation")]
    private Animator animator;

    void Awake()
    {
        // Add trail renderer if not present
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = 0.5f;
            trailRenderer.startWidth = 0.2f;
            trailRenderer.endWidth = 0.05f;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            //trailRenderer.color = Color.cyan;
        }

        // Get animator component
        //animator = GetComponent<Animator>();
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

        // Start rotation animation
        if (animator != null)
        {
            //animator.SetBool("rotate", true);
        }

        // Find player transform for real-time tracking during return
        Player_Explore player = FindObjectOfType<Player_Explore>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Rotate the boomerang for visual effect (in addition to animation)
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

        if (!isReturning && !hasReachedTarget)
        {
            // Move towards target
            MoveTowardsTarget();
        }
        else if (isReturning)
        {
            // Return to player's current position
            ReturnToPlayer();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance < 0.5f)
        {
            // Reached target, start returning
            hasReachedTarget = true;
            isReturning = true;
            return;
        }

        // Move towards target
        transform.position += direction * launchSpeed * Time.deltaTime;

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

    private void ReturnToPlayer()
    {
        if (playerTransform == null)
        {
            // Player not found, destroy boomerang
            ReturnComplete();
            return;
        }

        // Update target to player's current position
        currentTargetPosition = playerTransform.position;

        Vector3 direction = (currentTargetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, currentTargetPosition);

        if (distance < 0.8f)
        {
            // Reached player, complete return
            ReturnComplete();
            return;
        }

        // Move towards player
        transform.position += direction * returnSpeed * Time.deltaTime;

        ApplyRotation(direction);
    }

    private void ReturnComplete()
    {
        // Stop rotation animation
        if (animator != null)
        {
            //animator.SetBool("rotate", false);
        }

        onReturnCallback?.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        // Handle collision with SurroundingCollectable objects
        if (other.CompareTag("SurroundingCollectable"))
        {
            // Do something with the collectable (e.g., collect it, trigger effect)
            Debug.Log($"Boomerang passed through collectable: {other.name}");

            // Optional: Add collection logic here
            // other.GetComponent<Collectable>()?.Collect();

            // Don't return immediately - let boomerang pass through
            return;
        }

        // For other objects, trigger return only if not already returning and hit something solid
        if (!isReturning && !other.CompareTag("Player") && !other.isTrigger)
        {
            // Hit a solid object, start returning immediately
            hasReachedTarget = true;
            isReturning = true;
        }
    }
}