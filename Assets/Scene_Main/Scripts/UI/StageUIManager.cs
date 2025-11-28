using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMPro 사용
using System;
using System.Collections.Generic; // [추가] List 사용을 위해 추가

public class StageUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject mainUIPanel;

    // === UI 요소 연결 (인스펙터에서 할당) ===
    [Header("UI Text Fields")]
    public TextMeshProUGUI chapterNameText;
    public TextMeshProUGUI stageSynopsisText;

    [Header("UI Button")]
    public Button startButton;

    // --- [추가] 스테이지 번호 이미지 ---
    [Header("Stage Number Images")]
    [Tooltip("스테이지 번호에 따라 활성화될 이미지 리스트 (1번=인덱스0, 2번=인덱스1...)")]
    public List<Image> stageNumberImages;
    // --- [추가] 끝 ---

    // --- 퀘스트 및 별 UI 요소 ---
    [Header("Quest & Star UI")]
    [Tooltip("퀘스트 1 (메인) 텍스트")]
    public TextMeshProUGUI quest1Text;
    [Tooltip("퀘스트 1 (메인) 별 이미지")]
    public Image quest1Star;
    [Tooltip("퀘스트 2 (도전) 텍스트")]
    public TextMeshProUGUI quest2Text;
    [Tooltip("퀘스트 2 (도전) 별 이미지")]
    public Image quest2Star;
    [Tooltip("퀘스트 3 (탐험) 텍스트")]
    public TextMeshProUGUI quest3Text;
    [Tooltip("퀘스트 3 (탐험) 별 이미지")]
    public Image quest3Star;

    [Header("Star Sprites")]
    [Tooltip("별 획득 시 표시할 스프라이트")]
    public Sprite starFilledSprite;
    [Tooltip("별 미획득 시 표시할 스프라이트")]
    public Sprite starEmptySprite;


    [Header("참조")]
    public ChapterSelector chapterSelector;
    private StageSelector currentStageSelector;


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
        stageSynopsisText.text = "이전 챕터의 마지막 스테이지를 클리어하면 해금됩니다.";

        startButton.interactable = false;
        startButton.onClick.RemoveAllListeners();

        // [추가] 스테이지 번호 이미지 모두 숨기기
        ToggleStageNumberImages(false, 0);

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

        if (chapterSelector != null)
        {
            currentStageSelector = chapterSelector.GetCurrentStageSelector();
        }

        if (data == null)
        {
            Debug.LogError("StageData가 NULL입니다.");
            chapterNameText.text = "데이터 오류";
            gameObject.SetActive(false);
            return;
        }

        // 1. 기본 정보 업데이트
        chapterNameText.text = $"{data.chapterName}{"_"}";

        // 2. 시작 버튼 및 시놉시스
        startButton.onClick.RemoveAllListeners();
        if (isPlayable)
        {
            stageSynopsisText.text = data.synopsis;

            startButton.interactable = true;
            startButton.onClick.AddListener(() => StartStage(data));

            // [추가] 스테이지 번호에 맞는 이미지 표시
            ToggleStageNumberImages(true, data.stageID);
        }
        else
        {
            stageSynopsisText.text = "이전 스테이지를 클리어해야 플레이할 수 있습니다.";
            startButton.interactable = false;

            // [추가] 스테이지 번호 이미지 모두 숨기기
            ToggleStageNumberImages(false, 0);
        }

        // 3. 퀘스트 UI 업데이트
        if (currentStageSelector != null)
        {
            if (isPlayable)
            {
                ToggleQuestUI(true, data, currentStageSelector.chapterIndex, data.stageID);
            }
            else
            {
                ToggleQuestUI(false, null, 0, 0);
            }
        }
        else
        {
            Debug.LogError("CurrentStageSelector 참조가 없습니다. 퀘스트 UI를 업데이트할 수 없습니다.");
            ToggleQuestUI(false, null, 0, 0);
        }
    }

    // --- [추가] 스테이지 번호 이미지를 관리하는 함수 ---
    /// <summary>
    /// 모든 스테이지 번호 이미지를 끄고, 지정된 ID의 이미지만 켭니다.
    /// </summary>
    /// <param name="show">true면 해당 이미지를 켜고, false면 모두 끕니다.</param>
    /// <param name="stageID">활성화할 스테이지 ID (1부터 시작).</param>
    private void ToggleStageNumberImages(bool show, int stageID)
    {
        if (stageNumberImages == null || stageNumberImages.Count == 0)
        {
            return; // 리스트가 비어있으면 아무것도 안 함
        }

        // 1. 먼저 모든 이미지를 끈다.
        foreach (Image img in stageNumberImages)
        {
            if (img != null)
            {
                img.gameObject.SetActive(false);
            }
        }

        // 2. show가 true이고 stageID가 유효한 범위 내에 있을 때만 해당 이미지를 켠다.

        // [중요] stageID는 1부터 시작, 리스트 인덱스는 0부터 시작하므로 (stageID - 1)로 접근.
        int index = stageID - 1;

        if (show && index >= 0 && index < stageNumberImages.Count)
        {
            if (stageNumberImages[index] != null)
            {
                stageNumberImages[index].gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"StageNumberImages 리스트의 {index}번 인덱스(Stage ID: {stageID})가 비어있습니다.");
            }
        }
        else if (show)
        {
            Debug.LogWarning($"유효하지 않은 Stage ID({stageID}) 또는 stageNumberImages 리스트에 해당 인덱스({index})가 없습니다.");
        }
    }

    // --- 퀘스트 UI 관리 함수 (이전과 동일) ---
    private void ToggleQuestUI(bool show, StageData data, int chapterIndex, int stageID)
    {
        quest1Text?.gameObject.SetActive(show);
        quest1Star?.gameObject.SetActive(show);
        quest2Text?.gameObject.SetActive(show);
        quest2Star?.gameObject.SetActive(show);
        quest3Text?.gameObject.SetActive(show);
        quest3Star?.gameObject.SetActive(show);

        if (show && data != null)
        {
            // 퀘스트 1 (Main)
            if (data.quests.Count > 0)
                UpdateQuestSlot(quest1Text, quest1Star, data.quests[0],
                    GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 1));

            // 퀘스트 2
            if (data.quests.Count > 1)
                UpdateQuestSlot(quest2Text, quest2Star, data.quests[1],
                    GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 2));

            // 퀘스트 3
            if (data.quests.Count > 2)
                UpdateQuestSlot(quest3Text, quest3Star, data.quests[2],
                    GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 3));
        }
    }

    private void UpdateQuestSlot(TextMeshProUGUI questText, Image starImage, QuestData questData, bool isCompleted)
    {
        if (questData != null)
        {
            questText.text = $"{questData.questTitle}\n{questData.questDescription}";
            questText.gameObject.SetActive(true);
            starImage.gameObject.SetActive(true);
            starImage.sprite = isCompleted ? starFilledSprite : starEmptySprite;
        }
        else
        {
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
        // [변경] HideUI 호출 시 스테이지 번호 이미지도 확실히 끈다.
        ToggleStageNumberImages(false, 0);
    }

    private void StartStage(StageData data)
    {
        Debug.Log($"스테이지 시작: 챕터 {data.chapterName}, ID {data.stageID}. 로드할 씬: {data.targetSceneName}");

        if (!string.IsNullOrEmpty(data.targetSceneName))
        {
            // [수정] 문자열 대신 StageData 객체 자체를 넘깁니다.
            LoadingSceneController.LoadScene(data);
        }
        else
        {
            Debug.LogError($"StageData (ID: {data.stageID})에 유효한 씬 이름이 설정되지 않았습니다!");
        }
    }
}