using Michsky.MUIP;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RegularCollection : MonoBehaviour, ICollection
{
    [SerializeField] private int collectionScore = 10;
    [SerializeField] private int requiredTime = 3;

    [Header("UI")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject panel_1;
    [SerializeField] private GameObject panel_2;

    [System.Serializable]
    public class TriggerEvent
    {
        public string eventName;
        public string eventInput;
    }

    public List<TriggerEvent> triggerEvents = new List<TriggerEvent>();

    public string panelEventName;
    public string panelEventInput;

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
        EventManager.StartListening(panelEventName, ShowChatBalloon);
            
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
        panel_2.gameObject.SetActive(false);
        if (IsPlayer(other))
        {
            playerInTrigger = true;
            playerCollider = other;
            isCollecting = true;
            timer = 0f;

            if (canvas != null) { 
                canvas.gameObject.SetActive(true);
                panel_1.gameObject.SetActive(true);
                panel_2.gameObject.SetActive(false);
            }

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
        EventManager.StopListening(panelEventName, ShowChatBalloon);
    }

    public void ShowChatBalloon(object data)
    {
        if (data == null) return;

        string received = data.ToString().Trim();
        if (panelEventInput.Trim().Equals(received))
        {
            Debug.Log($"[RegularCollection] 事件匹配成功：{received}");
            canvas.gameObject.SetActive(true);
            panel_1.gameObject.SetActive(false);
            panel_2.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log($"[RegularCollection] 收到事件：{received}，但未匹配 {panelEventInput}");
        }
    }

    public void OnCollected(GameObject collector)
    {
        ScoreManager.Instance?.AddScoredPoints(collectionScore);
        foreach (var evt in triggerEvents)
        {
            if (!string.IsNullOrEmpty(evt.eventName))
            {
                EventManager.TriggerEvent(evt.eventName, evt.eventInput);
                Debug.Log($"[RegularCollection] 触发事件：{evt.eventName}（{evt.eventInput}）");
            }
        }
        // 可扩展：动画/音效/任务系统
    }

    private bool IsPlayer(Collider other)
    {
        return other.CompareTag("Player");
    }
}
