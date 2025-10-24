using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float initialForce = 50f;
    public float initialTorque = 10f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on the UI element.");
            return;
        }

        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        rb.AddForce(randomDirection * initialForce, ForceMode2D.Impulse);

        float randomTorqueDirection = Random.Range(-1f, 1f);
        rb.AddTorque(randomTorqueDirection * initialTorque, ForceMode2D.Impulse);
    }
}