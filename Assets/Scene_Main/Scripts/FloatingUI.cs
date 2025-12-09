using UnityEngine;
using UnityEngine.EventSystems; // 1. UI 이벤트 시스템 사용을 위해 추가

// 2. IPointerClickHandler 인터페이스 상속
public class FloatingUI : MonoBehaviour, IPointerClickHandler
{
    public float initialForce = 50f;
    public float initialTorque = 10f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the UI element.");
            return;
        }
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    void OnEnable()
    {
        // 3. 켜질 때 새 힘 적용
        ApplyNewForce();
    }

    // 4. 클릭을 감지하는 메서드 (IPointerClickHandler 인터페이스의 요구사항)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SFX.Nyaong);
        }
        // 5. 클릭되었을 때도 새 힘 적용
        ApplyNewForce();
    }

    // 6. 힘을 적용하는 로직을 별도 메서드로 분리 (재사용을 위해)
    public void ApplyNewForce()
    {
        if (rb != null)
        {
            // 기존 속도와 회전을 0으로 초기화
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.WakeUp();

            // 새로운 랜덤 방향으로 힘 적용 (반동 효과)
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * initialForce, ForceMode2D.Impulse);

            // 새로운 랜덤 방향으로 회전력 적용
            float randomTorqueDirection = Random.Range(-1f, 1f);
            rb.AddTorque(randomTorqueDirection * initialTorque, ForceMode2D.Impulse);
        }
    }
}