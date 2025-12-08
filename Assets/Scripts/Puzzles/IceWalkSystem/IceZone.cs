using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IceZone : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSlippery(true);
                // [신규] 미끄러질 때도 링이 반응하도록 시뮬레이션 켜기
                player.SetExternalForceSimulation(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSlippery(false);
                // [신규] 끄기
                player.SetExternalForceSimulation(false);
            }
        }
    }
}