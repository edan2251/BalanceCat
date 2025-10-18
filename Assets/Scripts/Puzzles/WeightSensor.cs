using UnityEngine;
using UnityEngine.Events; 

public class WeightSensor : MonoBehaviour
{
    [Header("���� ����")]
    [Tooltip("���� �����ų� �÷����� �۵��ϱ� ���� �ʿ��� �ּ� ����")]
    public float requiredWeight = 10.0f;

    // ���� ���� ���� �ִ� �� ����
    [Header("���� ����")]
    [Tooltip("���� ���� ���� �ִ� �� ���� (Read Only)")]
    [SerializeField] private float currentWeight = 0.0f;

    [Tooltip("�ּ� ���Ը� �����ߴ��� ���� (Read Only)")]
    public bool isWeightMet = false;

    // ���� ���� ��/������ �� �ܺο� �˸��� �̺�Ʈ
    [Header("�̺�Ʈ")]
    [Tooltip("�䱸 ���Ը� �������� �� �߻�")]
    public UnityEvent onWeightMet;
    [Tooltip("�䱸 ���� �̸����� �������� �� �߻�")]
    public UnityEvent onWeightUnmet;


    // --- ��� ���� ---

    // ���ǿ� ��ü�� �ö���� ��
    private void OnTriggerEnter(Collider other)
    {
        //WeightComponent��� ��ũ��Ʈ�� ���� ������Ʈ�� ���Է� ����
        WeightComponent wc = other.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeight += wc.objectWeight;
            CheckWeightStatus();
        }
    }

    // ���ǿ��� ��ü�� �������� ��
    private void OnTriggerExit(Collider other)
    {
        WeightComponent wc = other.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeight -= wc.objectWeight;

            // ���԰� ������ �Ǵ� ���� ���� (���� ��ġ)
            if (currentWeight < 0) currentWeight = 0;

            CheckWeightStatus();
        }
    }

    // ���� ���Ը� Ȯ���ϰ� ���� ��ȭ�� üũ�ϴ� �Լ�
    private void CheckWeightStatus()
    {
        bool newStatus = currentWeight >= requiredWeight;

        // ���°� '������ -> ����'���� �ٲ���� �� (�� ����)
        if (newStatus && !isWeightMet)
        {
            isWeightMet = true;
            onWeightMet.Invoke(); // �̺�Ʈ �߻�
            Debug.Log($"[WeightSensor] ���� ����! ���� ����: {currentWeight}");
        }
        // ���°� '���� -> ������'���� �ٲ���� �� (�� ����)
        else if (!newStatus && isWeightMet)
        {
            isWeightMet = false;
            onWeightUnmet.Invoke(); // �̺�Ʈ �߻�
            Debug.Log($"[WeightSensor] ���� ������. ���� ����: {currentWeight}");
        }
    }
}