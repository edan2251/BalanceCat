using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StageUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject mainUIPanel;

    [Header("UI Text Fields")]
    public TextMeshProUGUI chapterNameText; // 여기에 "챕터이름 - Stage N" 형식으로 표시
    public TextMeshProUGUI stageSynopsisText;

    [Header("UI Button")]
    public Button startButton;

    // [삭제됨] public List<Image> stageNumberImages; -> 더 이상 필요 없음

    [Header("Quest & Star UI")]
    public TextMeshProUGUI quest1Text;
    public Image quest1Star;
    public TextMeshProUGUI quest2Text;
    public Image quest2Star;
    public TextMeshProUGUI quest3Text;
    public Image quest3Star;

    [Header("Star Sprites")]
    public Sprite starFilledSprite;
    public Sprite starEmptySprite;

    [Header("참조")]
    public ChapterSelector chapterSelector;
    private StageSelector currentStageSelector;

    [Header("Lock State Texts")]
    [TextArea] public string chapterLockedText = "이전 챕터를 클리어하여 해금하세요.";
    [TextArea] public string stageLockedText = "이전 스테이지를 클리어해야\n플레이할 수 있습니다.";

    void Start()
    {
        if (chapterSelector == null) chapterSelector = FindObjectOfType<ChapterSelector>();
        if (chapterSelector == null || !chapterSelector.IsChapterSelectionActive()) gameObject.SetActive(false);
    }

    // 1. 챕터 자체가 잠겼을 때
    public void ShowChapterLockedMessage(string chapterName)
    {
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        gameObject.SetActive(true);

        // 잠긴 챕터는 이름만 표시 (스테이지 번호는 의미 없으므로)
        chapterNameText.text = $"{chapterName}";

        stageSynopsisText.text = chapterLockedText;

        startButton.interactable = false;
        startButton.onClick.RemoveAllListeners();

        ToggleQuestUI(false, null, 0, 0);
    }

    // 2. 챕터는 열려있고, 스테이지 정보를 갱신할 때
    public void UpdateStageInfo(StageData data, bool isPlayable)
    {
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        gameObject.SetActive(true);

        if (chapterSelector != null) currentStageSelector = chapterSelector.GetCurrentStageSelector();

        if (data == null) return;

        // --- [핵심 수정] 텍스트 하나에 "챕터이름 - Stage 번호" 형태로 합쳐서 표시 ---
        // 예: "숲속 마을 - Stage 1"
        chapterNameText.text = $"{data.chapterName} - {data.stageID}";

        startButton.onClick.RemoveAllListeners();

        // --- 상태별 분기 ---
        if (isPlayable)
        {
            // [상태 1: 플레이 가능]
            stageSynopsisText.text = data.synopsis;
            startButton.interactable = true;
            startButton.onClick.AddListener(() => StartStage(data));
        }
        else
        {
            // [상태 2: 스테이지 잠김]
            stageSynopsisText.text = stageLockedText;
            startButton.interactable = false;
        }

        // 퀘스트 UI 업데이트
        if (currentStageSelector != null)
        {
            if (isPlayable)
                ToggleQuestUI(true, data, currentStageSelector.chapterIndex, data.stageID);
            else
                ToggleQuestUI(false, null, 0, 0);
        }
    }

    // [삭제됨] ToggleStageNumberImages 함수 삭제

    private void ToggleQuestUI(bool show, StageData data, int chapterIndex, int stageID)
    {
        // (기존 코드와 동일)
        quest1Text?.gameObject.SetActive(show); quest1Star?.gameObject.SetActive(show);
        quest2Text?.gameObject.SetActive(show); quest2Star?.gameObject.SetActive(show);
        quest3Text?.gameObject.SetActive(show); quest3Star?.gameObject.SetActive(show);

        if (show && data != null)
        {
            if (data.quests.Count > 0) UpdateQuestSlot(quest1Text, quest1Star, data.quests[0], GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 1));
            if (data.quests.Count > 1) UpdateQuestSlot(quest2Text, quest2Star, data.quests[1], GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 2));
            if (data.quests.Count > 2) UpdateQuestSlot(quest3Text, quest3Star, data.quests[2], GameProgressManager.IsQuestCompleted(chapterIndex, stageID, 3));
        }
    }

    private void UpdateQuestSlot(TextMeshProUGUI questText, Image starImage, QuestData questData, bool isCompleted)
    {
        // (기존 코드와 동일)
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
        if (mainUIPanel != null) mainUIPanel.SetActive(false);
    }

    private void StartStage(StageData data)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }
        if (!string.IsNullOrEmpty(data.targetSceneName)) LoadingSceneController.LoadScene(data);
        else Debug.LogError($"StageData ID {data.stageID}: Scene Name Missing");
    }
}