using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightSensorVisualFeedback : MonoBehaviour
{
    [Header("�ð��� �ǵ�� ����")]
    [Tooltip("������ ���� �� �Ʒ��� �������� �Ÿ� (Y��)")]
    public float pressDistance = 0.2f;

    [Tooltip("����/���� �ִϸ��̼� �ӵ�")]
    public float moveSpeed = 5.0f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        // ������Ʈ�� ���� ������Ʈ�� �ʱ� ��ġ�� �����մϴ�.
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    private void Update()
    {
        // ��ǥ ��ġ�� �ε巴�� �̵��մϴ�.
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    // (���� ���� ��: ������ �����ϴ�)
    public void PressSensor()
    {
        // ���� ��ġ���� pressDistance��ŭ �Ʒ��� �̵��ϴ� ��ġ�� ��ǥ�� ����
        targetPosition = originalPosition - new Vector3(0, pressDistance, 0);
    }

    // (���� ������ ��: ������ �����մϴ�)
    public void ReleaseSensor()
    {
        // ���� ��ġ�� ����
        targetPosition = originalPosition;
    }
}
