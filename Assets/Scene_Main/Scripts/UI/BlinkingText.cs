using UnityEngine;
using TMPro; // TextMeshPro를 쓰기 위해 필수!

public class BlinkingText : MonoBehaviour
{
    [Header("타겟 텍스트 (비워두면 내꺼 씀)")]
    public TextMeshProUGUI targetText;

    [Header("깜빡임 설정")]
    [Tooltip("깜빡이는 속도 (숫자가 클수록 빠름)")]
    [Range(0.1f, 10f)]
    public float blinkSpeed = 3.0f;

    [Tooltip("가장 흐려졌을 때의 투명도 (0 = 완전 투명, 1 = 불투명)")]
    [Range(0f, 1f)]
    public float minAlpha = 0.1f;

    [Tooltip("가장 진해졌을 때의 투명도")]
    [Range(0f, 1f)]
    public float maxAlpha = 1.0f;

    void Start()
    {
        // 인스펙터에 연결 안 했으면 자동으로 자기 자신 컴포넌트 가져옴
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (targetText == null) return;

        // --- 수학 로직 설명 ---
        // Mathf.Sin: -1 ~ 1 사이를 오가는 파동을 만듭니다.
        // (Sin + 1) / 2: 값을 0 ~ 1 사이로 변환합니다.
        float time = Time.unscaledTime * blinkSpeed; // Time.time 대신 unscaledTime을 쓰면 게임이 멈춰도(Timescale 0) 깜빡임
        float alphaWave = (Mathf.Sin(time) + 1.0f) / 2.0f;

        // 최소값(minAlpha)과 최대값(maxAlpha) 사이를 부드럽게 오가게 만듭니다.
        float currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, alphaWave);

        // 텍스트 색상에 알파값 적용
        Color color = targetText.color;
        color.a = currentAlpha;
        targetText.color = color;
    }
}