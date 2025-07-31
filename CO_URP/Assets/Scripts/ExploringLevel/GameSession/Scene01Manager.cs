using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene01Manager : MonoBehaviour
{
    [Header("Player����")]
    public GameObject player;

    [Header("��ʼ�¼�����")]
    public bool triggerInitialEvents = true;

    [Header("������")]
    public GameObject birth;

    private bool phoneRing = false;
    private bool davidSpeak01 = false;
    private bool guidance01 = false;

    void Start()
    {
        // ����������ɺ��ʼ��
        InitializeScene();
        EventManager.StartListening("GuideClosed", TimeResume);
    }

    void InitializeScene()
    {
        MovePlayerToSpawn();

        
    }

    private void Update()
    {
        if (SceneTimer.Instance.GetTime() >= 2f && !phoneRing) {
            EventManager.TriggerEvent("PhoneRing", 1);
            phoneRing = true;
        }


        if (SceneTimer.Instance.GetTime() >= 3f && !davidSpeak01) {
            EventManager.TriggerEvent("NPCSpeak", "npc_001");
            davidSpeak01 = true;
        }

        if (SceneTimer.Instance.GetTime() >= 7f && !guidance01)
        {
            TriggerInitialEvents();
            guidance01 = true;
        }
    }

    void MovePlayerToSpawn()
    {
        //GameObject spawnPoint = GameObject.Find("SpawnPoint");

        if (birth != null && player != null)
        {
            player.transform.position = birth.transform.position;
            player.transform.rotation = birth.transform.rotation;
        }
        else
        {
            Debug.LogWarning("SpawnPoint��Playerδ��ȷ���ã�");
        }
    }

    void TriggerInitialEvents()
    {
        // ʾ�������� EventManager ������ʼ�¼�
        Debug.Log("������һ���Ի�");
        EventManager.TriggerEvent("ShowGuide", "guide_004");
        Time.timeScale = 0f;

        // ������Ҳ����ֱ�ӵ��ó�ʼ�¼�����
        // StartIntroDialogue();
        // PlayInitialCutscene();
    }

    private void TimeResume(object data)
    {
        Time.timeScale = 1f;
    }
}
