using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    // 점수 로직이 연결하면 됨
    public int currentScore = 0;
    public int requiredScore = 1500;

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 들어오면
        if (other.CompareTag("Player"))
        {
            // 플레이어의 말풍선 컨트롤러를 찾음
            SpeechBubbleController bubble = other.GetComponentInChildren<SpeechBubbleController>(true);

            if (bubble != null)
            {
                // 지속 메시지 모드로 켬
                bubble.ShowContinuousMessage($"{currentScore} / {requiredScore}");
            }
        }
    }

    // 구역 안에 있는 동안 점수가 바뀌면 텍스트도 계속 갱신
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SpeechBubbleController bubble = other.GetComponentInChildren<SpeechBubbleController>(true);
            if (bubble != null)
            {
                // 계속 호출해도 깜빡거리지 않고 텍스트만 바뀜
                bubble.ShowContinuousMessage($"{currentScore} / {requiredScore}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 나가면
        if (other.CompareTag("Player"))
        {
            SpeechBubbleController bubble = other.GetComponentInChildren<SpeechBubbleController>(true);

            if (bubble != null)
            {
                // 말풍선을 부드럽게 끔
                bubble.HideContinuousMessage(true);
            }
        }
    }
}