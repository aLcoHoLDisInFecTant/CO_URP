using UnityEngine;

public interface IScoreReceivable
{
    float GetScore();                      // ���ص��߶�Ӧ�ķ�ֵ
    void OnReceived(GameObject receiver); // �����ʰȡ�󴥷����߼�
}
