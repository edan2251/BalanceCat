using UnityEngine;

[RequireComponent(typeof(Collider))]
public class IceZone : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSlippery(true);
                player.SetExternalForceSimulation(true);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement player = collision.gameObject.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSlippery(false);
                player.SetExternalForceSimulation(false);
            }
        }
    }
}