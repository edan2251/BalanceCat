using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySideBias : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] Rigidbody rb;

    [SerializeField] Transform facing;   // 예: ThirdPersonCam.playerOBJ 또는 캐릭터 본체

    [Header("Bias")]
    [SerializeField] float maxBiasAcceleration = 20f; // (Right-Left)==carryCapacity일 때 가속도
    [SerializeField] float deadZoneKg = 0.1f;
    [SerializeField] bool scaleByCapacity = true;

    [Header("편향 각도")]
    [SerializeField, Range(0f, 85f)] float biasAngleDeg = 60f; // 전방 기준 대각 각도

    [Header("적용 조건/제동")]
    [SerializeField] bool onlyWhenMoving = true;       // 이동 입력 있을 때만 편향
    [SerializeField] float inputDeadZone = 0.05f;      // 입력 데드존
    [SerializeField] bool autoBrakeWhenNoInput = true; // 입력 없을 때 옆 미끄럼 제거
    [SerializeField] float brakeStrength = 10f;        // 제동 세기(가로 성분만 감쇠)

    // ===== Z/C 보정 =====
    [Header("Z/C Correction")]
    [SerializeField] float correctionStrength = 1.0f;  // Z/C 입력 영향도
    float _correctionInput;                            // -1(Z) ~ +1(C)

    // ===== 공중 튜닝 =====
    [Header("Air Bias Tuning")]
    [SerializeField] PlayerMovement playerMovement;          // 공중/입력 상태 참조
    [SerializeField, Range(0f, 1f)] float airBiasMultiplier = 0.35f; // 공중 편향 약화
    [SerializeField] float maxAirSideSpeed = 6f;             // 공중 옆속도 캡(0이면 미사용)

    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!facing) facing = transform; // 폴백
    }

    public void SetCorrectionInput(float v)
    {
        _correctionInput = Mathf.Clamp(v, -1f, 1f);
    }

    void FixedUpdate()
    {
        if (!inventory || !rb || !facing) return;

        if (playerMovement && !playerMovement.IsControlEnabled) return;

        float ix, iz;
        if (playerMovement)
        {
            ix = playerMovement.horizontalInput;
            iz = playerMovement.verticalInput;
        }
        else
        {
            ix = Input.GetAxisRaw("Horizontal");
            iz = Input.GetAxisRaw("Vertical");
        }

        bool hasInput = (Mathf.Abs(ix) + Mathf.Abs(iz)) > inputDeadZone;

        // === 캐릭터 기준 오른쪽(수평) 축 ===
        Vector3 rightAxis = Vector3.ProjectOnPlane(facing.right, Vector3.up).normalized;
        if (rightAxis.sqrMagnitude < 1e-4f) return;

        // === 캐릭터 기준 전방(수평) 축 ===
        Vector3 forwardAxis = Vector3.ProjectOnPlane(facing.forward, Vector3.up).normalized;
        if (forwardAxis.sqrMagnitude < 1e-4f) forwardAxis = Vector3.forward;

        // === 입력 없으면 옆 미끄럼 자동 감쇠 ===
        if (!hasInput && autoBrakeWhenNoInput)
        {
            Vector3 v = rb.velocity;
            Vector3 sideV = Vector3.Project(v, rightAxis); // 옆방향 속도 성분만
            if (sideV.sqrMagnitude > 1e-6f)
                rb.AddForce(-sideV.normalized * Mathf.Min(sideV.magnitude, brakeStrength), ForceMode.Acceleration);

            if (onlyWhenMoving) return; // [기존 로직 유지]
        }
        else if (onlyWhenMoving && !hasInput)
        {
            return;
        }

        // === 좌/우 가방 무게 합 ===
        float left = SumWeight(inventory.leftGrid);
        float right = SumWeight(inventory.rightGrid);
        float delta = right - left; // (+)면 오른쪽이 더 무거움

        // 데드존
        if (Mathf.Abs(delta) <= deadZoneKg) return;

        // === 정규화(용량 기준 스케일 옵션) ===
        float t = scaleByCapacity && inventory.carryCapacity > 0f
            ? Mathf.Clamp(delta / inventory.carryCapacity, -1f, 1f)
            : Mathf.Clamp(delta, -1f, 1f);

        // 전방 기준 ±biasAngleDeg(도) 대각으로 고정
        float sign = Mathf.Sign(t);
        float ang = biasAngleDeg * Mathf.Deg2Rad;
        Vector3 diag = (Mathf.Cos(ang) * forwardAxis + Mathf.Sin(ang) * sign * rightAxis).normalized;

        rb.AddForce(diag * (Mathf.Abs(t) * maxBiasAcceleration), ForceMode.Acceleration);
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
