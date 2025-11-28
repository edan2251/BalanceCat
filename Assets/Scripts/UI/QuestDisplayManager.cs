using UnityEngine;
using TMPro;
using System.Collections;

public class QuestDisplayManager : MonoBehaviour
{
    [Header("1. 스테이지 데이터")]
    [Tooltip("표시할 퀘스트 정보가 담긴 StageData SO")]
    public StageData currentStageData;

    [Header("2. UI 연결")]
    [Tooltip("퀘스트 1 (메인) 텍스트 필드")]
    public TextMeshProUGUI questText1;

    [Tooltip("퀘스트 2 (도전) 텍스트 필드")]
    public TextMeshProUGUI questText2;

    [Tooltip("퀘스트 3 (탐험) 텍스트 필드")]
    public TextMeshProUGUI questText3;

    [Header("3. 애니메이션")]
    [Tooltip("QuestPanel에 붙어있는 Animator")]
    public Animator panelAnimator;

    // 패널의 현재 열림/닫힘 상태
    private bool isPanelOpen = false;

    public float autoCloseDelay = 3.0f;

    void Start()
    {
        if (panelAnimator == null)
        {
            panelAnimator = GetComponent<Animator>();
        }

        // 1. 퀘스트 데이터부터 로드
        LoadQuestData();

        // 2. 시작 상태를 '열림'으로 설정
        isPanelOpen = true;
        panelAnimator.SetBool("isOpen", true);

        // 3. n초 후에 자동으로 닫는 코루틴 시작
        StartCoroutine(AutoCloseAfterDelay(autoCloseDelay));
    }

    void Update()
    {
        // M 키를 눌렀을 때
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
            TogglePanel(); // 닫기 실행
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelAnimator.SetBool("isOpen", isPanelOpen);
    }

    /// <summary>
    /// StageData에서 퀘스트 정보를 가져와 UI 텍스트에 채웁니다. (리스트 방식 수정됨)
    /// </summary>
    public void LoadQuestData()
    {
        if (currentStageData == null)
        {
            Debug.LogError("표시할 StageData가 할당되지 않았습니다!");
            return;
        }

        // --- 퀘스트 1 (메인) : 리스트 인덱스 0 ---
        // 리스트가 존재하고, 0번 요소가 있는지 확인
        if (currentStageData.quests != null && currentStageData.quests.Count > 0 && currentStageData.quests[0] != null)
        {
            QuestData q1 = currentStageData.quests[0];
            questText1.text = $"{q1.questTitle}\n{q1.questDescription}";
            questText1.gameObject.SetActive(true);
        }
        else
        {
            questText1.gameObject.SetActive(false);
        }

        // --- 퀘스트 2 (도전) : 리스트 인덱스 1 ---
        if (currentStageData.quests != null && currentStageData.quests.Count > 1 && currentStageData.quests[1] != null)
        {
            QuestData q2 = currentStageData.quests[1];
            questText2.text = $"{q2.questTitle}\n{q2.questDescription}";
            questText2.gameObject.SetActive(true);
        }
        else
        {
            questText2.gameObject.SetActive(false);
        }

        // --- 퀘스트 3 (탐험) : 리스트 인덱스 2 ---
        if (currentStageData.quests != null && currentStageData.quests.Count > 2 && currentStageData.quests[2] != null)
        {
            QuestData q3 = currentStageData.quests[2];
            questText3.text = $"{q3.questTitle}\n{q3.questDescription}";
            questText3.gameObject.SetActive(true);
        }
        else
        {
            questText3.gameObject.SetActive(false);
        }
    }
}