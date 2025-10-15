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
    [SerializeField] bool onlyWhenMoving = true;     // 이동 입력 있을 때만 편향
    [SerializeField] float inputDeadZone = 0.05f;     // 입력 데드존
    [SerializeField] bool autoBrakeWhenNoInput = true; // 입력 없을 때 옆 미끄럼 제거
    [SerializeField] float brakeStrength = 10f;       // 제동 세기(가로 성분만 감쇠)

    void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!facing) facing = transform; // 폴백
    }

    void FixedUpdate()
    {
        if (!inventory || !rb || !facing) return;

        // === 이동 입력 체크(입력 있을 때만 편향) ===
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");
        bool hasInput = (Mathf.Abs(ix) + Mathf.Abs(iz)) > inputDeadZone;

        // === 캐릭터 '기준 오른쪽' 축(수평면) ===
        Vector3 rightAxis = Vector3.ProjectOnPlane(facing.right, Vector3.up).normalized;
        if (rightAxis.sqrMagnitude < 1e-4f) return;

        // === 입력 없으면 옆 미끄럼 자동 감쇠 ===
        if (!hasInput && autoBrakeWhenNoInput)
        {
            Vector3 v = rb.velocity;
            Vector3 sideV = Vector3.Project(v, rightAxis);         // 옆방향 속도 성분만
            // 감쇠(가속도 방식): sideV 반대방향으로 제동
            rb.AddForce(-sideV.normalized * Mathf.Min(sideV.magnitude, brakeStrength), ForceMode.Acceleration);
            return; // 편향 힘 미적용
        }

        if (onlyWhenMoving && !hasInput) return;

        // === 좌/우 가방 무게 차이 → 편향 가속도 ===
        float leftW = SumWeight(inventory.leftGrid);
        float rightW = SumWeight(inventory.rightGrid);
        float delta = rightW - leftW; // +면 '오른쪽 가방'이 무겁다 => 캐릭터 기준 오른쪽으로 밀힘

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
