// ✅ TranslationalController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TranslationalController : MonoBehaviour
{
    [Header("平移控制设置")]
    [Tooltip("移动速度 (单位/秒)")]
    public float moveSpeed = 2f;

    [Tooltip("平移惯性系数(0-1)")]
    [Range(0f, 0.99f)] public float inertia = 0.6f;

    [Tooltip("平移中心参考点 (非必要，仅用于 Gizmo 可视化)")]
    public Transform moveCenterReference;

    [Header("可控制轴")]
    public bool allowXTranslation = true;
    public bool allowYTranslation = false;
    public bool allowZTranslation = true;

    [Header("控制键位")]
    public KeyCode positiveXKey = KeyCode.D;
    public KeyCode negativeXKey = KeyCode.A;
    public KeyCode positiveYKey = KeyCode.E;
    public KeyCode negativeYKey = KeyCode.Q;
    public KeyCode positiveZKey = KeyCode.W;
    public KeyCode negativeZKey = KeyCode.S;

    [Header("状态")]
    public bool isControllable = false;
    public bool delegateCompletionCheck = true;

    private Rigidbody _rb;
    private Vector3 _targetVelocity;
    private Vector3 _currentVelocity;

    public EncoderInputBridgeV2 encoderBridge;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.drag = 0;
        _rb.angularDrag = 0;

        if (moveCenterReference == null)
        {
            Debug.LogWarning("[TranslationalController] 未指定中心参考对象，仅用于调试可视化。");
        }

        Debug.Log($"[TranslationalController] {name} 初始化完成");
    }

    void Update()
    {
        if (!isControllable)
        {
            _targetVelocity = Vector3.zero;
            return;
        }

        float x;
        float y;
        float z;

        if (encoderBridge)
        {
            x = encoderBridge != null ? encoderBridge.GetHorizontal() : 0f;
            y = encoderBridge != null ? encoderBridge.GetRotationY() : 0f; // 使用 y 轴旋转输入作为 Y 方向平移（如需）
            z = encoderBridge != null ? encoderBridge.GetVertical() : 0f;

        }
        else
        {
            x = (Input.GetKey(positiveXKey) ? 1 : 0) + (Input.GetKey(negativeXKey) ? -1 : 0);
            y = (Input.GetKey(positiveYKey) ? 1 : 0) + (Input.GetKey(negativeYKey) ? -1 : 0);
            z = (Input.GetKey(positiveZKey) ? 1 : 0) + (Input.GetKey(negativeZKey) ? -1 : 0);
        }



        Vector3 moveInput = Vector3.zero;
        if (allowXTranslation) moveInput += transform.right * x;
        if (allowYTranslation) moveInput += transform.up * y;
        if (allowZTranslation) moveInput += transform.forward * z;

        _targetVelocity = moveInput.normalized * moveSpeed;
    }

    void FixedUpdate()
    {
        _currentVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, 1 - inertia);
        _rb.MovePosition(_rb.position + _currentVelocity * Time.fixedDeltaTime);
    }

    public void SetControllable(bool controllable)
    {
        isControllable = controllable;

        if (_rb != null)
        {
            _rb.isKinematic = !controllable;
            Debug.Log($"[TranslationalController] {name} 控制状态: {controllable} | Rigidbody.isKinematic: {_rb.isKinematic}");
        }
    }

    public bool ShouldReturnControl()
    {
        return !delegateCompletionCheck && false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 center = moveCenterReference != null ? moveCenterReference.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(center, 0.1f);

        Gizmos.color = allowXTranslation ? Color.red : new Color(1f, 0.5f, 0.5f);
        Gizmos.DrawLine(center, center + transform.right);
        Gizmos.color = allowYTranslation ? Color.green : new Color(0.5f, 1f, 0.5f);
        Gizmos.DrawLine(center, center + transform.up);
        Gizmos.color = allowZTranslation ? Color.blue : new Color(0.5f, 0.5f, 1f);
        Gizmos.DrawLine(center, center + transform.forward);
    }

    //public ICommandTranslator GetCommandTranslator() => this;

    public void TranslateCommand(ECommand command, PressedState state)
    {
        if (!isControllable || !state.IsPressed)
            return;

        Vector3 moveInput = Vector3.zero;

        switch (command)
        {
            case ECommand.UP:
                if (allowZTranslation)
                    moveInput += transform.forward;
                break;
            case ECommand.DOWN:
                if (allowZTranslation)
                    moveInput -= transform.forward;
                break;
            case ECommand.LEFT:
                if (allowXTranslation)
                    moveInput -= transform.right;
                break;
            case ECommand.RIGHT:
                if (allowXTranslation)
                    moveInput += transform.right;
                break;
            case ECommand.SLIDELEFT:
                if (allowYTranslation)
                    moveInput += transform.up;
                break;
            case ECommand.SLIDERIGHT:
                if (allowYTranslation)
                    moveInput -= transform.up;
                break;
            default:
                return;
        }

        _targetVelocity = moveInput.normalized * moveSpeed;
    }
#endif
}
