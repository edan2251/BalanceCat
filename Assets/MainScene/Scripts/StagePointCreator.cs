using UnityEngine;
using System.Collections.Generic;

public class StagePointCreator : MonoBehaviour
{
    // 생성할 스테이지 포인트 프리팹 (작은 구체 등)
    public GameObject stagePointPrefab;

    // 생성할 스테이지 포인트의 총 개수
    public int numberOfStages = 7;

    // 행성 반지름 (포인트가 행성 표면에 붙게 하기 위함)
    public float planetRadius = 1.0f;

    [Header("경로 굴곡 설정 (위도/Y축 무작위성)")]
    // 위아래로 굴곡지는 정도 (0이면 모두 적도에, 숫자가 커지면 더 극단적인 위도까지 퍼짐)
    public float maxPathWiggle = 0.5f;

    // 생성된 스테이지 포인트 리스트 (StageSelector에서 사용할 리스트)
    [HideInInspector]
    public List<GameObject> createdPoints = new List<GameObject>();

    // 인스펙터에서 마우스 오른쪽 버튼을 눌러 실행할 수 있습니다.
    [ContextMenu("1. Create Stage Points Automatically")]
    private void CreateStagePoints()
    {
        // 기존 포인트 제거 (재생성 시)
        ClearExistingPoints();

        float angleStep = 360f / numberOfStages; // 경도 간격
        planetRadius = transform.localScale.x * 0.5f; // 구체의 기본 반지름 가정

        for (int i = 0; i < numberOfStages; i++)
        {
            // 1. 경도 (Longitude) 계산: 일정한 간격
            float longitudeAngle = i * angleStep;

            // 2. 위도 (Latitude) 계산: 무작위 굴곡 추가
            // sin 함수를 사용하여 무작위 굴곡을 시간에 따라 조금씩 다르게 줍니다.
            // i/numberOfStages로 전체 경로 진행도를 얻어 자연스러운 굴곡을 만듭니다.
            float latitudeOffset = Mathf.Sin(i * 0.5f) * maxPathWiggle;

            // 3. 3D 위치 계산
            // Quaternion.Euler로 회전 값을 만든 후, Vector3.forward(Z축)에 곱하여 위치를 정합니다.
            Quaternion rotation = Quaternion.Euler(latitudeOffset * 90f, longitudeAngle, 0);
            Vector3 position = rotation * Vector3.forward * planetRadius;

            // 4. 오브젝트 생성
            GameObject newPoint = Instantiate(stagePointPrefab, transform);
            newPoint.name = $"Stage_{i + 1}";
            newPoint.transform.localPosition = position;
            // 항상 행성의 중심을 바라보도록 회전 (선택 사항)
            newPoint.transform.rotation = Quaternion.LookRotation(position.normalized);

            createdPoints.Add(newPoint);
        }

        Debug.Log($"{numberOfStages}개의 스테이지 포인트가 자동으로 생성되었습니다.");

        // 생성 후 StageSelector의 리스트에 자동 할당 (선택 사항: Inspector에서 수동 연결 가능)
        StageSelector selector = GetComponent<StageSelector>();
        if (selector != null)
        {
            selector.stagePoints = createdPoints;
        }
    }

    [ContextMenu("2. Clear Existing Points")]
    private void ClearExistingPoints()
    {
        // 인스펙터에서 생성된 포인트들을 깔끔하게 지웁니다.
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