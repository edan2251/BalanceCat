using UnityEngine;
using DG.Tweening; // DOTween�� ����մϴ�.
using System.Collections.Generic;

public class ChapterSelector : MonoBehaviour
{
    // === �ν����� ���� ===
    public List<GameObject> planets; // 5���� �༺ ������Ʈ ����Ʈ
    public Transform planetPivot;    // �༺���� �θ� (ȸ�� ��)
    public Camera mainCamera;        // ���� ī�޶�

    [Header("DOTween ����")]
    public float rotationDuration = 0.5f; // ȸ�� �ð�
    public Ease easeType = Ease.OutBack;  // �ִϸ��̼� �

    // === ���� ���� ===
    private int currentChapterIndex = 0;
    private int totalChapters = 0;
    private bool isAnimating = false;
    private float currentTargetRotation = 0f;

    // ���� Ȱ��ȭ�� StageSelector�� ������ ����
    private StageSelector currentStageSelector = null;

    void Start()
    {
        totalChapters = planets.Count;
        // �ʱ� ���� ��ġ ���� (��: 0�� �༺�� �˵��� 0�� ��ġ�� �ְ� �մϴ�)
        currentTargetRotation = 0f;
        UpdatePlanetSelection(0);
    }

    void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeChapter(-1); // ���� é�� (�˵� �ݽð� ���� ȸ��)
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeChapter(1); // ���� é�� (�˵� �ð� ���� ȸ��)
        }
    }

    // é�� ���� ����
    void ChangeChapter(int direction)
    {
        if (isAnimating) return;
        isAnimating = true;

        // �ε��� ��� �� ��ȯ
        int nextIndex = (currentChapterIndex + direction + totalChapters) % totalChapters;

        // 1. �༺ 1���� ȸ�� ���� ���
        float anglePerChapter = 360f / totalChapters;

        // 2. ��ǥ Y�� ȸ�� ���� ���
        // ���� é�Ͱ� ���� �˵� ��ġ�� ������ ��ǥ ������ �����մϴ�.
        float targetYRotation = -nextIndex * anglePerChapter;
        targetYRotation %= 360f;

        // 3. ��ǥ Vector3 ȸ�� �� ���� (�ٽ� ����)
        //  PlanetPivot�� ���� X�� ȸ�� ���� �״�� �����մϴ�.
        // Z�� ȸ�� ���� ���� ���� �״�� �����մϴ�.
        float currentXRotation = planetPivot.localEulerAngles.x;
        float currentZRotation = planetPivot.localEulerAngles.z;

        Vector3 targetRotationVector = new Vector3(
            currentXRotation,     // �߶Ծ��� X�� ȸ�� �� ����
            targetYRotation,      // ���� ��ǥ Y�� ȸ�� �� ����
            currentZRotation      // Z�� ȸ�� �� ����
        );

        // 4. PlanetPivot ȸ�� ����
        planetPivot.DORotate(
            targetRotationVector, // Vector3 ��ǥ ȸ�� ��
            rotationDuration
        ).SetEase(easeType)
        .OnComplete(() =>
        {
            currentChapterIndex = nextIndex;
            // ���� ȸ���� ������ �� ���� ��ǥ ����(Y��)�� ���� (�ʼ��� �ƴ����� �ϰ��� ����)
            currentTargetRotation = targetYRotation;

            UpdatePlanetSelection(currentChapterIndex);
            isAnimating = false;
            HighlightPlanet(planets[currentChapterIndex]);
        });
    }

    // ���õ� �༺ ���̶���Ʈ �� ī�޶� �� (����)
    void HighlightPlanet(GameObject selectedPlanet)
    {
        // ��� �༺ ũ�� �ʱ�ȭ (����)
        foreach (var p in planets)
        {
            p.transform.DOScale(Vector3.one, 0.3f);
        }

        // ���õ� �༺ Ȯ�� �� ī�޶� �̵� (��������� ����)
        selectedPlanet.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutQuad);

        // **Cinemachine ����� �����մϴ�.**
        // Cinemachine Virtual Camera�� ���õ� �༺�� �����Ͽ� �ε巴�� �� ��/�ƿ� ȿ���� �� �� �ֽ��ϴ�.
        // ����� ������ �༺ ũ�� ��ȭ�� ��ü�߽��ϴ�.
    }

    // ������ �Ϸ�� �� ȣ��Ǿ�� �մϴ�.
    void UpdatePlanetSelection(int newIndex)
    {
        // 1. ��� �༺�� ��ȸ�ϸ� ���ü��� ��ũ��Ʈ Ȱ��ȭ ����
        for (int i = 0; i < planets.Count; i++)
        {
            StageSelector selector = planets[i].GetComponent<StageSelector>();
            if (selector == null) continue;

            bool isSelected = (i == newIndex);

            // a. ��ũ��Ʈ Ȱ��ȭ/��Ȱ��ȭ (�Է� ����)
            selector.enabled = isSelected;

            // b. �������� ����Ʈ �� ���� ���ü� ����
            selector.SetStagesVisibility(isSelected);

            if (isSelected)
            {
                // ���� ���õ� StageSelector ������Ʈ
                currentStageSelector = selector;
                // c. ���õ� �༺ ���̶���Ʈ
                HighlightPlanet(planets[i]);
            }
        }
    }

}