using UnityEngine;

public class DestructibleBox : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.7f;
    [SerializeField] private GameObject destructionFX; // ָ��FXԤ����

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
            Destroy(fx, 2f); // ������Ч����2��
        }

        Destroy(gameObject); // �������Ӷ���
    }
}
