using UnityEngine;
using UnityEngine.UI; // UI 사용을 위해 필수
using System;

public class StageUIManager : MonoBehaviour
{
    [Header("Main UI Panel")]
    public GameObject mainUIPanel;

    // === UI 요소 연결 (인스펙터에서 할당) ===
    [Header("UI Text Fields")]
    public Text chapterNameText;    // [ 챕터명 ]
    public Text stageTitleText;     // [ 1 스테이지 ]
    public Text stageSynopsisText;  // 스테이지 시놉시스
    public Text rewardText;         // 보상 정보

    [Header("UI Button")]
    public Button startButton;      // 시작 버튼

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

    // --- NEW: 챕터가 잠겨있을 때 호출할 함수 ---
    /// <summary>
    /// 챕터가 잠겨있음을 UI에 표시합니다.
    /// </summary>
    /// <param name="chapterName">잠긴 챕터의 이름</param>
    public void ShowChapterLockedMessage(string chapterName)
    {
        // 1. 메인 UI 패널을 켭니다.
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        gameObject.SetActive(true);

        // 2. 텍스트 필드를 '잠김' 상태로 설정합니다.
        chapterNameText.text = $"{chapterName}";
        stageTitleText.text = "챕터 잠김";
        stageSynopsisText.text = "이전 챕터의 마지막 스테이지를 클리어하면 해금됩니다.";
        rewardText.text = "---";

        // 3. 시작 버튼을 비활성화합니다.
        startButton.interactable = false;
        startButton.onClick.RemoveAllListeners();
    }
    // --- END NEW ---


    /// <summary>
    /// StageSelector가 호출하여 UI 정보를 업데이트하는 함수 (챕터가 잠금 해제되었을 때)
    /// </summary>
    public void UpdateStageInfo(StageData data, bool isPlayable)
    {
        // 1. 메인 UI 패널을 켭니다.
        if (mainUIPanel != null) mainUIPanel.SetActive(true);
        gameObject.SetActive(true);

        if (data == null)
        {
            Debug.LogError("StageData가 NULL입니다. UI 업데이트를 건너뜁니다.");
            chapterNameText.text = "데이터 오류";
            stageTitleText.text = "";
            gameObject.SetActive(false); // 오류 시 패널 끄기
            return;
        }

        // 2. 데이터 기반으로 UI 텍스트 업데이트
        chapterNameText.text = $"{data.chapterName}";
        stageTitleText.text = $"{data.stageTitle}";

        // 3. 시작 버튼 리스너 및 활성화 상태 업데이트
        startButton.onClick.RemoveAllListeners();

        // --- CHANGED: 'isPlayable'에 따라 시놉시스와 버튼 상태 변경 ---
        if (isPlayable)
        {
            // [플레이 가능]
            stageSynopsisText.text = data.synopsis;
            rewardText.text = data.rewardName;

            startButton.interactable = true;
            startButton.onClick.AddListener(() => StartStage(data));
        }
        else
        {
            // [잠김]
            stageSynopsisText.text = "이전 스테이지를 클리어해야 플레이할 수 있습니다.";
            rewardText.text = "---";

            startButton.interactable = false;
        }
    }

    // UI 숨기기
    public void HideUI()
    {
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(false);
        }
    }

    private void StartStage(StageData data)
    {
        // ... (이하 동일) ...
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