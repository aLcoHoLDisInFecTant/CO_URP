using UnityEngine;

public class BoomerangLauncher : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public GameObject boomerangPrefab;
    public float launchSpeed = 20f;
    public float returnSpeed = 15f;
    public float maxDistance = 30f;

    [Header("Preview")]
    public LineRenderer trajectoryRenderer;
    public int trajectoryPoints = 50;

    private GameObject currentBoomerang;
    private Player_Explore player;
    private bool isBoomerangActive = false;

    void Awake()
    {
        player = GetComponent<Player_Explore>();

        // Setup trajectory line renderer
        if (trajectoryRenderer == null)
        {
            trajectoryRenderer = gameObject.AddComponent<LineRenderer>();
        }

        trajectoryRenderer.material = new Material(Shader.Find("Sprites/Default"));
        //trajectoryRenderer.color = Color.yellow;
        trajectoryRenderer.startWidth = 0.1f;
        trajectoryRenderer.endWidth = 0.1f;
        trajectoryRenderer.positionCount = 0;
    }

    public void ShowTrajectoryPreview(Vector3 targetDirection)
    {
        if (isBoomerangActive) return;

        // Hide line renderer trajectory - we'll use preview controller instead
        trajectoryRenderer.positionCount = 0;
    }

    public void HideTrajectoryPreview()
    {
        trajectoryRenderer.positionCount = 0;
    }

    public void LaunchBoomerang(Vector3 targetPosition)
    {
        if (isBoomerangActive || boomerangPrefab == null) return;

        HideTrajectoryPreview();

        // Instantiate boomerang
        currentBoomerang = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);

        // Get or add BoomerangProjectile component
        BoomerangProjectile projectile = currentBoomerang.GetComponent<BoomerangProjectile>();
        if (projectile == null)
        {
            projectile = currentBoomerang.AddComponent<BoomerangProjectile>();
        }

        // Configure and launch
        projectile.Initialize(transform.position, targetPosition, launchSpeed, returnSpeed, OnBoomerangReturned);

        isBoomerangActive = true;
        player.isBoomerangFlying = true;
    }

    private void OnBoomerangReturned()
    {
        isBoomerangActive = false;
        player.isBoomerangFlying = false;
        player.OnBoomerangReturned();

        if (currentBoomerang != null)
        {
            Destroy(currentBoomerang);
            currentBoomerang = null;
        }
    }

    public bool IsBoomerangActive => isBoomerangActive;

    public void ForceStopBoomerang()
    {
        if (currentBoomerang != null)
        {
            Destroy(currentBoomerang);
            currentBoomerang = null;
        }

        isBoomerangActive = false;
        player.isBoomerangFlying = false;
        HideTrajectoryPreview();
    }
}