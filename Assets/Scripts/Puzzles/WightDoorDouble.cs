using UnityEngine;
using UnityEngine.Events;
using System.Collections; 

// WeightSensor�� �̺�Ʈ�� �����Ͽ� ���� ȸ���ϸ� ���ݴ� ������Ʈ
public class WeightDoorRotating : MonoBehaviour
{
    [Header("�� ������Ʈ ����")]
    [Tooltip("���� �� ������Ʈ�� �����ϼ���. (�ǹ��� ��ø ��ġ�� �־�� �մϴ�)")]
    public Transform leftDoor;

    [Tooltip("������ �� ������Ʈ�� �����ϼ���. (�ǹ��� ��ø ��ġ�� �־�� �մϴ�)")]
    public Transform rightDoor;

    [Header("ȸ�� ����")]
    [Tooltip("���� ���� �� ȸ���� ���� (��: 90.0)")]
    public float openAngle = 90.0f;

    [Tooltip("���� ȸ���ϴ� �ӵ�")]
    public float rotateSpeed = 3.0f;

    [Header("������ ����")]
    [Tooltip("���԰� ���ŵ� �� ���� ����������� ������ �ð� (��)")]
    public float closeDelay = 3.0f; // 3�� ������ �߰�

    // ȸ�� ��ǥ ���ʹϾ� (����/���� ����)
    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;
    private Quaternion leftTargetRotation;
    private Quaternion rightTargetRotation;

    private Coroutine closeCoroutine; // �ݱ� �ڷ�ƾ ������ ���� ����

    private void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("[WeightDoorRotating] ��/�� �� Transform�� ��� �����ؾ� �մϴ�!");
            enabled = false;
            return;
        }

        // �ʱ� ȸ�� ���¸� ���� ���·� �����մϴ�.
        leftClosedRotation = leftDoor.localRotation;
        rightClosedRotation = rightDoor.localRotation;
        leftTargetRotation = leftClosedRotation;
        rightTargetRotation = rightClosedRotation;
    }

    private void Update()
    {
        // ���� �� �ε巯�� ȸ��
        leftDoor.localRotation = Quaternion.Slerp(
            leftDoor.localRotation,
            leftTargetRotation,
            Time.deltaTime * rotateSpeed
        );

        // ������ �� �ε巯�� ȸ��
        rightDoor.localRotation = Quaternion.Slerp(
            rightDoor.localRotation,
            rightTargetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    public void OpenDoor()
    {
        // ���� ���� ���� ������ ���� ��� �۵��ؾ� �մϴ�.
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine); // �ݱ� �ڷ�ƾ�� ���� ���̾��ٸ� ��� �ߴ�
            closeCoroutine = null;
        }

        // ���� ���¸� �������� ȸ�� ��ǥ�� �����մϴ�.
        leftTargetRotation = leftClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        rightTargetRotation = rightClosedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    public void CloseDoor()
    {
        // �̹� �ݱ� �ڷ�ƾ�� ���� ���� �ƴ϶�� ���� �����մϴ�.
        if (closeCoroutine == null)
        {
            closeCoroutine = StartCoroutine(CloseDoorWithDelay());
        }
    }

    // �ݱ� �����̸� ó���ϴ� �ڷ�ƾ
    private IEnumerator CloseDoorWithDelay()
    {
        Debug.Log($"[WeightDoorRotating] �ݱ� ������ ����: {closeDelay}��");
        yield return new WaitForSeconds(closeDelay);

        // �����̰� ���� �� ���� ��ǥ ����
        leftTargetRotation = leftClosedRotation;
        rightTargetRotation = rightClosedRotation;

        Debug.Log("[WeightDoorRotating] �� �ݱ� ����.");

        closeCoroutine = null; // �ڷ�ƾ�� �Ϸ�Ǿ����� ǥ��
    }
}