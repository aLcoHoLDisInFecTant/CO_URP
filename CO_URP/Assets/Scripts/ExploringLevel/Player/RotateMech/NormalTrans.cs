using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTrans : MonoBehaviour
{
    [Header("移动设置")]
    public Vector3 moveDirection = Vector3.up;
    public float moveDistance = 2f;
    public float moveSpeed = 1f;

    [Header("停顿设置")]
    public float pauseDuration = 1f;

    private Vector3 startPoint;
    private float timer;
    private bool isMovingForward = true;
    private bool isPaused = false;
    private float pauseTimer = 0f;

    private Vector3 lastPosition;
    private List<CharacterController> riders = new List<CharacterController>();

    void Start()
    {
        moveDirection.Normalize();
        startPoint = transform.position;
        lastPosition = transform.position;

        Debug.Log("[PingPongMover] 平台初始化完成");
    }

    void Update()
    {
        if (isPaused)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseDuration)
            {
                isPaused = false;
                pauseTimer = 0f;
                isMovingForward = !isMovingForward;
                Debug.Log("[PingPongMover] 切换方向：" + (isMovingForward ? "正向" : "反向"));
            }
            return;
        }

        timer += Time.deltaTime * moveSpeed;
        float t = timer / moveDistance;

        if (t >= 1f)
        {
            t = 1f;
            isPaused = true;
            timer = 0f;
            Debug.Log("[PingPongMover] 到达终点，暂停中");
        }

        Vector3 from = startPoint;
        Vector3 to = startPoint + moveDirection * moveDistance;
        Vector3 newPos = Vector3.Lerp(isMovingForward ? from : to, isMovingForward ? to : from, t);

        Vector3 platformDelta = newPos - transform.position;
        transform.position = newPos;

        // 🚀 移动玩家
        foreach (var rider in riders)
        {
            if (rider != null && rider.enabled)
            {
                rider.Move(platformDelta); // ✅ 关键点：用 Move 而不是直接加 position
            }
        }

        lastPosition = transform.position;
    }

    // ✅ 被子物体 PlayerTriggerZone 调用
    public void RegisterRider(Transform player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null && !riders.Contains(cc))
        {
            riders.Add(cc);
            Debug.Log("[PingPongMover] 玩家绑定成功：" + player.name);
        }
    }

    public void UnregisterRider(Transform player)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null && riders.Contains(cc))
        {
            riders.Remove(cc);
            Debug.Log("[PingPongMover] 玩家解绑：" + player.name);
        }
    }

}
