using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SmoothFaceRotator : MonoBehaviour
{
    [Header("六面对象（可空）")]
    public Transform posX, negX, posY, negY, posZ, negZ;

    [Header("是否旋转对象姿态（Rotation）")]
    public bool rotateOrientation = true;

    [Header("旋转参数")]
    public float rotateSpeed = 120f;       // 角速度：度/秒
    public float snapThreshold = 45f;      // 超过此角度将补足，否则回弹
    public float snapDuration = 0.2f;      // 补足/回弹耗时
    [Range(0, 0.99f)] public float angularMomentum = 0.7f; // 旋转惯性系数

    [Header("控制设置")]
    public bool isControllable = true;
    public bool delegateCompletionCheck = true;
    public bool allowXRotation = true;
    public bool allowYRotation = true;
    public bool allowZRotation = true;
    public KeyCode positiveXAxisKey = KeyCode.W;
    public KeyCode negativeXAxisKey = KeyCode.S;
    public KeyCode positiveYAxisKey = KeyCode.A;
    public KeyCode negativeYAxisKey = KeyCode.D;
    public KeyCode positiveZAxisKey = KeyCode.Q;
    public KeyCode negativeZAxisKey = KeyCode.E;

    [Header("旋转中心")]
    public Transform rotationCenterTransform;

    private Vector3 currentAxis = Vector3.zero;
    private float currentAngle = 0f;
    private bool isRotating = false;
    private bool isSnapping = false;
    private Quaternion initialRotation;
    private Rigidbody _rb;
    private Vector3 _currentAngularVelocity;
    private Vector3 _targetAngularVelocity;

    // 子对象信息缓存
    private Dictionary<Transform, Vector3> originalOffsets = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> originalRotations = new Dictionary<Transform, Quaternion>();
    private Dictionary<Transform, Vector3> originalDirections = new Dictionary<Transform, Vector3>();

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.maxAngularVelocity = Mathf.Infinity;
        initialRotation = transform.rotation;

        if (rotationCenterTransform == null)
        {
            Debug.LogWarning($"[SmoothFaceRotator] 未指定旋转中心对象，将使用自身位置。");
            rotationCenterTransform = transform;
        }

        (Transform, Vector3)[] faceBindings = {
            (posX, Vector3.right),
            (negX, Vector3.left),
            (posY, Vector3.up),
            (negY, Vector3.down),
            (posZ, Vector3.forward),
            (negZ, Vector3.back),
        };

        foreach (var (obj, dir) in faceBindings)
        {
            if (obj == null) continue;

            originalOffsets[obj] = obj.position - transform.position;
            originalRotations[obj] = obj.rotation;
            originalDirections[obj] = dir;
        }
    }

    void Update()
    {
        if (!isControllable)
        {
            _targetAngularVelocity = Vector3.zero;
            return;
        }

        if (isSnapping) return;

        // 平滑旋转模式输入检测
        Vector3 axisInput = GetDiscreteInputAxis();
        if (!isRotating && axisInput != Vector3.zero)
        {
            StartRotation(axisInput);
        }

        // 平滑旋转模式更新
        if (isRotating)
        {
            float step = rotateSpeed * Time.deltaTime;
            transform.Rotate(currentAxis, step, Space.World);
            currentAngle += step;

            if (GetDiscreteInputAxis() == Vector3.zero)
            {
                isRotating = false;
                StartCoroutine(SnapBackOrForward());
            }
        }

        // 物理旋转模式输入检测
        if (!isRotating && !isSnapping)
        {
            float xInput = (Input.GetKey(positiveXAxisKey) ? 1 : 0) + (Input.GetKey(negativeXAxisKey) ? -1 : 0);
            float yInput = (Input.GetKey(positiveYAxisKey) ? 1 : 0) + (Input.GetKey(negativeYAxisKey) ? -1 : 0);
            float zInput = (Input.GetKey(positiveZAxisKey) ? 1 : 0) + (Input.GetKey(negativeZAxisKey) ? -1 : 0);

            Vector3 result = Vector3.zero;

            if (allowXRotation)
            {
                Vector3 yAxis = rotationCenterTransform.TransformDirection(Vector3.right);
                result += yAxis * xInput * rotateSpeed * Mathf.Deg2Rad;
            }
            if (allowYRotation)
            {
                Vector3 zAxis = rotationCenterTransform.TransformDirection(Vector3.up);
                result += zAxis * yInput * rotateSpeed * Mathf.Deg2Rad;
            }
            if (allowZRotation)
            {
                Vector3 xAxis = rotationCenterTransform.TransformDirection(Vector3.forward);
                result += xAxis * zInput * rotateSpeed * Mathf.Deg2Rad;
            }

            _targetAngularVelocity = result;
        }

        UpdateChildFollowRotation();
    }

    void FixedUpdate()
    {
        if (!isControllable || isRotating || isSnapping) return;

        // 物理旋转模式更新
        _currentAngularVelocity = Vector3.Lerp(_currentAngularVelocity, _targetAngularVelocity, 1 - angularMomentum);

        Vector3 center = rotationCenterTransform != null ? rotationCenterTransform.position : transform.position;
        Vector3 positionDelta = RotatePointAroundPivot(transform.position, center, _currentAngularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg);

        _rb.MoveRotation(Quaternion.Euler(_currentAngularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg) * _rb.rotation);
        _rb.MovePosition(positionDelta);
    }

    Vector3 GetDiscreteInputAxis()
    {
        if (Input.GetKey(KeyCode.W)) return Vector3.right;
        if (Input.GetKey(KeyCode.S)) return -Vector3.right;
        if (Input.GetKey(KeyCode.A)) return Vector3.up;
        if (Input.GetKey(KeyCode.D)) return -Vector3.up;
        if (Input.GetKey(KeyCode.Q)) return Vector3.forward;
        if (Input.GetKey(KeyCode.E)) return -Vector3.forward;
        return Vector3.zero;
    }

    void StartRotation(Vector3 axis)
    {
        currentAxis = axis;
        currentAngle = 0f;
        isRotating = true;
        initialRotation = transform.rotation;
    }

    IEnumerator SnapBackOrForward()
    {
        isSnapping = true;

        float toAngle = (Mathf.Abs(currentAngle) >= snapThreshold)
            ? 90f * Mathf.Sign(currentAngle)
            : 0f;

        float delta = toAngle - currentAngle;
        float elapsed = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion deltaRot = Quaternion.AngleAxis(delta, currentAxis);
        Quaternion endRot = deltaRot * transform.rotation;

        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapDuration;
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            UpdateChildFollowRotation();
            yield return null;
        }

        transform.rotation = endRot;
        isSnapping = false;
    }

    void UpdateChildFollowRotation()
    {
        foreach (var obj in originalOffsets.Keys)
        {
            if (obj.GetComponent<Camera>() != null) continue;
            Vector3 offset = originalOffsets[obj];
            Vector3 normal = originalDirections[obj];

            // 更新位置（所有子对象都跟随位置变化）
            Vector3 rotatedOffset = transform.rotation * offset;
            obj.position = transform.position + rotatedOffset;


            // 姿态控制
            if (rotateOrientation && Mathf.Abs(Vector3.Dot(normal.normalized, currentAxis.normalized)) < 0.01f)
            {
                // 只有在旋转平面上且允许旋转姿态的子对象才会更新旋转
                obj.rotation = transform.rotation * Quaternion.Inverse(initialRotation) * originalRotations[obj];
            }
            else
            {
                // 其他情况保持原始姿态
                obj.rotation = originalRotations[obj];
            }
        }
    }

    // IControllable 接口实现
    public void SetControllable(bool controllable)
    {
        isControllable = controllable;
        if (_rb != null)
        {
            _rb.isKinematic = !controllable;
        }
    }

    public bool ShouldReturnControl()
    {
        return !delegateCompletionCheck && false;
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        return pivot + dir;
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

    //public ICommandTranslator GetCommandTranslator() => this;

    public void TranslateCommand(ECommand command, PressedState state)
    {
        if (!isControllable || isRotating || isSnapping || !state.IsPressed)
            return;

        Vector3 axis = Vector3.zero;

        switch (command)
        {
            case ECommand.SLIDELEFT: // Q
                if (allowYRotation)
                    axis = rotationCenterTransform.TransformDirection(Vector3.up);
                break;
            case ECommand.SLIDERIGHT: // E
                if (allowYRotation)
                    axis = -rotationCenterTransform.TransformDirection(Vector3.up);
                break;
            default:
                return;
        }

        if (axis != Vector3.zero)
        {
            StartRotation(axis);
        }
    }
#endif
}
