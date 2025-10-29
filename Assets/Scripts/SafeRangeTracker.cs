using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SafeRangeTracker : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("���� �ݰ�(����)")]
    public float radius = 2f;

    [Tooltip("�Է� ���� �ӵ�(�ʴ� ����) - �⺻ �̵��� �ݿ�")]
    public float trackSpeedPerSec = 3f;

    [Tooltip("Raw �Է� �� �̸�")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";

    [Header("State (ReadOnly)")]
    [SerializeField] bool inRange = true;
    [SerializeField] Vector2 localPos; // �߽�(0,0) ���� ���� ��ġ(�Է� ����)

    public bool InRange => inRange;
    public Vector2 LocalPos => localPos;

    [Header("Events")]
    public UnityEvent OnExited;   // ���� ������ ó�� ���� ��
    public UnityEvent OnReturned; // ���� ������ �ٽ� ���� ��

    void Update()
    {
        // �⺻ �̵���: Raw �Է� �ุ ���(����/���� ����)
        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);
        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f) input.Normalize();

        localPos += input * (trackSpeedPerSec * Time.deltaTime);

        // �ݰ� üũ
        bool nowIn = (localPos.sqrMagnitude <= radius * radius);
        if (inRange != nowIn)
        {
            inRange = nowIn;
            if (inRange) OnReturned?.Invoke();
            else OnExited?.Invoke();
        }
    }

    public void ResetToCenter() => localPos = Vector2.zero;
}
