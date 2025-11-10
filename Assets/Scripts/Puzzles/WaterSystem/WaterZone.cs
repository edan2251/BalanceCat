using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    // [신규] 물 오브젝트의 콜라이더를 저장할 변수
    private Collider _waterCollider;

    private void Awake()
    {
        _waterCollider = GetComponent<Collider>();
        if (!_waterCollider.isTrigger)
        {
            Debug.LogWarning($"WaterZone '{gameObject.name}'의 콜라이더가 Trigger가 아닙니다. Is Trigger를 체크해주세요.", this);
            _waterCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                // [수정] 물에 들어왔다고 알릴 때, '이 물의 콜라이더' 정보도 함께 전달
                player.SetInWater(true, _waterCollider);
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
                // [수정] 물에서 나갈 때도 정보 전달 (null)
                player.SetInWater(false, null);
            }
        }
    }
}