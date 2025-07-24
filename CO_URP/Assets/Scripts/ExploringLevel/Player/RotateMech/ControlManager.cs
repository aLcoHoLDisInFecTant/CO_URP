using UnityEngine;
using Cinemachine;
using System.Collections;

[DefaultExecutionOrder(-100)]
public class ControlManager : MonoBehaviour
{
    public static ControlManager Instance { get; private set; }

    [Header("玩家设置")]
    [SerializeField] private GameObject player;

    [Header("摄像机设置")]
    [Tooltip("Cinemachine 虚拟摄像机")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("摄像机模板")]
    public CameraFollowProfile playerCameraProfile;
    public CameraFollowProfile objectCameraProfile;

    private IControllable playerController;
    private IControllable currentControlledObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSystem();
    }

    void InitializeSystem()
    {
        InitializePlayerController();
        InitializeCameraSystem();
        Debug.Log("[ControlManager] 系统初始化完成");
    }

    void InitializePlayerController()
    {
        if (player == null)
        {
            Debug.LogError("玩家对象未分配！");
            return;
        }

        playerController = player.GetComponent<IControllable>();
        if (playerController == null)
        {
            Debug.LogError($"玩家对象 {player.name} 没有实现IControllable接口！");
        }
    }

    void InitializeCameraSystem()
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("未找到 Cinemachine 虚拟摄像机！");
                return;
            }
        }

        if (player != null)
        {
            virtualCamera.Follow = player.transform;
        }
    }

    public void SwitchToControl(IControllable target, Camera _ = null)
    {
        if (target == null)
        {
            Debug.LogWarning("无法切换到空目标");
            return;
        }

        SetCurrentController(target);
        HandleCameraSwitch(target);
        ApplyCameraProfile(objectCameraProfile);

        Debug.Log($"控制权已切换到: {GetObjectName(target)}");
    }

    public void SwitchBackToPlayer()
    {
        if (currentControlledObject != null)
        {
            currentControlledObject.SetControllable(false);
            GameSessionExplore.Instance.RemoveCommandTranslator(currentControlledObject.GetCommandTranslator());
        }

        if (playerController != null)
        {
            playerController.SetControllable(true);
            GameSessionExplore.Instance.AddCommandTranslator(playerController.GetCommandTranslator());
        }

        currentControlledObject = null;

        // 摄像机归位
        if (virtualCamera != null && player != null)
        {
            virtualCamera.Follow = player.transform;
            virtualCamera.LookAt = player.transform;
        }

        ApplyCameraProfile(playerCameraProfile);
        Debug.Log("[ControlManager] 控制权已归还玩家");
    }


    void SetCurrentController(IControllable target)
    {
        if (currentControlledObject != null)
        {
            currentControlledObject.SetControllable(false);
        }

        if (playerController != null)
        {
            playerController.SetControllable(false);
        }

        currentControlledObject = target;
        currentControlledObject.SetControllable(true);

        var newTranslator = currentControlledObject.GetCommandTranslator();
        GameSessionExplore.Instance.AddCommandTranslator(newTranslator);
    }

    void HandleCameraSwitch(IControllable target)
    {
        if (virtualCamera == null) return;

        MonoBehaviour targetMono = target as MonoBehaviour;
        if (targetMono != null)
        {
            virtualCamera.Follow = targetMono.transform;
            virtualCamera.LookAt = targetMono.transform;
        }
    }

    void ApplyCameraProfile(CameraFollowProfile profile)
    {
        if (virtualCamera == null || profile == null) return;

        var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_FollowOffset = profile.followOffset;
        }

        var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        if (composer != null)
        {
            composer.m_TrackedObjectOffset = profile.trackedObjectOffset;
            composer.m_ScreenX = profile.screenX;
            composer.m_ScreenY = profile.screenY;
        }
    }

    string GetObjectName(IControllable obj)
    {
        MonoBehaviour mono = obj as MonoBehaviour;
        return mono != null ? mono.name : "Unknown Object";
    }

    public IControllable GetCurrentController()
    {
        return currentControlledObject;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (player != null && player.GetComponent<IControllable>() == null)
        {
            // Debug.LogError($"玩家对象 {player.name} 没有实现IControllable接口！", player);
        }

        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
    }
#endif
}
