using UnityEngine;
using UnityEngine.UI; // UI 사용을 위해 필수
using System;

public class StageUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject mainUIPanel;

    // === UI 요소 연결 (인스펙터에서 할당) ===
    [Header("UI Text Fields")]
    public Text chapterNameText;     // [ 챕터명 ]
    public Text stageTitleText;      // [ 1 스테이지 ]
    public Text stageSynopsisText;   // 스테이지 시놉시스
    public Text questText;           // 주어진 퀘스트
    public Text rewardText;          // 보상 정보

    [Header("UI Button")]
    public Button startButton;       // 시작 버튼

    [Header("참조")]
    public ChapterSelector chapterSelector; 


    void Start()
    {
        if (chapterSelector == null)
        {
            chapterSelector = FindObjectOfType<ChapterSelector>();
        }

        if (chapterSelector == null || !chapterSelector.IsChapterSelectionActive())
        {
            gameObject.SetActive(false);
        }
    }


    // StageSelector가 호출하여 UI 정보를 업데이트하는 함수
    public void UpdateStageInfo(StageData data)
    {
        gameObject.SetActive(true);

        if (data == null)
        {
            Debug.LogError("StageData가 NULL입니다. UI 업데이트를 건너뜁니다.");
            // 데이터가 없으면 UI를 숨깁니다.
            chapterNameText.text = "데이터 오류";
            stageTitleText.text = "";
            gameObject.SetActive(false);
            return;
        }

        // === 데이터 기반으로 UI 텍스트 업데이트 ===

        // 1. 챕터명 및 스테이지명
        chapterNameText.text = $"{data.chapterName}";
        stageTitleText.text = $"{data.stageTitle}";

        // 2. 상세 정보
        stageSynopsisText.text = data.synopsis;
        questText.text = $"{data.questDescription}";
        rewardText.text = $"{data.rewardName}";

        // 3. 시작 버튼 리스너 업데이트
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => StartStage(data));
    }

    // UI 숨기기 (예: 챕터 선택 모드를 빠져나갈 때 ChapterSelector에서 호출)
    public void HideUI()
    {
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(false);
        }
    }

    private void StartStage(StageData data)
    {
        Debug.Log($"스테이지 시작: 챕터 {data.chapterName}, ID {data.stageID}");

        // TODO:
        // 1. ChapterSelector를 통해 챕터 선택 모드 비활성화 (카메라 원래 위치 복귀 등)
        // 2. 실제 게임 씬 로드 로직 (SceneManager.LoadScene 등) 추가
    }
}