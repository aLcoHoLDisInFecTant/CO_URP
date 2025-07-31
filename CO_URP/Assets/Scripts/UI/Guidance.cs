using Michsky.MUIP;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GuideManager : MonoBehaviour
{
    [SerializeField] public Player_Explore player;
    [SerializeField] public GameObject guidance;
    [SerializeField] public TMP_InputField inputfield;
    //[SerializeField] public ProgressBar progressbar;
    private Transform canvas;
    //private bool guideCompleted = false;


    private void OnEnable()
    {
        EventManager.StartListening("ShowGuide", OnShowGuide);
        canvas = guidance.transform.Find("Canvas");
        
    }

    private void OnDisable()
    {
        EventManager.StopListening("ShowGuide", OnShowGuide);
    }

    private void Update()
    {
        // �û����� ESC �ֶ��ر�
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //guideCompleted = true;
            OnGuideClosed();
            return;
        }
    }


    void OnShowGuide(object data)
    {
        //guideCompleted = false;
        canvas.gameObject.SetActive(true);
        string guideID = data as string;
        string content = CSVTextManager.Instance.GetTextByID(guideID);
        //Debug.Log("�ı���ȡ" + content);
        //GuideUI.Instance.ShowGuide(content, OnGuideClosed);
        inputfield.text = content;
        // ���� ShowGuide �����ı����ڹر�ʱ���ûص�
    }

    void OnGuideClosed()
    {
        //Debug.Log("����ҳ�ر�");
        canvas.gameObject.SetActive(false);
        EventManager.TriggerEvent("GuideClosed");
    }
}
