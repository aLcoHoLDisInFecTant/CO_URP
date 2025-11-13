using UnityEngine;
using ChosTIS;

public class SortingAreaTrigger : MonoBehaviour
{
    [SerializeField] TetrisItemGrid targetGrid;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && targetGrid != null)
        {
            InventoryManager.Instance.EnterSortingArea(targetGrid);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InventoryManager.Instance.ExitSortingArea();
        }
    }
}

