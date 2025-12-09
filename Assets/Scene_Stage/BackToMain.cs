using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMain : MonoBehaviour
{

    public void LoadMainScene()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }

        SceneManager.LoadScene("Main");
    }

    public void ReloadCurrentStage()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }
        // 현재 활성화된 씬의 이름을 가져와서 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}