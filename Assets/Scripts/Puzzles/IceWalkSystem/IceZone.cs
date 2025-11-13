using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IceZone : MonoBehaviour
{
    private void Awake()
    {
        // 얼음 구역은 반드시 트리거여야 함
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어만 감지
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // 플레이어에게 "얼음 위에 있다"고 알림
                player.SetSlippery(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어만 감지
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // 플레이어에게 "얼음에서 나갔다"고 알림
                player.SetSlippery(false);
            }
        }
    }
}