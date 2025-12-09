using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;

public class ChapterSelector : MonoBehaviour
{
    // === 인스펙터 설정 ===
    public List<GameObject> planets;
    public Transform planetPivot;
    public Camera mainCamera;

    [Header("DOTween 설정")]
    public float rotationDuration = 0.5f;
    public Ease easeType = Ease.OutBack;
    public float cameraBlendDelay = 1.0f;

    [Header("Cinemachine 제어")]
    public CinemachineVirtualCamera logoVCam;
    public CinemachineVirtualCamera chapterVCam;
    public int activePriority = 20;
    public int inactivePriority = 10;
    public float continuousRotationSpeed = 5f;

    [Header("Chapter Alignment")]
    [Tooltip("회전 보정을 위한 각도 오프셋.")]
    public float alignmentOffset = 0f;

    [Header("Chapter Lock Settings")]
    [Tooltip("잠긴 챕터(행성)에 적용할 머티리얼")]
    public Material lockedPlanetMaterial;

    private Dictionary<MeshRenderer, Material[]> originalPlanetMaterials = new Dictionary<MeshRenderer, Material[]>();

    // === 참조 관리 ===
    public StageUIManager uiManager;
    [Header("메인 UI 관리자")]
    public MainUIManager mainUIManager;

    // === 내부 변수 ===
    private int currentChapterIndex = 0;
    private int totalChapters = 0;
    private bool isAnimating = false;
    private float currentTargetRotation = 0f;
    private StageSelector currentStageSelector = null;

    private bool isFirstEntry = true;
    private int lastSelectedChapterIndex = 0;
    private bool isChapterSelectionActive = false;

    void Start()
    {
        //배경음 출력
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMainMenuBGM();
        }

        totalChapters = planets.Count;
        currentTargetRotation = 0f;

        logoVCam.Priority = activePriority;
        chapterVCam.Priority = inactivePriority;
        chapterVCam.Follow = null;
        chapterVCam.LookAt = null;

        originalPlanetMaterials.Clear();
        for (int i = 0; i < totalChapters; i++)
        {
            if (planets[i] != null)
            {
                StageSelector selector = planets[i].GetComponent<StageSelector>();
                if (selector != null)
                {
                    selector.chapterIndex = i;

                    if (selector.materialTargets != null)
                    {
                        foreach (MeshRenderer renderer in selector.materialTargets)
                        {
                            if (renderer != null && !originalPlanetMaterials.ContainsKey(renderer))
                            {
                                originalPlanetMaterials[renderer] = renderer.materials;
                            }
                        }
                    }
                }
            }
        }

        isChapterSelectionActive = false;
        UpdatePlanetSelection(0);

