using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LavaZone : MonoBehaviour
{
    private void Awake()
    {
        // 트리거 설정 강제
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. 플레이어 오브젝트에서 PlayerRespawn 컴포넌트를 찾습니다.
            PlayerRespawn respawnManager = other.GetComponent<PlayerRespawn>();

            // (혹시 못 찾았다면 PlayerMovement를 통해서 찾아봅니다 - 안전장치)
            if (respawnManager == null)
            {
                PlayerMovement movement = other.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    respawnManager = movement.playerRespawn;
                }
            }

            // 2. 찾았다면 용암 사망 함수 호출!
            if (respawnManager != null)
            {
                respawnManager.TriggerLavaDeath();
            }
            else
            {
                Debug.LogWarning("LavaZone: 플레이어에게 PlayerRespawn 컴포넌트가 없습니다!");
            }
        }
    }
}