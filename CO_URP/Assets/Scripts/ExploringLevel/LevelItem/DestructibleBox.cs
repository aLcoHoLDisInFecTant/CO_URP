using UnityEngine;

public class DestructibleBox : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.7f;
    [SerializeField] private GameObject destructionFX; // 指向FX预制体

    private bool isTriggered = false;
    private float timer = 0f;
    private Collider playerCollider;

    private void FixedUpdate()
    {
        if (isTriggered)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= destroyDelay)
            {
                TriggerDestruction();
                isTriggered = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            timer = 0f;
            playerCollider = other;
        }
    }

    private void TriggerDestruction()
    {
        if (destructionFX != null)
        {
            GameObject fx = Instantiate(destructionFX, transform.position, Quaternion.identity);
            Destroy(fx, 2f); // 假设特效持续2秒
        }

        Destroy(gameObject); // 销毁箱子对象
    }
}
