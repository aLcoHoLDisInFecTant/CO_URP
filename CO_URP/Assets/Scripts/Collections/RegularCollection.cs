using Michsky.MUIP;
using UnityEngine;

public class RegularCollection : MonoBehaviour, ICollection
{
    [SerializeField] private int collectionScore = 10;
    [SerializeField] private int requiredTime = 3;

    [Header("UI")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Canvas canvas;

    private float timer = 0f;
    private bool isCollecting = false;
    private bool playerInTrigger = false;
    private Collider playerCollider;

    private void Start()
    {
        if (canvas != null)
            canvas.gameObject.SetActive(false);

        if (progressBar != null) {
            progressBar.currentPercent = 0f;
            progressBar.speed = (1 / requiredTime);  // 使其 1 秒填充 1.0
            progressBar.invert = false;
            progressBar.restart = false;
            progressBar.isOn = false;

            //progressBar.onValueChanged.AddListener(OnProgressUpdated);
        }
            
    }

    private void FixedUpdate()
    {
        if (playerInTrigger && isCollecting)
        {
            timer += Time.fixedDeltaTime;
            //Debug.Log("timer" + timer);

            float percent = Mathf.Clamp01(timer / requiredTime);
            //Debug.Log(percent);
            if (progressBar != null)
            {

                progressBar.currentPercent = Mathf.Clamp01(timer / requiredTime) * 100f;
                progressBar.isOn = true; // 开始驱动内部更新
            }

            if (timer >= requiredTime)
            {
                //Debug.Log("endTimer" + timer + "require" + requiredTime);
                OnCollected(playerCollider.gameObject);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
        {
            playerInTrigger = true;
            playerCollider = other;
            isCollecting = true;
            timer = 0f;

            if (canvas != null)
                canvas.gameObject.SetActive(true);

            if (progressBar != null)
            {
                progressBar.currentPercent = 0f;
                progressBar.isOn = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == playerCollider)
        {
            playerInTrigger = false;
            isCollecting = false;
            timer = 0f;

            if (progressBar != null)
            {
                progressBar.currentPercent = 0f;
                progressBar.isOn = false;
            }

            if (canvas != null)
                canvas.gameObject.SetActive(false);
        }
    }

    public void OnCollected(GameObject collector)
    {
        ScoreManager.Instance?.AddPickupScore(collectionScore);
        // 可扩展：动画/音效/任务系统
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player");
    }
}
