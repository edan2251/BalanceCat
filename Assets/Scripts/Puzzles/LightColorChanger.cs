using UnityEngine;
using System.Collections;

public class LightColorChanger : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("변경할 목표 색상 (파란색)")]
    public Color targetColor = Color.blue;

    [Tooltip("색상이 변하는 데 걸리는 시간 (초)")]
    public float duration = 0.5f;

    [Header("참조 (비워두면 내꺼 씀)")]
    public Light myLight;

    private Color _originalColor;
    private Coroutine _colorCoroutine;

    private void Awake()
    {
        if (myLight == null) myLight = GetComponent<Light>();

        // 시작할 때 원래 색상을 기억해둠 (나중에 되돌리기 위해)
        if (myLight != null)
        {
            _originalColor = myLight.color;
        }
    }

    // --- 외부(이벤트)에서 부를 함수들 ---

    // 1. 파란색(목표 색)으로 변경
    public void ChangeToTargetColor()
    {
        if (myLight == null) return;

        // 실행 중인 코루틴이 있다면 끄고 새로 시작
        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(ColorTransitionProcess(targetColor));
    }

    // 2. 원래 색으로 복귀
    public void ResetToOriginalColor()
    {
        if (myLight == null) return;

        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(ColorTransitionProcess(_originalColor));
    }

    // --- 부드러운 전환 로직 ---
    private IEnumerator ColorTransitionProcess(Color endColor)
    {
        Color startColor = myLight.color;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Lerp를 사용해 부드럽게 색상 섞기
            myLight.color = Color.Lerp(startColor, endColor, timer / duration);
            yield return null;
        }

        // 끝까지 확실하게 적용
        myLight.color = endColor;
    }
}