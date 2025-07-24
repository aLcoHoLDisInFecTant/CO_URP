using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPreviewController : MonoBehaviour
{
    [Header("引用")]
    public Player_Explore player;                     // 玩家引用
    public GameObject targetPointPrefab;     // 可检测碰撞的终点预制体
    //public GameObject arrowLinePrefab;       // 用于连接玩家与终点的箭头

    [Header("参数")]
    public float previewSpeed = 3f;

    private GameObject currentTargetPoint;
    //private GameObject currentArrowLine;
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

        direction = GetCameraRelativeDirection(dir); // ⬅ 转换方向
        Vector3 startPosition = player.transform.position;

        currentTargetPoint = Instantiate(targetPointPrefab, startPosition, Quaternion.identity);
        isPreviewing = true;
    }

    public void UpdatePreview(Vector3 dir)
    {
        if (!isPreviewing || currentTargetPoint == null) return;

        direction = GetCameraRelativeDirection(dir); // ⬅ 转换方向
        Vector3 move = direction * previewSpeed * Time.deltaTime;

        currentTargetPoint.transform.position += move;
    }


    /*
    public void StartPreview(Vector3 dir)
    {
        cameraTransform = player.cameraTransform;

        CancelPreview(); // 清理旧状态

        direction = dir.normalized;
        Vector3 startPosition = player.transform.position;

        currentTargetPoint = Instantiate(targetPointPrefab, startPosition, Quaternion.identity);
        //currentArrowLine = Instantiate(arrowLinePrefab);
        isPreviewing = true;
    }

    public void UpdatePreview(Vector3 dir)
    {
        if (!isPreviewing || currentTargetPoint == null) return;

        direction = dir.normalized;
        Vector3 move = direction * previewSpeed * Time.deltaTime;

        // 移动由终点预制体的 Rigidbody + 碰撞自动处理是否停止
        currentTargetPoint.transform.position += move;

        //UpdateArrowLine();
    }
    */
    /*
    void UpdateArrowLine()
    {
        if (currentArrowLine == null || currentTargetPoint == null) return;

        Vector3 start = player.transform.position;
        Vector3 end = currentTargetPoint.transform.position;

        currentArrowLine.transform.position = start;
        currentArrowLine.transform.rotation = Quaternion.LookRotation(end - start);

        float length = Vector3.Distance(start, end);
        Vector3 scale = currentArrowLine.transform.localScale;
        scale.z = length;
        currentArrowLine.transform.localScale = scale;
    }
    */

    public Vector3 EndPreview()
    {
        isPreviewing = false;

        Vector3 result = currentTargetPoint != null ? currentTargetPoint.transform.position : player.transform.position;

        //if (currentArrowLine != null) Destroy(currentArrowLine);
        if (currentTargetPoint != null) Destroy(currentTargetPoint);

        return result;
    }

    public void CancelPreview()
    {
        isPreviewing = false;

        //if (currentArrowLine != null) Destroy(currentArrowLine);
        if (currentTargetPoint != null) Destroy(currentTargetPoint);
    }

    public bool IsPreviewing => isPreviewing;
}