using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    private void Awake()
    {
        // 이 스크립트가 작동하려면 반드시 콜라이더가 Trigger여야 함
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"WaterZone '{gameObject.name}'의 콜라이더가 Trigger가 아닙니다. Is Trigger를 체크해주세요.", this);
            col.isTrigger = true; // 강제로 설정
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 감지
        if (other.CompareTag("Player"))
        {
            // 플레이어의 PlayerMovement 스크립트를 찾음
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // 플레이어에게 물에 들어왔다고 알림
                player.SetInWater(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어만 감지
        if (other.CompareTag("Player"))
        {
            // 플레이어의 PlayerMovement 스크립트를 찾음
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // 플레이어에게 물에서 나갔다고 알림
                player.SetInWater(false);
            }
        }
    }
}