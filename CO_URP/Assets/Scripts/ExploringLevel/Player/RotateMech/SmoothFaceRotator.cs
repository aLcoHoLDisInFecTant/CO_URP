using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SmoothFaceRotator : MonoBehaviour
{
    [Header("������󣨿ɿգ�")]
    public Transform posX, negX, posY, negY, posZ, negZ;

    [Header("�Ƿ���ת������̬��Rotation��")]
    public bool rotateOrientation = true;

    [Header("��ת����")]
    public float rotateSpeed = 120f;       // ���ٶȣ���/��
    public float snapThreshold = 45f;      // �����˽ǶȽ����㣬����ص�
    public float snapDuration = 0.2f;      // ����/�ص���ʱ
    [Range(0, 0.99f)] public float angularMomentum = 0.7f; // ��ת����ϵ��

    [Header("��������")]
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

    [Header("��ת����")]
    public Transform rotationCenterTransform;

    private Vector3 currentAxis = Vector3.zero;
    private float currentAngle = 0f;
    private bool isRotating = false;
    private bool isSnapping = false;
    private Quaternion initialRotation;
    private Rigidbody _rb;
    private Vector3 _currentAngularVelocity;
    private Vector3 _targetAngularVelocity;

    // �Ӷ�����Ϣ����
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
            Debug.LogWarning($"[SmoothFaceRotator] δָ����ת���Ķ��󣬽�ʹ������λ�á�");
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

        // ƽ����תģʽ������
        Vector3 axisInput = GetDiscreteInputAxis();
        if (!isRotating && axisInput != Vector3.zero)
        {
            StartRotation(axisInput);
        }

        // ƽ����תģʽ����
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

        // ������תģʽ������
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

        // ������תģʽ����
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

            // ����λ�ã������Ӷ��󶼸���λ�ñ仯��
            Vector3 rotatedOffset = transform.rotation * offset;
            obj.position = transform.position + rotatedOffset;


            // ��̬����
            if (rotateOrientation && Mathf.Abs(Vector3.Dot(normal.normalized, currentAxis.normalized)) < 0.01f)
            {
                // ֻ������תƽ������������ת��̬���Ӷ���Ż������ת
                obj.rotation = transform.rotation * Quaternion.Inverse(initialRotation) * originalRotations[obj];
            }
            else
            {
                // �����������ԭʼ��̬
                obj.rotation = originalRotations[obj];
            }
        }
    }

    // IControllable �ӿ�ʵ��
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
