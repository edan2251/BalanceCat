using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform player;  // 따라갈 플레이어
    public float smoothSpeed = 5f;

    private float fixedY;
    private float fixedZ;

    void Start()
    {
        // 시작할 때 카메라의 Y, Z 값 고정
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 플레이어의 X만 따라감
        Vector3 targetPos = new Vector3(player.position.x, fixedY, fixedZ);

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}