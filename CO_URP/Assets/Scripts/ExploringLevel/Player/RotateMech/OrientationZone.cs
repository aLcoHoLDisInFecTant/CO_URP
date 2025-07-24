using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class OrientationZone : MonoBehaviour
{
    [Header("检测目标设置")]
    [Tooltip("主目标对象（可以是父对象或子对象）")]
    public Transform mainTarget;

    [Tooltip("实际检测的子对象（可选）")]
    public Transform childTarget;

    [Tooltip("是否自动检测SmoothFaceRotator子对象")]
    public bool autoDetectChildFaces = false;

    [Header("检测条件设置")]
    public float requiredStayDuration = 1f;
    public bool requireCorrectOrientation = true;
    public bool requireCorrectPosition = false;
    [Range(1, 30)] public float angleThreshold = 10f;
    public float positionThreshold = 0.2f;

    [Header("视觉反馈")]
    public Material defaultMaterial;
    public Material highlightMaterial;
    public Material successMaterial;
    public Renderer zoneRenderer;

    [Header("事件")]
    public UnityEvent onPuzzleSolved;

    private float _stayTimer = 0f;
    private bool _isInZone = false;
    private bool _conditionMet = false;
    private Coroutine _checkCoroutine;
    private Collider _zoneCollider;
    private Transform _actualTarget;
    private SmoothFaceRotator _cachedRotator;

    void Start()
    {
        _zoneCollider = GetComponent<Collider>();
        _zoneCollider.isTrigger = true;

        if (zoneRenderer == null)
        {
            zoneRenderer = GetComponent<Renderer>();
        }

        if (zoneRenderer && defaultMaterial)
        {
            zoneRenderer.material = defaultMaterial;
        }

        SetupTarget();
        _checkCoroutine = StartCoroutine(CheckConditionsRoutine());
    }

    void SetupTarget()
    {
        if (childTarget != null)
        {
            // 如果明确指定了子对象，优先使用
            _actualTarget = childTarget;
        }
        else if (mainTarget != null)
        {
            // 检查主目标是否有SmoothFaceRotator组件
            _cachedRotator = mainTarget.GetComponent<SmoothFaceRotator>();
            if (_cachedRotator != null && autoDetectChildFaces)
            {
                // 自动选择第一个非空的子对象作为实际目标
                if (_cachedRotator.posX != null) _actualTarget = _cachedRotator.posX;
                else if (_cachedRotator.negX != null) _actualTarget = _cachedRotator.negX;
                else if (_cachedRotator.posY != null) _actualTarget = _cachedRotator.posY;
                else if (_cachedRotator.negY != null) _actualTarget = _cachedRotator.negY;
                else if (_cachedRotator.posZ != null) _actualTarget = _cachedRotator.posZ;
                else if (_cachedRotator.negZ != null) _actualTarget = _cachedRotator.negZ;
                else _actualTarget = mainTarget;
            }
            else
            {
                _actualTarget = mainTarget;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (mainTarget == null && other.attachedRigidbody != null)
        {
            mainTarget = other.attachedRigidbody.transform;
            SetupTarget();
        }

        if (_actualTarget != null && (other.transform == _actualTarget || other.transform == mainTarget))
        {
            _isInZone = true;
            _stayTimer = 0f;
            if (highlightMaterial && zoneRenderer)
            {
                zoneRenderer.material = highlightMaterial;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_actualTarget != null && (other.transform == _actualTarget || other.transform == mainTarget))
        {
            _isInZone = false;
            _stayTimer = 0f;
            _conditionMet = false;
            if (zoneRenderer && defaultMaterial)
            {
                zoneRenderer.material = defaultMaterial;
            }
        }
    }

    private IEnumerator CheckConditionsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (!_isInZone || _actualTarget == null) continue;

            bool match = CheckConditions();
            if (match)
            {
                _stayTimer += 0.2f;

                if (!_conditionMet && successMaterial && zoneRenderer)
                {
                    zoneRenderer.material = successMaterial;
                    _conditionMet = true;
                }

                if (_stayTimer >= requiredStayDuration)
                {
                    PuzzleSolved();
                    yield break;
                }
            }
            else
            {
                _stayTimer = 0f;
                _conditionMet = false;
                if (highlightMaterial && zoneRenderer)
                {
                    zoneRenderer.material = highlightMaterial;
                }
            }
        }
    }

    private bool CheckConditions()
    {
        if (_actualTarget == null) return false;

        bool angleOk = true, positionOk = true;

        if (requireCorrectOrientation)
        {
            float diff = Quaternion.Angle(transform.rotation, _actualTarget.rotation);
            angleOk = diff <= angleThreshold;
        }

        if (requireCorrectPosition)
        {
            float dist = Vector3.Distance(transform.position, _actualTarget.position);
            positionOk = dist <= positionThreshold;
        }

        return angleOk && positionOk;
    }

    private void PuzzleSolved()
    {
        onPuzzleSolved?.Invoke();

        if (ControlManager.Instance != null)
        {
            // 获取主目标的摄像机（如果有）
            Camera targetCamera = mainTarget?.GetComponentInChildren<Camera>();
            ControlManager.Instance.SwitchBackToPlayer();
        }

        if (_zoneCollider) _zoneCollider.enabled = false;
        enabled = false;

    }

    // 编辑器方法，用于在Inspector中更新目标
    public void UpdateTargetReferences()
    {
        SetupTarget();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 zoneCenter = transform.position;

        if (_actualTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(zoneCenter, _actualTarget.position);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(_actualTarget.position, _actualTarget.forward);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(_actualTarget.position, _actualTarget.up);
        }

        if (mainTarget != null && _actualTarget != mainTarget)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(zoneCenter, mainTarget.position);
        }

        if (_zoneCollider == null) _zoneCollider = GetComponent<Collider>();
        if (_zoneCollider != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            if (_zoneCollider is BoxCollider box)
            {
                Gizmos.DrawWireCube(transform.position + box.center, box.size);
            }
            else if (_zoneCollider is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
#endif
}
