using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 위해 필수
using System.Collections;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("설정")]
    [Tooltip("마우스를 올렸을 때 얼마나 커질지 (1.2 = 20% 커짐)")]
    public float hoverScale = 1.2f;

    [Tooltip("크기가 변하는 속도 (초 단위)")]
    public float duration = 0.1f;

    private Vector3 _originalScale;
    private Coroutine _scaleCoroutine;

    private void Start()
    {
        // 시작할 때 원래 크기를 저장해둡니다.
        _originalScale = transform.localScale;
    }

    // 마우스가 버튼 위에 올라왔을 때 실행
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaling(_originalScale * hoverScale);
    }

    // 마우스가 버튼에서 나갔을 때 실행
    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaling(_originalScale);
    }

    // 크기 변경을 시작하는 헬퍼 함수
    private void StartScaling(Vector3 targetScale)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleProcess(targetScale));
    }

    // 부드럽게 크기를 변경하는 코루틴
    private IEnumerator ScaleProcess(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // UI는 보통 TimeScale 영향 안 받게 unscaled 사용
            float t = timer / duration;

            // SmoothStep을 쓰면 움직임이 더 부드러워집니다.
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale; // 오차 방지용으로 최종값 확정
    }

    // (선택사항) 버튼이 비활성화되면 코루틴 정지
    private void OnDisable()
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        transform.localScale = _originalScale;
    }
}