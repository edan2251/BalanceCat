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
    public TextMeshProUGUI questText1; // 일반 Text는 public Text questText1;

    [Tooltip("퀘스트 2 (도전) 텍스트 필드")]
    public TextMeshProUGUI questText2; // 일반 Text는 public Text questText2;

    [Tooltip("퀘스트 3 (탐험) 텍스트 필드")]
    public TextMeshProUGUI questText3; // 일반 Text는 public Text questText3;

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

        // 2. [수정] 시작 상태를 '열림'으로 설정
        isPanelOpen = true;
        panelAnimator.SetBool("isOpen", true); // 애니메이터 상태도 동기화

        // 3. [추가] n초 후에 자동으로 닫는 코루틴 시작
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
        // 1. 지정된 시간만큼 대기
        yield return new WaitForSeconds(delay);

        // 2. (중요) 3초가 지났는데 유저가 M키를 눌러서
        //    이미 닫지 않은 경우에만 닫습니다.
        if (isPanelOpen)
        {
            TogglePanel(); // 닫기 실행
        }
    }

    /// <summary>
    /// 퀘스트 패널을 열거나 닫습니다.
    /// </summary>
    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        panelAnimator.SetBool("isOpen", isPanelOpen);
    }

    /// <summary>
    /// StageData에서 퀘스트 정보를 가져와 UI 텍스트에 채웁니다.
    /// </summary>
    public void LoadQuestData()
    {
        if (currentStageData == null)
        {
            Debug.LogError("표시할 StageData가 할당되지 않았습니다!");
            return;
        }

        // --- 퀘스트 1 (메인) ---
        // SO가 할당되어 있는지 확인
        if (currentStageData.MainQuest != null)
        {
            questText1.text = $"{currentStageData.MainQuest.questTitle}: {currentStageData.MainQuest.questDescription}";
            questText1.gameObject.SetActive(true); // 텍스트 필드 활성화
        }
        else
        {
            questText1.gameObject.SetActive(false); // SO가 없으면 비활성화
        }

        // --- 퀘스트 2 (도전) ---
        if (currentStageData.quest2 != null)
        {
            questText2.text = $"{currentStageData.quest2.questTitle}: {currentStageData.quest2.questDescription}";
            questText2.gameObject.SetActive(true);
        }
        else
        {
            questText2.gameObject.SetActive(false);
        }

        // --- 퀘스트 3 (탐험) ---
        if (currentStageData.quest3 != null)
        {
            questText3.text = $"{currentStageData.quest3.questTitle}: {currentStageData.quest3.questDescription}";
            questText3.gameObject.SetActive(true);
        }
        else
        {
            questText3.gameObject.SetActive(false);
        }
    }
}