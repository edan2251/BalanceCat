using UnityEngine;
using TMPro;
using System.Collections;

public class QuestDisplayManager : MonoBehaviour
{
    [Header("1. 스테이지 데이터")]
    public StageData currentStageData;

    [Header("2. UI 연결")]
    public TextMeshProUGUI questText1;
    public TextMeshProUGUI questText2;
    public TextMeshProUGUI questText3;

    [Header("3. 애니메이션")]
    public Animator panelAnimator;

    [Header("4. 튜토리얼 가이드")]
    // [수정] GameObject 대신 CanvasGroup을 사용하여 투명도 조절
    [Tooltip("'M 키를 누르세요' 오브젝트에 CanvasGroup 컴포넌트를 붙이고 여기에 연결하세요")]
    public CanvasGroup promptCanvasGroup;

    [Tooltip("안내 문구가 페이드되는 시간")]
    public float promptFadeDuration = 0.5f;

    private bool isPanelOpen = false;
    public float autoCloseDelay = 3.0f;

    // 페이드 코루틴 충돌 방지용 변수
    private Coroutine _fadeCoroutine;

    void Start()
    {
        if (panelAnimator == null) panelAnimator = GetComponent<Animator>();

        LoadQuestData();

        isPanelOpen = true;
        panelAnimator.SetBool("isOpen", true);

        // [수정] 시작할 땐 패널이 열려있으므로 안내 문구 투명하게(0) 설정
        if (promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            // 상호작용 차단 (클릭 방지 등)
            promptCanvasGroup.blocksRaycasts = false;
        }

        StartCoroutine(AutoCloseAfterDelay(autoCloseDelay));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            TogglePanel();
        }
    }

    private IEnumerator AutoCloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (isPanelOpen)
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelAnimator.SetBool("isOpen", isPanelOpen);

        // [수정] 패널 상태에 따라 페이드 효과 실행
        // 패널이 열리면(true) -> 문구 사라짐(Target Alpha 0)
        // 패널이 닫히면(false) -> 문구 나타남(Target Alpha 1)
        float targetAlpha = isPanelOpen ? 0f : 1f;

        if (promptCanvasGroup != null)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadePrompt(targetAlpha));
        }
    }

    // [신규] 안내 문구 페이드 코루틴
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

        // 완전히 꺼졌을 때는 레이캐스트 차단 (선택 사항)
        // promptCanvasGroup.blocksRaycasts = (targetAlpha > 0.9f);
    }

    public void LoadQuestData()
    {
        // (기존 코드와 동일하여 생략)
        if (currentStageData == null) { Debug.LogError("No StageData!"); return; }

        if (currentStageData.quests != null && currentStageData.quests.Count > 0)
        {
            questText1.text = $"{currentStageData.quests[0].questTitle}\n{currentStageData.quests[0].questDescription}";
            questText1.gameObject.SetActive(true);
        }
        else questText1.gameObject.SetActive(false);

        if (currentStageData.quests != null && currentStageData.quests.Count > 1)
        {
            questText2.text = $"{currentStageData.quests[1].questTitle}\n{currentStageData.quests[1].questDescription}";
            questText2.gameObject.SetActive(true);
        }
        else questText2.gameObject.SetActive(false);

        if (currentStageData.quests != null && currentStageData.quests.Count > 2)
        {
            questText3.text = $"{currentStageData.quests[2].questTitle}\n{currentStageData.quests[2].questDescription}";
            questText3.gameObject.SetActive(true);
        }
        else questText3.gameObject.SetActive(false);
    }
}