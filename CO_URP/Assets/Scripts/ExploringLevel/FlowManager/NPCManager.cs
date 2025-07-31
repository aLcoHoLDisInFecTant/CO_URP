using TMPro;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [SerializeField] public GameObject NPCchat;
    [SerializeField] public TMP_Text _textMeshPro;
    [SerializeField] public GameObject assistant;
    [SerializeField] public GameObject david;
    private float timeCount;
    private Transform panel;
    private void OnEnable()
    {
        EventManager.StartListening("NPCSpeak", OnSpeak);
        assistant.SetActive(false);
        david.SetActive(false);
        panel = NPCchat.transform.Find("Panel");
        panel.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        EventManager.StopListening("NPCSpeak", OnSpeak);
    }

    private void Update()
    {
        if (SceneTimer.Instance.GetTime() - timeCount >= 3f) {
            CloseContact();
        }
    }

    void OnSpeak(object data)
    {
        panel.gameObject.SetActive(true);
        timeCount = SceneTimer.Instance.GetTime();
        string dialogueID = data as string;
        string content = CSVTextManager.Instance.GetTextByID(dialogueID);
        Debug.Log("̨��" + content);
        if (dialogueID == "npc_001")
        {
            assistant.SetActive(false);
            david.SetActive(true);
        }
        else if (dialogueID == "npc_002")
        {
            assistant.SetActive(true);
            david.SetActive(false);
        }
        else if (dialogueID == "npc_003")
        {
            assistant.SetActive(true);
            david.SetActive(false);
        }
        else if (dialogueID == "npc_004")
        {
            assistant.SetActive(true);
            david.SetActive(false);
        }
        else if (dialogueID == "npc_005")
        {
            assistant.SetActive(true);
            david.SetActive(false);
        }
        else if (dialogueID == "npc_006")
        {
            assistant.SetActive(true);
            david.SetActive(false);
        }
        //DialogueUI.Instance.ShowDialogue(content); // ���������� DialogueUI �ű�����Ի���ʾ
        _textMeshPro.text = content;

    }

    public void CloseContact() {
        assistant.SetActive(false);
        david.SetActive(false);
        _textMeshPro.ClearMesh();
        panel.gameObject.SetActive(false);
    }
}
