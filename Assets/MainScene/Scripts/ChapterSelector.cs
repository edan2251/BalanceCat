using UnityEngine;
using DG.Tweening; // DOTween을 사용합니다.
using System.Collections.Generic;

public class ChapterSelector : MonoBehaviour
{
    // === 인스펙터 설정 ===
    public List<GameObject> planets; // 5개의 행성 오브젝트 리스트
    public Transform planetPivot;    // 행성들의 부모 (회전 축)
    public Camera mainCamera;        // 메인 카메라

    [Header("DOTween 설정")]
    public float rotationDuration = 0.5f; // 회전 시간
    public Ease easeType = Ease.OutBack;  // 애니메이션 곡선

    // === 내부 변수 ===
    private int currentChapterIndex = 0;
    private int totalChapters = 0;
    private bool isAnimating = false;
    private float currentTargetRotation = 0f;

    // 현재 활성화된 StageSelector를 저장할 변수
    private StageSelector currentStageSelector = null;

    void Start()
    {
        totalChapters = planets.Count;
        // 초기 시작 위치 설정 (예: 0번 행성이 궤도의 0도 위치에 있게 합니다)
        currentTargetRotation = 0f;
        UpdatePlanetSelection(0);
    }

    void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeChapter(-1); // 이전 챕터 (궤도 반시계 방향 회전)
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeChapter(1); // 다음 챕터 (궤도 시계 방향 회전)
        }
    }

    // 챕터 변경 로직
    void ChangeChapter(int direction)
    {
        if (isAnimating) return;
        isAnimating = true;

        // 인덱스 계산 및 순환
        int nextIndex = (currentChapterIndex + direction + totalChapters) % totalChapters;

        // 1. 행성 1개당 회전 각도 계산
        float anglePerChapter = 360f / totalChapters;

        // 2. 목표 Y축 회전 각도 계산
        // 다음 챕터가 정면 궤도 위치에 오도록 목표 각도를 설정합니다.
        float targetYRotation = -nextIndex * anglePerChapter;
        targetYRotation %= 360f;

        // 3. 목표 Vector3 회전 값 설정 (핵심 수정)
        //  PlanetPivot의 현재 X축 회전 값을 그대로 유지합니다.
        // Z축 회전 값도 현재 값을 그대로 유지합니다.
        float currentXRotation = planetPivot.localEulerAngles.x;
        float currentZRotation = planetPivot.localEulerAngles.z;

        Vector3 targetRotationVector = new Vector3(
            currentXRotation,     // 삐뚤어진 X축 회전 값 유지
            targetYRotation,      // 계산된 목표 Y축 회전 값 적용
            currentZRotation      // Z축 회전 값 유지
        );

        // 4. PlanetPivot 회전 실행
        planetPivot.DORotate(
            targetRotationVector, // Vector3 목표 회전 값
            rotationDuration
        ).SetEase(easeType)
        .OnComplete(() =>
        {
            currentChapterIndex = nextIndex;
            // 다음 회전의 기준이 될 현재 목표 각도(Y축)를 저장 (필수는 아니지만 일관성 유지)
            currentTargetRotation = targetYRotation;

            UpdatePlanetSelection(currentChapterIndex);
            isAnimating = false;
            HighlightPlanet(planets[currentChapterIndex]);
        });
    }

    // 선택된 행성 하이라이트 및 카메라 줌 (예시)
    void HighlightPlanet(GameObject selectedPlanet)
    {
        // 모든 행성 크기 초기화 (예시)
        foreach (var p in planets)
        {
            p.transform.DOScale(Vector3.one, 0.3f);
        }

        // 선택된 행성 확대 및 카메라 이동 (가까워지는 느낌)
        selectedPlanet.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutQuad);

        // **Cinemachine 사용을 권장합니다.**
        // Cinemachine Virtual Camera를 선택된 행성에 연결하여 부드럽게 줌 인/아웃 효과를 줄 수 있습니다.
        // 현재는 간단히 행성 크기 변화로 대체했습니다.
    }

    // 선택이 완료된 후 호출되어야 합니다.
    void UpdatePlanetSelection(int newIndex)
    {
        // 1. 모든 행성을 순회하며 가시성과 스크립트 활성화 제어
        for (int i = 0; i < planets.Count; i++)
        {
            StageSelector selector = planets[i].GetComponent<StageSelector>();
            if (selector == null) continue;

            bool isSelected = (i == newIndex);

            // a. 스크립트 활성화/비활성화 (입력 제어)
            selector.enabled = isSelected;

            // b. 스테이지 포인트 및 라인 가시성 제어
            selector.SetStagesVisibility(isSelected);

            if (isSelected)
            {
                // 현재 선택된 StageSelector 업데이트
                currentStageSelector = selector;
                // c. 선택된 행성 하이라이트
                HighlightPlanet(planets[i]);
            }
        }
    }

}