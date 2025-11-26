using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [Tooltip("실제 텍스트를 표시하는 TMP 컴포넌트")]
    public TextMeshPro textMeshPro;
    [Tooltip("말풍선 전체를 끄고 킬 부모 오브젝트")]
    public GameObject bubbleRoot;

    [Header("설정")]
    [Tooltip("텍스트가 사라질 때 걸리는 시간 (초)")]
    public float fadeOutDuration = 1.0f;

    private Coroutine hideCoroutine;
    private Camera mainCamera;

    void Start()
    {
        // 시작할 땐 무조건 안 보이게 설정
        if (bubbleRoot != null)
        {
            bubbleRoot.SetActive(false);
        }

        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("메인 카메라를 찾을 수 없습니다!");
    }

    void LateUpdate()
    {
        // 빌보드 기능: 항상 카메라를 정면으로 바라보게 함
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    // 외부(트리거)에서 이 함수를 호출해서 메시지를 띄웁니다.
    public void ShowMessage(string message, float duration = 10f)
    {
        if (textMeshPro == null || bubbleRoot == null) return;

        // 1. 메시지 설정
        textMeshPro.text = message;

        // 2. [중요] 나타날 땐 알파값을 1(완전 불투명)로 즉시 초기화
        SetTextAlpha(1f);
        bubbleRoot.SetActive(true);

        // 3. 만약 이미 메시지를 끄려고 기다리던 중이었다면, 그 타이머 취소
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        // 4. 새로운 사라지기 코루틴 시작
        hideCoroutine = StartCoroutine(HideProcess(duration));
    }

    // 정해진 시간 대기 후 -> 페이드 아웃 -> 끄기
    private IEnumerator HideProcess(float duration)
    {
        // 전체 지속 시간에서 페이드 아웃 시간을 뺀 만큼만 대기
        // (예: 10초 유지인데 페이드가 1초면, 9초 동안은 가만히 있다가 1초 동안 사라짐)
        float waitTime = Mathf.Max(0, duration - fadeOutDuration);

        yield return new WaitForSeconds(waitTime);

        // 페이드 아웃 시작
        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            // 1에서 0으로 서서히 줄어듦
            float newAlpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            SetTextAlpha(newAlpha);
            yield return null;
        }

        // 확실하게 투명하게 만들고 오브젝트 끄기
        SetTextAlpha(0f);
        bubbleRoot.SetActive(false);
        hideCoroutine = null;
    }

    // TMP의 투명도(Alpha)를 조절하는 헬퍼 함수
    private void SetTextAlpha(float alpha)
    {
        if (textMeshPro != null)
        {
            textMeshPro.alpha = alpha; // TMP 고유 기능 사용
        }
    }
}