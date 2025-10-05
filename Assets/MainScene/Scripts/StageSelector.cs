using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class StageSelector : MonoBehaviour
{
    // === 인스펙터 설정 ===
    // 이 리스트는 ChapterPlanetCreator나 수동으로 연결됩니다.
    public List<GameObject> stagePoints;
    public float rotationDuration = 0.3f;
    public Ease easeType = Ease.OutQuad;

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


    void Start()
    {
        chapterController = FindObjectOfType<ChapterSelector>();
        if (chapterController == null)
        {
            Debug.LogError("ChapterSelector를 찾을 수 없습니다. PlanetPivot에 붙어 있는지 확인하세요.");
        }

        totalStages = stagePoints.Count;

        if (stagePoints.Count > 0)
        {
            HighlightStage(stagePoints[0]);
        }

        //처음에는 StageSelector를 비활성화 상태로 시작해야 합니다.
        // ChapterSelector가 선택된 행성만 활성화합니다.
        this.enabled = false;

        // 모든 스테이지 포인트를 보이도록 설정 (사용자 요청 반영)
        foreach (var point in stagePoints)
        {
            point.SetActive(true);
        }

        // 라인 초기화 (행성 반지름은 행성 스케일 기반으로 가정)
        float planetRadius = transform.localScale.x * 0.5f;
        DrawCurvedPath(planetRadius);

        InitializeRotation();
    }

    void Update()
    {
        if (isAnimating || chapterController == null || !chapterController.IsChapterSelectionActive())
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


    // 초기 회전 설정: 0번 인덱스 스테이지가 화면 중앙에 오도록 행성을 돌립니다.
    void InitializeRotation()
    {
        float anglePerStage = 360f / totalStages;
        transform.localEulerAngles = new Vector3(0, 0, 0);
    }


    // 스테이지 변경 로직
    void ChangeStage(int direction)
    {
        // 1. 다음 스테이지 인덱스 계산 및 순환
        int nextIndex = (currentStageIndex + direction + totalStages) % totalStages;

        isAnimating = true;

        // 2. 다음 선택될 스테이지 포인트의 로컬 위치를 가져옵니다.
        GameObject nextStagePoint = stagePoints[nextIndex];
        Vector3 pointLocalPosition = nextStagePoint.transform.localPosition;

        // 3. 목표 회전 계산 (최종 안정화 로직)
        // -----------------------------------------------------------

        // 3-1. Up 벡터 정의:
        Vector3 localUp = Vector3.up;

        // 3-2. LookRotation 계산: 스테이지 포인트가 행성의 로컬 -Z축에 오도록 하는 회전값
        Quaternion rotationToPointBack = Quaternion.LookRotation(
            -pointLocalPosition.normalized, // 행성 중심 방향 (Local -Z축에 정렬될 방향)
            localUp                        // Y축을 기준으로 회전하도록 강제
        );

        // 3-3. 정면 응시 목표 회전값: Inverse를 적용하여 스테이지 포인트가 로컬 +Z축에 오도록 합니다.
        Quaternion targetRotation = Quaternion.Inverse(rotationToPointBack);

        // -----------------------------------------------------------

        // 4. DOTween을 사용하여 행성 오브젝트 자체를 목표 회전값으로 부드럽게 회전시킵니다.
        transform.DOLocalRotateQuaternion(
            targetRotation,
            rotationDuration
        ).SetEase(easeType)
        .OnComplete(() =>
        {
            currentStageIndex = nextIndex;
            isAnimating = false;

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
            // SharedMaterial 대신 .material을 사용해야 인스턴스에만 적용됩니다.
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