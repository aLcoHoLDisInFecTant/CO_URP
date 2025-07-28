using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HandController : MonoBehaviour
{
    public Player_Explore player;
    public Transform hand;     // Hand/Hand
    public Transform handRig;  // Hand/HandRig

    void Update()
    {
        UpdatePose(); // 或在 Visual Scripting 中通过 Custom Event 调用
    }

    public void UpdatePose()
    {
        if (player == null) return;

        // 复制队列
        List<ECommand> activeInputs = player.inputQueue.ToList();

        // 处理 Hand（姿态：LEFT, RIGHT, UP, DOWN）
        float xAngle = 0;
        float zAngle = 0;

        if (activeInputs.Contains(ECommand.SLIDELEFT) && activeInputs.Contains(ECommand.SLIDERIGHT))
        {
            var last = activeInputs.Last(c => c == ECommand.SLIDELEFT || c == ECommand.SLIDERIGHT);
            xAngle = (last == ECommand.SLIDELEFT) ? -30f : 30f;
        }
        else if (activeInputs.Contains(ECommand.SLIDELEFT))
            xAngle = 30f;
        else if (activeInputs.Contains(ECommand.SLIDERIGHT))
            xAngle = -30f;

        if (activeInputs.Contains(ECommand.UP) && activeInputs.Contains(ECommand.DOWN))
        {
            var last = activeInputs.Last(c => c == ECommand.UP || c == ECommand.DOWN);
            zAngle = (last == ECommand.UP) ? 30f : -30f;
        }
        else if (activeInputs.Contains(ECommand.UP))
            zAngle = -30f;
        else if (activeInputs.Contains(ECommand.DOWN))
            zAngle = 30f;

        hand.localRotation = Quaternion.Euler(xAngle, 0f, zAngle);

        // 处理 HandRig（姿态：SLIDELEFT, SLIDERIGHT）
        float rigZ = 0f;
        if (activeInputs.Contains(ECommand.LEFT) && activeInputs.Contains(ECommand.RIGHT))
        {
            var last = activeInputs.Last(c => c == ECommand.LEFT || c == ECommand.RIGHT);
            rigZ = (last == ECommand.LEFT) ? 30f : -30f;
        }
        else if (activeInputs.Contains(ECommand.LEFT))
            rigZ = 30f;
        else if (activeInputs.Contains(ECommand.RIGHT))
            rigZ = -30f;

        handRig.localRotation = Quaternion.Euler(0f, 0f, rigZ);
    }
}
