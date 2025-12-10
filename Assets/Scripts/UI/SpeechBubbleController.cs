using UnityEngine;
using TMPro;
using System.Collections;

public class SpeechBubbleController : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    public TextMeshPro textMeshPro;
    public GameObject bubbleRoot;

    [Header("설정")]
    public float fadeOutDuration = 1.0f;

    private Coroutine hideCoroutine;
    private Camera mainCamera;

    // [신규] 우선순위 메시지가 떠있는지 확인하는 플래그
    private bool _isLockedByPriority = false;

    void Start()
    {
        if (bubbleRoot != null) bubbleRoot.SetActive(false);
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    // ---------------------------------------------------------
    // 메시지 표시 함수들 (우선순위 로직 추가)
    // ---------------------------------------------------------

    // [수정] isPriority 매개변수 추가 (기본값 false)
    public void ShowMessage(string message, float duration = 10f, bool isPriority = false)
    {
        if (textMeshPro == null || bubbleRoot == null) return;

        // 1. 이미 우선순위 메시지가 떠있는데, 새로 온 메시지가 우선순위가 아니라면? -> 무시
        if (_isLockedByPriority && !isPriority) return;

        // 2. 메시지 설정
        textMeshPro.text = message;
        SetTextAlpha(1f);
        bubbleRoot.SetActive(true);

        // 3. 우선순위 상태 업데이트
        _isLockedByPriority = isPriority;

        // 4. 기존 타이머 끄고 새로 시작
        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideProcess(duration));
    }

    // [수정] 지속 메시지에도 우선순위 로직 적용
    public void ShowContinuousMessage(string message, bool isPriority = false)
    {
        if (textMeshPro == null || bubbleRoot == null) return;

        // 우선순위 체크 (중요: 아이템 획득 메시지 등은 보통 isPriority = false로 호출됨)
        if (_isLockedByPriority && !isPriority) return;

        textMeshPro.text = message;
        SetTextAlpha(1f);
        bubbleRoot.SetActive(true);

        // 지속 메시지는 우선순위를 점유할지 말지 결정 (보통 아이템 메시지는 점유 안 함)
        _isLockedByPriority = isPriority;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    public void HideContinuousMessage(bool fadeOut = true)
    {
        // 끄기 명령이 들어오면 우선순위 잠금도 해제해야 함
        _isLockedByPriority = false;

        if (fadeOut && gameObject.activeInHierarchy)
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideProcess(0f));
        }
        else
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = null;

            SetTextAlpha(0f);
            if (bubbleRoot != null) bubbleRoot.SetActive(false);
        }
    }

    // ---------------------------------------------------------

    private IEnumerator HideProcess(float duration)
    {
        float waitTime = Mathf.Max(0, duration - fadeOutDuration);
        yield return new WaitForSeconds(waitTime);

        float timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            SetTextAlpha(newAlpha);
            yield return null;
        }

        SetTextAlpha(0f);
        bubbleRoot.SetActive(false);
        hideCoroutine = null;

        // [중요] 메시지가 완전히 사라지면 잠금 해제
        _isLockedByPriority = false;
    }

    private void SetTextAlpha(float alpha)
    {
        if (textMeshPro != null) textMeshPro.alpha = alpha;
    }
}