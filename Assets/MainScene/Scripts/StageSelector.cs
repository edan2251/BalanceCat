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

    [Header("하이라이트 설정")]
    // 인스펙터에 할당할 하이라이트 머티리얼
    public Material highlightMaterial;

    // 이전 선택된 스테이지의 MeshRenderer 컴포넌트를 저장
    private MeshRenderer lastSelectedRenderer;
    // 이전 선택된 스테이지의 원래 머티리얼을 저장
    private Material originalMaterial;


    void Start()
    {
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
        // UpdateStageVisibility 함수는 이제 필요 없으므로 제거했습니다.
    }

    void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeStage(-1); // 이전 스테이지
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeStage(1); // 다음 스테이지
        }
    }


    // 초기 회전 설정: 0번 인덱스 스테이지가 화면 중앙에 오도록 행성을 돌립니다.
    void InitializeRotation()
    {
        float anglePerStage = 360f / totalStages;

        // 0번 스테이지가 0도 위치에 오도록 해야 하므로, 목표 각도는 0입니다.
        // 현재 행성의 Y축 회전 값을 초기화하는 것이 목표입니다.
        transform.localEulerAngles = new Vector3(0, 0, 0);

        // 만약 0번이 아닌 다른 인덱스에서 시작해야 한다면, 아래 코드를 사용합니다.
        // float targetAngle = -currentStageIndex * anglePerStage; 
        // transform.localEulerAngles = new Vector3(0, targetAngle, 0); 

        // 주의: 행성 표면에 스테이지 포인트가 배치될 때, 0번 포인트가 Z축 방향(카메라 방향)에
        // 가까이 배치되었다고 가정합니다.
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

        // 우리가 원하는 것: pointLocalPosition 벡터를 행성의 로컬 Z축(정면, Vector3.forward)으로 옮기는 행성 회전값.

        // A. 스테이지 포인트를 행성의 로컬 -Z축(뒤쪽)으로 향하게 하는 회전값
        // LookRotation(target, up)을 사용하여 행성의 Up 벡터(Y축)를 보존하며 회전값을 계산합니다.
        Quaternion rotationToPointBack = Quaternion.LookRotation(
            -pointLocalPosition.normalized, // 행성 중심을 향하는 방향
            Vector3.up                     // 행성의 Up 벡터를 고정
        );

        // B. 정면을 향하게 하는 회전값 (rotationToPointBack의 역회전)
        // 이 회전값은 다음 스테이지 포인트가 행성의 로컬 +Z축에 오도록 합니다.
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

        int totalStages = stagePoints.Count;
        // 전체 라인에서 필요한 총 점의 개수 (마지막 지점은 루프 때문에 포함하지 않음)
        int totalPoints = totalStages * segmentsPerStage;

        pathRenderer.positionCount = totalPoints;

        Vector3 lastPointLocalPos = stagePoints[totalStages - 1].transform.localPosition;

        for (int i = 0; i < totalStages; i++)
        {
            Vector3 startLocalPos = stagePoints[i].transform.localPosition;

            // 루프 때문에 마지막 포인트는 첫 번째 포인트와 연결되어야 합니다.
            // 다음 포인트가 리스트의 끝이면, 0번 포인트의 위치를 가져옵니다.
            Vector3 endLocalPos = (i == totalStages - 1)
                                ? stagePoints[0].transform.localPosition
                                : stagePoints[i + 1].transform.localPosition;

            for (int j = 0; j < segmentsPerStage; j++)
            {
                // T 값: 0부터 1까지, 세그먼트 내의 현재 위치 비율
                float t = (float)j / segmentsPerStage;

                // **핵심: Slerp (구면 선형 보간)**
                // LineRenderer가 행성(구체)의 표면을 따라가도록 만듭니다.
                // Slerp는 두 벡터 사이의 구면을 따라 보간하며, 결과 벡터의 크기(radius)를 유지합니다.
                Vector3 currentPos = Vector3.Slerp(startLocalPos, endLocalPos, t);

                // Slerp 결과 벡터의 크기를 행성 반지름으로 정규화/스케일링
                // 이렇게 하면 모든 중간 지점이 정확히 행성 표면(반지름)에 위치하게 됩니다.
                currentPos = currentPos.normalized * radius;

                // LineRenderer의 PositionCount에 맞게 인덱스 계산
                int pointIndex = (i * segmentsPerStage) + j;
                pathRenderer.SetPosition(pointIndex, currentPos);
            }
        }

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