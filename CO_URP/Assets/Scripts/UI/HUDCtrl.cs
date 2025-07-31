using UnityEngine;
using TMPro; // ����TextMeshPro�������ռ�

/// <summary>
/// �������HUD����ʾ���ر��ǽ��������
/// ��Ҫ�����ڡ�playerHUD����Ϸ�����ϡ�
/// </summary>
public class HUDCtrl : MonoBehaviour
{
    public int starCount;

    // ��Unity�༭���У���playerHUD���Ӷ���������ʾ���������TMP Text����ק������ֶ���
    [SerializeField]
    private TMP_InputField coinCountText;
    [SerializeField]
    private TMP_InputField ScoreText;
    [SerializeField]
    private GameObject starIcon;
    private GameObject starIcon_2;
    private GameObject starIcon_3;
    private int starsCount = 0;

    void Start()
    {
        EventManager.StartListening("LightStar", LightStar);
        EventManager.StartListening("ResultMedium", BoostResult);
        // ȷ��coinCountText�Ѿ�����ֵ�����û�У��ڿ���̨���������ʾ
        starIcon.SetActive(false);
        starIcon_2.SetActive(false);
        starIcon_3.SetActive(false);
        if (coinCountText == null)
        {
            Debug.LogError("HUDCtrl�ű��ϵ�Coin Count Textû�б�ָ��������Inspector�������קһ��TextMeshProUGUI�����", this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (ScoreHUDBridge.Instance != null && coinCountText != null)
        {

            int currentCoins = ScoreHUDBridge.Instance.CoinCount;
            int currentScore = Mathf.RoundToInt(ScoreHUDBridge.Instance.GetTotalScore());

            coinCountText.text = currentCoins.ToString();
            ScoreText.text = currentScore.ToString();

        }
    }

    void LightStar(object id) 
    {
        switch (id) 
        {
            case 0:
                starIcon.SetActive(true);
                starCount++;
                break;
            case 1: 
                starIcon_2.SetActive(true);
                starCount++;
                break;
            case 2:
                starIcon_3.SetActive(true);
                starCount++;
                break;
        }
    }

    void BoostResult(object data) 
    {
        EventManager.TriggerEvent("settleResult", starCount);
    
    }
}