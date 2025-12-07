using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class QuestDisplayManager : MonoBehaviour
{
    // ... (기존 변수들: Header 1~5 그대로 유지) ...
    [Header("1. 스테이지 데이터")]
    public StageData currentStageData;

    [Header("2. UI 연결")]
    public TextMeshProUGUI questText1;
    public Image questStar1;
    public TextMeshProUGUI questText2;
    public Image questStar2;
    public TextMeshProUGUI questText3;
    public Image questStar3;

    [Header("3. 별 스프라이트")]
    public Sprite filledStar;
    public Sprite emptyStar;

    // ... (애니메이션, 튜토리얼 변수들 그대로 유지) ...
    [Header("4. 애니메이션")]
    public Animator panelAnimator;
    public float panelOpenDuration = 0.5f;

    [Header("5. 튜토리얼 가이드")]
    public CanvasGroup promptCanvasGroup;
    public float promptFadeDuration = 0.5f;

    private bool isPanelOpen = false;
    public float autoCloseDelay = 3.0f;
    private Coroutine _autoCloseCoroutine;
    private Coroutine _fadeCoroutine;

    private float _panelOpenStartTime = -999f;

    void Start()
    {
        if (panelAnimator == null) panelAnimator = GetComponent<Animator>();

        LoadQuestData();

        isPanelOpen = true;
        panelAnimator.SetBool("isOpen", true);

        _panelOpenStartTime = -999f;

        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.blocksRaycasts = false;
        }

        RestartAutoCloseTimer(autoCloseDelay);
    }

    void Update() { if (Input.GetKeyDown(KeyCode.M)) TogglePanel(); }

    public void RestartAutoCloseTimer(float delay)
    {
        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
        _autoCloseCoroutine = StartCoroutine(AutoCloseRoutine(delay));
    }
    private IEnumerator AutoCloseRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isPanelOpen) TogglePanel();
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelAnimator.SetBool("isOpen", isPanelOpen);

        if (isPanelOpen) _panelOpenStartTime = Time.time;

        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);

        float targetAlpha = isPanelOpen ? 0f : 1f;
        UpdatePromptAlpha(targetAlpha);
    }

    public void OpenPanelTemporary(float duration)
    {
        // 패널 열기
        if (!isPanelOpen)
        {
            isPanelOpen = true;
            panelAnimator.SetBool("isOpen", true);
            UpdatePromptAlpha(0f);

            _panelOpenStartTime = Time.time;
        }

        // 닫기 타이머만 연장 (기존 애니메이션 방해 안 함)
        RestartAutoCloseTimer(duration);
    }

    public void ShowQuestClearSequence(int questIndex, float displayTime)
    {
        float delay = 0f;

        // 1. 패널이 닫혀있으면 -> 열고 딜레이 추가
        if (!isPanelOpen)
        {
            isPanelOpen = true;
            panelAnimator.SetBool("isOpen", true);
            UpdatePromptAlpha(0f);
            
            _panelOpenStartTime = Time.time; // 여는 시간 기록
            delay = panelOpenDuration;       // 애니메이션 시간만큼 대기
        }
        else
        {
            // 2. 이미 열려있다면 -> "지금 열리는 중인지" 확인!
            float timeSinceOpen = Time.time - _panelOpenStartTime;
            
            if (timeSinceOpen < panelOpenDuration)
            {
                // 아직 열리는 중이라면, 남은 시간만큼 기다려야 함!
                delay = panelOpenDuration - timeSinceOpen;
                
                // 혹시 모르니 음수 방지
                if (delay < 0f) delay = 0f;
            }
        }

        // 3. 닫기 타이머 연장 (딜레이 + 보여줄 시간)
        RestartAutoCloseTimer(delay + displayTime);

        // 4. 별 애니메이션 실행 (계산된 딜레이 전달)
        StartCoroutine(AnimateStarRoutine(questIndex, delay));
    }

    private IEnumerator AnimateStarRoutine(int questIndex, float delay)
    {
        // [중요] 패널이 열리는 중이라면 다 열릴 때까지 대기
        if (delay > 0f) yield return new WaitForSeconds(delay);

        Image targetStar = GetStarImage(questIndex);
        if (targetStar != null && filledStar != null)
        {
            targetStar.sprite = filledStar;

            // DOTween 연출 (각 별마다 독립적으로 실행됨)
            targetStar.transform.DOKill();
            targetStar.transform.localScale = Vector3.one;
            targetStar.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 5, 1);
        }
    }

    /* 
    private IEnumerator QuestClearRoutine(int questIndex, float displayTime)
    {
        if (!isPanelOpen)
        {
            isPanelOpen = true;
            panelAnimator.SetBool("isOpen", true);
            UpdatePromptAlpha(0f);
            yield return new WaitForSeconds(panelOpenDuration);
        }

        Image targetStar = GetStarImage(questIndex);
        if (targetStar != null && filledStar != null)
        {
            targetStar.sprite = filledStar;
            targetStar.transform.DOKill();
            targetStar.transform.localScale = Vector3.one;
            targetStar.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 5, 1);
        }

        yield return new WaitForSeconds(displayTime);

        isPanelOpen = false;
        panelAnimator.SetBool("isOpen", false);
        UpdatePromptAlpha(1f);
    }
    */

    public void UpdateQuestRealtime(int questIndex, string newDescription, bool isStarActive)
    {
        TextMeshProUGUI targetText = null;
        if (questIndex == 0) targetText = questText1;
        else if (questIndex == 1) targetText = questText2;
        else if (questIndex == 2) targetText = questText3;

        if (targetText != null) targetText.text = newDescription;

        Image targetStar = GetStarImage(questIndex);
        if (targetStar != null)
        {
            targetStar.sprite = isStarActive ? filledStar : emptyStar;
        }
    }

    private Image GetStarImage(int index)
    {
        if (index == 0) return questStar1;
        if (index == 1) return questStar2;
        if (index == 2) return questStar3;
        return null;
    }

    void UpdatePromptAlpha(float targetAlpha)
    {
        if (promptCanvasGroup != null)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadePrompt(targetAlpha));
        }
    }

    private IEnumerator FadePrompt(float targetAlpha)
    {
        float startAlpha = promptCanvasGroup.alpha;
        float timer = 0f;
        while (timer < promptFadeDuration)
        {
            timer += Time.deltaTime;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / promptFadeDuration);
            yield return null;
        }
        promptCanvasGroup.alpha = targetAlpha;
    }

    public void LoadQuestData()
    {
        // 초기 로드는 RefreshUI에서 덮어씌워질 것이므로 기본만 설정
        if (currentStageData == null) return;
        // ... (기존 SetQuestUI 로직 유지하되, 별은 기본적으로 empty로 시작)
        SetQuestUI(0, questText1, questStar1);
        SetQuestUI(1, questText2, questStar2);
        SetQuestUI(2, questText3, questStar3);
    }

    void SetQuestUI(int index, TextMeshProUGUI textUI, Image starUI)
    {
        if (currentStageData.quests != null && currentStageData.quests.Count > index && currentStageData.quests[index] != null)
        {
            textUI.text = $"{currentStageData.quests[index].questTitle}\n{currentStageData.quests[index].questDescription}";
            textUI.gameObject.SetActive(true);
            if (starUI != null) { starUI.sprite = emptyStar; starUI.gameObject.SetActive(true); }
        }
        else
        {
            textUI.gameObject.SetActive(false);
            if (starUI != null) starUI.gameObject.SetActive(false);
        }
    }

    // [추가] 퀘스트 타이틀만 가져오기 편하게 (옵션)
    public void UpdateQuestState(int questIndex, bool isCleared) { /*기존 호환용, 안써도 됨*/ }
}