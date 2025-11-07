using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Cinemachine;

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

    // --- CHANGED: 렌더러별 '원본 머티리얼 배열'을 저장하도록 타입 변경 ---
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
                                // --- CHANGED: 'renderer.material' (단수) 대신 'renderer.materials' (복수 배열)을 저장 ---
                                originalPlanetMaterials[renderer] = renderer.materials;
                                // --- END CHANGED ---
                            }
                        }
                    }
                }
            }
        }

        isChapterSelectionActive = false;
        UpdatePlanetSelection(0);

        if (uiManager != null)
        {
            uiManager.HideUI();
        }

        if (mainUIManager != null)
        {
            mainUIManager.ShowUI();
        }
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
                UpdatePlanetMaterials(); // 머티리얼 갱신

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

        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeChapter(1);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeChapter(-1);
        }

        // 'P' 키로 현재 스테이지 클리어
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (currentStageSelector != null)
            {
                StageData currentStage = currentStageSelector.GetCurrentSelectedStageData();
                if (currentStage != null)
                {
                    GameProgressManager.ClearStage(currentStageSelector.chapterIndex, currentStage.stageID);

                    // 1. 챕터 잠금 상태(머티리얼) 즉시 갱신
                    UpdatePlanetMaterials();

                    // --- NEW: 스테이지 잠금 상태(머티리얼) 즉시 갱신 ---
                    // (StageSelector의 UpdateAllStageMaterials 함수를 호출)
                    currentStageSelector.UpdateAllStageMaterials();
                    // --- END NEW ---

                    // 2. 현재 UI 갱신 (버튼 활성화 등)
                    bool isPlayable = currentStageSelector.IsStagePlayable(currentStageSelector.currentStageIndex);
                    uiManager.UpdateStageInfo(currentStage, isPlayable);
                }
            }
        }
    }

    // ... (GetTotalChapters, EnterChapterSelectionFromUI, IsChapterSelectionActive, GetChapterData, IsChapterUnlocked 함수는 동일) ...
    public int GetTotalChapters()
    {
        return totalChapters;
    }

    public void EnterChapterSelectionFromUI()
    {
        if (isChapterSelectionActive || isAnimating) return;

        if (mainUIManager != null)
        {
            mainUIManager.HideUI();
        }

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
        if (index < 0 || index >= planets.Count || planets[index] == null)
        {
            return null;
        }
        StageSelector selector = planets[index].GetComponent<StageSelector>();
        if (selector == null)
        {
            return null;
        }
        return selector.chapterData;
    }

    public bool IsChapterUnlocked(int chapterIndex)
    {
        if (chapterIndex == 0) return true;

        ChapterData prevChapterData = GetChapterData(chapterIndex - 1);

        if (prevChapterData == null || prevChapterData.stages.Count == 0)
        {
            Debug.LogWarning($"[ChapterLock] 챕터 {chapterIndex}의 잠금 상태 확인 실패: 이전 챕터({chapterIndex - 1}) 데이터를 찾을 수 없음.");
            return false;
        }

        StageData lastStageOfPrevChapter = prevChapterData.stages[prevChapterData.stages.Count - 1];
        int prevChapterIndex = chapterIndex - 1;
        return GameProgressManager.IsStageCleared(prevChapterIndex, lastStageOfPrevChapter.stageID);
    }


    // --- CHANGED: 'renderer.materials' (복수)를 사용하도록 로직 수정 ---
    void UpdatePlanetMaterials()
    {
        if (lockedPlanetMaterial == null) return;

        // 모든 행성(챕터)을 순회
        for (int i = 0; i < planets.Count; i++)
        {
            GameObject planet = planets[i];
            if (planet == null) continue;

            StageSelector selector = planet.GetComponent<StageSelector>();
            if (selector == null || selector.materialTargets == null) continue;

            // 이 챕터(행성)가 잠금 해제되었는지 확인
            bool isUnlocked = IsChapterUnlocked(i);

            // 이 챕터에 속한 모든 'materialTargets' 렌더러를 순회
            foreach (MeshRenderer renderer in selector.materialTargets)
            {
                if (renderer == null) continue;

                if (isUnlocked)
                {
                    // 잠금 해제됨: 딕셔너리에 저장된 '원본 머티리얼 배열'로 복구
                    if (originalPlanetMaterials.ContainsKey(renderer))
                    {
                        renderer.materials = originalPlanetMaterials[renderer];
                    }
                }
                else
                {
                    // 잠김: 'lockedPlanetMaterial'로 모든 슬롯을 채움

                    // 1. 딕셔너리에 원본 배열이 저장되어 있는지, 슬롯 개수를 확인하기 위해 체크
                    if (originalPlanetMaterials.ContainsKey(renderer))
                    {
                        // 2. 원본 배열의 길이(머티리얼 슬롯 개수)를 가져옴
                        int materialCount = originalPlanetMaterials[renderer].Length;

                        // 3. 해당 길이만큼 '새 머티리얼 배열' 생성
                        Material[] newLockedMaterials = new Material[materialCount];

                        // 4. 새 배열의 모든 요소를 'lockedPlanetMaterial'로 채움
                        for (int j = 0; j < materialCount; j++)
                        {
                            newLockedMaterials[j] = lockedPlanetMaterial;
                        }

                        // 5. 'renderer.materials' (복수) 속성에 새 배열을 할당
                        renderer.materials = newLockedMaterials;
                    }
                }
            }
        }
    }
    // --- END CHANGED ---


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
            UpdatePlanetSelection(currentChapterIndex);
            HighlightPlanet(planets[currentChapterIndex]);
            isAnimating = false;
        });
    }

    void DeactivateChapterSelection()
    {
        lastSelectedChapterIndex = currentChapterIndex;
        isChapterSelectionActive = false;
        isAnimating = false;

        if (uiManager != null)
        {
            uiManager.HideUI();
        }
        if (mainUIManager != null)
        {
            mainUIManager.ShowUI();
        }

        if (currentStageSelector != null)
        {
            Transform selectedPlanetTransform = currentStageSelector.transform;
            selectedPlanetTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
        }

        chapterVCam.Priority = inactivePriority;
        logoVCam.Priority = activePriority;

        UpdatePlanetSelection(currentChapterIndex);
    }

    void ChangeChapter(int direction)
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

            UpdatePlanetSelection(currentChapterIndex);
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

    void UpdatePlanetSelection(int newIndex)
    {
        // 챕터 선택 변경 시 행성 머티리얼 갱신 (잠금 상태 반영)
        UpdatePlanetMaterials();

        for (int i = 0; i < planets.Count; i++)
        {
            StageSelector selector = planets[i].GetComponent<StageSelector>();
            if (selector == null) continue;

            bool isSelected = (i == newIndex);

            // --- CHANGED: 잠금 상태 확인 로직 추가 ---

            // 1. 이 챕터가 잠금 해제되었는지 확인
            bool isUnlocked = IsChapterUnlocked(i);

            // 2. StageSelector 활성화 여부 결정
            // (선택되었고, 챕터 선택 모드이며, 잠금 해제 상태일 때만 활성화)
            // -> .enabled = false이면 Update()가 돌지 않아 A/D 키 입력이 막힘
            selector.enabled = isSelected && isChapterSelectionActive && isUnlocked;

            // 3. 스테이지 가시성 설정 (활성화 조건과 동일)
            selector.SetStagesVisibility(isSelected && isChapterSelectionActive && isUnlocked);

            // 4. 현재 선택된 행성(챕터)에 대한 처리
            if (isSelected)
            {
                currentStageSelector = selector;

                // 챕터 선택 모드(로고 모드X)일 때만 UI 및 하이라이트 처리
                if (isChapterSelectionActive)
                {
                    // 행성 하이라이트 (크기 키우기)
                    HighlightPlanet(planets[i]);

                    if (isUnlocked)
                    {
                        // 4-A. [잠금 해제됨]
                        // Stage UI 패널을 활성화 (필요한 경우)
                        if (uiManager != null && uiManager.mainUIPanel != null && !uiManager.mainUIPanel.activeSelf)
                        {
                            uiManager.mainUIPanel.SetActive(true);
                        }

                        // StageSelector의 첫 번째 스테이지를 선택하도록 초기화
                        // (이 함수 내부에서 UpdateStageInfo가 호출됨)
                        selector.InitializeSelection();
                    }
                    else
                    {
                        // 4-B. [잠김]
                        // StageUIManager의 '잠김 메시지' 함수 호출
                        if (uiManager != null)
                        {
                            string chapterName = selector.chapterData != null ? selector.chapterData.chapterName : "챕터";
                            uiManager.ShowChapterLockedMessage(chapterName);
                        }
                    }
                }
            }
            // --- END CHANGED ---
        }
    }
}