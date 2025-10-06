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
    public float cameraBlendDelay = 1.0f; // 카메라 블렌딩 후 기능 활성화까지 대기 시간 (초)

    [Header("Cinemachine 제어")]
    public CinemachineVirtualCamera logoVCam;
    public CinemachineVirtualCamera chapterVCam;
    public int activePriority = 20;
    public int inactivePriority = 10;
    public float continuousRotationSpeed = 5f;

    [Header("Chapter Alignment")]
    [Tooltip("회전 보정을 위한 각도 오프셋.")]
    public float alignmentOffset = 0f; // 오프셋 변수 추가

    // === 내부 변수 ===
    private int currentChapterIndex = 0;
    private int totalChapters = 0;
    private bool isAnimating = false;
    private float currentTargetRotation = 0f;
    private StageSelector currentStageSelector = null;

    // 상태 제어를 위한 핵심 변수
    private bool isChapterSelectionActive = false; // 초기값: false (로고 모드)

    void Start()
    {
        totalChapters = planets.Count;
        currentTargetRotation = 0f;

        // VCam 초기 설정
        logoVCam.Priority = activePriority;
        chapterVCam.Priority = inactivePriority;

        // Chapter VCam Follow/LookAt은 고정 카메라 사용을 위해 null로 설정
        chapterVCam.Follow = null;
        chapterVCam.LookAt = null;

        isChapterSelectionActive = false;
        UpdatePlanetSelection(0); // 로고 모드 초기화
    }

    void Update()
    {
        // 1. 로고 모드
        if (!isChapterSelectionActive)
        {
            planetPivot.Rotate(0, continuousRotationSpeed * Time.deltaTime, 0, Space.Self);

            if (Input.GetMouseButtonDown(0))
            {
                //PlanetClicker.cs
            }
            return;
        }

        // 2. 챕터 선택 모드
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateChapterSelection();
            return;
        }

        // 인덱스를 역전하는 로직 없이, 상/하 입력에 따라 정직하게 -1, 1을 전달합니다.
        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeChapter(1); // 이전 챕터
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeChapter(-1); // 다음 챕터
        }
    }

    public int GetTotalChapters()
    {
        return totalChapters;
    }

    public void HandlePlanetClick(int chapterIndex)
    {
        // 챕터 선택 모드가 이미 활성화되어 있거나 애니메이션 중이면 무시
        if (isChapterSelectionActive || isAnimating) return;

        // 현재 챕터 인덱스를 클릭된 행성으로 설정
        currentChapterIndex = chapterIndex;

        // 챕터 선택 모드 활성화 시작
        ActivateChapterSelection();
    }

    public bool IsChapterSelectionActive()
    {
        return isChapterSelectionActive;
    }

    // 로고 상태에서 챕터 선택 상태로 전환하는 함수
    void ActivateChapterSelection()
    {
        isAnimating = true;
        isChapterSelectionActive = true;

        int targetIndex = currentChapterIndex;

        // 1. 카메라 전환
        logoVCam.Priority = inactivePriority;
        chapterVCam.Priority = activePriority;

        // VCam의 추적 대상 해제 (고정 시점 유지)
        chapterVCam.Follow = null;
        chapterVCam.LookAt = null;

        // 2. 행성 궤도 정렬을 위한 목표 회전값 계산
        float anglePerChapter = 360f / totalChapters;
        float targetYRotation = (targetIndex * anglePerChapter) + alignmentOffset;
        targetYRotation %= 360f;

        // DOTween Sequence 시작
        Sequence activationSequence = DOTween.Sequence();

        // 행성 궤도 정렬 애니메이션 추가
        activationSequence.Append(
      planetPivot.DORotate(
        new Vector3(0, targetYRotation, 0),
        rotationDuration
      ).SetEase(easeType)
    );

        // 카메라 블렌딩이 끝날 때까지 추가 대기
        activationSequence.AppendInterval(cameraBlendDelay);

        // Sequence 완료 시 기능 활성화
        activationSequence.OnComplete(() =>
        {
            UpdatePlanetSelection(currentChapterIndex);
            HighlightPlanet(planets[currentChapterIndex]);

            isAnimating = false;
        });
    }

    void DeactivateChapterSelection()
    {
        isChapterSelectionActive = false;
        isAnimating = false;

        // 행성 크기 원상 복구
        if (currentStageSelector != null)
        {
            Transform selectedPlanetTransform = currentStageSelector.transform;
            selectedPlanetTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
        }

        // 1. 카메라 전환
        chapterVCam.Priority = inactivePriority;
        logoVCam.Priority = activePriority;

        // 2. 챕터 기능 비활성화 (모든 스테이지 숨김)
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

        Vector3 targetRotationVector = new Vector3(
          0,
          targetYRotation,
          0
        );

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
            p.transform.DOScale(Vector3.one, 0.3f);
        }

        selectedPlanet.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutQuad);
    }

    void UpdatePlanetSelection(int newIndex)
    {
        for (int i = 0; i < planets.Count; i++)
        {
            StageSelector selector = planets[i].GetComponent<StageSelector>();
            if (selector == null) continue;

            bool isSelected = (i == newIndex);

            selector.enabled = isSelected;

            selector.SetStagesVisibility(isSelected && isChapterSelectionActive);

            if (isSelected)
            {
                currentStageSelector = selector;

                if (isChapterSelectionActive)
                {
                    HighlightPlanet(planets[i]);

                    selector.OnEnable();

                    if (selector != null)
                    {
                        selector.InitializeSelection();
                    }                    
                }
            }
        }
    }
}
