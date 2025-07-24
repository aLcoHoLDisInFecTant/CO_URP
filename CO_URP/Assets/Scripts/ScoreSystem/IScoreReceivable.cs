using UnityEngine;

public interface IScoreReceivable
{
    float GetScore();                      // 返回道具对应的分值
    void OnReceived(GameObject receiver); // 被玩家拾取后触发的逻辑
}
