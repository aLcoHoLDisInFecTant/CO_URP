using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game.EnableObjectsWithDelay
{
public class EnableObjectsWithDelay : MonoBehaviour
{
    [System.Serializable]
    public class ObjectDelay
    {
        public GameObject targetObject; // The object to enable
        public float startDelay = 1f; // Delay in seconds
    }

    [SerializeField] private List<ObjectDelay> objectsWithDelays; // List of objects with delays

    private void Start()
    {
        if (objectsWithDelays == null || objectsWithDelays.Count == 0)
        {
            Debug.LogWarning("The list of objects with delays is empty.");
            return;
        }

        foreach (var objectDelay in objectsWithDelays)
        {
            if (objectDelay.targetObject != null)
            {
                objectDelay.targetObject.SetActive(false); // Disable the object immediately
                StartCoroutine(EnableObjectAfterDelay(objectDelay.targetObject, objectDelay.startDelay));
            }
            else
            {
                Debug.LogWarning("Target object is not assigned!");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            PlayStarEndAnimationForAll();
        }
    }

    private IEnumerator EnableObjectAfterDelay(GameObject targetObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        targetObject.SetActive(true); 
    }

    private void PlayStarEndAnimationForAll()
    {
        foreach (var objectDelay in objectsWithDelays)
        {
            Animator animator = objectDelay.targetObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Star_End"); 
            }
            else
            {
                Debug.LogWarning("Animator not found on " + objectDelay.targetObject.name);
            }
        }
    }
}
}
