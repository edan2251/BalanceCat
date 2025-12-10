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
    public LayerMask whatIsSafeGround;

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

    public bool IsRespawning => _isRespawning;
    public bool IsLavaDying => _isLavaDying;

    private void Start()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        _lastSafePosition = transform.position;
    }

    private void Update()
    {
        if (_isRespawning || _isLavaDying) return;

        UpdateSafePosition();

        if (playerMovement.IsInWater)
        {
            CheckDrowning();
        }
    }

    private void UpdateSafePosition()
    {
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

    private bool CheckIfOnSafeGround()
    {
        float checkDist = playerMovement.playerHeight + 0.2f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, checkDist, whatIsSafeGround))
        {
            return true;
        }
        return false;
    }

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

    // --- 공개 함수들 ---

    public void TriggerRespawn()
    {
        if (_isRespawning) return;
        // 일반 사망은 아이템 드랍 false
        StartCoroutine(RespawnCoroutine(true));
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
        yield return new WaitForSeconds(0.4f);

        if (_isDrowning)
        {
            // 익사는 아이템 드랍 false (필요하면 true로 변경 가능)
            StartCoroutine(RespawnCoroutine(false));
        }
    }

    private IEnumerator LavaDeathProcessCoroutine()
    {
        _isLavaDying = true;
        playerMovement.SetControlEnabled(false);

        Rigidbody rb = playerMovement.rb;
        rb.velocity = Vector3.zero;
        rb.drag = 3.0f;

        yield return new WaitForSeconds(0.5f);

        // [핵심 변경] 리스폰 코루틴을 실행하되, '아이템 드랍 = true'로 전달
        // 기존에 여기서 아이템을 떨구던 로직은 삭제했습니다.
        StartCoroutine(RespawnCoroutine(true));
    }

    // [핵심 변경] 매개변수 bool dropItem 추가 (기본값 false)
    private IEnumerator RespawnCoroutine(bool dropItem = false)
    {
        _isRespawning = true;

        if (InGameQuestManager.Instance != null)
        {
            InGameQuestManager.Instance.OnPlayerRespawn();
        }

        playerMovement.SetControlEnabled(false);

        // 1. 화면 깜빡임 (Fade Out) - 아직 플레이어는 죽은 위치
        yield return StartCoroutine(FadeCanvas(1f, 0.5f));

        // 2. 수위 리셋
        if (mainWaterObject != null) mainWaterObject.ResetToDefaultHeight();

        // 3. 위치 이동 및 물리 초기화 (이제 플레이어는 안전지대로 이동됨)
        transform.position = _lastSafePosition;

        Rigidbody rb = playerMovement.rb;
        rb.velocity = Vector3.zero;

        if (playerMovement.IsInWater) playerMovement.ForceExitWater();
        playerMovement.SetSlippery(false);
        rb.drag = 0f;

        _isLavaDying = false;
        _isDrowning = false;

        // [여기입니다!] 플레이어가 안전한 위치로 온 직후에 아이템 드랍
        if (dropItem && inventorySlotBreaker != null)
        {
            // 이제 transform.position이 _lastSafePosition이므로 안전한 땅 위에 아이템이 떨어짐
            inventorySlotBreaker.BreakRandomSlotAndDrop();
        }

        // 4. 화면 밝아짐 (Fade In)
        yield return StartCoroutine(FadeCanvas(0f, 0.5f));

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