using UnityEngine;

// WeightSensor�� �̺�Ʈ�� �����Ͽ� ���� ���ݴ� ������Ʈ
public class WeightDoor : MonoBehaviour
{
    [Header("�� ����")]
    public Vector3 openPositionOffset = new Vector3(0, 5, 0); // ���� �� �����̴� �Ÿ� (Y������ 5 ���� ����)
    public float moveSpeed = 2.0f; // ���� �����̴� �ӵ�

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        closedPosition = transform.position; // ���� ��ġ�� ���� ��ġ�� ����
        targetPosition = closedPosition;
    }

    private void Update()
    {
        // ��ǥ ��ġ�� �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    // WeightSensor�� onWeightMet �̺�Ʈ�� ������ �Լ�
    public void OpenDoor()
    {
        targetPosition = closedPosition + openPositionOffset;
        Debug.Log("[WeightDoor] ���� �����ϴ�.");
    }

    // WeightSensor�� onWeightUnmet �̺�Ʈ�� ������ �Լ�
    public void CloseDoor()
    {
        targetPosition = closedPosition;
        Debug.Log("[WeightDoor] ���� �����ϴ�.");
    }
}