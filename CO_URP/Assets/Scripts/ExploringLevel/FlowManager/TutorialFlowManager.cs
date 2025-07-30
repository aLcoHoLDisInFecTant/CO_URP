using UnityEngine;

public class TutorialFlowManager : MonoBehaviour
{
    private int step = 0;

    void Start()
    {
        EventManager.StartListening("GuideClosed", OnGuideClosed);
        EventManager.StartListening("InteractionComplete", OnInteractionComplete);
        StartTutorial();
    }

    void StartTutorial()
    {
        step = 0;
        TriggerStep(step);
    }

    void TriggerStep(int index)
    {
        switch (index)
        {
            case 0:
                EventManager.TriggerEvent("NPCSpeak", "npc_001");
                EventManager.TriggerEvent("ShowGuide", "guide_004");
                break;

            case 1:
                EventManager.TriggerEvent("EnableInteraction", "item1");
                break;

            case 2:
                EventManager.TriggerEvent("NPCSpeak", "step2_npc2");
                EventManager.TriggerEvent("ShowGuide", "guide1");
                break;

            case 3:
                EventManager.TriggerEvent("EnableInteraction", "item2");
                break;

            case 4:
                EventManager.TriggerEvent("ShowGuide", "guide2");
                break;

            case 5:
                EventManager.TriggerEvent("EnableInteraction", "item3");
                break;

            case 6:
                EventManager.TriggerEvent("NPCSpeak", "end");
                break;
        }
    }

    void OnGuideClosed(object _)
    {
        step++;
        TriggerStep(step);
    }

    void OnInteractionComplete(object obj)
    {
        // obj 为当前完成交互的物体ID
        if ((string)obj == $"item{(step - 1)}")
        {
            step++;
            TriggerStep(step);
        }
    }

    void OnDestroy()
    {
        EventManager.StopListening("GuideClosed", OnGuideClosed);
        EventManager.StopListening("InteractionComplete", OnInteractionComplete);
    }
}
