using UnityEngine;

public class GameExit : MonoBehaviour
{
    // �� �Լ��� ��ư Ŭ�� �̺�Ʈ�� �����մϴ�.
    public void QuitGame()
    {
        // ����Ƽ �����Ϳ��� ���� ���� ���
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ����� ���ӿ��� ���� ���� ���
        Application.Quit();
#endif
    }
}