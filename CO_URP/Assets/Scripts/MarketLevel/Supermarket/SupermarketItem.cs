using UnityEngine;
using ChosTIS;

public class SupermarketItem : MonoBehaviour
{
    [SerializeField] int itemID = 0;
    [SerializeField] float pickupThreshold = 1.5f;
    [SerializeField] bool oneShot = true;
    float stayTimer;
    bool picked;

    void OnTriggerStay(Collider other)
    {
        if (picked && oneShot) return;
        if (!other.CompareTag("Player")) return;
        if (InventoryManager.Instance == null) return;
        if (InventoryManager.Instance.isSortingArea) return;

        stayTimer += Time.deltaTime;
        if (stayTimer >= pickupThreshold)
        {
            InventoryManager.Instance.BeginOrganizeCurrentItemFromPickup(itemID);
            picked = true;
            stayTimer = 0f;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        stayTimer = 0f;
    }
}

