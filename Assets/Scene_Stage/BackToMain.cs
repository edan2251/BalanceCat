using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMain : MonoBehaviour
{
    void Update()
    {
        // 스페이스바(KeyCode.Space)가 눌렸는지 확인합니다.
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