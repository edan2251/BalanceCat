using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

// WeightSensor 이벤트에 반응하여 문을 파괴(비활성화)하는 컴포넌트
public class WeightDoorDestructible : MonoBehaviour
{
    [Header("파괴 설정")]
    [Tooltip("파괴 효과를 위한 파티클 시스템 (옵션)")]
    public ParticleSystem destructionEffect;

    [Tooltip("파괴(비활성화)되기까지의 딜레이 시간")]
    public float destructionDelay = 0.5f;

    private bool isDestroyed = false;

    public void DestroyDoor()
    {
        if (isDestroyed) return; // 이미 파괴된 경우 중복 실행 방지

        isDestroyed = true;
        StartCoroutine(DestructionSequence());
    }

    private IEnumerator DestructionSequence()
    {
        // 1. 파괴 전 딜레이
        yield return new WaitForSeconds(destructionDelay);

        // 2. 파티클 효과 재생 (연결되어 있다면)
        if (destructionEffect != null)
        {
            // 문 위치에서 파티클 재생
            destructionEffect.transform.position = transform.position;
            destructionEffect.Play();
        }

        // 3. 문 오브젝트 비활성화 (보이지 않게 됨)
        gameObject.SetActive(false);

        // 4. (선택적) 파티클이 끝난 후 스스로 제거 (장면 정리)
        if (destructionEffect != null)
        {
            Destroy(destructionEffect.gameObject, destructionEffect.main.duration);
        }
    }

}