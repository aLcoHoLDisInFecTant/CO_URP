using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceivePanelCtrl : MonoBehaviour
{
    [Header("collection")]
    public GameObject star;
    public GameObject phone;
    public GameObject boomerang;
    public Canvas canva;
    public float startTime;
    private bool isPoped;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.StartListening("PopReceive", PopCollection);
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneTimer.Instance.GetTime() - startTime >= 2f && startTime!=0f && isPoped) {
            close();
            startTime = 0f;
        }
    }

    private void PopCollection(object data)
    {
        startTime = SceneTimer.Instance.GetTime();
        canva.gameObject.SetActive(true);
        star.gameObject.SetActive(false);
        phone.gameObject.SetActive(false);
        boomerang.gameObject.SetActive(false);
        if (data != null)
        {
            if ("Star".Equals(data))
            {
                star.SetActive(true);
            }
            else if ("Phone".Equals(data))
            {
                phone.SetActive(true);
            }
            else if ("boomerang".Equals(data)) {
                boomerang.SetActive(true);
            }
        }
        isPoped = true;
    }

    private void close() 
    {
        canva.gameObject.SetActive(false);
        star.gameObject.SetActive(false);
        phone.gameObject.SetActive(false);
        boomerang.gameObject.SetActive(false);
        isPoped = false; 
    }
}
