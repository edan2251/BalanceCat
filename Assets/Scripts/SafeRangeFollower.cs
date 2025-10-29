// SafeRangeFollower.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeRangeFollower : MonoBehaviour
{
    [Header("Links")]
    public Transform player;                 // 고양이 루트(지면 Pivot)
    public Transform orientation;            // PlayerMovement에서 쓰는 그 orientation
    public PlayerMovement movement;          // 있으면 입력/속도 참조
    public Rigidbody playerRb;               // [ADD] 편향(측면 드리프트) 계산용

    [Header("Speed Model")]
    public float baseSpeed = 5f;             // movement 없을 때 사용
    public bool useRunKey = true;            // Shift 가속 반영
    public bool useAirMultiplier = true;     // [KEEP] 필요시 그대로 사용
    public bool ignoreAirMul = true;         // [ADD] 공중에서도 지상 속도로 추적(airMultiplier 무시)

    [Header("Y Align")]
    public float yOffset = 0.02f;            // 링 살짝 띄우기
    public bool followPlayerY = true;        // 플레이어 높이 따라감

    [Header("Bias Drift Removal")]
    public bool subtractSideDrift = true;    // [ADD] 쏠림(좌/우 측면 속도)만 추적에서 제거
    public Transform facingForSide;          // [ADD] 우/좌 축 기준(없으면 orientation 사용)
    public float sideDriftScale = 1f;        // [ADD] 제거 강도(1=완전제거)

    Vector3 _pos;
    public enum FollowUpdateMode { Update, FixedUpdate }
    [SerializeField] FollowUpdateMode updateMode = FollowUpdateMode.Update;

    void Awake()
    {
        if (!player) player = movement ? movement.transform : null;
        if (!playerRb && player) playerRb = player.GetComponent<Rigidbody>();  // [ADD]
        if (!facingForSide) facingForSide = orientation;                       // [ADD]
        _pos = (player ? player.position : transform.position);
        transform.position = _pos;
    }

    void Update()
    {
        if (updateMode == FollowUpdateMode.Update) Tick(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (updateMode == FollowUpdateMode.FixedUpdate) Tick(Time.fixedDeltaTime);
    }

    void Tick(float dt)
    {
        if (!orientation) return;

        // 1) 원시 입력
        float h, v;
        if (movement)
        {
            h = movement.horizontalInput;
            v = movement.verticalInput;
        }
        else
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 dir = Vector3.zero;
        if (Mathf.Abs(h) + Mathf.Abs(v) > 0f)
        {
            Vector3 fwd = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
            Vector3 rgt = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
            dir = (fwd * v + rgt * h).normalized;
        }

        // 2) 추적 속도(공중에서도 끊기지 않게)
        float spd = movement ? movement.moveSpeed : baseSpeed;
        if (useRunKey && movement && Input.GetKey(KeyCode.LeftShift)) spd *= movement.runMultiplier;

        if (movement && !movement.IsGrounded)
        {
            // [FIX] 기본적으로 공중에서도 동일 속도로 추적
            if (!ignoreAirMul && useAirMultiplier)
                spd *= Mathf.Max(0.01f, movement.airMultiplier); // 선택적으로 스케일
            // ignoreAirMul=true 면 지상과 동일 속도 유지
        }

        // 3) 입력 기반 적분
        _pos += dir * spd * dt;

        // 4) 쏠림 측면 드리프트 제거(옵션)
        if (subtractSideDrift && playerRb)
        {
            Transform sideRef = facingForSide ? facingForSide : orientation;
            Vector3 rightAxis = Vector3.ProjectOnPlane(sideRef.right, Vector3.up).normalized;
            Vector3 sideV = Vector3.Project(playerRb.velocity, rightAxis);
            _pos -= sideV * sideDriftScale * dt; // 좌/우로 밀리는 만큼 빼기
        }

        // 5) Y 정렬
        if (followPlayerY && player) _pos.y = player.position.y + yOffset;
        else _pos.y = (player ? player.position.y : _pos.y) + yOffset;

        // 6) 적용
        transform.position = _pos;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}
