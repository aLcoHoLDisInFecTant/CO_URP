using UnityEngine;

public class EncoderInputBridgeV2 : MonoBehaviour
{
    [Header("外部输入源")]
    public EncoderManager encoder;

    [Header("映射阈值设置")]
    public float threshold;

    [Header("调试模式")]
    public bool debugLog = false;

    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public float RotationY { get; private set; }

    // 相对零点功能
    private Vector3 sessionZero = Vector3.zero;
    private Vector3 driftOffset = Vector3.zero;
    private bool isZeroSet = false;

    // 智能漂移
    private Vector3 lastRotation;
    private float stillTimer = 0f;
    public float stillThreshold = 1f;
    public float stillDuration = 5f;
    public float driftSpeed = 0.01f;

    void Update()
    {
        if (encoder == null) return;

        Vector3 currentRaw = new Vector3(encoder.x_rotation, encoder.y_rotation, encoder.z_rotation);
        Debug.Log("x_rotation :" + encoder.x_rotation + " y_rotation :" + encoder.y_rotation + "  z_rotation :" + encoder.z_rotation);
        // 计算相对值
        Vector3 relative = isZeroSet ? currentRaw - sessionZero + driftOffset : Vector3.zero;

        // 应用映射逻辑
        Vertical = relative.x < -threshold ? 1f : relative.x > threshold ? -1f : 0f;
        Horizontal = relative.z < -threshold ? -1f : relative.z > threshold ? 1f : 0f;
        RotationY = relative.y < -threshold ? -1f : relative.y > threshold ? 1f : 0f;

        if (debugLog)
        {
            Debug.Log($"[EncoderInputBridgeV2] Vertical: {Vertical}, Rotation: {RotationY}, Horizontal: {Horizontal}");
        }

        // 智能漂移补偿
        if (Vector3.Distance(currentRaw, lastRotation) < stillThreshold)
        {
            stillTimer += Time.deltaTime;
            if (stillTimer > stillDuration && isZeroSet)
            {
                Vector3 targetOffset = currentRaw - sessionZero;
                driftOffset = Vector3.Lerp(driftOffset, targetOffset, driftSpeed * Time.deltaTime);
            }
        }
        else
        {
            stillTimer = 0f;
        }
        lastRotation = currentRaw;

        if (debugLog)
        {
            Debug.Log($"[EncoderInputBridgeV2] ΔX: {relative.x:F2}, ΔY: {relative.y:F2}, ΔZ: {relative.z:F2}");
        }
    }

    public void SetSessionZeroPoint()
    {
        if (encoder == null) return;
        sessionZero = new Vector3(encoder.x_rotation, encoder.y_rotation, encoder.z_rotation);
        driftOffset = Vector3.zero;
        isZeroSet = true;
        Debug.Log("Session zero point set to: " + sessionZero);
    }

    // 对外提供相对姿态访问
    public Vector3 GetRelativeRotation()
    {
        return isZeroSet ? new Vector3(encoder.x_rotation, encoder.y_rotation, encoder.z_rotation) - sessionZero + driftOffset : Vector3.zero;
    }

    public float GetHorizontal() => Horizontal;
    public float GetVertical() => Vertical;
    public float GetRotationY() => RotationY;

    public float GetRelativeHorizontal() => GetRelativeRotation().z;
    public float GetRelativeVertical() => GetRelativeRotation().x;
    public float GetRelativeRotationY() => GetRelativeRotation().y;
}
