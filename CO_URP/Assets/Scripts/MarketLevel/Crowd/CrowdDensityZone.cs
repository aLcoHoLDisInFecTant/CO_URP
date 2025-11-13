using UnityEngine;

public class CrowdDensityZone : MonoBehaviour
{
    [SerializeField] string modifierKey = "crowd";
    [SerializeField] float k = 0.3f;
    [SerializeField] float sampleInterval = 0.2f;
    [SerializeField] LayerMask npcLayer;
    float nextSample;
    float area;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            Vector3 size = col.bounds.size;
            area = Mathf.Max(1f, size.x * size.z);
        }
        else
        {
            area = 10f;
        }
        if (npcLayer.value == 0) npcLayer = LayerMask.GetMask("NPC");
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < nextSample) return;
        nextSample = Time.time + sampleInterval;

        int count = 0;
        var colliders = Physics.OverlapBox(transform.position, GetHalfExtents(), Quaternion.identity, npcLayer);
        count = colliders.Length;
        float density = count / Mathf.Max(1f, area);
        float multiplier = 1f / (1f + k * density);
        var mod = other.GetComponent<PlayerSpeedModifier>();
        if (mod != null) mod.SetModifier(modifierKey, multiplier);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var mod = other.GetComponent<PlayerSpeedModifier>();
        if (mod != null) mod.RemoveModifier(modifierKey);
    }

    Vector3 GetHalfExtents()
    {
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            return box.size * 0.5f;
        }
        return Vector3.one;
    }
}

