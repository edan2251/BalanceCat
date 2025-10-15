using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySideBias : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] Rigidbody rb;

    [Header("Facing ����(�ݵ�� ĳ���Ͱ� �ٶ󺸴� Transform)")]
    [SerializeField] Transform facing;   // ��: ThirdPersonCam.playerOBJ �Ǵ� ĳ���� ��ü

    [Header("Bias")]
    [SerializeField] float maxBiasAcceleration = 20f; // (Right-Left)==carryCapacity�� �� ���ӵ�
    [SerializeField] float deadZoneKg = 0.1f;
    [SerializeField] bool scaleByCapacity = true;

    [Header("���� ����/����")]
    [SerializeField] bool onlyWhenMoving = true;     // �̵� �Է� ���� ���� ����
    [SerializeField] float inputDeadZone = 0.05f;     // �Է� ������
    [SerializeField] bool autoBrakeWhenNoInput = true; // �Է� ���� �� �� �̲��� ����
    [SerializeField] float brakeStrength = 10f;       // ���� ����(���� ���и� ����)

    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!facing) facing = transform; // ����
    }

    void FixedUpdate()
    {
        if (!inventory || !rb || !facing) return;

        // === �̵� �Է� üũ(�Է� ���� ���� ����) ===
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");
        bool hasInput = (Mathf.Abs(ix) + Mathf.Abs(iz)) > inputDeadZone;

        // === ĳ���� '���� ������' ��(�����) ===
        Vector3 rightAxis = Vector3.ProjectOnPlane(facing.right, Vector3.up).normalized;
        if (rightAxis.sqrMagnitude < 1e-4f) return;

        // === �Է� ������ �� �̲��� �ڵ� ���� ===
        if (!hasInput && autoBrakeWhenNoInput)
        {
            Vector3 v = rb.velocity;
            Vector3 sideV = Vector3.Project(v, rightAxis);         // ������ �ӵ� ���и�
            // ����(���ӵ� ���): sideV �ݴ�������� ����
            rb.AddForce(-sideV.normalized * Mathf.Min(sideV.magnitude, brakeStrength), ForceMode.Acceleration);
            return; // ���� �� ������
        }

        if (onlyWhenMoving && !hasInput) return;

        // === ��/�� ���� ���� ���� �� ���� ���ӵ� ===
        float leftW = SumWeight(inventory.leftGrid);
        float rightW = SumWeight(inventory.rightGrid);
        float delta = rightW - leftW; // +�� '������ ����'�� ���̴� => ĳ���� ���� ���������� ����

        if (Mathf.Abs(delta) < deadZoneKg) return;

        float t = scaleByCapacity && inventory.carryCapacity > 0f
            ? Mathf.Clamp(delta / inventory.carryCapacity, -1f, 1f)
            : Mathf.Clamp(delta, -1f, 1f);

        rb.AddForce(rightAxis * (t * maxBiasAcceleration), ForceMode.Acceleration);
    }

    float SumWeight(InventoryGrid g)
    {
        if (g == null) return 0f;
        float s = 0f;
        foreach (var p in g.placements) if (p?.item != null) s += p.item.TotalWeight;
        return s;
    }
}
