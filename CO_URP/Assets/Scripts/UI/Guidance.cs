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
        // 用户按下 ESC 手动关闭
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
        //Debug.Log("文本读取" + content);
        //GuideUI.Instance.ShowGuide(content, OnGuideClosed);
        inputfield.text = content;
        // 假设 ShowGuide 接收文本并在关闭时调用回调
    }

    void OnGuideClosed()
    {
        //Debug.Log("引导页关闭");
        canvas.gameObject.SetActive(false);
        EventManager.TriggerEvent("GuideClosed");
    }
}
