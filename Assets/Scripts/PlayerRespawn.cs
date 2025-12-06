using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("References")]
    [Tooltip("이동 제어를 위해 연결")]
    public PlayerMovement playerMovement;
    [Tooltip("화면 페이드를 위한 CanvasGroup")]
    public CanvasGroup fadePanelCanvasGroup;
    [Tooltip("수위 조절 물 오브젝트")]
    public MovableWater mainWaterObject;
    [Tooltip("용암 사망 시 아이템 드랍용")]
    public InventorySlotBreaker inventorySlotBreaker;

    [Header("Respawn Settings")]
    [Tooltip("닿으면 즉사(리스폰)하는 레이어")]
    public LayerMask whatIsRespawn;

    [Tooltip("안전 지점으로 기록할 수 있는 땅 레이어 (무너지는 땅 제외)")]
    public LayerMask whatIsSafeGround; // [신규] 안전한 땅 레이어

    [Tooltip("안전 지점 저장을 위한 최소 대기 시간")]
    public float safePosSaveTime = 0.5f;

    [Header("Drowning Settings")]
    [Tooltip("머리 위치 감지용")]
    public Transform headCheckPoint;

    // --- 상태 변수 ---
    private Vector3 _lastSafePosition;
    private float _safePositionTimer = 0f;
    private bool _isRespawning = false;
    private bool _isLavaDying = false;
    private bool _isDrowning = false;

    // 외부에서 상태 확인용 프로퍼티
    public bool IsRespawning => _isRespawning;
    public bool IsLavaDying => _isLavaDying;

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        // 시작 위치 저장
        _lastSafePosition = transform.position;
    }

    private void Update()
    {
        if (_isRespawning || _isLavaDying) return;

        // 1. 안전 지점 저장 로직
        UpdateSafePosition();

        // 2. 익사 체크 (플레이어가 물에 있을 때만)
        if (playerMovement.IsInWater)
        {
            CheckDrowning();
        }
    }

    // --- [신규] 안전한 땅인지 체크하고 위치 저장 ---
    private void UpdateSafePosition()
    {
        // 플레이어가 땅에 있고 + 그 땅이 '안전한 땅' 레이어라면
        if (playerMovement.IsGrounded && CheckIfOnSafeGround())
        {
            _safePositionTimer += Time.deltaTime;
            if (_safePositionTimer > safePosSaveTime)
            {
                _lastSafePosition = transform.position;
                _safePositionTimer = 0f;
            }
        }
        else
        {
            _safePositionTimer = 0f;
        }
    }

    // 발 밑의 땅이 SafeLayer인지 확인하는 레이캐스트
    private bool CheckIfOnSafeGround()
    {
        // 플레이어 발 밑으로 짧게 레이를 쏴서 확인
        float checkDist = playerMovement.playerHeight + 0.2f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDist, whatIsSafeGround))
        {
            return true;
        }
        return false;
    }

    // --- 익사 감지 ---
    private void CheckDrowning()
    {
        Collider waterCol = playerMovement.CurrentWaterCollider;
        if (headCheckPoint == null || waterCol == null) return;

        float waterSurfaceY = waterCol.bounds.max.y;
        float headY = headCheckPoint.position.y;

        if (headY < waterSurfaceY && !_isDrowning)
        {
            StartCoroutine(DrowningProcessCoroutine());
        }
    }

    // --- 충돌 감지 (낙사/함정) ---
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & whatIsRespawn) != 0)
        {
            TriggerRespawn();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & whatIsRespawn) != 0)
        {
            TriggerRespawn();
        }
    }

    // --- 공개 함수들 (외부 호출용) ---

    public void TriggerRespawn()
    {
        if (_isRespawning) return;
        StartCoroutine(RespawnCoroutine());
    }

    public void TriggerLavaDeath()
    {
        if (_isLavaDying || _isRespawning) return;
        StartCoroutine(LavaDeathProcessCoroutine());
    }

    // --- 코루틴 로직 ---

    private IEnumerator DrowningProcessCoroutine()
    {
        _isDrowning = true;
        yield return new WaitForSeconds(0.4f); // 연출 대기

        // 여전히 가라앉은 상태라면 리스폰
        if (_isDrowning)
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    private IEnumerator LavaDeathProcessCoroutine()
    {
        _isLavaDying = true;
        playerMovement.SetControlEnabled(false); // 조작 차단

        

        // 2. 끈적하게 가라앉기 (물리 효과)
        Rigidbody rb = playerMovement.rb;
        rb.velocity = Vector3.zero; // 속도 초기화
        rb.drag = 3.0f;             // 꿀처럼 끈적하게 떨어지도록 저항 높임

        // 3. 0.5초 대기 (가라앉는 연출)
        yield return new WaitForSeconds(0.5f);

        // 4. 리스폰 시작
        StartCoroutine(RespawnCoroutine());

        // 1. 아이템 떨구기 (InventorySlotBreaker 연결 확인)
        if (inventorySlotBreaker != null)
        {
            inventorySlotBreaker.BreakRandomSlotAndDrop();
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        _isRespawning = true;

        // [추가] 퀘스트 매니저에게 리스폰 사실 알림
        if (InGameQuestManager.Instance != null)
        {
            InGameQuestManager.Instance.OnPlayerRespawn();
        }

        playerMovement.SetControlEnabled(false);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1f, 0.5f));

        // 수위 리셋
        if (mainWaterObject != null) mainWaterObject.ResetToDefaultHeight();

        // 위치 이동 및 물리 초기화
        transform.position = _lastSafePosition;
        Rigidbody rb = playerMovement.rb;
        rb.velocity = Vector3.zero;

        // 상태 초기화
        if (playerMovement.IsInWater) playerMovement.ForceExitWater(); // 물에서 강제 퇴장
        playerMovement.SetSlippery(false);

        _isLavaDying = false;
        _isDrowning = false;
        rb.drag = 0f;

        // Fade In
        yield return StartCoroutine(FadeCanvas(0f, 0.5f));

        // 조작 복구
        playerMovement.SetSpeedControl(true);
        playerMovement.SetControlEnabled(true);
        _isRespawning = false;
    }

    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        if (fadePanelCanvasGroup == null) yield break;
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
}