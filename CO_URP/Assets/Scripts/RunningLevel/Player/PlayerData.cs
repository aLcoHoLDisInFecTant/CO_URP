using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/Player")]
public class PlayerData : ScriptableObject
{
    [Header("��������")]
    [field: SerializeField] public int InvincibilityTime { get; private set; }

    [Header("����״̬�����ݣ�")]
    //[field: SerializeField] public float JumpHeight { get; private set; }
    [field: SerializeField] public float SwitchSpeed { get; private set; }
    //[field: SerializeField] public float Speed { get; private set; }

    [Header("����״̬����")]
    [field: SerializeField] public float FlyingSpeed { get; private set; }
    //[field: SerializeField] public float AltitudeSwitchSpeed { get; private set; }
    [field: SerializeField] public float SlideRollDuration { get; private set; }

    [Header("�������")]
    [field: SerializeField] public int MaxHorizontalLanes { get; private set; } = 3; // Ĭ������
    [field: SerializeField] public int MaxAltitudeLevels { get; private set; } = 2;  // Ĭ����������
}
