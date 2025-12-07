using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float runMultiplier = 3.0f;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Jump Control")]
    public float ascentMultiplier = 1.5f;
    public float descentMultiplier = 1.5f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode balanceLeftKey = KeyCode.Z;
    public KeyCode balanceRightKey = KeyCode.C;

    [Header("Water Physics")]
    public float waterMoveSpeed = 1.5f;
    public float floatingForce = 10.0f;
    public float waterDrag = 2.0f;
    public float waterBobbingSpeed = 1.0f;
    public float waterBobbingAmount = 0.5f;

    [Header("Ice Physics")]
    public float iceGroundDrag = 0.1f;
    [Range(0f, 1f)]
    public float iceControlMultiplier = 0.1f;

    [Header("Ground Check")]
    public float playerHeight = 1.0f;
    public float groundCheckExtent = 0.4f;
    public float groundCheckMargin = 0.1f;
    public LayerMask whatIsGround;
    bool grounded;

    private bool _isInWater = false;
    private bool _isOnIce = false;
    private Collider _currentWaterCollider;

    //외부에서 속도를 줄이기 위한 배율 변수 (1.0 = 정상, 0.5 = 절반 속도)
    private float _externalSpeedMultiplier = 1.0f;
    public float _weightSpeedMultiplier = 1.0f; //무게 관련 속도

    [Header("Weight Penalty")]
    public bool useWeightPenalty = true;

    [Range(0f, 100f)] public float penalty1WeightPercent = 30f;
    [Range(0f, 100f)] public float penalty2WeightPercent = 60f;
    [Range(0f, 100f)] public float penalty3WeightPercent = 100f;

    [Range(0f, 100f)] public float penalty1SpeedDecreasePercent = 20f;
    [Range(0f, 100f)] public float penalty2SpeedDecreasePercent = 20f;
    [Range(0f, 100f)] public float penalty3SpeedDecreasePercent = 100f;

    int _currentWeightPenaltyStage = 0;

    // --- 외부 참조용 프로퍼티 ---
    public bool IsGrounded => grounded;
    public bool IsInWater => _isInWater;
    public Collider CurrentWaterCollider => _currentWaterCollider;
    public float FlatSpeed => new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
    public bool IsControlEnabled => _controlEnabled;
    public float CurrentMoveSpeed => currentMoveSpeed;

    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public Animator playerAnimator;

    // 리스폰 매니저 참조 (상태 확인용)
    public PlayerRespawn playerRespawn;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    [Header("Control/Links")]
    public InventorySideBias sideBias;

    bool _controlEnabled = true;
    private bool _speedControlEnabled = true;

    Vector3 moveDirection;
    float currentMoveSpeed;

    // 밸런스 관련 변수
    public float balanceAdjustSpeed = 2.0f;
    public float balanceReturnSpeed = 1.0f;
    float _manualTilt = 0f;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (playerAnimator == null) playerAnimator = GetComponentInChildren<Animator>();

        // 리스폰 매니저 찾기
        if (playerRespawn == null) playerRespawn = GetComponent<PlayerRespawn>();
    }

    private void Update()
    {
        // 1. 지면 체크
        CheckGround();

        // 2. 미니게임 중 정지 로직
        if (BalanceMiniGame.IsRunning)
        {
            StopMovementCompletely();
            return;
        }

        // 3. 밸런스 업데이트
        UpdateBalanceControl();

        // 4. 입력 및 이동 처리
        GetInput();

        if (_speedControlEnabled) SpeedControl();

        UpdateAnimator();
        ApplyDrag();
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();

        if (_isInWater) ApplyWaterBuoyancy();
        else ApplyVariableGravity();
    }

    // --- 내부 로직 함수들 ---

    private void CheckGround()
    {
        Vector3 footPos = transform.position + Vector3.down * (playerHeight - 0.5f); // capsuleRadius 가정값 0.5
        Vector3 boxCenter = footPos + Vector3.up * 0.01f;
        Vector3 boxHalf = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);

        grounded = !_isInWater && Physics.BoxCast(boxCenter, boxHalf, Vector3.down, Quaternion.identity, groundCheckMargin, whatIsGround);
    }

    void UpdateWeightPenalty()
    {
        _currentWeightPenaltyStage = 0;
        _weightSpeedMultiplier = 1.0f;

        if (!useWeightPenalty) return;
        if (sideBias == null) return;

        float maxW = sideBias.maxWeight;
        float curW = sideBias.weightAmount;

        if (maxW <= 0f) return;

        float ratio = curW / maxW; // 현재 무게비 (0~1+)

        float p1 = Mathf.Max(0f, penalty1WeightPercent) * 0.01f;
        float p2 = Mathf.Max(0f, penalty2WeightPercent) * 0.01f;
        float p3 = Mathf.Max(0f, penalty3WeightPercent) * 0.01f;

        // 단계 결정(3 → 2 → 1 순으로 체크)
        if (ratio >= p3) _currentWeightPenaltyStage = 3;
        else if (ratio >= p2) _currentWeightPenaltyStage = 2;
        else if (ratio >= p1) _currentWeightPenaltyStage = 1;
        else _currentWeightPenaltyStage = 0;

        // 각 단계별 이동속도 감소 비율
        float s1 = Mathf.Clamp01(penalty1SpeedDecreasePercent * 0.01f);
        float s2 = Mathf.Clamp01(penalty2SpeedDecreasePercent * 0.01f);
        float s3 = Mathf.Clamp01(penalty3SpeedDecreasePercent * 0.01f);

        switch (_currentWeightPenaltyStage)
        {
            case 0:
                _weightSpeedMultiplier = 1.0f;
                break;
            case 1:
                _weightSpeedMultiplier = 1.0f - s1;
                break;
            case 2:
                _weightSpeedMultiplier = 1.0f - s2;
                break;
            case 3:
                _weightSpeedMultiplier = 1.0f - s3; // 기본값 100% 감소 → 0
                break;
        }

        if (_weightSpeedMultiplier < 0f)
            _weightSpeedMultiplier = 0f;
    }

    private void GetInput()
    {
        UpdateWeightPenalty();

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 3단계 패널티: 이동 불가 → 입력 자체를 막음
        if (_currentWeightPenaltyStage >= 3)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
        }

        bool canRunByPenalty = (_currentWeightPenaltyStage < 2);    // 2단계부터 런 금지

        bool isRunning = Input.GetKey(runKey) && !_isInWater && canRunByPenalty;
        float speedMult = isRunning ? runMultiplier : 1.0f;

        // 기본 속도 계산 (물 속 or 지상)
        if (_isInWater)
            currentMoveSpeed = waterMoveSpeed;
        else
            currentMoveSpeed = moveSpeed * speedMult;

        //  이동속도 최종계산
        currentMoveSpeed *= _externalSpeedMultiplier * _weightSpeedMultiplier;

        // 리스폰 중이거나 용암 사망 중이면 점프 불가
        bool isRespawning = (playerRespawn != null && (playerRespawn.IsRespawning || playerRespawn.IsLavaDying));

        bool canJumpByPenalty = (_currentWeightPenaltyStage < 2); // 2단계부터 점프 금지

        if (Input.GetKey(jumpKey) && readyToJump && grounded && !_isInWater && !isRespawning && canJumpByPenalty)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // 용암 사망 중이면 이동 불가
        if (playerRespawn != null && playerRespawn.IsLavaDying) return;

        Vector3 camFwd = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 camRight = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
        moveDirection = (camFwd * verticalInput + camRight * horizontalInput).normalized;

        Vector3 finalDir = moveDirection;

        // 밸런스 쏠림 적용
        if (sideBias != null && finalDir.sqrMagnitude > 0.001f)
        {
            float tilt = GetEffectiveTilt();
            if (Mathf.Abs(tilt) > 0.001f)
            {
                Vector3 sideDir = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
                finalDir += sideDir * tilt;
                finalDir.Normalize();
            }
        }

        // 힘 적용
        float controlMult = _isOnIce ? iceControlMultiplier : 1.0f;
        float forceMult = 10f;

        if (_isInWater)
            rb.AddForce(finalDir * currentMoveSpeed * forceMult, ForceMode.Force);
        else if (grounded)
            rb.AddForce(finalDir * currentMoveSpeed * forceMult * controlMult, ForceMode.Force);
        else
            rb.AddForce(finalDir * currentMoveSpeed * forceMult * airMultiplier * controlMult, ForceMode.Force);
    }

    private void ApplyDrag()
    {
        if (_isInWater) { /* 물 드래그는 SetInWater에서 */ }
        else if (grounded) rb.drag = _isOnIce ? iceGroundDrag : groundDrag;
        else rb.drag = 0f;
    }

    // --- 공개 함수 (외부 제어용) ---

    // 외부(얼음물 상태 스크립트 등)에서 속도 배율을 조절하는 함수
    public void SetSpeedMultiplier(float multiplier)
    {
        // 0.1 ~ 1.0 사이로 안전하게 제한 (최소 10% 속도는 유지)
        _externalSpeedMultiplier = Mathf.Clamp(multiplier, 0.1f, 1.0f);
    }

    public void SetInWater(bool inWater, Collider waterCollider)
    {
        _isInWater = inWater;
        _currentWaterCollider = waterCollider;

        if (_isInWater)
        {
            rb.drag = waterDrag;
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.1f, rb.velocity.z);
        }
        else
        {
            rb.drag = 0f;
            if (!_isOnIce) SetSpeedControl(true);
        }
    }

    // 리스폰 시 강제로 물 상태 해제용
    public void ForceExitWater()
    {
        SetInWater(false, null);
    }

    public void SetSlippery(bool isOnIce)
    {
        _isOnIce = isOnIce;
    }

    public void SetSpeedControl(bool isEnabled)
    {
        if (_isOnIce) { _speedControlEnabled = true; return; }
        _speedControlEnabled = isEnabled;
    }

    public void SetControlEnabled(bool v)
    {
        _controlEnabled = v;
        if (!v) StopMovementCompletely();
    }

    private void StopMovementCompletely()
    {
        horizontalInput = 0f; verticalInput = 0f; moveDirection = Vector3.zero;
        if (rb) rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        if (playerAnimator) { playerAnimator.SetBool("isRunning", false); playerAnimator.SetBool("isMoving", false); }
    }

    // --- 기타 유틸리티 함수들 (기존과 동일) ---
    private void RotatePlayer()
    {
        Vector3 flatInput = new Vector3(horizontalInput, 0f, verticalInput);
        if (flatInput.magnitude >= 0.1f && (playerRespawn == null || !playerRespawn.IsLavaDying))
            transform.forward = moveDirection;
    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce * ascentMultiplier, ForceMode.Impulse);
        grounded = false;
        if (playerAnimator != null) playerAnimator.SetTrigger("JumpTrigger");
    }
    private void ResetJump() { readyToJump = true; }
    private void ApplyVariableGravity()
    {
        if (grounded || _isInWater || (playerRespawn != null && playerRespawn.IsLavaDying)) return;
        float gravityScale = (rb.velocity.y > 0) ? ascentMultiplier : descentMultiplier;
        if (gravityScale > 1.0f) rb.AddForce(Vector3.up * Physics.gravity.y * (gravityScale - 1) * rb.mass, ForceMode.Force);
    }
    private void ApplyWaterBuoyancy()
    {
        if (!_isInWater) return;
        // 익사 연출 중 부력 처리는 이제 PlayerRespawn이 담당하므로 여기선 기본 부력만
        float bobbing = Mathf.Sin(Time.time * waterBobbingSpeed) * waterBobbingAmount;
        rb.AddForce(Vector3.up * (floatingForce + bobbing), ForceMode.Force);
    }
    private void UpdateAnimator()
    {
        if (playerAnimator == null) return;
        playerAnimator.SetBool("isInWater", _isInWater);
        if (_isInWater) { playerAnimator.SetBool("isRunning", false); return; }
        bool isMoving = horizontalInput != 0 || verticalInput != 0;
        bool canRunByPenalty = (_currentWeightPenaltyStage < 2);
        playerAnimator.SetBool("isRunning", Input.GetKey(runKey) && isMoving);
    }

    // 밸런스 관련
    public float GetEffectiveTilt()
    {
        float baseTilt = (sideBias != null) ? sideBias.tilt : 0f;
        return Mathf.Clamp(baseTilt + _manualTilt, -1f, 1f);
    }
    void UpdateBalanceControl()
    {
        if (sideBias == null) { _manualTilt = 0f; return; }
        float baseTilt = sideBias.tilt;
        if (Mathf.Abs(baseTilt) < 0.001f) { _manualTilt = Mathf.MoveTowards(_manualTilt, 0f, balanceReturnSpeed * Time.deltaTime); return; }
        bool pressLeft = Input.GetKey(balanceLeftKey); bool pressRight = Input.GetKey(balanceRightKey);
        bool correct = (baseTilt > 0f && pressLeft) || (baseTilt < 0f && pressRight);
        _manualTilt = Mathf.MoveTowards(_manualTilt, correct ? -baseTilt : 0f, (correct ? balanceAdjustSpeed : balanceReturnSpeed) * Time.deltaTime);
    }
    private void OnDrawGizmos()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float capsuleRadius = capsule != null ? capsule.radius : 0.5f;

        Vector3 footPosition = transform.position + Vector3.down * (playerHeight - capsuleRadius);
        Vector3 boxCenter = footPosition + Vector3.up * 0.01f;
        Vector3 boxHalfExtents = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);
        Vector3 boxSize = boxHalfExtents * 2f;

        if (Application.isPlaying)
        {
            Gizmos.color = grounded ? Color.green : Color.red;
        }
        else
        {
            Gizmos.color = Color.cyan;
        }

        Gizmos.DrawWireCube(boxCenter, boxSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(boxCenter + Vector3.down * groundCheckMargin, boxSize);
    }

}