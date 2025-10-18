using UnityEngine;

// ���Կ� ���� Y������ �������� �÷��� ������Ʈ
public class WeightPlatform : MonoBehaviour
{
    // ... (���� �������� ����) ...

    [Header("�÷��� ����")]
    public float maxDropDistance = 3.0f; // �ִ� �ϰ� �Ÿ�
    public float maxWeight = 50.0f;      // �ִ� �ϰ��� �����ϴ� ���� (�ִ� �ϰ� ����)
    public float dropSpeed = 1.0f;       // �÷����� �����̴� ��

    private float currentWeightOnPlatform = 0.0f;
    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        float normalizedWeight = Mathf.Clamp(currentWeightOnPlatform, 0, maxWeight) / maxWeight;
        float dropAmount = normalizedWeight * maxDropDistance;
        Vector3 targetPosition = startPosition - new Vector3(0, dropAmount, 0);

        // ��ǥ ��ġ�� �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dropSpeed);
    }

    // --- ���� �� ž�� ����-

    private void OnCollisionEnter(Collision collision)
    {
        WeightComponent wc = collision.gameObject.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeightOnPlatform += wc.objectWeight;

            collision.transform.SetParent(transform);
            Debug.Log($"[WeightPlatform] {collision.gameObject.name} ž��, ���� ����: {currentWeightOnPlatform}");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        WeightComponent wc = collision.gameObject.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeightOnPlatform -= wc.objectWeight;
            if (currentWeightOnPlatform < 0) currentWeightOnPlatform = 0;

            collision.transform.SetParent(null);
            Debug.Log($"[WeightPlatform] {collision.gameObject.name} ����, ���� ����: {currentWeightOnPlatform}");
        }
    }
}