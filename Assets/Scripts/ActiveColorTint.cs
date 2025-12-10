using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveColorTint : MonoBehaviour
{
    [Header("Colors")]
    public Color normalColor = Color.white; // 평소 색 (흰색)
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 잠겼을 때 색 (회색)

    [Header("Settings")]
    public float duration = 0.5f; // 색이 변하는 시간

    // 최적화를 위한 변수들
    private MaterialPropertyBlock _propBlock;
    private Renderer[] _renderers;
    private Coroutine _colorCoroutine;

    void Awake()
    {
        // 시작할 때 미리 컴포넌트들을 찾아둡니다.
        _propBlock = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>();
    }

    void OnEnable()
    {
        // 켜질 때 초기화
        ApplyColorImmediate(normalColor);
    }

    // 에디터에서 값 바꿀 때 바로 반영 (기존 기능 유지)
    void OnValidate()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderers = GetComponentsInChildren<Renderer>();
        ApplyColorImmediate(normalColor);
    }

    // --- 외부에서 호출할 함수들 (이벤트 연결용) ---

    public void SetLockedColor()
    {
        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(SmoothColorChange(lockedColor));
    }

    public void SetNormalColor()
    {
        if (_colorCoroutine != null) StopCoroutine(_colorCoroutine);
        _colorCoroutine = StartCoroutine(SmoothColorChange(normalColor));
    }

    // --- 내부 로직 ---

    // 즉시 색상 변경 (초기화용)
    private void ApplyColorImmediate(Color targetColor)
    {
        if (_renderers == null) return;

        foreach (Renderer r in _renderers)
        {
            r.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_BaseColor", targetColor); // URP 기준 이름
            r.SetPropertyBlock(_propBlock);
        }
    }

    // 부드러운 색상 변경 코루틴
    private IEnumerator SmoothColorChange(Color targetColor)
    {
        // 현재 색상을 알기 위해 첫 번째 렌더러의 정보를 가져옴 (다 같다고 가정)
        Color startColor = normalColor;
        if (_renderers.Length > 0)
        {
            _renderers[0].GetPropertyBlock(_propBlock);
            // _BaseColor 프로퍼티가 없으면 기본값 사용
            startColor = _propBlock.GetColor("_BaseColor");
            // 만약 PropertyBlock에 색이 아직 세팅 안된 상태라면(Color.clear 등), normalColor로 안전하게 시작
            if (startColor.a == 0) startColor = normalColor;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // 색상 보간 (Lerp)
            Color currentColor = Color.Lerp(startColor, targetColor, t);

            // 모든 렌더러에 적용
            foreach (Renderer r in _renderers)
            {
                r.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_BaseColor", currentColor);
                r.SetPropertyBlock(_propBlock);
            }

            yield return null;
        }

        // 최종 색상 확실하게 적용
        ApplyColorImmediate(targetColor);
    }
}