using UnityEngine;
using TMPro; // 引用TextMeshPro的命名空间

/// <summary>
/// 控制玩家HUD的显示，特别是金币数量。
/// 需要挂载在“playerHUD”游戏对象上。
/// </summary>
public class HUDCtrl : MonoBehaviour
{
    // 在Unity编辑器中，将playerHUD的子对象（用于显示金币数量的TMP Text）拖拽到这个字段上
    [SerializeField]
    private TMP_InputField coinCountText;
    [SerializeField]
    private TMP_InputField ScoreText;

    void Start()
    {
        // 确保coinCountText已经被赋值，如果没有，在控制台输出错误提示
        if (coinCountText == null)
        {
            Debug.LogError("HUDCtrl脚本上的Coin Count Text没有被指定！请在Inspector面板中拖拽一个TextMeshProUGUI组件。", this.gameObject);
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
}