        if (uiManager != null) uiManager.HideUI();
        if (mainUIManager != null) mainUIManager.ShowUI();
    }

    void Update()
    {
        // 'O' 키로 진행 상황 리셋
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameProgressManager.ResetAllProgress();
            Debug.Log("[GameProgress] 모든 진행 상황이 리셋되었습니다. (O 키)");

            if (isChapterSelectionActive)
            {
                UpdatePlanetMaterials();
                if (currentStageSelector != null)
                {
                    currentStageSelector.UpdateAllStageMaterials();
                }

                if (currentStageSelector != null && uiManager != null)
                {
                    StageData currentData = currentStageSelector.GetCurrentSelectedStageData();
                    if (currentData != null)
                    {
                        bool isPlayable = currentStageSelector.IsStagePlayable(currentStageSelector.currentStageIndex);
                        uiManager.UpdateStageInfo(currentData, isPlayable);
                    }
                }
            }
            return;
        }

        // 로고 모드
        if (!isChapterSelectionActive)
        {
            if (uiManager != null && uiManager.mainUIPanel != null && uiManager.mainUIPanel.activeSelf)
            {
                uiManager.HideUI();
            }
            planetPivot.Rotate(0, continuousRotationSpeed * Time.deltaTime, 0, Space.Self);
            return;
        }

        // 챕터 선택 모드
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateChapterSelection();
            return;
        }

        if (Input.GetKeyDown(KeyCode.W)) ChangeChapter(1);
        else if (Input.GetKeyDown(KeyCode.S)) ChangeChapter(-1);

        // --- 테스트 키 입력 ---
        if (currentStageSelector != null)
        {
            StageData currentStage = currentStageSelector.GetCurrentSelectedStageData();
            if (currentStage == null) return;

            int chapterIdx = currentStageSelector.chapterIndex;
            int stageID = currentStage.stageID;

            // 'P' 키: 퀘스트 1 완료 + 스테이지 잠금 해제
            if (Input.GetKeyDown(KeyCode.P))
            {
                GameProgressManager.ClearStage(chapterIdx, stageID);
                GameProgressManager.CompleteQuest(chapterIdx, stageID, 1);
                Debug.Log($"테스트: {chapterIdx}-{stageID} 퀘스트 1 (메인) 완료");
                RefreshCurrentUIAndCheckRewards(chapterIdx);
            }
            // 'K' 키: 퀘스트 2 완료
            else if (Input.GetKeyDown(KeyCode.K))
            {
                GameProgressManager.CompleteQuest(chapterIdx, stageID, 2);
                Debug.Log($"테스트: {chapterIdx}-{stageID} 퀘스트 2 완료");
                RefreshCurrentUIAndCheckRewards(chapterIdx);
            }
            // 'L' 키: 퀘스트 3 완료
            else if (Input.GetKeyDown(KeyCode.L))
            {
                GameProgressManager.CompleteQuest(chapterIdx, stageID, 3);
                Debug.Log($"테스트: {chapterIdx}-{stageID} 퀘스트 3 완료");
                RefreshCurrentUIAndCheckRewards(chapterIdx);
            }
        }
    }

    private void RefreshCurrentUIAndCheckRewards(int chapterIdx)
    {
        currentStageSelector.UpdateAllStageMaterials();
        UpdatePlanetMaterials();

        if (uiManager != null)
        {
            StageData data = currentStageSelector.GetCurrentSelectedStageData();
            bool isPlayable = currentStageSelector.IsStagePlayable(currentStageSelector.currentStageIndex);
            uiManager.UpdateStageInfo(data, isPlayable);
        }

        CheckAndLogChapterReward(chapterIdx);
    }

    public StageSelector GetCurrentStageSelector()
    {
        return currentStageSelector;
    }

    // --- [수정됨] 퀘스트 리스트를 순회하며 완료 여부 체크 ---
    private void CheckAndLogChapterReward(int chapterIndex)
    {
        ChapterData chapterData = GetChapterData(chapterIndex);
        if (chapterData == null) return;

        // 1. 이 챕터의 모든 스테이지를 순회
        foreach (var stageData in chapterData.stages)
        {
            // 2. 해당 스테이지에 할당된 퀘스트 리스트 확인
            if (stageData.quests == null) continue;

            for (int i = 0; i < stageData.quests.Count; i++)
            {
                // 3. 퀘스트 데이터가 있는지 확인
                if (stageData.quests[i] != null)
                {
                    // 저장된 키값은 1부터 시작하므로 (인덱스 + 1) 사용
                    int questKeyIndex = i + 1;

                    if (!GameProgressManager.IsQuestCompleted(chapterIndex, stageData.stageID, questKeyIndex))
                    {
                        // 하나라도 안 깬 게 있으면 보상 없음 (함수 종료)
                        return;
                    }
                }
            }
        }

        // 5. (모든 반복문 통과) 이 챕터의 할당된 모든 퀘스트 완료
        Debug.LogWarning($"--- 🏆 챕터 {chapterIndex} 모든 별 획득! 🏆 ---");
        Debug.LogWarning($"--- 보상 지급 로직을 여기에 추가하세요! ---");
    }


    public int GetTotalChapters()
    {
        return totalChapters;
    }

    public void EnterChapterSelectionFromUI()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.ButtonClick);
        }

        if (isChapterSelectionActive || isAnimating) return;

        if (mainUIManager != null) mainUIManager.HideUI();

        int chapterToSelect = isFirstEntry ? 0 : lastSelectedChapterIndex;
        currentChapterIndex = chapterToSelect;
        isFirstEntry = false;

        ActivateChapterSelection();
    }


    public bool IsChapterSelectionActive()
    {
        return isChapterSelectionActive;
    }

    ChapterData GetChapterData(int index)
    {
        if (index < 0 || index >= planets.Count || planets[index] == null) return null;
        StageSelector selector = planets[index].GetComponent<StageSelector>();
        if (selector == null) return null;
        return selector.chapterData;
    }

    public bool IsChapterUnlocked(int chapterIndex)
    {
        if (chapterIndex == 0) return true;

        ChapterData prevChapterData = GetChapterData(chapterIndex - 1);

        if (prevChapterData == null || prevChapterData.stages.Count == 0)
        {
            Debug.LogWarning($"[ChapterLock] 챕터 {chapterIndex} 잠금 확인 실패: 이전 챕터 데이터 없음.");
            return false;
        }

        StageData lastStageOfPrevChapter = prevChapterData.stages[prevChapterData.stages.Count - 1];
        int prevChapterIndex = chapterIndex - 1;
        return GameProgressManager.IsStageCleared(prevChapterIndex, lastStageOfPrevChapter.stageID);
    }

    void UpdatePlanetMaterials()
    {
        if (lockedPlanetMaterial == null) return;

        for (int i = 0; i < planets.Count; i++)
        {
            GameObject planet = planets[i];
            if (planet == null) continue;

            StageSelector selector = planet.GetComponent<StageSelector>();
            if (selector == null || selector.materialTargets == null) continue;

            bool isUnlocked = IsChapterUnlocked(i);

            foreach (MeshRenderer renderer in selector.materialTargets)
            {
                if (renderer == null) continue;

                if (isUnlocked)
                {
                    if (originalPlanetMaterials.ContainsKey(renderer))
                    {
                        renderer.materials = originalPlanetMaterials[renderer];
                    }
                }
                else
                {
                    if (originalPlanetMaterials.ContainsKey(renderer))
                    {
                        int materialCount = originalPlanetMaterials[renderer].Length;
                        Material[] newLockedMaterials = new Material[materialCount];
                        for (int j = 0; j < materialCount; j++)
                        {
                            newLockedMaterials[j] = lockedPlanetMaterial;
                        }
                        renderer.materials = newLockedMaterials;
                    }
                }
            }
        }
    }


    void ActivateChapterSelection()
    {
        isAnimating = true;
        isChapterSelectionActive = true;

        int targetIndex = currentChapterIndex;

        logoVCam.Priority = inactivePriority;
        chapterVCam.Priority = activePriority;

        float anglePerChapter = 360f / totalChapters;
        float targetYRotation = (targetIndex * anglePerChapter) + alignmentOffset;
        targetYRotation %= 360f;

        Sequence activationSequence = DOTween.Sequence();

        activationSequence.Append(
        planetPivot.DORotate(
            new Vector3(0, targetYRotation, 0),
            rotationDuration
        ).SetEase(easeType)
        );

        activationSequence.AppendInterval(cameraBlendDelay);

        activationSequence.OnComplete(() =>
        {
            // 처음 켜질 때는 무조건 1스테이지부터 시작하므로 false (또는 생략 가능)
            UpdatePlanetSelection(currentChapterIndex, false);
            HighlightPlanet(planets[currentChapterIndex]);
            isAnimating = false;
        });
    }

    void DeactivateChapterSelection()
    {
        lastSelectedChapterIndex = currentChapterIndex;
        isChapterSelectionActive = false;
        isAnimating = false;

        if (uiManager != null) uiManager.HideUI();
        if (mainUIManager != null) mainUIManager.ShowUI();

        if (currentStageSelector != null)
        {
            Transform selectedPlanetTransform = currentStageSelector.transform;
            selectedPlanetTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
        }

        chapterVCam.Priority = inactivePriority;
        logoVCam.Priority = activePriority;

        UpdatePlanetSelection(currentChapterIndex);
    }

    public void ChangeChapter(int direction)
    {
        if (isAnimating) return;
        isAnimating = true;

        int nextIndex = (currentChapterIndex + direction + totalChapters) % totalChapters;

        float anglePerChapter = 360f / totalChapters;
        float targetYRotation = (nextIndex * anglePerChapter) + alignmentOffset;
        targetYRotation %= 360f;

        Vector3 targetRotationVector = new Vector3(0, targetYRotation, 0);

        planetPivot.DORotate(
            targetRotationVector,
            rotationDuration
        ).SetEase(easeType)
        .OnComplete(() =>
        {
            currentChapterIndex = nextIndex;
            currentTargetRotation = targetYRotation;

            bool isMovingBack = direction < 0;
            UpdatePlanetSelection(currentChapterIndex, isMovingBack);
            isAnimating = false;
            HighlightPlanet(planets[currentChapterIndex]);
        });
    }

    void HighlightPlanet(GameObject selectedPlanet)
    {
        foreach (var p in planets)
        {
            if (p != selectedPlanet)
            {
                p.transform.DOScale(Vector3.one, 0.3f);
            }
        }
        selectedPlanet.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutQuad);
    }

    void UpdatePlanetSelection(int newIndex, bool selectLastStage = false)
    {
        UpdatePlanetMaterials();

        for (int i = 0; i < planets.Count; i++)
        {
            StageSelector selector = planets[i].GetComponent<StageSelector>();
            if (selector == null) continue;

            bool isSelected = (i == newIndex);
            bool isUnlocked = IsChapterUnlocked(i);

            selector.enabled = isSelected && isChapterSelectionActive && isUnlocked;
            selector.SetStagesVisibility(isSelected && isChapterSelectionActive && isUnlocked);

            if (isSelected)
            {
                currentStageSelector = selector;

                if (isChapterSelectionActive)
                {
                    HighlightPlanet(planets[i]);

                    if (isUnlocked)
                    {
                        if (uiManager != null && uiManager.mainUIPanel != null && !uiManager.mainUIPanel.activeSelf)
                        {
                            uiManager.mainUIPanel.SetActive(true);
                        }
                        // [수정] 여기서 true/false를 전달합니다!
                        selector.InitializeSelection(selectLastStage);
                    }
                    else
                    {
                        if (uiManager != null)
                        {
                            string chapterName = selector.chapterData != null ? selector.chapterData.chapterName : "챕터";
                            uiManager.ShowChapterLockedMessage(chapterName);
                        }
                    }
                }
            }
        }
    }
}