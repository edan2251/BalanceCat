using UnityEngine;

public class GameExit : MonoBehaviour
{
    // 이 함수를 버튼 클릭 이벤트에 연결합니다.
    public void QuitGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }
        // 유니티 에디터에서 실행 중일 경우
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서 실행 중일 경우
        Application.Quit();
#endif
    }
}