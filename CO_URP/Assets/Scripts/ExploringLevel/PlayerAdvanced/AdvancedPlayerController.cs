using UnityEngine;

public class AdvancedPlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Rigidbody rb;
    [SerializeField] GrappleLauncher grapple;
    [SerializeField] GameObject boomerangPrefab;
    [SerializeField] float boomerangLaunchSpeed = 12f;
    [SerializeField] float boomerangReturnSpeed = 18f;
    [SerializeField] float rayMaxDistance = 30f;
    Camera cam;
    bool useRigidbody;
    bool swinging;

    void Awake()
    {
        cam = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody>();
        useRigidbody = rb != null;
    }

    void Update()
    {
        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) dir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) dir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) dir += Vector3.right;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 delta = dir * moveSpeed * Time.deltaTime;
        if (useRigidbody)
        {
            rb.MovePosition(rb.position + delta);
        }
        else
        {
            transform.position += delta;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 aimPoint = GetAimPoint();
            if (grapple != null)
            {
                grapple.StartSwing(aimPoint);
                swinging = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            if (grapple != null && swinging)
            {
                grapple.StopSwing();
                swinging = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (boomerangPrefab != null)
            {
                Vector3 start = transform.position + transform.forward * 1.2f + Vector3.up * 1.2f;
                Vector3 target = GetAimPoint();
                GameObject obj = Instantiate(boomerangPrefab);
                var proj = obj.GetComponent<BoomerangProjectile>();
                if (proj != null)
                {
                    proj.InitializeForward(transform, start, target, boomerangLaunchSpeed, boomerangReturnSpeed, null);
                }
            }
        }
    }

    Vector3 GetAimPoint()
    {
        if (cam == null) cam = Camera.main;
        Vector3 aim = transform.position + transform.forward * 10f;
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
            if (Physics.Raycast(ray, out var hit, rayMaxDistance))
            {
                aim = hit.point;
            }
            else
            {
                aim = ray.origin + ray.direction * 10f;
            }
        }
        return aim;
    }
}

