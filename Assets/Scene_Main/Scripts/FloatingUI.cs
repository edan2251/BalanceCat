//using UnityEngine;

//public class FloatingUI : MonoBehaviour
//{
//    public float initialForce = 50f;
//    public float initialTorque = 10f;

//    private Rigidbody2D rb;

//    void Awake() 
//    {
//        rb = GetComponent<Rigidbody2D>();
//        if (rb == null)
//        {
//            Debug.LogError("Rigidbody2D component not found on the UI element.");
//            return;
//        }
//        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
//    }

//    void OnEnable() 
//    {
//        if (rb != null)
//        {
//            rb.velocity = Vector2.zero;
//            rb.angularVelocity = 0f;

//            rb.WakeUp();

//            // ���ο� ���� �����մϴ�.
//            Vector2 randomDirection = Random.insideUnitCircle.normalized;
//            rb.AddForce(randomDirection * initialForce, ForceMode2D.Impulse);

//            float randomTorqueDirection = Random.Range(-1f, 1f);
//            rb.AddTorque(randomTorqueDirection * initialTorque, ForceMode2D.Impulse);
//        }
//    }

//}
using UnityEngine;
using UnityEngine.EventSystems; // 1. UI �̺�Ʈ �ý��� ����� ���� �߰�

// 2. IPointerClickHandler �������̽� ���
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
        // 3. ���� �� �� �� ����
        ApplyNewForce();
    }

    // 4. Ŭ���� �����ϴ� �޼��� (IPointerClickHandler �������̽��� �䱸����)
    public void OnPointerClick(PointerEventData eventData)
    {

        // 5. Ŭ���Ǿ��� ���� �� �� ����
        ApplyNewForce();
    }

    // 6. ���� �����ϴ� ������ ���� �޼���� �и� (������ ����)
    public void ApplyNewForce()
    {
        if (rb != null)
        {
            // ���� �ӵ��� ȸ���� 0���� �ʱ�ȭ
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.WakeUp();

            // ���ο� ���� �������� �� ���� (�ݵ� ȿ��)
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * initialForce, ForceMode2D.Impulse);

            // ���ο� ���� �������� ȸ���� ����
            float randomTorqueDirection = Random.Range(-1f, 1f);
            rb.AddTorque(randomTorqueDirection * initialTorque, ForceMode2D.Impulse);
        }
    }
}