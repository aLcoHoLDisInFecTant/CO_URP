using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTransTrigger : MonoBehaviour
{
    public NormalTrans platform; // ��ƽ̨�ű�����

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.RegisterRider(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platform.UnregisterRider(other.transform);
        }
    }
}
