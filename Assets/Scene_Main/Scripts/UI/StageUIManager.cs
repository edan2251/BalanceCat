using UnityEngine;
using UnityEngine.UI; // UI 사용을 위해 필수
using System;

public class StageUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject mainUIPanel;

    // === UI 요소 연결 (인스펙터에서 할당) ===
    [Header("UI Text Fields")]
    public Text chapterNameText;
    public Text stageTitleText;
    public Text stageSynopsisText;

    [Header("UI Button")]
    public Button startButton;

    // --- NEW: 퀘스트 및 별 UI 요소 ---
    [Header("Quest & Star UI")]
    [Tooltip("퀘스트 1 (메인) 텍스트")]
    public Text quest1Text;
    [Tooltip("퀘스트 1 (메인) 별 이미지")]
    public Image quest1Star;

    [Tooltip("퀘스트 2 (도전) 텍스트")]
    public Text quest2Text;
    [Tooltip("퀘스트 2 (도전) 별 이미지")]
    public Image quest2Star;

    [Tooltip("퀘스트 3 (탐험) 텍스트")]
    public Text quest3Text;
    [Tooltip("퀘스트 3 (탐험) 별 이미지")]
    public Image quest3Star;

    [Header("Star Sprites")]
    [Tooltip("별 획득 시 표시할 스프라이트")]
    public Sprite starFilledSprite;
    [Tooltip("별 미획득 시 표시할 스프라이트")]
    public Sprite starEmptySprite;
    // --- END NEW ---


    [Header("참조")]
    public ChapterSelector chapterSelector;
    private StageSelector currentStageSelector; // 챕터 인덱스를 가져오기 위해 참조


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

    /// <summary>
    /// 챕터가 잠겨있음을 UI에 표시합니다.
    /// </summary>
    public void ShowChapterLockedMessage(string chapterName)
    {
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        gameObject.SetActive(true);

        chapterNameText.text = $"{chapterName}";
        stageTitleText.text = "챕터 잠김";
        stageSynopsisText.text = "이전 챕터의 마지막 스테이지를 클리어하면 해금됩니다.";

        startButton.interactable = false;
        startButton.onClick.RemoveAllListeners();

        // 퀘스트 UI 숨기기 또는 비활성화
        ToggleQuestUI(false, null, 0, 0);
    }


    /// <summary>
    /// StageSelector가 호출하여 UI 정보를 업데이트하는 함수
    /// </summary>
    public void UpdateStageInfo(StageData data, bool isPlayable)
    {
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        gameObject.SetActive(true);

        // --- NEW: StageSelector로부터 챕터 인덱스 가져오기 ---
        if (chapterSelector != null)
        {
            // (ChapterSelector가 현재 StageSelector를 들고 있음)
            currentStageSelector = chapterSelector.GetCurrentStageSelector();
        }
        // --- END NEW ---

        if (data == null)
        {
            Debug.LogError("StageData가 NULL입니다.");
            chapterNameText.text = "데이터 오류";
            stageTitleText.text = "";
            gameObject.SetActive(false);
            return;
        }

        // 1. 기본 정보 업데이트
        chapterNameText.text = $"{data.chapterName}";
        stageTitleText.text = $"{data.stageTitle}";

        // 2. 시작 버튼 및 시놉시스
        startButton.onClick.RemoveAllListeners();
        if (isPlayable)
        {
            stageSynopsisText.text = data.synopsis;

            startButton.interactable = true;
            startButton.onClick.AddListener(() => StartStage(data));
        }
        else
        {
            stageSynopsisText.text = "이전 스테이지를 클리어해야 플레이할 수 있습니다.";
            startButton.interactable = false;
        }

        // --- NEW: 퀘스트 UI 업데이트 ---
        if (currentStageSelector != null)
        {
            ToggleQuestUI(true, data, currentStageSelector.chapterIndex, data.stageID);
        }
        else
        {
            Debug.LogError("CurrentStageSelector 참조가 없습니다. 퀘스트 UI를 업데이트할 수 없습니다.");
            ToggleQuestUI(false, null, 0, 0);
        }
        // --- END NEW ---
    }

    // --- NEW: 퀘스트 UI를 켜고 끄거나 내용을 업데이트하는 함수 ---
    private void ToggleQuestUI(bool show, StageData data, int chapterIndex, int stageID)
    {
        // UI 오브젝트의 부모를 켜고 끔 (가정)
        quest1Text?.gameObject.SetActive(show);
        quest1Star?.gameObject.SetActive(show);
        quest2Text?.gameObject.SetActive(show);
        quest2Star?.gameObject.SetActive(show);
        quest3Text?.gameObject.SetActive(show);
        quest3Star?.gameObject.SetActive(show);

        if (show && data != null && starFilledSprite != null && starEmptySprite != null)
        {
            // 퀘스트 1 (MainQuest)
            UpdateQuestSlot(quest1Text, quest1Star, data.MainQuest,
                GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 1));

            // 퀘스트 2 (quest2)
            UpdateQuestSlot(quest2Text, quest2Star, data.quest2,
                GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 2));

            // 퀘스트 3 (quest3)
            UpdateQuestSlot(quest3Text, quest3Star, data.quest3,
                GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 3));
        }
    }

    // --- NEW: 개별 퀘스트 슬롯(텍스트, 별)을 업데이트하는 헬퍼 함수 ---
    private void UpdateQuestSlot(Text questText, Image starImage, QuestData questData, bool isCompleted)
    {
        if (questData != null)
        {
            // [수정] questTitle과 questDescription을 함께 표시
            questText.text = $"{questData.questTitle}\n{questData.questDescription}";

            questText.gameObject.SetActive(true);
            starImage.gameObject.SetActive(true);

            // 별 이미지 업데이트
            starImage.sprite = isCompleted ? starFilledSprite : starEmptySprite;
        }
        else
        {
            // SO가 할당되지 않은 슬롯은 숨김
            questText.gameObject.SetActive(false);
            starImage.gameObject.SetActive(false);
        }
    }


    public void HideUI()
    {
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(false);
        }
    }

    private void StartStage(StageData data)
    {
        Debug.Log($"스테이지 시작: 챕터 {data.chapterName}, ID {data.stageID}. 로드할 씬: {data.targetSceneName}");
        if (!string.IsNullOrEmpty(data.targetSceneName))
        {
            LoadingSceneController.LoadScene(data.targetSceneName);
        }
        else
        {
            Debug.LogError($"StageData (ID: {data.stageID})에 유효한 씬 이름이 설정되지 않았습니다!");
        }
    }
}