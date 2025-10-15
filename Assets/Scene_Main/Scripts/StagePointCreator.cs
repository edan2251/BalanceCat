using UnityEngine;
using System.Collections.Generic;

public class StagePointCreator : MonoBehaviour
{
    // ������ �������� ����Ʈ ������ (���� ��ü ��)
    public GameObject stagePointPrefab;

    // ������ �������� ����Ʈ�� �� ����
    public int numberOfStages = 7;

    // �༺ ������ (����Ʈ�� �༺ ǥ�鿡 �ٰ� �ϱ� ����)
    public float planetRadius = 1.0f;

    [Header("��� ���� ���� (����/Y�� ��������)")]
    // ���Ʒ��� �������� ���� (0�̸� ��� ������, ���ڰ� Ŀ���� �� �ش����� �������� ����)
    public float maxPathWiggle = 0.5f;

    // ������ �������� ����Ʈ ����Ʈ (StageSelector���� ����� ����Ʈ)
    [HideInInspector]
    public List<GameObject> createdPoints = new List<GameObject>();

    // �ν����Ϳ��� ���콺 ������ ��ư�� ���� ������ �� �ֽ��ϴ�.
    [ContextMenu("1. Create Stage Points Automatically")]
    private void CreateStagePoints()
    {
        // ���� ����Ʈ ���� (����� ��)
        ClearExistingPoints();

        float angleStep = 360f / numberOfStages; // �浵 ����
        planetRadius = transform.localScale.x * 0.5f; // ��ü�� �⺻ ������ ����

        for (int i = 0; i < numberOfStages; i++)
        {
            // 1. �浵 (Longitude) ���: ������ ����
            float longitudeAngle = i * angleStep;

            // 2. ���� (Latitude) ���: ������ ���� �߰�
            // sin �Լ��� ����Ͽ� ������ ������ �ð��� ���� ���ݾ� �ٸ��� �ݴϴ�.
            // i/numberOfStages�� ��ü ��� ���൵�� ��� �ڿ������� ������ ����ϴ�.
            float latitudeOffset = Mathf.Sin(i * 0.5f) * maxPathWiggle;

            // 3. 3D ��ġ ���
            // Quaternion.Euler�� ȸ�� ���� ���� ��, Vector3.forward(Z��)�� ���Ͽ� ��ġ�� ���մϴ�.
            Quaternion rotation = Quaternion.Euler(latitudeOffset * 90f, longitudeAngle, 0);
            Vector3 position = rotation * Vector3.forward * planetRadius;

            // 4. ������Ʈ ����
            GameObject newPoint = Instantiate(stagePointPrefab, transform);
            newPoint.name = $"Stage_{i + 1}";
            newPoint.transform.localPosition = position;
            // �׻� �༺�� �߽��� �ٶ󺸵��� ȸ�� (���� ����)
            newPoint.transform.rotation = Quaternion.LookRotation(position.normalized);

            createdPoints.Add(newPoint);
        }

        Debug.Log($"{numberOfStages}���� �������� ����Ʈ�� �ڵ����� �����Ǿ����ϴ�.");

        // ���� �� StageSelector�� ����Ʈ�� �ڵ� �Ҵ� (���� ����: Inspector���� ���� ���� ����)
        StageSelector selector = GetComponent<StageSelector>();
        if (selector != null)
        {
            selector.stagePoints = createdPoints;
        }
    }

    [ContextMenu("2. Clear Existing Points")]
    private void ClearExistingPoints()
    {
        // �ν����Ϳ��� ������ ����Ʈ���� ����ϰ� ����ϴ�.
        for (int i = createdPoints.Count - 1; i >= 0; i--)
        {
            if (createdPoints[i] != null)
            {
                DestroyImmediate(createdPoints[i]);
            }
        }
        createdPoints.Clear();
    }
}