using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Player")]
public class PlayerData : ScriptableObject
{
    [Header("基础配置")]
    [field: SerializeField] public int InvincibilityTime { get; private set; }

    [Header("地面状态（兼容）")]
    //[field: SerializeField] public float JumpHeight { get; private set; }
    [field: SerializeField] public float SwitchSpeed { get; private set; }
    //[field: SerializeField] public float Speed { get; private set; }

    [Header("飞行状态参数")]
    [field: SerializeField] public float FlyingSpeed { get; private set; }
    //[field: SerializeField] public float AltitudeSwitchSpeed { get; private set; }
    [field: SerializeField] public float SlideRollDuration { get; private set; }

    [Header("轨道设置")]
    [field: SerializeField] public int MaxHorizontalLanes { get; private set; } = 3; // 默认三段
    [field: SerializeField] public int MaxAltitudeLevels { get; private set; } = 2;  // 默认上下两层
}
