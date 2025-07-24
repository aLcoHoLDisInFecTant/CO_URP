using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTracker : MonoBehaviour
{
    public float frequencyCoefficient = 1f;
    public float durationCoefficient = 1f;

    private float actionThreshold = 10f;
    private float lastActionTime;
    private float activeTime;

    public float UpdateAndGetScore(float input)
    {
        float abs = Mathf.Abs(input);
        float score = 0f;

        if (abs > actionThreshold)
        {
            frequencyCoefficient = Mathf.Min(frequencyCoefficient + Time.deltaTime * 0.1f, 2f);
            durationCoefficient = Mathf.Min(durationCoefficient + Time.deltaTime * 0.2f, 2f);
            activeTime += Time.deltaTime;
            score = 1f * frequencyCoefficient * durationCoefficient;
            lastActionTime = Time.time;
        }
        else
        {
            float timeSinceLast = Time.time - lastActionTime;
            frequencyCoefficient = Mathf.Max(frequencyCoefficient - timeSinceLast * 0.05f, 0.5f);
            durationCoefficient = Mathf.Max(durationCoefficient - timeSinceLast * 0.1f, 0.5f);
        }

        return score;
    }
}

