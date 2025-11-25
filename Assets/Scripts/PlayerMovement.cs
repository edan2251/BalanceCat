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
    public KeyCode balanceLeftKey = KeyCode.Z;
    public KeyCode balanceRightKey = KeyCode.C;

    // --- 물 관련 설정 추가 ---
    [Header("Water Physics")]
    [Tooltip("물 속에서 움직이는 속도 (천천히)")]
    public float waterMoveSpeed = 2.0f;
    [Tooltip("물에서 둥둥 뜨게 하는 기본 부력")]
    public float floatingForce = 10.0f;
    [Tooltip("물 속에서의 저항 (움직임을 둔하게 만듦)")]
    public float waterDrag = 2.0f;
    [Tooltip("둥실거리는 파도의 속도 (낮을수록 '두웅~')")]
    public float waterBobbingSpeed = 1.0f;
    [Tooltip("둥실거리는 파도의 높낮이 (클수록 '시일~')")]
    public float waterBobbingAmount = 0.5f;

    [Header("Ice Physics")]
    [Tooltip("얼음 위에서의 지면 저항 (거의 0)")]
    public float iceGroundDrag = 0.1f;
    [Tooltip("얼음 위에서의 조작 민첩성 (0~1 사이, 낮을수록 미끄러짐)")]
    [Range(0f, 1f)]
    public float iceControlMultiplier = 0.1f;

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

    private bool _isInWater = false;
    private bool _isOnIce = false;

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
    bool _controlEnabled = true;
    private bool _speedControlEnabled = true;

    Vector3 moveDirection;
    float currentMoveSpeed;

    public bool IsControlEnabled => _controlEnabled;

    public float CurrentMoveSpeed => currentMoveSpeed;

    public InventorySideBias sideBias;


    [Header("Respawn")]
    [Tooltip("머리 위치를 감지할 빈 오브젝트")]
    [SerializeField] private Transform headCheckPoint;
    [SerializeField] CanvasGroup fadePanelCanvasGroup;

    [Tooltip("수위 조절이 되는 메인 물 오브젝트 (MovableWater 스크립트가 있는)")]
    [SerializeField] private MovableWater mainWaterObject;

    private Vector3 _lastSafePosition;
    private float _safePositionTimer = 0f;
    private bool _isRespawning = false;
    private bool _isDrowning = false;

    private Collider _currentWaterCollider;

    public float balanceAdjustSpeed = 2.0f;
    public float balanceReturnSpeed = 1.0f;
    float _manualTilt = 0f;

    public float GetEffectiveTilt()
    {
        float baseTilt = (sideBias != null) ? sideBias.tilt : 0f; // 인벤토리 쏠림
        float result = baseTilt + _manualTilt;                    // Z/C 보정 더함
        return Mathf.Clamp(result, -1f, 1f);
    }

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
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

        if (grounded)
        {
            _safePositionTimer += Time.deltaTime;
            if (_safePositionTimer > 0.5f)
            {
                _lastSafePosition = transform.position;
                _safePositionTimer = 0f;
            }
        }
        else
        {
            _safePositionTimer = 0f;
        }

        if (_isInWater && !_isRespawning)
        {
            CheckDrowning();
        }

        if (BalanceMiniGame.IsRunning)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
            moveDirection = Vector3.zero;

            if (rb)
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

            if (playerAnimator)
            {
                playerAnimator.SetBool("isRunning", false);
                playerAnimator.SetBool("isMoving", false);
            }

            return;
        }

        UpdateBalanceControl();

        GetInput();
        if (_speedControlEnabled)
        {
            SpeedControl();
        }
        UpdateAnimator();

        // 지면에 있을 때만 드래그 적용
        if (_isInWater)
        {
            // (기존) 물 속에 있을 땐 SetInWater에서 waterDrag를 이미 적용함
            // (rb.drag = waterDrag; 가 SetInWater에 있어야 함)
        }
        else if (grounded)
        {
            // 땅에 있을 때
            rb.drag = _isOnIce ? iceGroundDrag : groundDrag;
        }
        else
        {
            // 공중에 있을 때
            rb.drag = 0f;
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

        if (Input.GetKey(jumpKey) && readyToJump && grounded && !_isInWater)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    public void SetSpeedControl(bool isEnabled)
    {
        // [수정] 얼음 위에 있을 때는 속도 제한을 끄지 않도록 함
        if (_isOnIce)
        {
            _speedControlEnabled = true;
            return;
        }
        _speedControlEnabled = isEnabled;
    }

    private void MovePlayer()
    {
        // 카메라 방향에 맞게 이동 방향 계산
        Vector3 camForward = new Vector3(orientation.forward.x, 0f, orientation.forward.z).normalized;
        Vector3 camRight = new Vector3(orientation.right.x, 0f, orientation.right.z).normalized;
        moveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;
        Vector3 finalMoveDir = moveDirection;

        float currentControlMultiplier = _isOnIce ? iceControlMultiplier : 1.0f;

        if (sideBias != null && finalMoveDir.sqrMagnitude > 0.001f)
        {
            float tilt = GetEffectiveTilt();

            if (Mathf.Abs(tilt) > 0.001f)
            {
                Vector3 sideDir = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
                float driftScale = 1f;      // 0~1 편향 세기 조절
                Vector3 drift = sideDir * (tilt * driftScale);

                finalMoveDir += drift;
                finalMoveDir.Normalize();
            }
        }

        if (_isInWater)
        {
            rb.AddForce(finalMoveDir * currentMoveSpeed * 10f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(finalMoveDir * currentMoveSpeed * 10f * currentControlMultiplier, ForceMode.Force);
        }
        else
        {
            rb.AddForce(finalMoveDir * currentMoveSpeed * 10f * airMultiplier * currentControlMultiplier, ForceMode.Force);
        }
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

        if (_isDrowning)
        {
            rb.AddForce(Vector3.up * 9.3f, ForceMode.Force);
        }
        else
        {
            // 기존 둥실 로직: 파도 효과 + 기본 부력
            float bobbingFactor = Mathf.Sin(Time.time * waterBobbingSpeed) * waterBobbingAmount;
            float totalUpwardForce = floatingForce + bobbingFactor;
            rb.AddForce(Vector3.up * totalUpwardForce, ForceMode.Force);
        }
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

    public void SetSlippery(bool isOnIce)
    {
        _isOnIce = isOnIce;
    }

    public void SetInWater(bool inWater, Collider waterCollider)
    {
        _isInWater = inWater;
        _currentWaterCollider = waterCollider; 

        if (_isInWater)
        {
            // 물에 들어갔을 때
            rb.drag = waterDrag;
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.1f, rb.velocity.z);
        }
        else
        {
            // 물에서 나왔을 때
            _isDrowning = false;
            rb.drag = 0f;
        }

        if (!inWater)
        {
            // 물에서 나왔을 때
            _isDrowning = false;
            rb.drag = 0f;
            if (!_isOnIce) SetSpeedControl(true);
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

    private void CheckDrowning()
    {
        // 머리 위치 표식이 없거나, 물 콜라이더 정보가 없으면 실행 안 함
        if (headCheckPoint == null || _currentWaterCollider == null) return;

        // [수정] 물 표면 높이를 '콜라이더의 가장 높은 지점(bounds.max.y)'으로 계산
        float waterSurfaceY = _currentWaterCollider.bounds.max.y;

        // 머리 위치
        float headY = headCheckPoint.position.y;

        // 머리가 물 표면보다 낮으면 리스폰 트리거
        if (headY < waterSurfaceY && !_isDrowning && !_isRespawning)
        {
            // 0.3초 뒤 리스폰을 실행하는 코루틴을 *시작만* 함
            StartCoroutine(DrowningProcessCoroutine());
        }
    }

    // [신규] 리스폰 트리거 함수 (DeepWaterZone이 호출)
    public void TriggerRespawn()
    {
        // 리스폰 중이 아닐 때만 실행
        if (_isRespawning) return;

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        _isRespawning = true;
        SetControlEnabled(false); // 플레이어 조작 비활성화

        // 1. 화면 어둡게 (Fade Out)
        float fadeDuration = 0.5f;
        yield return StartCoroutine(FadeCanvas(3f, fadeDuration));

        // --- [신규] 물 높이 리셋 ---
        if (mainWaterObject != null)
        {
            mainWaterObject.ResetToDefaultHeight();
        }
        else
        {
            Debug.LogWarning("PlayerMovement: mainWaterObject가 연결되지 않아 수위를 리셋할 수 없습니다.");
        }
        // --- [신규] 끝 ---

        // 2. 플레이어 위치 및 상태 리셋
        transform.position = _lastSafePosition;
        rb.velocity = Vector3.zero;

        // 3. 물에서 강제로 꺼냄
        if (_isInWater)
        {
            SetInWater(false, null);
        }
        SetSlippery(false);

        // 4. 화면 밝게 (Fade In)
        yield return StartCoroutine(FadeCanvas(0f, fadeDuration));

        // 5. 조작 권한 돌려주기
        SetControlEnabled(true);
        _isRespawning = false;

        // 4. 조작 권한 돌려주기
        SetSpeedControl(true); // [신규] 속도 제한 복구
        SetControlEnabled(true);
        _isRespawning = false;
    }

    // [신규] 캔버스 페이드 효과를 위한 코루틴
    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        if (fadePanelCanvasGroup == null) yield break; // 패널 없으면 스킵

        float startAlpha = fadePanelCanvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadePanelCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            yield return null;
        }
        fadePanelCanvasGroup.alpha = targetAlpha;
    }

    private IEnumerator DrowningProcessCoroutine()
    {
        _isDrowning = true;

        // 2. 0.3초 대기 (가라앉는 연출 시간)
        yield return new WaitForSeconds(0.4f);

        if (_isDrowning)
        {
            // 기존 리스폰 코루틴 실행
            StartCoroutine(RespawnCoroutine());
        }
    }

    void UpdateBalanceControl()
    {
        if (sideBias == null)
        {
            _manualTilt = 0f;
            return;
        }

        float baseTilt = sideBias.tilt;

        // 무게 차이가 거의 없으면 자동으로 보정값만 0으로 복귀
        if (Mathf.Abs(baseTilt) < 0.001f)
        {
            _manualTilt = Mathf.MoveTowards(_manualTilt, 0f,
                balanceReturnSpeed * Time.deltaTime);
            return;
        }

        bool pressLeft = Input.GetKey(balanceLeftKey);
        bool pressRight = Input.GetKey(balanceRightKey);

        // 오른쪽으로 쏠리면(Z) 왼쪽으로 균형, 왼쪽으로 쏠리면(C) 오른쪽으로 균형
        bool pressingCorrectKey =
            (baseTilt > 0f && pressLeft) ||   // 오른쪽 무거움 → Z
            (baseTilt < 0f && pressRight);    // 왼쪽 무거움 → C

        float target = pressingCorrectKey ? -baseTilt : 0f; // 완전히 상쇄하는 방향으로
        float speed = pressingCorrectKey ? balanceAdjustSpeed : balanceReturnSpeed;

        _manualTilt = Mathf.MoveTowards(_manualTilt, target, speed * Time.deltaTime);
    }
}