using System.Collections;
using UnityEngine;

public class MagicCubeController : MonoBehaviour
{
    [System.Serializable]
    public class FaceSettings
    {
        public GameObject faceObject;
        public Vector3 originalRotation;
        public bool keepUpright = true;
    }

    [Header("Face Settings")]
    public FaceSettings[] faces = new FaceSettings[6];

    [Header("Rotation Settings")]
    public float rotationSpeed = 180f; // 度/秒
    public float snapThreshold = 45f; // 吸附阈值(度)
    public float returnSpeed = 360f; // 回弹速度(度/秒)

    private bool isRotating = false;
    private Axis currentAxis;
    private bool currentClockwise;
    private float currentRotationAmount = 0f;
    private Quaternion startRotation;
    private Coroutine rotationCoroutine;

    void Update()
    {
        if (isRotating) return;

        // 检测新旋转输入
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartRotation(Axis.X, !Input.GetKey(KeyCode.LeftShift));
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            StartRotation(Axis.Y, !Input.GetKey(KeyCode.LeftShift));
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            StartRotation(Axis.Z, !Input.GetKey(KeyCode.LeftShift));
        }

        // 检测旋转释放
        if (rotationCoroutine != null)
        {
            if ((currentAxis == Axis.X && !Input.GetKey(KeyCode.X)) ||
                (currentAxis == Axis.Y && !Input.GetKey(KeyCode.Y)) ||
                (currentAxis == Axis.Z && !Input.GetKey(KeyCode.Z)))
            {
                StopCoroutine(rotationCoroutine);
                DecideSnapOrReturn();
            }
        }
    }

    public void StartRotation(Axis axis, bool clockwise)
    {
        if (isRotating) return;

        currentAxis = axis;
        currentClockwise = clockwise;
        currentRotationAmount = 0f;
        startRotation = transform.rotation;

        rotationCoroutine = StartCoroutine(PerformRotation());
    }

    IEnumerator PerformRotation()
    {
        isRotating = true;
        Vector3 rotationAxis = GetAxisVector(currentAxis);
        float targetAngle = 90f * (currentClockwise ? 1 : -1);

        while (true)
        {
            // 计算本帧旋转量
            float step = rotationSpeed * Time.deltaTime;
            currentRotationAmount += step;

            // 限制旋转量不超过目标角度
            if (Mathf.Abs(currentRotationAmount) >= Mathf.Abs(targetAngle))
            {
                currentRotationAmount = targetAngle;
                transform.rotation = startRotation * Quaternion.Euler(rotationAxis * currentRotationAmount);
                UpdateFacePositionsAndRotations();
                break;
            }

            // 应用旋转
            transform.rotation = startRotation * Quaternion.Euler(rotationAxis * currentRotationAmount);
            UpdateFacePositionsAndRotations();

            yield return null;
        }

        // 旋转完成后重新排列面
        ReorderFaces(currentAxis, currentClockwise);
        isRotating = false;
        rotationCoroutine = null;
    }

    void DecideSnapOrReturn()
    {
        // 决定是吸附到90度还是返回0度
        if (Mathf.Abs(currentRotationAmount) > snapThreshold)
        {
            StartCoroutine(CompleteRotation());
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
    }

    IEnumerator CompleteRotation()
    {
        Vector3 rotationAxis = GetAxisVector(currentAxis);
        float targetAngle = 90f * (currentClockwise ? 1 : -1);
        float remainingAngle = targetAngle - currentRotationAmount;

        while (Mathf.Abs(remainingAngle) > 0.1f)
        {
            float step = rotationSpeed * Time.deltaTime * Mathf.Sign(remainingAngle);
            if (Mathf.Abs(step) > Mathf.Abs(remainingAngle))
            {
                step = remainingAngle;
            }

            currentRotationAmount += step;
            remainingAngle = targetAngle - currentRotationAmount;

            transform.rotation = startRotation * Quaternion.Euler(rotationAxis * currentRotationAmount);
            UpdateFacePositionsAndRotations();

            yield return null;
        }

        // 确保最终旋转准确
        currentRotationAmount = targetAngle;
        transform.rotation = startRotation * Quaternion.Euler(rotationAxis * currentRotationAmount);
        UpdateFacePositionsAndRotations();

        // 重新排列面
        ReorderFaces(currentAxis, currentClockwise);
        isRotating = false;
        rotationCoroutine = null;
    }

    IEnumerator ReturnToStart()
    {
        Vector3 rotationAxis = GetAxisVector(currentAxis);

        while (Mathf.Abs(currentRotationAmount) > 0.1f)
        {
            float step = returnSpeed * Time.deltaTime * -Mathf.Sign(currentRotationAmount);
            if (Mathf.Abs(step) > Mathf.Abs(currentRotationAmount))
            {
                step = -currentRotationAmount;
            }

            currentRotationAmount += step;

            transform.rotation = startRotation * Quaternion.Euler(rotationAxis * currentRotationAmount);
            UpdateFacePositionsAndRotations();

            yield return null;
        }

        // 确保完全回到起点
        currentRotationAmount = 0f;
        transform.rotation = startRotation;
        UpdateFacePositionsAndRotations();

        isRotating = false;
        rotationCoroutine = null;
    }

    Vector3 GetAxisVector(Axis axis)
    {
        switch (axis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
            default: return Vector3.zero;
        }
    }

    void UpdateFacePositionsAndRotations()
    {
        foreach (var face in faces)
        {
            if (face.faceObject != null)
            {
                Vector3 targetPosition = transform.rotation * GetLocalPositionForFace(faces, face);
                face.faceObject.transform.position = transform.position + targetPosition;

                if (face.keepUpright)
                {
                    face.faceObject.transform.rotation = Quaternion.Euler(face.originalRotation);
                }
                else
                {
                    face.faceObject.transform.rotation = transform.rotation * Quaternion.Euler(face.originalRotation);
                }
            }
        }
    }

    Vector3 GetLocalPositionForFace(FaceSettings[] allFaces, FaceSettings face)
    {
        int index = System.Array.IndexOf(allFaces, face);
        switch (index)
        {
            case 0: return Vector3.forward * 0.5f;
            case 1: return Vector3.back * 0.5f;
            case 2: return Vector3.left * 0.5f;
            case 3: return Vector3.right * 0.5f;
            case 4: return Vector3.up * 0.5f;
            case 5: return Vector3.down * 0.5f;
            default: return Vector3.zero;
        }
    }

    void ReorderFaces(Axis axis, bool clockwise)
    {
        FaceSettings[] newFaces = new FaceSettings[6];
        System.Array.Copy(faces, newFaces, 6);

        // ... (保持之前的ReorderFaces实现不变)
        // 这里省略具体实现，使用你之前的面重新排序逻辑

        faces = newFaces;
    }
}

public enum Axis
{
    X,
    Y,
    Z
}
