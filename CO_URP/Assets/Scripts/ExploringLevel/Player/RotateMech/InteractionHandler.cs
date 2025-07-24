// ✅ InteractionHandler.cs - 同时支持旋转和平移控制对象
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractionHandler : MonoBehaviour
{
    [Header("必要设置")]
    [Tooltip("拖拽 RotationalController 或 TranslationalController 对象")]
    public MonoBehaviour targetObject; // 可为旋转或平移控制器

    [Header("交互设置")]
    public float interactionDuration = 2f;
    public Material highlightMaterial;

    [Header("状态管理")]
    public bool isInteractable = true;

    [Header("调试")]
    public bool debugMode = true;

    private Material originalMaterial;
    private Renderer rend;
    private float interactionTimer = 0f;
    private bool isInteracting = false;
    private bool hasControl = false;

    private Player_Explore player;

    void Start()
    {
        InitializeComponents();
        ValidateSettings();
    }

    void InitializeComponents()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalMaterial = rend.material;
        }
    }

    void ValidateSettings()
    {
        if (targetObject == null)
        {
            Debug.LogError($"[{name}] 未设置 targetObject！", this);
            enabled = false;
        }

        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger && debugMode)
        {
            Debug.LogWarning($"[{name}] Collider 建议设置为 Trigger");
        }
    }

    void Update()
    {
        if (!isInteractable) return;

        if (hasControl && ShouldReturnControl())
        {
            ReturnControlToPlayer();
        }

        if (!isInteracting) return;

        interactionTimer += Time.deltaTime;
        if (interactionTimer >= interactionDuration)
        {
            TransferControlToObject();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isInteractable) return;

        if (other.CompareTag("Player") && !hasControl && targetObject != null)
        {
            if (debugMode) Debug.Log($"[{name}] 玩家开始交互");
            isInteracting = true;
            if (rend != null) rend.material = highlightMaterial;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!isInteractable) return;

        if (other.CompareTag("Player"))
        {
            if (debugMode) Debug.Log($"[{name}] 玩家结束交互");
            ResetInteraction();
        }
    }

    void TransferControlToObject()
    {
        if (!isInteractable || targetObject == null || ControlManager.Instance == null)
            return;

        if (!hasControl)
        {
            // 获取目标对象上的摄像机
            MonoBehaviour targetMono = targetObject as MonoBehaviour;
            Camera targetCamera = targetMono.GetComponentInChildren<Camera>(true); // 包含未激活的

            if (targetCamera == null)
            {
                Debug.LogWarning($"目标对象 {targetMono.name} 没有子摄像机", targetMono.gameObject);
            }
            else
            {
                Debug.Log($"找到目标摄像机: {targetCamera.name}", targetCamera.gameObject);
            }

            ControlManager.Instance.SwitchToControl(targetObject as IControllable, targetCamera);

            hasControl = true;
            isInteractable = false;
            ResetInteraction();
        }
    }


    void ReturnControlToPlayer()
    {
        if (hasControl)
        {
            if (ControlManager.Instance != null)
            {
                if (debugMode) Debug.Log($"[{name}] 控制权返回玩家");
                ControlManager.Instance.SwitchBackToPlayer();
            }
            hasControl = false;
        }
    }

    bool ShouldReturnControl()
    {
        if (targetObject is RotationalController rot)
        {
            return rot.ShouldReturnControl();
        }
        else if (targetObject is TranslationalController trans)
        {
            return trans.ShouldReturnControl();
        }
        return false;
    }

    void ResetInteraction()
    {
        isInteracting = false;
        interactionTimer = 0f;
        if (rend != null) rend.material = originalMaterial;
    }

}
