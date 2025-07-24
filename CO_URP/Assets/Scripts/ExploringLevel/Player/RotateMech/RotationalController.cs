// ✅ RotationalController.cs（支持轴向自定义旋转）
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RotationalController : MonoBehaviour, IControllable, ICommandTranslator
{
    [Header("旋转控制设置")]
    [Tooltip("旋转速度(度/秒)")]
    public float rotationSpeed = 90f;

    [Tooltip("旋转惯性系数(0-1)")]
    [Range(0, 0.99f)] public float angularMomentum = 0.7f;

    [Tooltip("旋转中心对象（场景中的空物体）")]
    public Transform rotationCenterTransform;

    [Header("可控制轴")]
    public bool allowXRotation = true;
    public bool allowYRotation = true;
    public bool allowZRotation = true;

    [Header("控制键位")]
    public KeyCode positiveXAxisKey = KeyCode.D;
    public KeyCode negativeXAxisKey = KeyCode.A;
    public KeyCode positiveYAxisKey = KeyCode.W;
    public KeyCode negativeYAxisKey = KeyCode.S;
    public KeyCode positiveZAxisKey = KeyCode.RightArrow;
    public KeyCode negativeZAxisKey = KeyCode.LeftArrow;

    [Header("状态")]
    public bool isControllable = false;
    public bool delegateCompletionCheck = true;

    private Rigidbody _rb;
    private Vector3 _currentAngularVelocity;
    private Vector3 _targetAngularVelocity;

    public EncoderInputBridgeV2 encoderBridge;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.maxAngularVelocity = Mathf.Infinity;

        if (rotationCenterTransform == null)
        {
            Debug.LogWarning($"[RotationalController] 未指定旋转中心对象，将使用自身位置。");
        }

        Debug.Log($"[RotationalController] {name} 初始化完成 | 中心对象: {rotationCenterTransform?.name ?? "未设置"}");
    }

    void Update()
    {
        if (!isControllable)
        {
            _targetAngularVelocity = Vector3.zero;
            return;
        }

        float xInput;
        float yInput;
        float zInput;

        if (encoderBridge)
        {
            xInput = encoderBridge != null ? encoderBridge.GetHorizontal() : 0f;
            yInput = encoderBridge != null ? encoderBridge.GetVertical() : 0f;
            zInput = encoderBridge != null ? encoderBridge.GetRotationY() : 0f;

        }
        else {
            xInput = (Input.GetKey(positiveXAxisKey) ? 1 : 0) + (Input.GetKey(negativeXAxisKey) ? -1 : 0);
            yInput = (Input.GetKey(positiveYAxisKey) ? 1 : 0) + (Input.GetKey(negativeYAxisKey) ? -1 : 0);
            zInput = (Input.GetKey(positiveZAxisKey) ? 1 : 0) + (Input.GetKey(negativeZAxisKey) ? -1 : 0);
        }
        

        Vector3 result = Vector3.zero;

        if (allowXRotation)
        {
            Vector3 yAxis = transform.TransformDirection(Vector3.right); // x旋转影响ZY平面
            result += yAxis * xInput * rotationSpeed * Mathf.Deg2Rad;
        }
        if (allowYRotation)
        {
            Vector3 zAxis = transform.TransformDirection(Vector3.up); // y旋转影响XZ平面
            result += zAxis * yInput * rotationSpeed * Mathf.Deg2Rad;
        }
        if (allowZRotation)
        {
            Vector3 xAxis = transform.TransformDirection(Vector3.forward); // z旋转影响XY平面
            result += xAxis * zInput * rotationSpeed * Mathf.Deg2Rad;
        }

        _targetAngularVelocity = result;
    }

    void FixedUpdate()
    {
        _currentAngularVelocity = Vector3.Lerp(_currentAngularVelocity, _targetAngularVelocity, 1 - angularMomentum);

        Vector3 center = rotationCenterTransform != null ? rotationCenterTransform.position : transform.position;
        Vector3 positionDelta = RotatePointAroundPivot(transform.position, center, _currentAngularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg);

        _rb.MoveRotation(Quaternion.Euler(_currentAngularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg) * _rb.rotation);
        _rb.MovePosition(positionDelta);
    }

    public void SetControllable(bool controllable)
    {
        isControllable = controllable;

        if (_rb != null)
        {
            _rb.isKinematic = !controllable;
            Debug.Log($"[RotationalController] {name} 控制状态: {controllable} | Rigidbody.isKinematic: {_rb.isKinematic}");
        }
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        return pivot + dir;
    }

    public bool ShouldReturnControl()
    {
        return !delegateCompletionCheck && false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 center = rotationCenterTransform != null ? rotationCenterTransform.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.15f);

        float axisLength = 0.5f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + transform.right * axisLength);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + transform.up * axisLength);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(center, center + transform.forward * axisLength);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(center, _currentAngularVelocity.normalized * 1f);
        }
    }

#endif

    public ICommandTranslator GetCommandTranslator() => this;

    public void TranslateCommand(ECommand command, PressedState state)
    {
        if (!isControllable || !state.IsPressed)
            return;

        Vector3 result = Vector3.zero;

        switch (command)
        {
            case ECommand.SLIDELEFT: // Q
                if (allowYRotation)
                    result += transform.up * rotationSpeed * Mathf.Deg2Rad;
                break;
            case ECommand.SLIDERIGHT: // E
                if (allowYRotation)
                    result -= transform.up * rotationSpeed * Mathf.Deg2Rad;
                break;
            default:
                return;
        }

        _targetAngularVelocity = result;
    }
}
