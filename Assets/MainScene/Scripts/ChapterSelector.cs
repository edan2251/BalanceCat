using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using Cinemachine;

public class ChapterSelector : MonoBehaviour
{
    // === �ν����� ���� ===
    public List<GameObject> planets;
    public Transform planetPivot;
    public Camera mainCamera; 

    [Header("DOTween ����")]
    public float rotationDuration = 0.5f;
    public Ease easeType = Ease.OutBack;
    public float cameraBlendDelay = 1.0f; // ī�޶� ���� �� ��� Ȱ��ȭ���� ��� �ð� (��)

    [Header("Cinemachine ����")]
    public CinemachineVirtualCamera logoVCam;
    public CinemachineVirtualCamera chapterVCam;
    public int activePriority = 20;
    public int inactivePriority = 10;
    public float continuousRotationSpeed = 5f;

    [Header("Chapter Alignment")]
    [Tooltip("ȸ�� ������ ���� ���� ������.")]
    public float alignmentOffset = 0f; // ������ ���� �߰�

    // === ���� ���� ===
    private int currentChapterIndex = 0;
    private int totalChapters = 0;
    private bool isAnimating = false;
    private float currentTargetRotation = 0f;
    private StageSelector currentStageSelector = null;

    // ���� ��� ���� �ٽ� ����
    private bool isChapterSelectionActive = false; // �ʱⰪ: false (�ΰ� ���)

    void Start()
    {
        totalChapters = planets.Count;
        currentTargetRotation = 0f;

        // VCam �ʱ� ����
        logoVCam.Priority = activePriority;
        chapterVCam.Priority = inactivePriority;

        // Chapter VCam Follow/LookAt�� ���� ī�޶� ����� ���� null�� ����
        chapterVCam.Follow = null;
        chapterVCam.LookAt = null;

        isChapterSelectionActive = false;
        UpdatePlanetSelection(0); // �ΰ� ��� �ʱ�ȭ
    }

    void Update()
    {
        // 1. �ΰ� ���
        if (!isChapterSelectionActive)
        {
            planetPivot.Rotate(0, continuousRotationSpeed * Time.deltaTime, 0, Space.Self);

            if (Input.GetMouseButtonDown(0))
            {
                //PlanetClicker.cs
            }
            return;
        }

        // 2. é�� ���� ���
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateChapterSelection();
            return;
        }

        // �ε����� �����ϴ� ���� ����, ��/�� �Է¿� ���� �����ϰ� -1, 1�� �����մϴ�.
        if (Input.GetKeyDown(KeyCode.W))
        {
            ChangeChapter(1); // ���� é��
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ChangeChapter(-1); // ���� é��
        }
    }

    public int GetTotalChapters()
    {
        return totalChapters;
    }

    public void HandlePlanetClick(int chapterIndex)
    {
        // é�� ���� ��尡 �̹� Ȱ��ȭ�Ǿ� �ְų� �ִϸ��̼� ���̸� ����
        if (isChapterSelectionActive || isAnimating) return;

        // ���� é�� �ε����� Ŭ���� �༺���� ����
        currentChapterIndex = chapterIndex;

        // é�� ���� ��� Ȱ��ȭ ����
        ActivateChapterSelection();
    }

    public bool IsChapterSelectionActive()
    {
        return isChapterSelectionActive;
    }

    // �ΰ� ���¿��� é�� ���� ���·� ��ȯ�ϴ� �Լ�
    void ActivateChapterSelection()
    {
        isAnimating = true;
        isChapterSelectionActive = true;

        int targetIndex = currentChapterIndex;

        // 1. ī�޶� ��ȯ
        logoVCam.Priority = inactivePriority;
        chapterVCam.Priority = activePriority;

        // VCam�� ���� ��� ���� (���� ���� ����)
        chapterVCam.Follow = null;
        chapterVCam.LookAt = null;

        // 2. �༺ �˵� ������ ���� ��ǥ ȸ���� ���
        float anglePerChapter = 360f / totalChapters;
        float targetYRotation = (targetIndex * anglePerChapter) + alignmentOffset;
        targetYRotation %= 360f;

        // DOTween Sequence ����
        Sequence activationSequence = DOTween.Sequence();

        // �༺ �˵� ���� �ִϸ��̼� �߰�
        activationSequence.Append(
            planetPivot.DORotate(
                new Vector3(0, targetYRotation, 0),
                rotationDuration
            ).SetEase(easeType)
        );

        // ī�޶� ������ ���� ������ �߰� ���
        activationSequence.AppendInterval(cameraBlendDelay);

        // Sequence �Ϸ� �� ��� Ȱ��ȭ
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

        // �༺ ũ�� ���� ����
        if (currentStageSelector != null)
        {
            Transform selectedPlanetTransform = currentStageSelector.transform;
            selectedPlanetTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuad);
        }

        // 1. ī�޶� ��ȯ
        chapterVCam.Priority = inactivePriority;
        logoVCam.Priority = activePriority;

        // 2. é�� ��� ��Ȱ��ȭ (��� �������� ����)
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
                }
            }
        }
    }
}