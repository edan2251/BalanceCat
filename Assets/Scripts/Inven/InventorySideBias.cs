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
    [SerializeField] bool onlyWhenMoving = true;       // �̵� �Է� ���� ���� ����
    [SerializeField] float inputDeadZone = 0.05f;      // �Է� ������
    [SerializeField] bool autoBrakeWhenNoInput = true; // �Է� ���� �� �� �̲��� ����
    [SerializeField] float brakeStrength = 10f;        // ���� ����(���� ���и� ����)

    // ===== Z/C ���� =====
    [Header("Z/C Correction")]
    [SerializeField] float correctionStrength = 1.0f;  // Z/C �Է� ���⵵
    float _correctionInput;                            // -1(Z) ~ +1(C)
    bool _paused;

    // ===== ���� Ʃ�� =====
    [Header("Air Bias Tuning")]
    [SerializeField] PlayerMovement playerMovement;          // ������ ���� ���� ���
    [SerializeField, Range(0f, 1f)] float airBiasMultiplier = 0.35f; // ���� ���� ��ȭ
    [SerializeField] float maxAirSideSpeed = 6f;             // ���� ���ӵ� ĸ(0�̸� �̻��)

    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!facing) facing = transform; // ����
    }

    public void SetCorrectionInput(float v)
    {
        _correctionInput = Mathf.Clamp(v, -1f, 1f);
    }

    public void SetPaused(bool pause) => _paused = pause;

    void FixedUpdate()
    {
        if (_paused) return;
        if (!inventory || !rb || !facing) return;

        // === �̵� �Է� ===
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");
        float inMag = Mathf.Clamp01(Mathf.Abs(ix) + Mathf.Abs(iz)); // 0~1
        bool hasInput = inMag > inputDeadZone;

        // === ĳ���� ���� ������(����) �� ===
        Vector3 rightAxis = Vector3.ProjectOnPlane(facing.right, Vector3.up).normalized;
        if (rightAxis.sqrMagnitude < 1e-4f) return;

        // === �Է� ������ �� �̲��� ����(���ӵ� ���) ===
        if (!hasInput && autoBrakeWhenNoInput)
        {
            Vector3 v = rb.velocity;
            Vector3 sideV = Vector3.Project(v, rightAxis);
            if (sideV.sqrMagnitude > 1e-6f)
            {
                // ���߿����� ���ϰԶ� ����ǵ��� ���� ó��
                float decel = Mathf.Min(sideV.magnitude, brakeStrength);
                rb.AddForce(-sideV.normalized * decel, ForceMode.Acceleration);
            }
            if (onlyWhenMoving) return; // �Է� ������ ���⼭ ����
        }

        if (onlyWhenMoving && !hasInput) return;

        // === ��/�� ���� ���� ���� �� ���� ���� t(-1..+1) ===
        float leftW = SumWeight(inventory.leftGrid);
        float rightW = SumWeight(inventory.rightGrid);
        float delta = rightW - leftW; // +�� �������� ���ſ� �� +X(������)���� ����

        float t = 0f;
        float abs = Mathf.Abs(delta) - Mathf.Max(0f, deadZoneKg);
        if (abs > 0f)
        {
            float denom = scaleByCapacity ? Mathf.Max(1f, inventory.carryCapacity) : abs; // scaleOff�� ��� 1
            float ratio = Mathf.Clamp01(abs / denom);
            t = Mathf.Sign(delta) * ratio;
        }

        // === Z/C ���� �Է� �ݿ� ===
        t += (_correctionInput * correctionStrength);
        t = Mathf.Clamp(t, -1f, 1f);

        // === �Է� ���⿡ ��� ===
        t *= inMag;

        // === ���� ��ȭ ��� ===
        bool isAir = (playerMovement && !playerMovement.IsGrounded);
        float airMul = isAir ? airBiasMultiplier : 1f;

        // === ���� ���� ���ӵ� ���� ===
        rb.AddForce(rightAxis * (t * maxBiasAcceleration * airMul), ForceMode.Acceleration);

        // === ���� ���ӵ� ĸ ===
        if (isAir && maxAirSideSpeed > 0f)
        {
            Vector3 v = rb.velocity;
            float side = Vector3.Dot(v, rightAxis);
            float cap = maxAirSideSpeed;
            if (side > cap) v += rightAxis * (cap - side);
            else if (side < -cap) v += rightAxis * (-cap - side);
            rb.velocity = v;
        }
    }

    float SumWeight(InventoryGrid g)
    {
        if (g == null || g.placements == null) return 0f;
        float sum = 0f;
        foreach (var p in g.placements)
            if (p != null && p.item != null) sum += p.item.TotalWeight;
        return sum;
    }
}
