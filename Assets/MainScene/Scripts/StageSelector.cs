using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class StageSelector : MonoBehaviour
{
    // === �ν����� ���� ===
    // �� ����Ʈ�� ChapterPlanetCreator�� �������� ����˴ϴ�.
    public List<GameObject> stagePoints;
    public float rotationDuration = 0.3f;
    public Ease easeType = Ease.OutQuad;

    [Header("���� ���� ����")]
    public LineRenderer pathRenderer;
    public int segmentsPerStage = 20;

    // === ���� ���� ===
    private int currentStageIndex = 0;
    private int totalStages = 0;
    private bool isAnimating = false;

    [Header("���̶���Ʈ ����")]
    // �ν����Ϳ� �Ҵ��� ���̶���Ʈ ��Ƽ����
    public Material highlightMaterial;

    // ���� ���õ� ���������� MeshRenderer ������Ʈ�� ����
    private MeshRenderer lastSelectedRenderer;
    // ���� ���õ� ���������� ���� ��Ƽ������ ����
    private Material originalMaterial;


    void Start()
    {
        totalStages = stagePoints.Count;

        if (stagePoints.Count > 0)
        {
            HighlightStage(stagePoints[0]);
        }

        //ó������ StageSelector�� ��Ȱ��ȭ ���·� �����ؾ� �մϴ�.
        // ChapterSelector�� ���õ� �༺�� Ȱ��ȭ�մϴ�.
        this.enabled = false;

        // ��� �������� ����Ʈ�� ���̵��� ���� (����� ��û �ݿ�)
        foreach (var point in stagePoints)
        {
            point.SetActive(true);
        }

        // ���� �ʱ�ȭ (�༺ �������� �༺ ������ ������� ����)
        float planetRadius = transform.localScale.x * 0.5f;
        DrawCurvedPath(planetRadius);

        InitializeRotation();
        // UpdateStageVisibility �Լ��� ���� �ʿ� �����Ƿ� �����߽��ϴ�.
    }

    void Update()
    {
        if (isAnimating) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeStage(-1); // ���� ��������
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeStage(1); // ���� ��������
        }
    }


    // �ʱ� ȸ�� ����: 0�� �ε��� ���������� ȭ�� �߾ӿ� ������ �༺�� �����ϴ�.
    void InitializeRotation()
    {
        float anglePerStage = 360f / totalStages;

        // 0�� ���������� 0�� ��ġ�� ������ �ؾ� �ϹǷ�, ��ǥ ������ 0�Դϴ�.
        // ���� �༺�� Y�� ȸ�� ���� �ʱ�ȭ�ϴ� ���� ��ǥ�Դϴ�.
        transform.localEulerAngles = new Vector3(0, 0, 0);

        // ���� 0���� �ƴ� �ٸ� �ε������� �����ؾ� �Ѵٸ�, �Ʒ� �ڵ带 ����մϴ�.
        // float targetAngle = -currentStageIndex * anglePerStage; 
        // transform.localEulerAngles = new Vector3(0, targetAngle, 0); 

        // ����: �༺ ǥ�鿡 �������� ����Ʈ�� ��ġ�� ��, 0�� ����Ʈ�� Z�� ����(ī�޶� ����)��
        // ������ ��ġ�Ǿ��ٰ� �����մϴ�.
    }


    // �������� ���� ����
    void ChangeStage(int direction)
    {
        // 1. ���� �������� �ε��� ��� �� ��ȯ
        int nextIndex = (currentStageIndex + direction + totalStages) % totalStages;

        isAnimating = true;

        // 2. ���� ���õ� �������� ����Ʈ�� ���� ��ġ�� �����ɴϴ�.
        GameObject nextStagePoint = stagePoints[nextIndex];
        Vector3 pointLocalPosition = nextStagePoint.transform.localPosition;

        // 3. ��ǥ ȸ�� ��� (���� ����ȭ ����)
        // -----------------------------------------------------------

        // �츮�� ���ϴ� ��: pointLocalPosition ���͸� �༺�� ���� Z��(����, Vector3.forward)���� �ű�� �༺ ȸ����.

        // A. �������� ����Ʈ�� �༺�� ���� -Z��(����)���� ���ϰ� �ϴ� ȸ����
        // LookRotation(target, up)�� ����Ͽ� �༺�� Up ����(Y��)�� �����ϸ� ȸ������ ����մϴ�.
        Quaternion rotationToPointBack = Quaternion.LookRotation(
            -pointLocalPosition.normalized, // �༺ �߽��� ���ϴ� ����
            Vector3.up                     // �༺�� Up ���͸� ����
        );

        // B. ������ ���ϰ� �ϴ� ȸ���� (rotationToPointBack�� ��ȸ��)
        // �� ȸ������ ���� �������� ����Ʈ�� �༺�� ���� +Z�࿡ ������ �մϴ�.
        Quaternion targetRotation = Quaternion.Inverse(rotationToPointBack);

        // -----------------------------------------------------------

        // 4. DOTween�� ����Ͽ� �༺ ������Ʈ ��ü�� ��ǥ ȸ�������� �ε巴�� ȸ����ŵ�ϴ�.
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

    // ���õ� �������� ���̶���Ʈ (����)
    void HighlightStage(GameObject selectedPoint)
    {
        // 1. ���� �������� ����
        if (lastSelectedRenderer != null && originalMaterial != null)
        {
            // ������ ���õ� ���������� ��Ƽ������ ���� ��Ƽ����� �ǵ����ϴ�.
            lastSelectedRenderer.material = originalMaterial;
        }

        // 2. ���ο� �������� ���̶���Ʈ
        MeshRenderer currentRenderer = selectedPoint.GetComponent<MeshRenderer>();

        if (currentRenderer != null && highlightMaterial != null)
        {
            // 2-1. ���� ��Ƽ���� ����
            // SharedMaterial ��� .material�� ����ؾ� �ν��Ͻ����� ����˴ϴ�.
            originalMaterial = currentRenderer.material;

            // 2-2. ���̶���Ʈ ��Ƽ����� ��ü
            currentRenderer.material = highlightMaterial;

            // 2-3. ���� Renderer�� �����Ͽ� ���� ���� ������ �� �ֵ��� �غ�
            lastSelectedRenderer = currentRenderer;
        }
        else if (currentRenderer == null)
        {
            // �������� ����Ʈ�� MeshRenderer�� ������ ��� �޽��� ���
            Debug.LogWarning($"{selectedPoint.name}�� MeshRenderer ������Ʈ�� �����ϴ�. ���̶���Ʈ �� �� �����ϴ�.");
        }
    }

    // ������ �������� ����Ʈ�� LocalPosition�� �������� �༺ ǥ���� ���󰡵��� �׸��ϴ�.
    public void DrawCurvedPath(float radius)
    {
        if (pathRenderer == null || stagePoints.Count < 2) return;

        int totalStages = stagePoints.Count;
        // ��ü ���ο��� �ʿ��� �� ���� ���� (������ ������ ���� ������ �������� ����)
        int totalPoints = totalStages * segmentsPerStage;

        pathRenderer.positionCount = totalPoints;

        Vector3 lastPointLocalPos = stagePoints[totalStages - 1].transform.localPosition;

        for (int i = 0; i < totalStages; i++)
        {
            Vector3 startLocalPos = stagePoints[i].transform.localPosition;

            // ���� ������ ������ ����Ʈ�� ù ��° ����Ʈ�� ����Ǿ�� �մϴ�.
            // ���� ����Ʈ�� ����Ʈ�� ���̸�, 0�� ����Ʈ�� ��ġ�� �����ɴϴ�.
            Vector3 endLocalPos = (i == totalStages - 1)
                                ? stagePoints[0].transform.localPosition
                                : stagePoints[i + 1].transform.localPosition;

            for (int j = 0; j < segmentsPerStage; j++)
            {
                // T ��: 0���� 1����, ���׸�Ʈ ���� ���� ��ġ ����
                float t = (float)j / segmentsPerStage;

                // **�ٽ�: Slerp (���� ���� ����)**
                // LineRenderer�� �༺(��ü)�� ǥ���� ���󰡵��� ����ϴ�.
                // Slerp�� �� ���� ������ ������ ���� �����ϸ�, ��� ������ ũ��(radius)�� �����մϴ�.
                Vector3 currentPos = Vector3.Slerp(startLocalPos, endLocalPos, t);

                // Slerp ��� ������ ũ�⸦ �༺ ���������� ����ȭ/�����ϸ�
                // �̷��� �ϸ� ��� �߰� ������ ��Ȯ�� �༺ ǥ��(������)�� ��ġ�ϰ� �˴ϴ�.
                currentPos = currentPos.normalized * radius;

                // LineRenderer�� PositionCount�� �°� �ε��� ���
                int pointIndex = (i * segmentsPerStage) + j;
                pathRenderer.SetPosition(pointIndex, currentPos);
            }
        }

    }

    public void SetStagesVisibility(bool isVisible)
    {
        // �������� ����Ʈ ��ü ���ü� ����
        foreach (var point in stagePoints)
        {
            if (point != null)
            {
                point.SetActive(isVisible);
            }
        }

        // ���� ������ ���ü� ����
        if (pathRenderer != null)
        {
            pathRenderer.enabled = isVisible;
        }
    }
}