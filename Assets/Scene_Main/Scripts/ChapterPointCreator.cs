using UnityEngine;
using System.Collections.Generic;

public class ChapterPlanetCreator : MonoBehaviour
{
    // === 인스펙터 설정 변수 ===

    // 생성할 행성 프리팹 (3D 구체 또는 모델)
    public GameObject planetPrefab;

    // 궤도 반지름 (중심에서 행성까지의 거리)
    public float orbitRadius = 10f;

    // 생성할 행성(챕터)의 총 개수
    public int numberOfPlanets = 5;

    // 생성된 행성 리스트 (ChapterSelector에서 사용할 리스트)
    [HideInInspector]
    public List<GameObject> createdPlanets = new List<GameObject>();


    // 인스펙터에서 마우스 오른쪽 버튼을 눌러 실행할 수 있습니다.
    [ContextMenu("1. Create Planets on Orbit")]
    private void CreatePlanetsOnOrbit()
    {
        // 1. 기존 행성 제거 (재생성 시)
        ClearExistingPlanets();

        if (numberOfPlanets <= 0 || planetPrefab == null)
        {
            Debug.LogError("행성 개수나 프리팹을 설정해주세요.");
            return;
        }

        // 2. 행성 1개당 돌아야 할 각도 계산
        float angleStep = 360f / numberOfPlanets;

        for (int i = 0; i < numberOfPlanets; i++)
        {
            // 3. 현재 행성의 궤도 각도 (Y축 기준)
            float currentAngle = -(i * angleStep);

            // 4. Quaternion을 사용하여 회전값 생성
            Quaternion rotation = Quaternion.Euler(0, currentAngle, 0);

            // 5. 회전된 방향(앞쪽)으로 반지름만큼 이동
            // Vector3.forward (0, 0, 1)을 회전시킨 후, orbitRadius를 곱하여 위치 결정
            Vector3 position = rotation * Vector3.forward * orbitRadius;

            // 6. 오브젝트 생성 및 배치
            GameObject newPlanet = Instantiate(planetPrefab, transform);
            newPlanet.name = $"Chapter_{i + 1}_Planet";

            // PlanetPivot (이 스크립트가 붙은 오브젝트)의 Local 좌표계에 배치
            newPlanet.transform.localPosition = position;

            // 필요하다면 행성이 중심을 바라보도록 회전 (선택 사항)
            // newPlanet.transform.LookAt(transform.position); 

            createdPlanets.Add(newPlanet);
        }

        Debug.Log($"{numberOfPlanets}개의 행성이 궤도에 맞춰 생성되었습니다.");

        // 생성된 행성 리스트를 ChapterSelector에 연결 (이전 스크립트를 사용한다면)
        ChapterSelector selector = GetComponent<ChapterSelector>();
        if (selector != null)
        {
            selector.planets = createdPlanets;
        }
    }

    [ContextMenu("2. Clear All Planets")]
    private void ClearExistingPlanets()
    {
        // 인스펙터에서 생성된 행성들을 깔끔하게 지웁니다.
        for (int i = createdPlanets.Count - 1; i >= 0; i--)
        {
            if (createdPlanets[i] != null)
            {
                // 에디터 모드에서 오브젝트를 즉시 삭제합니다.
                DestroyImmediate(createdPlanets[i]);
            }
        }
        createdPlanets.Clear();
    }
}