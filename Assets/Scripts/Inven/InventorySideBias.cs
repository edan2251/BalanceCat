using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySideBias : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] Rigidbody rb;

    [Header("Facing 기준(반드시 캐릭터가 바라보는 Transform)")]
    [SerializeField] Transform facing;   // 예: ThirdPersonCam.playerOBJ 또는 캐릭터 본체

    [Header("Bias")]
    [SerializeField] float maxBiasAcceleration = 20f; // (Right-Left)==carryCapacity일 때 가속도
    [SerializeField] float deadZoneKg = 0.1f;
    [SerializeField] bool scaleByCapacity = true;

    [Header("적용 조건/제동")]
    [SerializeField] bool onlyWhenMoving = true;       // 이동 입력 있을 때만 편향
    [SerializeField] float inputDeadZone = 0.05f;      // 입력 데드존
    [SerializeField] bool autoBrakeWhenNoInput = true; // 입력 없을 때 옆 미끄럼 제거
    [SerializeField] float brakeStrength = 10f;        // 제동 세기(가로 성분만 감쇠)

    // ===== Z/C 보정 =====
    [Header("Z/C Correction")]
    [SerializeField] float correctionStrength = 1.0f;  // Z/C 입력 영향도
    float _correctionInput;                            // -1(Z) ~ +1(C)
    bool _paused;

    // ===== 공중 튜닝 =====
    [Header("Air Bias Tuning")]
    [SerializeField] PlayerMovement playerMovement;          // 있으면 공중 판정 사용
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

    public void SetPaused(bool pause) => _paused = pause;

    void FixedUpdate()
    {
        if (_paused) return;
        if (!inventory || !rb || !facing) return;

        // === 이동 입력 ===
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");
        float inMag = Mathf.Clamp01(Mathf.Abs(ix) + Mathf.Abs(iz)); // 0~1
        bool hasInput = inMag > inputDeadZone;

        // === 캐릭터 기준 오른쪽(수평) 축 ===
        Vector3 rightAxis = Vector3.ProjectOnPlane(facing.right, Vector3.up).normalized;
        if (rightAxis.sqrMagnitude < 1e-4f) return;

        // === 입력 없으면 옆 미끄럼 감쇠(가속도 방식) ===
        if (!hasInput && autoBrakeWhenNoInput)
        {
            Vector3 v = rb.velocity;
            Vector3 sideV = Vector3.Project(v, rightAxis);
            if (sideV.sqrMagnitude > 1e-6f)
            {
                // 공중에서도 약하게라도 감쇠되도록 동일 처리
                float decel = Mathf.Min(sideV.magnitude, brakeStrength);
                rb.AddForce(-sideV.normalized * decel, ForceMode.Acceleration);
            }
            if (onlyWhenMoving) return; // 입력 없으면 여기서 종료
        }

        if (onlyWhenMoving && !hasInput) return;

        // === 좌/우 가방 무게 차이 → 편향 비율 t(-1..+1) ===
        float leftW = SumWeight(inventory.leftGrid);
        float rightW = SumWeight(inventory.rightGrid);
        float delta = rightW - leftW; // +면 오른쪽이 무거움 → +X(오른쪽)으로 밀힘

        float t = 0f;
        float abs = Mathf.Abs(delta) - Mathf.Max(0f, deadZoneKg);
        if (abs > 0f)
        {
            float denom = scaleByCapacity ? Mathf.Max(1f, inventory.carryCapacity) : abs; // scaleOff면 즉시 1
            float ratio = Mathf.Clamp01(abs / denom);
            t = Mathf.Sign(delta) * ratio;
        }

        // === Z/C 보정 입력 반영 ===
        t += (_correctionInput * correctionStrength);
        t = Mathf.Clamp(t, -1f, 1f);

        // === 입력 세기에 비례 ===
        t *= inMag;

        // === 공중 약화 계수 ===
        bool isAir = (playerMovement && !playerMovement.IsGrounded);
        float airMul = isAir ? airBiasMultiplier : 1f;

        // === 최종 편향 가속도 적용 ===
        rb.AddForce(rightAxis * (t * maxBiasAcceleration * airMul), ForceMode.Acceleration);

        // === 공중 옆속도 캡 ===
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
