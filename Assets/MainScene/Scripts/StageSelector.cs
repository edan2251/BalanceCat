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

    private ChapterSelector chapterController;

    [Header("���̶���Ʈ ����")]
    // �ν����Ϳ� �Ҵ��� ���̶���Ʈ ��Ƽ����
    public Material highlightMaterial;

    // ���� ���õ� ���������� MeshRenderer ������Ʈ�� ����
    private MeshRenderer lastSelectedRenderer;
    // ���� ���õ� ���������� ���� ��Ƽ������ ����
    private Material originalMaterial;


    void Start()
    {
        chapterController = FindObjectOfType<ChapterSelector>();
        if (chapterController == null)
        {
            Debug.LogError("ChapterSelector�� ã�� �� �����ϴ�. PlanetPivot�� �پ� �ִ��� Ȯ���ϼ���.");
        }

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
    }

    void Update()
    {
        if (isAnimating || chapterController == null || !chapterController.IsChapterSelectionActive())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeStage(-1); // ���� ��������
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ChangeStage(1); // ���� ��������
        }
    }


    // �ʱ� ȸ�� ����: 0�� �ε��� ���������� ȭ�� �߾ӿ� ������ �༺�� �����ϴ�.
    void InitializeRotation()
    {
        float anglePerStage = 360f / totalStages;
        transform.localEulerAngles = new Vector3(0, 0, 0);
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

        // 3-1. Up ���� ����:
        Vector3 localUp = Vector3.up;

        // 3-2. LookRotation ���: �������� ����Ʈ�� �༺�� ���� -Z�࿡ ������ �ϴ� ȸ����
        Quaternion rotationToPointBack = Quaternion.LookRotation(
            -pointLocalPosition.normalized, // �༺ �߽� ���� (Local -Z�࿡ ���ĵ� ����)
            localUp                        // Y���� �������� ȸ���ϵ��� ����
        );

        // 3-3. ���� ���� ��ǥ ȸ����: Inverse�� �����Ͽ� �������� ����Ʈ�� ���� +Z�࿡ ������ �մϴ�.
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

        // �����ؾ� �� ���׸�Ʈ�� �� ���� (7���� 6���� ����)
        int totalSegments = stagePoints.Count - 1;
        if (totalSegments <= 0)
        {
            pathRenderer.positionCount = 0;
            return;
        }

        // �ʿ��� �� ���� ����
        int totalPoints = totalSegments * segmentsPerStage;

        pathRenderer.positionCount = totalPoints;

        // ��ȯ ������ �ڵ�� ����� ����
        pathRenderer.loop = false;

        for (int i = 0; i < totalSegments; i++) // 
        {
            Vector3 startLocalPos = stagePoints[i].transform.localPosition;
            Vector3 endLocalPos = stagePoints[i + 1].transform.localPosition; // ���� ����Ʈ

            for (int j = 0; j < segmentsPerStage; j++)
            {
                float t = (float)j / segmentsPerStage;

                // Slerp (���� ���� ����)
                Vector3 currentPos = Vector3.Slerp(startLocalPos, endLocalPos, t);
                currentPos = currentPos.normalized * radius;

                // LineRenderer�� PositionCount�� �°� �ε��� ���
                int pointIndex = (i * segmentsPerStage) + j;

                pathRenderer.SetPosition(pointIndex, currentPos);
            }
        }

        // ������ ���� (7�� ��������)�� ��������� �߰��մϴ�.
        pathRenderer.positionCount = totalPoints + 1;
        pathRenderer.SetPosition(totalPoints, stagePoints[totalSegments].transform.localPosition);
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