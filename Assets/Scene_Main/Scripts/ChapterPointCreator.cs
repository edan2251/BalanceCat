using UnityEngine;
using System.Collections.Generic;

public class ChapterPlanetCreator : MonoBehaviour
{
    // === �ν����� ���� ���� ===

    // ������ �༺ ������ (3D ��ü �Ǵ� ��)
    public GameObject planetPrefab;

    // �˵� ������ (�߽ɿ��� �༺������ �Ÿ�)
    public float orbitRadius = 10f;

    // ������ �༺(é��)�� �� ����
    public int numberOfPlanets = 5;

    // ������ �༺ ����Ʈ (ChapterSelector���� ����� ����Ʈ)
    [HideInInspector]
    public List<GameObject> createdPlanets = new List<GameObject>();


    // �ν����Ϳ��� ���콺 ������ ��ư�� ���� ������ �� �ֽ��ϴ�.
    [ContextMenu("1. Create Planets on Orbit")]
    private void CreatePlanetsOnOrbit()
    {
        // 1. ���� �༺ ���� (����� ��)
        ClearExistingPlanets();

        if (numberOfPlanets <= 0 || planetPrefab == null)
        {
            Debug.LogError("�༺ ������ �������� �������ּ���.");
            return;
        }

        // 2. �༺ 1���� ���ƾ� �� ���� ���
        float angleStep = 360f / numberOfPlanets;

        for (int i = 0; i < numberOfPlanets; i++)
        {
            // 3. ���� �༺�� �˵� ���� (Y�� ����)
            float currentAngle = -(i * angleStep);

            // 4. Quaternion�� ����Ͽ� ȸ���� ����
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

            // 5. ȸ���� ����(����)���� ��������ŭ �̵�
            // Vector3.forward (0, 0, 1)�� ȸ����Ų ��, orbitRadius�� ���Ͽ� ��ġ ����
            Vector3 position = rotation * Vector3.forward * orbitRadius;

            // 6. ������Ʈ ���� �� ��ġ
            GameObject newPlanet = Instantiate(planetPrefab, transform);
            newPlanet.name = $"Chapter_{i + 1}_Planet";

            // PlanetPivot (�� ��ũ��Ʈ�� ���� ������Ʈ)�� Local ��ǥ�迡 ��ġ
            newPlanet.transform.localPosition = position;

            // �ʿ��ϴٸ� �༺�� �߽��� �ٶ󺸵��� ȸ�� (���� ����)
            // newPlanet.transform.LookAt(transform.position); 

            createdPlanets.Add(newPlanet);
        }

        Debug.Log($"{numberOfPlanets}���� �༺�� �˵��� ���� �����Ǿ����ϴ�.");

        // ������ �༺ ����Ʈ�� ChapterSelector�� ���� (���� ��ũ��Ʈ�� ����Ѵٸ�)
        ChapterSelector selector = GetComponent<ChapterSelector>();
        if (selector != null)
        {
            selector.planets = createdPlanets;
        }
    }

    [ContextMenu("2. Clear All Planets")]
    private void ClearExistingPlanets()
    {
        // �ν����Ϳ��� ������ �༺���� ����ϰ� ����ϴ�.
        for (int i = createdPlanets.Count - 1; i >= 0; i--)
        {
            if (createdPlanets[i] != null)
            {
                // ������ ��忡�� ������Ʈ�� ��� �����մϴ�.
                DestroyImmediate(createdPlanets[i]);
            }
        }
        createdPlanets.Clear();
    }
}