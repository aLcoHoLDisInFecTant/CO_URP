using UnityEngine;
using ChosTIS;

public class BasicPlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerSpeedModifier speedMod;
    Camera cam;
    bool useRigidbody;

    void Awake()
    {
        cam = Camera.main;
        if (rb == null) rb = GetComponent<Rigidbody>();
        useRigidbody = rb != null;
        if (speedMod == null) speedMod = GetComponent<PlayerSpeedModifier>();
        if (InventoryManager.Instance != null) InventoryManager.Instance.SetInventoryOpen(false);
    }

    void Update()
    {
        bool inventoryHeld = Input.GetKey(KeyCode.Space);
        if (InventoryManager.Instance != null) InventoryManager.Instance.SetInventoryOpen(inventoryHeld);
        if (inventoryHeld) return;

        Vector3 dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) dir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) dir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) dir += Vector3.right;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        float mult = speedMod != null ? speedMod.CurrentMultiplier : 1f;
        Vector3 delta = dir * (moveSpeed * mult) * Time.deltaTime;
        if (useRigidbody)
        {
            rb.MovePosition(rb.position + delta);
        }
        else
        {
            transform.position += delta;
        }
    }
}
