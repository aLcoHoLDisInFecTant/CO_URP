using UnityEngine;
using System;

public class BoomerangLauncher : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public GameObject boomerangPrefab;
    public float launchSpeed = 20f;
    public float returnSpeed = 18f;
    public float maxForwardDistance = 10f; // Max distance for the forward throw
    public float circleRadius = 5f;       // Radius for the circle throw
    public float circleSpeed = 270f;      // Degrees per second for circle throw

    [Header("Visuals")]
    public LineRenderer trajectoryRenderer; // Optional preview renderer

    private GameObject currentBoomerang;
    private BoomerangProjectile currentProjectile;
    private Player_Explore player;

    void Awake()
    {
        player = GetComponent<Player_Explore>();

        // Gracefully handle missing trajectory renderer
        if (trajectoryRenderer == null)
        {
            trajectoryRenderer = GetComponent<LineRenderer>();
        }
        if (trajectoryRenderer != null)
        {
            trajectoryRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Launches the boomerang straight forward from the player.
    /// The boomerang will wait at the destination until RequestReturn() is called.
    /// </summary>
    /// <param name="forwardDirection">The direction to throw the boomerang.</param>
    public void LaunchForward(Vector3 forwardDirection)
    {
        EventManager.TriggerEvent("PlaySFX", "swingFast");
        if (currentBoomerang != null || boomerangPrefab == null) return;

        Vector3 launchPosition = player.transform.position + Vector3.up * 1.5f; // Launch from chest height
        Vector3 targetPosition = launchPosition + forwardDirection.normalized * maxForwardDistance;

        InstantiateAndInitializeBoomerang();

        // Call the projectile's forward initialization method
        currentProjectile.InitializeForward(launchPosition, targetPosition, launchSpeed, returnSpeed, OnBoomerangReturned);

        player.isBoomerangFlying = true;
    }

    /// <summary>
    /// Launches the boomerang to circle around the player.
    /// </summary>
    /// <param name="clockwise">True for a clockwise circle, false for counter-clockwise.</param>
    public void LaunchCircle(bool clockwise)
    {
        EventManager.TriggerEvent("PlaySFX", "swingFast");
        if (currentBoomerang != null || boomerangPrefab == null) return;

        Vector3 launchPosition = player.transform.position + Vector3.up * 1.5f; // Launch from chest height

        InstantiateAndInitializeBoomerang();

        // Call the projectile's circle initialization method
        currentProjectile.InitializeCircle(player.transform, player.transform.position, circleRadius, circleSpeed, clockwise, OnBoomerangReturned);



        player.isBoomerangFlying = true;
    }

    /// <summary>
    /// Commands the currently active boomerang to start its return sequence.
    /// Primarily used for the forward throw to return after the player releases the charge button.
    /// </summary>
    public void RequestReturn()
    {
        if (currentProjectile != null)
        {
            currentProjectile.StartReturn();
        }
    }

    /// <summary>
    /// Instantiates the boomerang prefab and gets its projectile component.
    /// </summary>
    private void InstantiateAndInitializeBoomerang()
    {
        Vector3 launchPosition = player.transform.position + Vector3.up * 1.5f;
        currentBoomerang = Instantiate(boomerangPrefab, launchPosition, Quaternion.identity);
        currentProjectile = currentBoomerang.GetComponent<BoomerangProjectile>();
        if (currentProjectile == null)
        {
            // Add component if it's missing from the prefab for safety
            currentProjectile = currentBoomerang.AddComponent<BoomerangProjectile>();
        }
    }

    /// <summary>
    /// Callback method passed to the boomerang projectile. Called upon successful return.
    /// </summary>
    private void OnBoomerangReturned()
    {
        // This is the only place where the player's state is reset after a throw.
        player.OnBoomerangReturned();

        if (currentBoomerang != null)
        {
            Destroy(currentBoomerang);
        }
        currentBoomerang = null;
        currentProjectile = null;
    }

    public bool IsBoomerangActive()
    {
        return currentBoomerang != null;
    }

    /// <summary>
    /// Immediately destroys the boomerang if it exists. Used for resetting the player state.
    /// </summary>
    public void ForceStopBoomerang()
    {
        if (currentBoomerang != null)
        {
            Destroy(currentBoomerang);
        }
        currentBoomerang = null;
        currentProjectile = null;

        // Hide visuals if they are active
        if (trajectoryRenderer != null)
        {
            trajectoryRenderer.positionCount = 0;
        }
    }
}