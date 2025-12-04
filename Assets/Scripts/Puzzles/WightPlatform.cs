using UnityEngine;
using System.Collections.Generic;

public class WeightPlatform : MonoBehaviour
{
    [Header("플랫폼 설정")]
    public float maxDropDistance = 3.0f; // 최대 하강 거리

    [Tooltip("이 무게보다 가벼우면 꿈쩍도 안 합니다.")]
    public float minWeight = 10.0f;      // [신규] 작동 시작 최소 무게

    [Tooltip("이 무게에 도달하면 바닥까지 내려갑니다.")]
    public float maxWeight = 50.0f;      // 최대 하강 무게

    public float moveSpeed = 2.0f;

    [SerializeField] private float currentTotalWeight = 0.0f;

    private Vector3 startPos;
    private List<InventorySideBias> currentObjects = new List<InventorySideBias>();

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        CalculateWeight();

        // --- [수정된 로직] ---
        // 1. 현재 무게에서 최소 무게(minWeight)를 뺍니다. (음수면 0으로 처리)
        //    예: 무게가 5고 최소가 10이면 -> 유효 무게는 0 (안 움직임)
        //    예: 무게가 15고 최소가 10이면 -> 유효 무게는 5
        float effectiveWeight = Mathf.Max(0, currentTotalWeight - minWeight);

        // 2. 움직일 수 있는 무게 범위 (최대 - 최소)
        float weightRange = maxWeight - minWeight;

        // 3. 비율 계산 (0 ~ 1)
        //    범위가 0 이하일 경우(설정 오류 방지)를 대비해 삼항 연산자 사용
        float ratio = (weightRange > 0) ? Mathf.Clamp01(effectiveWeight / weightRange) : 0f;

        // 4. 목표 위치 계산 및 이동
        Vector3 targetPos = startPos - new Vector3(0, ratio * maxDropDistance, 0);
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
    }

    private void CalculateWeight()
    {
        float total = 0f;
        for (int i = currentObjects.Count - 1; i >= 0; i--)
        {
            if (currentObjects[i] == null)
            {
                currentObjects.RemoveAt(i);
                continue;
            }
            total += currentObjects[i].weightAmount;
        }
        currentTotalWeight = total;
    }

    // --- 충돌 감지 (PlayerMovement 경유) ---
    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovement player = collision.gameObject.GetComponentInParent<PlayerMovement>();

        if (player != null && player.sideBias != null)
        {
            if (!currentObjects.Contains(player.sideBias))
            {
                currentObjects.Add(player.sideBias);
                collision.transform.SetParent(transform);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayerMovement player = collision.gameObject.GetComponentInParent<PlayerMovement>();

        if (player != null && player.sideBias != null)
        {
            if (currentObjects.Contains(player.sideBias))
            {
                currentObjects.Remove(player.sideBias);
                collision.transform.SetParent(null);
            }
        }
    }
}