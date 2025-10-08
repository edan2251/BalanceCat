using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class StageSelector : MonoBehaviour
{
    [System.Serializable]
    public struct StageRotationGroup
    {
        [Tooltip("회전 시작하는 스테이지")]
        public int startIndex;
        [Tooltip("회전 끝나는 스테이지")]
        public int endIndex;
        [Tooltip("X축 회전 목표값")]
        public float targetLocalXRotation;
        [Tooltip("Y축 회전 목표값")]
        public float targetLocalYRotation; // Y축 회전값 추가
        [Tooltip("Z축 회전 목표값")]
        public float targetLocalZRotation;
    }

    // === 인스펙터 설정 ===
    // 이 리스트는 ChapterPlanetCreator나 수동으로 연결됩니다.
    public List<GameObject> stagePoints;
    public float rotationDuration = 0.3f;
    public Ease easeType = Ease.OutQuad;

    [Header("스테이지 기반 회전 설정")]
    public List<StageRotationGroup> rotationGroups;

    [Header("라인 연결 설정")]
    public LineRenderer pathRenderer;
    public int segmentsPerStage = 20;

    // === 내부 변수 ===
    private int currentStageIndex = 0;
    private int totalStages = 0;
    private bool isAnimating = false;

    private ChapterSelector chapterController;

    [Header("하이라이트 설정")]
    // 인스펙터에 할당할 하이라이트 머티리얼
    public Material highlightMaterial;

    // 이전 선택된 스테이지의 MeshRenderer 컴포넌트를 저장
    private MeshRenderer lastSelectedRenderer;
    // 이전 선택된 스테이지의 원래 머티리얼을 저장
    private Material originalMaterial;


    void Awake()
    {
        chapterController = FindObjectOfType<ChapterSelector>();

        totalStages = stagePoints.Count;

        if (totalStages == 0)
        {
            Debug.LogError(gameObject.name + "에 StagePoint가 없습니다. 스테이지 기능 비활성화.");
            this.enabled = false;
            return;
        }

        SetStagesVisibility(false);

        // 라인 초기화 (행성 반지름은 행성 스케일 기반으로 가정)
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


    // 초기 회전 설정: 0번 인덱스 스테이지가 화면 중앙에 오도록 행성을 돌립니다.
    void InitializeRotation()
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    public void InitializeSelection()
    {
        if (totalStages == 0) return;

        currentStageIndex = 0;

        // 0번 스테이지로 초기화하면서 회전 및 하이라이트 적용
        ChangeStage(0);

    }

    // 스테이지 변경 로직
    void ChangeStage(int direction)
    {
        // 1. 다음 스테이지 인덱스 계산 및 순환
        int nextIndex = (currentStageIndex + direction + totalStages) % totalStages;

        if (direction != 0 && nextIndex == currentStageIndex) return;

        // **딜레이 제거 핵심:** 현재 실행 중인 모든 DOTween 애니메이션을 즉시 중단합니다.
        // 이는 빠른 입력 시 애니메이션이 겹치거나 지연되는 것을 방지합니다.
        transform.DOKill();
        isAnimating = true; // 애니메이션 추적용 플래그만 유지

        // 2. 현재 인덱스에 해당하는 목표 X, Y, Z 회전 데이터를 찾습니다.
        StageRotationGroup rotationData = GetRotationDataForStage(nextIndex);

        // 3. 목표 로컬 오일러 각도 벡터 생성
        Vector3 targetRotationVector = new Vector3(
            rotationData.targetLocalXRotation, // X축 회전 적용
            rotationData.targetLocalYRotation, // Y축 회전 적용
            rotationData.targetLocalZRotation  // Z축 회전 적용
        );

        // 4. DOTween을 사용하여 행성 오브젝트 자체를 목표 회전값으로 부드럽게 회전시킵니다.
        transform.DOLocalRotate(
            targetRotationVector,
            rotationDuration
        ).SetEase(easeType)
        .OnComplete(() =>
        {
            currentStageIndex = nextIndex;
            isAnimating = false; // 애니메이션 완료

            // 회전 완료 후 하이라이트 적용
            HighlightStage(stagePoints[currentStageIndex]);
        });
    }

    // 선택된 스테이지 하이라이트 (예시)
    void HighlightStage(GameObject selectedPoint)
    {
        // 1. 이전 스테이지 원복
        if (lastSelectedRenderer != null && originalMaterial != null)
        {
            // 이전에 선택된 스테이지의 머티리얼을 원래 머티리얼로 되돌립니다.
            lastSelectedRenderer.material = originalMaterial;
        }

        // 2. 새로운 스테이지 하이라이트
        MeshRenderer currentRenderer = selectedPoint.GetComponent<MeshRenderer>();

        if (currentRenderer != null && highlightMaterial != null)
        {
            // 2-1. 원래 머티리얼 저장
            originalMaterial = currentRenderer.material;

            // 2-2. 하이라이트 머티리얼로 교체
            currentRenderer.material = highlightMaterial;

            // 2-3. 현재 Renderer를 저장하여 다음 번에 원복할 수 있도록 준비
            lastSelectedRenderer = currentRenderer;
        }
        else if (currentRenderer == null)
        {
            // 스테이지 포인트에 MeshRenderer가 없으면 경고 메시지 출력
            Debug.LogWarning($"{selectedPoint.name}에 MeshRenderer 컴포넌트가 없습니다. 하이라이트 할 수 없습니다.");
        }
    }

    // 현재 스테이지 인덱스에 맞는 목표 X, Z축 회전 데이터를 찾는 함수
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

        // 일치하는 그룹이 없으면 현재 회전값을 기본값으로 하는 데이터를 반환
        Vector3 currentRotation = transform.localEulerAngles;
        return new StageRotationGroup
        {
            startIndex = -1,
            endIndex = -1,
            targetLocalXRotation = currentRotation.x,
            targetLocalYRotation = currentRotation.y, // 현재 Y값 유지
            targetLocalZRotation = currentRotation.z
        };
    }

    // 라인이 스테이지 포인트의 LocalPosition을 기준으로 행성 표면을 따라가도록 그립니다.
    public void DrawCurvedPath(float radius)
    {
        if (pathRenderer == null || stagePoints.Count < 2) return;

        // 연결해야 할 세그먼트의 총 개수 (7개면 6개만 연결)
        int totalSegments = stagePoints.Count - 1;
        if (totalSegments <= 0)
        {
            pathRenderer.positionCount = 0;
            return;
        }

        // 필요한 총 점의 개수
        int totalPoints = totalSegments * segmentsPerStage;

        pathRenderer.positionCount = totalPoints;

        // 순환 연결을 코드로 명시적 해제
        pathRenderer.loop = false;

        for (int i = 0; i < totalSegments; i++) // 
        {
            Vector3 startLocalPos = stagePoints[i].transform.localPosition;
            Vector3 endLocalPos = stagePoints[i + 1].transform.localPosition; // 다음 포인트

            for (int j = 0; j < segmentsPerStage; j++)
            {
                float t = (float)j / segmentsPerStage;

                // Slerp (구면 선형 보간)
                Vector3 currentPos = Vector3.Slerp(startLocalPos, endLocalPos, t);
                currentPos = currentPos.normalized * radius;

                // LineRenderer의 PositionCount에 맞게 인덱스 계산
                int pointIndex = (i * segmentsPerStage) + j;

                pathRenderer.SetPosition(pointIndex, currentPos);
            }
        }

        // 마지막 지점 (7번 스테이지)을 명시적으로 추가합니다.
        pathRenderer.positionCount = totalPoints + 1;
        pathRenderer.SetPosition(totalPoints, stagePoints[totalSegments].transform.localPosition);
    }

    public void SetStagesVisibility(bool isVisible)
    {
        // 스테이지 포인트 전체 가시성 제어
        foreach (var point in stagePoints)
        {
            if (point != null)
            {
                point.SetActive(isVisible);
            }
        }

        // 라인 렌더러 가시성 제어
        if (pathRenderer != null)
        {
            pathRenderer.enabled = isVisible;
        }
    }
}
