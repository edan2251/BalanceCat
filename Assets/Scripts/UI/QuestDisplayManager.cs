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

    void Start()
    {
        if (panelAnimator == null) panelAnimator = GetComponent<Animator>();

        // [중요] Start에서는 LoadQuestData 대신 초기화만 하고, 
        // 실제 데이터 표시는 InGameQuestManager가 RefreshUI를 호출하면서 처리하도록 유도할 수도 있습니다.
        // 하지만 안전을 위해 일단 기본 로드.
        LoadQuestData();

        isPanelOpen = true;
        panelAnimator.SetBool("isOpen", true);

        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.blocksRaycasts = false;
        }

        _autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay(autoCloseDelay));
    }

    // ... (Update, TogglePanel, OpenPanelTemporary, ShowQuestClearSequence 등 기존 함수 그대로 유지) ...
    void Update() { if (Input.GetKeyDown(KeyCode.M)) TogglePanel(); }

    private IEnumerator AutoCloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isPanelOpen) TogglePanel();
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelAnimator.SetBool("isOpen", isPanelOpen);
        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
        float targetAlpha = isPanelOpen ? 0f : 1f;
        UpdatePromptAlpha(targetAlpha);
    }

    public void ShowQuestClearSequence(int questIndex, float displayTime)
    {
        if (_autoCloseCoroutine != null) StopCoroutine(_autoCloseCoroutine);
        _autoCloseCoroutine = StartCoroutine(QuestClearRoutine(questIndex, displayTime));
    }

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

    // ========================================================================
    // [신규] 실시간으로 텍스트와 별 상태를 갱신하는 함수
    // ========================================================================
    public void UpdateQuestRealtime(int questIndex, string newDescription, bool isStarActive)
    {
        // 1. 텍스트 갱신
        TextMeshProUGUI targetText = null;
        if (questIndex == 0) targetText = questText1;
        else if (questIndex == 1) targetText = questText2;
        else if (questIndex == 2) targetText = questText3;

        if (targetText != null)
        {
            // 타이틀은 유지하고 내용만 바꾸거나, 통째로 받은 문자열로 교체
            // 여기서는 InGameQuestManager가 조립해서 준 문자열을 그대로 넣음
            targetText.text = newDescription;
        }

        // 2. 별 상태 갱신 (성공/실패 여부에 따라 즉시 교체)
        Image targetStar = GetStarImage(questIndex);
        if (targetStar != null)
        {
            targetStar.sprite = isStarActive ? filledStar : emptyStar;
        }
    }
    // ========================================================================

    // ... (헬퍼 함수들 유지) ...
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