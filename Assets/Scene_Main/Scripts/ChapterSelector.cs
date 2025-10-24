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

    // === 참조 관리 ===
    public StageUIManager uiManager;
    [Header("메인 UI 관리자")]
    public MainUIManager mainUIManager; // MainUIManager 참조 추가

    // === 내부 변수 ===
    private int currentChapterIndex = 0;
    private int totalChapters = 0;
    private bool isAnimating = false;
    private float currentTargetRotation = 0f;
    private StageSelector currentStageSelector = null;

    // 새로운 로직을 위한 변수
    private bool isFirstEntry = true; // 첫 진입인지 확인
    private int lastSelectedChapterIndex = 0; // 마지막으로 선택된 챕터를 저장

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
        UpdatePlanetSelection(0); // 로고 모드 초기화 (모든 스테이지 비활성화)

        if (uiManager != null)
        {
            uiManager.HideUI(); // Stage UI 숨김
        }

        // MainUIManager가 있다면 로고 모드에서는 보이게 처리 (Start 버튼이 있어야 하므로)
        if (mainUIManager != null)
        {
            mainUIManager.ShowUI();
        }
    }

    void Update()
    {
        // 1. 로고 모드
        if (!isChapterSelectionActive)
        {
            // 로고 모드에서는 Stage UI 숨김 확인
            if (uiManager != null && uiManager.mainUIPanel != null && uiManager.mainUIPanel.activeSelf)
            {
                uiManager.HideUI();
            }

            // 회전
            planetPivot.Rotate(0, continuousRotationSpeed * Time.deltaTime, 0, Space.Self);

            // 로고 모드에서는 클릭 처리가 필요 없습니다.
            return;
        }

        // 2. 챕터 선택 모드
        if (isAnimating) return;

        // ESC 입력 시 로고 모드로 복귀
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateChapterSelection();
            return;
        }

        // 챕터 변경
        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeChapter(1); // 이전 챕터 (index 감소, 회전은 반대)
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeChapter(-1); // 다음 챕터 (index 증가, 회전은 반대)
        }
    }

    public int GetTotalChapters()
    {
        return totalChapters;
    }

    // MainUIManager의 Play 버튼에 연결될 공개 함수
    public void EnterChapterSelectionFromUI()
    {
        // 챕터 선택 모드가 이미 활성화되어 있거나 애니메이션 중이면 무시
        if (isChapterSelectionActive || isAnimating) return;

        // Main UI 숨기기
        if (mainUIManager != null)
        {
            mainUIManager.HideUI();
        }

        // 첫 진입인 경우 1챕터(인덱스 0)를 선택, 아니면 마지막 선택 챕터를 선택
        int chapterToSelect = isFirstEntry ? 0 : lastSelectedChapterIndex;

        currentChapterIndex = chapterToSelect;
        isFirstEntry = false; // 첫 진입 플래그를 false로 설정

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
        // 마지막으로 선택했던 챕터 인덱스를 저장
        lastSelectedChapterIndex = currentChapterIndex;

        isChapterSelectionActive = false;
        isAnimating = false;

        // Stage UI 숨김
        if (uiManager != null)
        {
            uiManager.HideUI();
        }

        // Main UI 다시 표시 (Start 버튼이 보이도록)
        if (mainUIManager != null)
        {
            mainUIManager.ShowUI();
        }


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
        // 로고 모드로 돌아갈 때는 스테이지를 숨기지만, 챕터 인덱스 자체는 유지
        UpdatePlanetSelection(currentChapterIndex);
    }

    void ChangeChapter(int direction)
    {
        if (isAnimating) return;
        isAnimating = true;

        // 챕터 인덱스 변경
        int nextIndex = (currentChapterIndex + direction + totalChapters) % totalChapters;

        // 목표 회전값 계산
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
            // 선택 해제되는 행성도 부드럽게 크기를 복구
            if (p != selectedPlanet)
            {
                p.transform.DOScale(Vector3.one, 0.3f);
            }
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

            // StageSelector 활성화/비활성화 (Update 로직이 있다면)
            selector.enabled = isSelected;

            // 스테이지 가시성 설정 (선택 모드일 때만 스테이지 표시)
            selector.SetStagesVisibility(isSelected && isChapterSelectionActive);

            if (isSelected)
            {
                currentStageSelector = selector;

                if (isChapterSelectionActive)
                {
                    // Stage UI 표시
                    if (uiManager != null)
                    {
                        if (uiManager.mainUIPanel != null && !uiManager.mainUIPanel.activeSelf)
                        {
                            uiManager.mainUIPanel.SetActive(true);
                        }
                    }

                    HighlightPlanet(planets[i]);

                    // StageSelector의 활성화 로직 실행
                    // Note: OnEnable은 이미 selector.enabled = true에 의해 호출될 수 있음.
                    // 명시적 초기화 함수 호출
                    if (selector != null)
                    {
                        selector.InitializeSelection();
                    }
                }
            }
        }
    }
}