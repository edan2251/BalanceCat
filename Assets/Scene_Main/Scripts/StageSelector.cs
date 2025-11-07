using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class StageSelector : MonoBehaviour
{
    [System.Serializable]
    public struct StageRotationGroup
    {
        public int startIndex;
        public int endIndex;
        public float targetLocalXRotation;
        public float targetLocalYRotation;
        public float targetLocalZRotation;
    }

    // === 인스펙터 설정 ===
    public List<GameObject> stagePoints;

    [Header("ChapterData ScriptableOBJ연결")]
    public ChapterData chapterData;

    public float rotationDuration = 0.3f;
    public Ease easeType = Ease.OutQuad;

    [Header("스테이지 기반 회전 설정")]
    public List<StageRotationGroup> rotationGroups;

    [Header("라인 연결 설정")]
    public LineRenderer pathRenderer;
    public int segmentsPerStage = 20;

    // === 내부 변수 ===
    public int currentStageIndex = 0;
    private int totalStages = 0;

    private StageUIManager uiManager;
    private ChapterSelector chapterController;

    [HideInInspector]
    public int chapterIndex = 0;

    [Header("하이라이트 및 잠금 머티리얼")]
    public Material highlightMaterial;
    // --- NEW: 스테이지 잠금 머티리얼 ---
    [Tooltip("잠긴 스테이지 포인트에 적용할 머티리얼 (회색 등)")]
    public Material lockedStageMaterial;

    [Header("챕터 잠금 머티리얼 설정")]
    [Tooltip("이 챕터(행성)가 잠겼을 때 머티리얼이 변경될 모든 MeshRenderer 리스트입니다.")]
    public List<MeshRenderer> materialTargets;

    // --- CHANGED: 다중 머티리얼 하이라이트/복원용 ---
    private MeshRenderer lastSelectedRenderer;
    private Material[] originalMaterials; // 'originalMaterial' (단수) -> 'originalMaterials' (배열)

    // --- NEW: 모든 스테이지의 원본 머티리얼(배열) 저장용 ---
    private Dictionary<MeshRenderer, Material[]> originalStageMaterialsCache = new Dictionary<MeshRenderer, Material[]>();


    void Awake()
    {
        uiManager = FindObjectOfType<StageUIManager>();
        chapterController = FindObjectOfType<ChapterSelector>();
        totalStages = stagePoints.Count;

        if (totalStages == 0)
        {
            Debug.LogError(gameObject.name + "에 StagePoint가 없습니다. 스테이지 기능 비활성화.");
            this.enabled = false;
            return;
        }

        // --- NEW: 모든 스테이지 포인트의 원본 머티리얼(들)을 캐시 ---
        originalStageMaterialsCache.Clear();
        foreach (var point in stagePoints)
        {
            // 자식에 메시가 있을 수 있으므로 GetComponentInChildren 사용
            MeshRenderer renderer = point.GetComponentInChildren<MeshRenderer>();
            if (renderer != null && !originalStageMaterialsCache.ContainsKey(renderer))
            {
                // 'materials' (복수)를 저장
                originalStageMaterialsCache[renderer] = renderer.materials;
            }
        }
        // --- END NEW ---

        SetStagesVisibility(false);
        float planetRadius = transform.localScale.x * 0.5f;
        DrawCurvedPath(planetRadius);

        InitializeRotation();
    }

    void Update()
    {
        if (chapterController == null || !chapterController.IsChapterSelectionActive())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeStage(-1); // 이전 스테이지
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeStage(1); // 다음 스테이지
        }
    }

    public void OnEnable()
    {
        if (chapterController == null)
            chapterController = FindObjectOfType<ChapterSelector>();

        totalStages = stagePoints.Count;

        if (totalStages > 0 && currentStageIndex < totalStages)
        {
            ChangeStage(0);
        }
    }


    void InitializeRotation()
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public void InitializeSelection()
    {
        if (totalStages == 0) return;

        // --- NEW: 스테이지 머티리얼 상태부터 갱신 ---
        UpdateAllStageMaterials();
        // --- END NEW ---

        currentStageIndex = 0;
        ChangeStage(0);
    }

    // --- NEW: 'P' 키 테스트용으로 public으로 변경 ---
    /// <summary>
    /// 모든 스테이지의 머티리얼을 잠금/해제 상태로 갱신합니다.
    /// </summary>
    public void UpdateAllStageMaterials()
    {
        if (lockedStageMaterial == null) return;

        for (int i = 0; i < totalStages; i++)
        {
            GameObject point = stagePoints[i];
            if (point == null) continue;

            MeshRenderer renderer = point.GetComponentInChildren<MeshRenderer>();
            if (renderer == null || !originalStageMaterialsCache.ContainsKey(renderer)) continue;

            bool isPlayable = IsStagePlayable(i);

            // 캐시에서 원본 머티리얼 배열을 가져옴
            Material[] originalMats = originalStageMaterialsCache[renderer];

            if (isPlayable)
            {
                // [플레이 가능]: 원본 머티리얼(들)로 복원
                renderer.materials = originalMats;
            }
            else
            {
                // [잠김]: 'lockedStageMaterial'로 모든 슬롯을 덮어씀
                int materialCount = originalMats.Length;
                Material[] lockedMaterials = new Material[materialCount];
                for (int j = 0; j < materialCount; j++)
                {
                    lockedMaterials[j] = lockedStageMaterial;
                }
                renderer.materials = lockedMaterials;
            }
        }
    }


    public bool IsStagePlayable(int stageIndex)
    {
        if (chapterData == null || stageIndex < 0 || stageIndex >= chapterData.stages.Count)
        {
            return false;
        }

        StageData targetStage = chapterData.stages[stageIndex];
        int targetStageID = targetStage.stageID; // 1-based ID

        // Case 1: 챕터 0의 스테이지 1 (ID 1)은 항상 플레이 가능
        if (chapterIndex == 0 && targetStageID == 1)
        {
            return true;
        }

        // Case 2: 챕터 내 두 번째 스테이지 이상 (ID > 1)
        if (targetStageID > 1)
        {
            // 바로 이전 스테이지(ID - 1)가 클리어되었는지 확인
            return GameProgressManager.IsStageCleared(chapterIndex, targetStageID - 1);
        }

        // Case 3: 챕터 1 이상의 첫 번째 스테이지 (ID == 1 && Chapter > 0)
        if (targetStageID == 1 && chapterIndex > 0)
        {
            if (chapterController == null) chapterController = FindObjectOfType<ChapterSelector>();
            return chapterController.IsChapterUnlocked(chapterIndex);
        }

        return false;
    }


    void ChangeStage(int direction)
    {
        int nextIndex = (currentStageIndex + direction + totalStages) % totalStages;
        if (direction != 0 && nextIndex == currentStageIndex) return;

        transform.DOKill();

        StageRotationGroup rotationData = GetRotationDataForStage(nextIndex);
        Vector3 targetRotationVector = new Vector3(
            rotationData.targetLocalXRotation,
            rotationData.targetLocalYRotation,
            rotationData.targetLocalZRotation
        );

        transform.DOLocalRotate(
            targetRotationVector,
            rotationDuration
        ).SetEase(easeType)
        .OnComplete(() =>
        {
            currentStageIndex = nextIndex;

            // --- CHANGED: isPlayable을 먼저 계산 ---
            bool isPlayable = IsStagePlayable(currentStageIndex);
            // --- END CHANGED ---

            // 1. 하이라이트 적용 (isPlayable 값 전달)
            HighlightStage(stagePoints[currentStageIndex], isPlayable);

            // 2. UI 매니저에 현재 데이터 전달
            if (uiManager != null)
            {
                StageData currentData = GetCurrentSelectedStageData();
                // isPlayable은 이미 위에서 계산됨
                uiManager.UpdateStageInfo(currentData, isPlayable);
            }
        });
    }

    // --- CHANGED: 다중 머티리얼을 지원하도록 수정 ---
    void HighlightStage(GameObject selectedPoint, bool isPlayable)
    {
        // 1. 이전 스테이지 원복
        if (lastSelectedRenderer != null && originalMaterials != null)
        {
            // 이전에 선택된 스테이지의 머티리얼을 'originalMaterials' (저장된 배열)로 되돌립니다.
            lastSelectedRenderer.materials = originalMaterials;
        }

        // 2. 새로운 스테이지 Renderer 찾기
        MeshRenderer currentRenderer = selectedPoint.GetComponentInChildren<MeshRenderer>();

        if (currentRenderer == null)
        {
            Debug.LogWarning($"{selectedPoint.name}에 MeshRenderer 컴포넌트가 없습니다.");
            lastSelectedRenderer = null; // 이전 선택 기록 초기화
            originalMaterials = null;    // 이전 머티리얼 기록 초기화
            return;
        }

        // 3. 'originalMaterials'에 현재 상태(locked일 수도, original일 수도 있음)를 저장
        //    (이후 '이전 스테이지 원복' 시 사용)
        originalMaterials = currentRenderer.materials;

        // --- CHANGED: 플레이 가능한 스테이지'만' 하이라이트 ---
        if (isPlayable && highlightMaterial != null)
        {
            // 4-A. [플레이 가능]: 하이라이트 머티리얼로 교체
            int materialCount = originalMaterials.Length;
            Material[] highlightMaterials = new Material[materialCount];
            for (int i = 0; i < materialCount; i++)
            {
                highlightMaterials[i] = highlightMaterial;
            }
            currentRenderer.materials = highlightMaterials;
        }
        // 4-B. [잠김] (isPlayable == false):
        //     하이라이트 머티리얼을 적용하지 않습니다.
        //     (originalMaterials 에는 이미 lockedStageMaterial이 적용된 상태가 저장됨)

        // 5. 현재 Renderer를 '마지막'으로 저장 (다음 원복을 위해)
        lastSelectedRenderer = currentRenderer;
    }

    StageRotationGroup GetRotationDataForStage(int stageIndex)
    {
        int stageID = stageIndex + 1;

        foreach (var group in rotationGroups)
        {
            if (stageID >= group.startIndex && stageID <= group.endIndex)
            {
                return group;
            }
        }

        Vector3 currentRotation = transform.localEulerAngles;
        return new StageRotationGroup
        {
            startIndex = -1,
            endIndex = -1,
            targetLocalXRotation = currentRotation.x,
            targetLocalYRotation = currentRotation.y,
            targetLocalZRotation = currentRotation.z
        };
    }

    public void DrawCurvedPath(float radius)
    {
        if (pathRenderer == null || stagePoints.Count < 2) return;

        int totalSegments = stagePoints.Count - 1;
        if (totalSegments <= 0)
        {
            pathRenderer.positionCount = 0;
            return;
        }

        int totalPoints = totalSegments * segmentsPerStage;
        pathRenderer.positionCount = totalPoints;
        pathRenderer.loop = false;

        for (int i = 0; i < totalSegments; i++)
        {
            Vector3 startLocalPos = stagePoints[i].transform.localPosition;
            Vector3 endLocalPos = stagePoints[i + 1].transform.localPosition;

            for (int j = 0; j < segmentsPerStage; j++)
            {
                float t = (float)j / segmentsPerStage;
                Vector3 currentPos = Vector3.Slerp(startLocalPos, endLocalPos, t);
                currentPos = currentPos.normalized * radius;

                int pointIndex = (i * segmentsPerStage) + j;
                pathRenderer.SetPosition(pointIndex, currentPos);
            }
        }

        pathRenderer.positionCount = totalPoints + 1;
        pathRenderer.SetPosition(totalPoints, stagePoints[totalSegments].transform.localPosition);
    }

    public void SetStagesVisibility(bool isVisible)
    {
        foreach (var point in stagePoints)
        {
            if (point != null)
            {
                point.SetActive(isVisible);
            }
        }

        if (pathRenderer != null)
        {
            pathRenderer.enabled = isVisible;
        }
    }

    public StageData GetCurrentSelectedStageData()
    {
        if (chapterData == null)
        {
            Debug.LogError("Chapter Data가 StageSelector에 연결되지 않았습니다!");
            return null;
        }

        if (currentStageIndex < 0 || currentStageIndex >= chapterData.stages.Count)
        {
            Debug.LogError($"잘못된 Stage Index: {currentStageIndex}.");
            return null;
        }

        return chapterData.stages[currentStageIndex];
    }
}