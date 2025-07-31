using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPreviewController : MonoBehaviour
{
    [Header("引用")]
    public Player_Explore player;
    public GameObject targetPointPrefab;

    [Header("参数")]
    public float previewSpeed = 3f;

    private GameObject currentTargetPoint;
    private Rigidbody targetPointRigidbody; // ⬅ 新增：用于存储刚体引用
    private Vector3 direction;

    private bool isPreviewing = false;

    [Header("摄像机控制")]
    public Transform cameraTransform;

    private Vector3 GetCameraRelativeDirection(Vector3 inputDir)
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = inputDir.z * camForward + inputDir.x * camRight;
        return moveDir.normalized;
    }

    public void StartPreview(Vector3 dir)
    {
        cameraTransform = player.cameraTransform;
        CancelPreview();

        direction = GetCameraRelativeDirection(dir);
        Vector3 startPosition = player.transform.position;

        currentTargetPoint = Instantiate(targetPointPrefab, startPosition, Quaternion.identity);
        targetPointRigidbody = currentTargetPoint.GetComponent<Rigidbody>(); // ⬅ 新增：获取刚体组件
        if (targetPointRigidbody == null)
        {
            Debug.LogError("目标点预制体 (targetPointPrefab) 上缺少 Rigidbody 组件！");
        }
        isPreviewing = true;
    }

    // 注意：由于移动将在 FixedUpdate 中处理，此方法可以移除或留空
    public void UpdatePreview(Vector3 dir)
    {
        if (!isPreviewing || currentTargetPoint == null) return;
        direction = GetCameraRelativeDirection(dir);
    }

    // 推荐：将物理相关操作放在 FixedUpdate 中
    void FixedUpdate()
    {
        if (isPreviewing && targetPointRigidbody != null)
        {
            Vector3 move = direction * previewSpeed * Time.fixedDeltaTime;
            targetPointRigidbody.MovePosition(targetPointRigidbody.position + move);
        }
    }


    public Vector3 EndPreview()
    {
        isPreviewing = false;
        Vector3 result = currentTargetPoint != null ? currentTargetPoint.transform.position : player.transform.position;
        if (currentTargetPoint != null) Destroy(currentTargetPoint);
        return result;
    }

    public void CancelPreview()
    {
        isPreviewing = false;
        if (currentTargetPoint != null) Destroy(currentTargetPoint);
    }

    public bool IsPreviewing => isPreviewing;
}