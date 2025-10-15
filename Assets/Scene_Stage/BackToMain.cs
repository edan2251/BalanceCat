using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMain : MonoBehaviour
{
    void Update()
    {
        // �����̽���(KeyCode.Space)�� ���ȴ��� Ȯ���մϴ�.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadMainScene();
        }
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}