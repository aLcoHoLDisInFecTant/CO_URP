using UnityEngine;
using TMPro; // ����TextMeshPro�������ռ�

/// <summary>
/// �������HUD����ʾ���ر��ǽ��������
/// ��Ҫ�����ڡ�playerHUD����Ϸ�����ϡ�
/// </summary>
public class HUDCtrl : MonoBehaviour
{
    // ��Unity�༭���У���playerHUD���Ӷ���������ʾ���������TMP Text����ק������ֶ���
    [SerializeField]
    private TMP_InputField coinCountText;
    [SerializeField]
    private TMP_InputField ScoreText;

    void Start()
    {
        // ȷ��coinCountText�Ѿ�����ֵ�����û�У��ڿ���̨���������ʾ
        if (coinCountText == null)
        {
            Debug.LogError("HUDCtrl�ű��ϵ�Coin Count Textû�б�ָ��������Inspector�������קһ��TextMeshProUGUI�����", this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ���ScoreHUDBridge��ʵ�����ı�����Ƿ񶼴��ڣ��Ա����ڳ������ػ�����ʱ���ֿ����ô���
        if (ScoreHUDBridge.Instance != null && coinCountText != null)
        {
            // ��ScoreHUDBridge�����л�ȡ��ǰ��CoinCount
            int currentCoins = ScoreHUDBridge.Instance.CoinCount;
            int currentScore = Mathf.RoundToInt(ScoreHUDBridge.Instance.GetTotalScore());
            // ����TextMeshProUGUI������ı�����
            // ToString()����������ת��Ϊ�ַ���
            coinCountText.text = currentCoins.ToString();
            ScoreText.text = currentScore.ToString();
            // ����������һЩǰ׺��������������
            // coinCountText.text = "Coins: " + currentCoins.ToString();
        }
    }
}