using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene01Manager : MonoBehaviour
{
    [Header("Player引用")]
    public GameObject player;

    [Header("初始事件开关")]
    public bool triggerInitialEvents = true;

    [Header("出生点")]
    public GameObject birth;

    private bool phoneRing = false;
    private bool davidSpeak01 = false;
    private bool guidance01 = false;

    void Start()
    {
        // 场景加载完成后初始化
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
            Debug.LogWarning("SpawnPoint或Player未正确设置！");
        }
    }

    void TriggerInitialEvents()
    {
        // 示例：调用 EventManager 触发初始事件
        Debug.Log("触发第一个对话");
        EventManager.TriggerEvent("ShowGuide", "guide_004");
        Time.timeScale = 0f;

        // 或者你也可以直接调用初始事件函数
        // StartIntroDialogue();
        // PlayInitialCutscene();
    }

    private void TimeResume(object data)
    {
        Time.timeScale = 1f;
    }
}
