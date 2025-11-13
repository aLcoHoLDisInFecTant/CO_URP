using System.Collections.Generic;
using UnityEngine;

public class PlayerSpeedModifier : MonoBehaviour
{
    Dictionary<string, float> multipliers = new Dictionary<string, float>();

    public float CurrentMultiplier
    {
        get
        {
            float result = 1f;
            foreach (var kv in multipliers)
            {
                result = Mathf.Min(result, kv.Value);
            }
            return result;
        }
    }

    public void SetModifier(string key, float multiplier)
    {
        multipliers[key] = Mathf.Clamp(multiplier, 0.2f, 1f);
    }

    public void RemoveModifier(string key)
    {
        if (multipliers.ContainsKey(key)) multipliers.Remove(key);
    }
}

