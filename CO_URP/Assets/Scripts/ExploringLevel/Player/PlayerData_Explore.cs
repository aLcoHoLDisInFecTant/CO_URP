using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData_Explore")]
public class PlayerData_Explore : ScriptableObject
{
    [Header("��������")]
    //[field: SerializeField] public int InvincibilityTime { get; private set; }

    [Header("����״̬�����ݣ�")]
    [field: SerializeField] public float JumpHeight { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    //[field: SerializeField] public float Speed { get; private set; }
    

}