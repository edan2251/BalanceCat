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

    // 일정 시간 뒤에 사라지는 메시지
    public void ShowMessage(string message, float duration = 10f)
    {
        if (textMeshPro == null || bubbleRoot == null) return;

        textMeshPro.text = message;
        SetTextAlpha(1f);
        bubbleRoot.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideProcess(duration));
    }

    // ---------------------------------------------------------
    //구역 안에 있을 때 계속 띄우기 위한 함수들
    // ---------------------------------------------------------

    // 사라지지 않고 계속 떠있는 메시지 (점수 갱신용)
    public void ShowContinuousMessage(string message)
    {
        if (textMeshPro == null || bubbleRoot == null) return;

        textMeshPro.text = message;

        SetTextAlpha(1f);
        bubbleRoot.SetActive(true);

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    public void HideContinuousMessage(bool fadeOut = true)
    {
        if (fadeOut)
        {
            if (hideCoroutine != null) StopCoroutine(hideCoroutine);
            hideCoroutine = StartCoroutine(HideProcess(0f));
        }
        else
        {
            if (bubbleRoot != null) bubbleRoot.SetActive(false);
        }
    }

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
    }

    private void SetTextAlpha(float alpha)
    {
        if (textMeshPro != null) textMeshPro.alpha = alpha;
    }
}