using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WeightSensor : MonoBehaviour
{
    [Header("설정")]
    public float requiredWeight = 10.0f;

    [Header("상태 확인")]
    [SerializeField] private float currentDetectedWeight = 0.0f;
    [SerializeField] private bool isActivated = false;

    public UnityEvent onWeightMet;
    public UnityEvent onWeightUnmet;

    private List<InventorySideBias> currentObjects = new List<InventorySideBias>();

    private void Update()
    {
        CalculateWeight();
        CheckCondition();
    }

    private void CalculateWeight()
    {
        float total = 0f;
        for (int i = currentObjects.Count - 1; i >= 0; i--)
        {
            if (currentObjects[i] == null || !currentObjects[i].gameObject.activeInHierarchy)
            {
                currentObjects.RemoveAt(i);
                continue;
            }
            total += currentObjects[i].weightAmount;
        }
        currentDetectedWeight = total;
    }

    private void CheckCondition()
    {
        bool condition = currentDetectedWeight >= requiredWeight;
        if (condition != isActivated)
        {
            isActivated = condition;
            if (isActivated) onWeightMet.Invoke();
            else onWeightUnmet.Invoke();
        }
    }

    // --- [수정된 부분] PlayerMovement를 통해 접근 ---
    private void OnTriggerEnter(Collider other)
    {
        // 1. 먼저 PlayerMovement를 찾습니다.
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null && player.sideBias != null)
        {
            // 2. 플레이어와 연결된 sideBias(인벤토리 무게)를 리스트에 추가
            if (!currentObjects.Contains(player.sideBias))
            {
                currentObjects.Add(player.sideBias);
            }
        }
        // (혹시 플레이어가 아닌 다른 무게 오브젝트가 있다면 여기서 추가 처리가능)
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player != null && player.sideBias != null)
        {
            if (currentObjects.Contains(player.sideBias))
            {
                currentObjects.Remove(player.sideBias);
            }
        }
    }
}