using UnityEngine;

public class FloatingUI : MonoBehaviour
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
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.WakeUp();

            // 새로운 힘을 적용합니다.
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * initialForce, ForceMode2D.Impulse);

            float randomTorqueDirection = Random.Range(-1f, 1f);
            rb.AddTorque(randomTorqueDirection * initialTorque, ForceMode2D.Impulse);
        }
    }

}