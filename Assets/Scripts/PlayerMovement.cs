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
    [Tooltip("점프 상승 시 중력에 추가할 배율 (1.0이면 기본 중력)")]
    public float ascentMultiplier = 1.5f;
    [Tooltip("점프 하강 시 중력에 추가할 배율 (1.0이면 기본 중력)")]
    public float descentMultiplier = 1.5f;


    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode runKey = KeyCode.LeftShift;

    // --- 물 관련 설정 추가 ---
    [Header("Water Physics")]
    [Tooltip("물 속에서 움직이는 속도 (천천히)")]
    public float waterMoveSpeed = 2.0f;
    [Tooltip("물에서 둥둥 뜨게 하는 기본 부력")]
    public float floatingForce = 10.0f;
    [Tooltip("물 속에서의 저항 (움직임을 둔하게 만듦)")]
    public float waterDrag = 2.0f;

    // --- [여기에 아래 2줄 추가] ---
    [Tooltip("둥실거리는 파도의 속도 (낮을수록 '두웅~')")]
    public float waterBobbingSpeed = 1.0f;
    [Tooltip("둥실거리는 파도의 높낮이 (클수록 '시일~')")]
    public float waterBobbingAmount = 0.5f;

    // --- 지면 체크 설정 ---
    [Header("Ground Check")]
    [Tooltip("플레이어 콜라이더의 높이 절반값 (반지름)")]
    public float playerHeight = 1.0f;
    [Tooltip("지면 체크 박스의 가로/세로 크기 (절반 크기)")]
    public float groundCheckExtent = 0.4f;
    [Tooltip("지면 체크 박스를 아래로 쏘는 추가 거리 (안정성 마진)")]
    public float groundCheckMargin = 0.1f;
    public LayerMask whatIsGround;
    bool grounded;

    // --- 물 상태 변수 추가 ---
    private bool _isInWater = false;

    // SpriteDirectionalController_Octo 에서 참조하는 속성
    [HideInInspector] public bool IsGrounded => grounded;
    [HideInInspector] public float FlatSpeed => new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;

    [Header("Animator")]
    public Animator playerAnimator;

    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    [Header("Control/Links")]
    [SerializeField] InventorySideBias sideBias;
    bool _controlEnabled = true;

    Vector3 moveDirection;
    float currentMoveSpeed;

    public bool IsControlEnabled => _controlEnabled;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        if (!_controlEnabled)
        {
            sideBias?.SetCorrectionInput(0f); // 잠금 시 보정 해제
            return;                           // 입력 처리 차단(중력/물리는 기존 FixedUpdate에 맡김)
        }

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        float capsuleRadius = capsule != null ? capsule.radius : 0.5f;

        Vector3 footPosition = transform.position + Vector3.down * (playerHeight - capsuleRadius);

        Vector3 boxCenter = footPosition + Vector3.up * 0.01f;

        Vector3 boxHalfExtents = new Vector3(groundCheckExtent, 0.05f, groundCheckExtent);

        grounded = !_isInWater && Physics.BoxCast(
             boxCenter,
             boxHalfExtents,
             Vector3.down,
             Quaternion.identity,
             groundCheckMargin,
             whatIsGround
         );

        GetInput();
        SpeedControl();
        UpdateAnimator();

        // 지면에 있을 때만 드래그 적용
        if (!_isInWater)
        {
            rb.drag = grounded ? groundDrag : 0f;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotatePlayer();

        if (_isInWater)
        {
            ApplyWaterBuoyancy();
        }
        else
        {
            ApplyVariableGravity();
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // [수정] 물 속에선 달리기(runKey) 입력을 무시
        bool isRunningInput = Input.GetKey(runKey) && !_isInWater;
        float speedMultiplier = isRunningInput ? runMultiplier : 1.0f;
        currentMoveSpeed = moveSpeed * speedMultiplier;

        // [추가] 물 속에 있다면, 이동 속도를 waterMoveSpeed로 덮어씀
        if (_isInWater)
        {
            currentMoveSpeed = waterMoveSpeed;
        }

        float corr = 0f;
        if (Input.GetKey(KeyCode.Z)) corr -= 1f;
        if (Input.GetKey(KeyCode.C)) corr += 1f;
        sideBias?.SetCorrectionInput(corr);

        if (Input.GetKey(jumpKey) && readyToJump && grounded && !_isInWater)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // 카메라 방향에 맞게 이동 방향 계산
        Vector3 camForward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 camRight = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
        moveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;

        if (_isInWater)
        {
            // 물 속에서는 airMultiplier 없이, waterMoveSpeed가 적용된 currentMoveSpeed 사용
            rb.AddForce(moveDirection * currentMoveSpeed * 10f, ForceMode.Force);
        }
        else if (grounded)
            rb.AddForce(moveDirection * currentMoveSpeed * 10f, ForceMode.Force);
        else
            // 공중에서는 airMultiplier를 곱하여 이동 제어
            rb.AddForce(moveDirection * currentMoveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void RotatePlayer()
    {
        Vector3 flatInput = new Vector3(horizontalInput, 0f, verticalInput);
        if (flatInput.magnitude >= 0.1f)
            transform.forward = moveDirection; // 입력 방향으로 회전
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
        // Y축 속도를 0으로 초기화하여 점프 높이를 일관성 있게 만듭니다.
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float adjustedJumpForce = jumpForce * ascentMultiplier;
        rb.AddForce(transform.up * adjustedJumpForce, ForceMode.Impulse);

        grounded = false; // 점프 시 강제로 착지 상태 해제

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("JumpTrigger");
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void ApplyVariableGravity()
    {
        // [수정] 물 속에 있거나 땅에 있을 때는 적용하지 않음
        if (grounded || _isInWater) return;

        float gravityScale;

        if (rb.velocity.y > 0)
        {
            // 상승 중: ascentMultiplier 적용
            gravityScale = ascentMultiplier;
        }
        else
        {
            // 하강 중: descentMultiplier 적용
            gravityScale = descentMultiplier;
        }

        // 기본 중력(Physics.gravity)은 이미 적용되고 있으므로,
        // (배율 - 1) 만큼 추가적인 중력 힘을 가합니다.
        if (gravityScale > 1.0f)
        {
            float additionalGravity = Physics.gravity.y * (gravityScale - 1) * rb.mass;
            rb.AddForce(Vector3.up * additionalGravity, ForceMode.Force);
        }
    }

    private void ApplyWaterBuoyancy()
    {
        if (!_isInWater) return;

        float bobbingFactor = Mathf.Sin(Time.time * waterBobbingSpeed) * waterBobbingAmount;

        float totalUpwardForce = floatingForce + bobbingFactor;

        rb.AddForce(Vector3.up * totalUpwardForce, ForceMode.Force);

    }

    private void UpdateAnimator()
    {
        if (playerAnimator == null) return;

        // [추가] 애니메이터에게 물 속에 있는지 전달
        playerAnimator.SetBool("isInWater", _isInWater);

        // [수정] 물 속에 있을 때는 강제로 멈춤 (Idle_Stand 상태 유도)
        if (_isInWater)
        {
            playerAnimator.SetBool("isRunning", false);
            // 만약 'isMoving' 같은 bool도 사용한다면 여기서 false로 설정
            // playerAnimator.SetBool("isMoving", false); 
            return; // 물 속에선 아래 로직 실행 안 함
        }

        bool isMoving = horizontalInput != 0 || verticalInput != 0;
        bool isRunning = Input.GetKey(runKey) && isMoving;
        playerAnimator.SetBool("isRunning", isRunning);
    }

    public void SetInWater(bool inWater)
    {
        _isInWater = inWater;

        if (_isInWater)
        {
            // 물에 들어갔을 때
            // [삭제] rb.useGravity = false; // <-- 이 줄을 반드시 삭제하거나 주석 처리!
            rb.drag = waterDrag;   // 물 저항 적용

            // 물에 빠지는 순간 Y축 속도를 0에 가깝게 줄여서 '첨벙' 느낌을 줌 (선택 사항)
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.1f, rb.velocity.z);
        }
        else
        {
            // 물에서 나왔을 때
            // [삭제] rb.useGravity = true; // <-- 이 줄도 삭제!
            rb.drag = 0f; // 기본 드래그 (Update에서 'grounded' 상태에 따라 다시 설정됨)
        }
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

    public void SetControlEnabled(bool v) 
    { 
        _controlEnabled = v;

        if (!v)
        {
            // [FIX] 입력/방향 클리어
            horizontalInput = 0f;
            verticalInput = 0f;
            moveDirection = Vector3.zero;

            // [FIX] 수평 속도 정지(관성 제거)
            if (rb)
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

            // [FIX] 애니메이터 기본화
            if (playerAnimator)
            {
                playerAnimator.SetBool("isRunning", false);
                // 있으면 쓰고, 없으면 무시됨(경고가 뜨면 빼도 됨)
                playerAnimator.SetBool("isMoving", false);
            }
        }
    }
}