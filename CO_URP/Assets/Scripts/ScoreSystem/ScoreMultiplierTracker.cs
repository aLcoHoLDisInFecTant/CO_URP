using System.Collections.Generic;
using UnityEngine;

public class ScoreMultiplierTracker
{
    public float Frequency { get; private set; } = 0.5f;
    public float Duration { get; private set; } = 0.5f;
    public float TotalMultiplier => Mathf.Clamp(Frequency + Duration, 1.0f, 4.0f);

    private float durationTimer = 0f;
    private HashSet<ECommand> previousCommands = new();

    private float timeSinceLastNewAction = 0f;
    private float timeSinceLastFrequencyIncrease = 0f;

    private float timeSinceLastSustainedInput = 0f;

    // === ÅäÖÃ²ÎÊý ===
    private const float frequencyIncreaseCooldown = 0.3f;
    private const float durationDecayDelay = 2.0f;

    private const float freqIncrement = 0.1f;
    private const float durIncrement = 0.1f;
    private const float durInterval = 0.1f;

    private const float freqDecayRate = 0.05f;
    private const float durDecayRate = 0.1f;

    // === Debug ¿ØÖÆ ===
    public bool EnableDebugLog = true;

    public void UpdateMultiplier(Queue<ECommand> inputQueue)
    {
        var currentCommands = new HashSet<ECommand>(inputQueue);
        timeSinceLastFrequencyIncrease += Time.deltaTime;

        // === Frequency ===
        bool newActionOccurred = false;
        foreach (var cmd in currentCommands)
        {
            if (!previousCommands.Contains(cmd))
            {
                newActionOccurred = true;
                break;
            }
        }

        if (newActionOccurred && timeSinceLastFrequencyIncrease >= frequencyIncreaseCooldown)
        {
            Frequency = Mathf.Min(Frequency + freqIncrement, 2.0f);
            timeSinceLastFrequencyIncrease = 0f;
            timeSinceLastNewAction = 0f;

            if (EnableDebugLog)
                Debug.Log($"[Multiplier] + Frequency: {freqIncrement} ¡ú {Frequency:F2}");
        }
        else
        {
            timeSinceLastNewAction += Time.deltaTime;
            if (timeSinceLastNewAction >= 1f)
            {
                float decay = freqDecayRate * Time.deltaTime;
                float prev = Frequency;
                Frequency = Mathf.Max(Frequency - decay, 0.5f);

                if (EnableDebugLog && prev != Frequency)
                    Debug.Log($"[Multiplier] - Frequency: {decay:F3} ¡ú {Frequency:F2}");
            }
        }

        // === Duration ===
        if (currentCommands.Count > 0)
        {
            durationTimer += Time.deltaTime;
            timeSinceLastSustainedInput = 0f;

            if (durationTimer >= durInterval)
            {
                float prev = Duration;
                Duration = Mathf.Min(Duration + durIncrement, 2.0f);
                durationTimer = 0f;

                if (EnableDebugLog && prev != Duration)
                    Debug.Log($"[Multiplier] + Duration: {durIncrement} ¡ú {Duration:F2}");
            }
        }
        else
        {
            durationTimer = 0f;
            timeSinceLastSustainedInput += Time.deltaTime;

            if (timeSinceLastSustainedInput >= durationDecayDelay)
            {
                float decay = durDecayRate * Time.deltaTime;
                float prev = Duration;
                Duration = Mathf.Max(Duration - decay, 0.5f);

                if (EnableDebugLog && prev != Duration)
                    Debug.Log($"[Multiplier] - Duration: {decay:F3} ¡ú {Duration:F2}");
            }
        }

        previousCommands = currentCommands;

        if (EnableDebugLog)
        {
            Debug.Log($"[Multiplier] Status: F={Frequency:F2}, D={Duration:F2}, Total={TotalMultiplier:F2}");
        }
    }
}
