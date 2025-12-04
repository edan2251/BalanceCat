using UnityEngine;
using System.Collections.Generic;

public class WeightPlatform : MonoBehaviour
{
    [Header("플랫폼 설정")]
    public float maxDropDistance = 3.0f;
    public float minWeight = 10.0f;
    public float maxWeight = 50.0f;
    public float moveSpeed = 2.0f;

    [SerializeField] private float currentTotalWeight = 0.0f;

    private Vector3 startPos;
    private List<InventorySideBias> currentObjects = new List<InventorySideBias>();

    // [핵심 1] 플레이어 위치 동기화를 위한 변수
    private Transform playerTransform;
    private Vector3 lastPlatformPos; // 플랫폼의 이전 위치 저장

    private void Start()
    {
        startPos = transform.position;
        lastPlatformPos = transform.position;
    }

    private void Update()
    {
        // 1. 무게 계산
        CalculateWeight();

        // 2. 목표 위치 계산 (기존 로직 유지)
        float effectiveWeight = Mathf.Max(0, currentTotalWeight - minWeight);
        float weightRange = maxWeight - minWeight;
        float ratio = (weightRange > 0) ? Mathf.Clamp01(effectiveWeight / weightRange) : 0f;

        Vector3 targetPos = startPos - new Vector3(0, ratio * maxDropDistance, 0);

        // 3. 플랫폼 이동
        // Lerp도 좋지만, 물리 동기화를 할 때는 MoveTowards가 더 예측 가능해서 떨림이 적습니다.
        // 부드러운 감속을 원하시면 다시 Lerp로 바꾸셔도 되지만, 이 방식이 기계적인 느낌은 더 좋습니다.
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // [핵심 2] 델타 이동 (Delta Movement) 구현
        // 플랫폼이 이번 프레임에 실제로 이동한 거리를 구합니다.
        Vector3 platformMovement = transform.position - lastPlatformPos;

        // 플레이어가 위에 있다면, 플랫폼이 이동한 만큼 플레이어도 강제로 이동시킵니다.
        if (playerTransform != null)
        {
            playerTransform.position += platformMovement;
        }

        // 현재 위치를 마지막 위치로 갱신
        lastPlatformPos = transform.position;
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

    // --- 충돌 감지 및 플레이어 등록 ---

    private void OnCollisionEnter(Collision collision)
    {
        // 1. 무게 객체 등록 (PlayerMovement를 통해 sideBias 찾기)
        PlayerMovement playerMovement = collision.gameObject.GetComponentInParent<PlayerMovement>();

        // (참고: 일반 사물인 경우 바로 컴포넌트 찾기)
        InventorySideBias itemBias = collision.gameObject.GetComponentInParent<InventorySideBias>();

        // 무게 리스트 추가 로직
        InventorySideBias targetBias = (playerMovement != null) ? playerMovement.sideBias : itemBias;

        if (targetBias != null && !currentObjects.Contains(targetBias))
        {
            currentObjects.Add(targetBias);
        }

        // 2. 플레이어 위치 동기화 대상 등록
        // 플레이어이고 + 플랫폼 위에서 밟았을 때만 (옆에서 비비거나 머리 박을 땐 제외)
        if (playerMovement != null)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // 법선 벡터(Normal)의 Y값이 -0.5보다 작으면 위에서 밟은 것
                if (contact.normal.y < -0.5f)
                {
                    playerTransform = collision.transform; // 부모 설정(SetParent) 대신 변수에만 담음
                    break;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayerMovement playerMovement = collision.gameObject.GetComponentInParent<PlayerMovement>();
        InventorySideBias itemBias = collision.gameObject.GetComponentInParent<InventorySideBias>();
        InventorySideBias targetBias = (playerMovement != null) ? playerMovement.sideBias : itemBias;

        // 무게 리스트 제거
        if (targetBias != null && currentObjects.Contains(targetBias))
        {
            currentObjects.Remove(targetBias);
        }

        // 플레이어 동기화 해제
        if (playerMovement != null)
        {
            // 나가려는 객체가 현재 잡고 있는 플레이어가 맞다면
            if (playerTransform == collision.transform)
            {
                playerTransform = null;
            }
        }
    }
}