using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    [SerializeField] private EncoderInputBridgeV2 inputBridge;
    [SerializeField] private ActionTracker trackerX, trackerY, trackerZ;

    public float CalculateActionScore()
    {
        float score = 0f;
        score += trackerX.UpdateAndGetScore(inputBridge.GetRelativeVertical());  // X
        score += trackerY.UpdateAndGetScore(inputBridge.GetRelativeRotationY()); // Y
        score += trackerZ.UpdateAndGetScore(inputBridge.GetRelativeHorizontal()); // Z
        return score;
    }
}
