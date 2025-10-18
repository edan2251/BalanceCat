using UnityEngine;

// 무게에 따라 Y축으로 내려가는 플랫폼 컴포넌트
public class WeightPlatform : MonoBehaviour
{
    // ... (기존 변수들은 동일) ...

    [Header("플랫폼 설정")]
    public float maxDropDistance = 3.0f; // 최대 하강 거리
    public float maxWeight = 50.0f;      // 최대 하강을 유발하는 무게 (최대 하강 시점)
    public float dropSpeed = 1.0f;       // 플랫폼이 움직이는 속

    private float currentWeightOnPlatform = 0.0f;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float normalizedWeight = Mathf.Clamp(currentWeightOnPlatform, 0, maxWeight) / maxWeight;
        float dropAmount = normalizedWeight * maxDropDistance;
        Vector3 targetPosition = startPosition - new Vector3(0, dropAmount, 0);

        // 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dropSpeed);
    }

    // --- 무게 및 탑승 감지-

    private void OnCollisionEnter(Collision collision)
    {
        WeightComponent wc = collision.gameObject.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeightOnPlatform += wc.objectWeight;

            collision.transform.SetParent(transform);
            Debug.Log($"[WeightPlatform] {collision.gameObject.name} 탑승, 현재 무게: {currentWeightOnPlatform}");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        WeightComponent wc = collision.gameObject.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeightOnPlatform -= wc.objectWeight;
            if (currentWeightOnPlatform < 0) currentWeightOnPlatform = 0;

            collision.transform.SetParent(null);
            Debug.Log($"[WeightPlatform] {collision.gameObject.name} 하차, 현재 무게: {currentWeightOnPlatform}");
        }
    }
}