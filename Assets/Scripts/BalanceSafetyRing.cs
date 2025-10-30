using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BalanceSafetyRing : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;            // 플레이어 Transform
    [SerializeField] Rigidbody rb;                // 플레이어 리지드바디
    [SerializeField] PlayerMovement movement;     // PlayerMovement (orientation, input 참조)
    [Tooltip("원 라인(링) Transform. 비우면 이 컴포넌트가 붙은 오브젝트를 사용")]
    [SerializeField] Transform ring;

    [Header("Ring Settings")]
    [Tooltip("이탈 판정 반지름(지면 평면 XZ 상 거리)")]
    [SerializeField] float ringRadius = 2f;
    [Tooltip("링 이동 보간 세기")]
    [SerializeField] float followSmoothing = 12f;
    [Tooltip("링의 Y를 항상 플레이어 높이에 고정")]
    [SerializeField] bool lockYToPlayer = true;

    [Header("Bias Detection")]
    [Tooltip("쏠림(측면 속도) 판정 임계값 m/s")]
    [SerializeField] float sideSpeedThreshold = 0.15f;
    [Tooltip("입력 유효 판정 데드존")]
    [SerializeField] float inputDeadZone = 0.08f;

    [Header("Events")]
    [Tooltip("플레이어가 원 반경을 벗어나는 순간 1회 호출")]
    public UnityEvent onExitRing;
    [Tooltip("플레이어가 다시 원 반경 안으로 복귀하는 순간 1회 호출(선택)")]
    public UnityEvent onReenterRing;

    // --- 내부 상태 ---
    Vector3 _neutralCenter;     // 쏠림 무시 경로의 기준 중심
    bool _initialized;
    bool _isOutside;

    void Reset()
    {
        // 자동 참조
        if (!player) player = transform;
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!ring) ring = transform; // 자기 자신을 링으로 사용
    }

    void Awake()
    {
        if (!player) player = transform;
        if (!ring) ring = transform;
        _neutralCenter = player.position;
    }

    void Update()
    {
        if (!player || !rb || !movement || !movement.orientation) return;

        // --- 입력/방향 계산 ---
        var ori = movement.orientation;
        Vector3 camFwd = new Vector3(ori.forward.x, 0f, ori.forward.z).normalized;
        Vector3 camRight = new Vector3(ori.right.x, 0f, ori.right.z).normalized;

        float hx = movement.horizontalInput;
        float vz = movement.verticalInput;
        Vector2 in2 = new Vector2(hx, vz);
        bool hasInput = in2.sqrMagnitude > (inputDeadZone * inputDeadZone);

        Vector3 moveDir = hasInput ? (camFwd * vz + camRight * hx).normalized : Vector3.zero;

        // --- 실제 속도에서 전방/측면 분해 ---
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 forwardVel = hasInput ? Vector3.Project(flatVel, moveDir) : Vector3.zero;
        Vector3 sideVel = hasInput ? (flatVel - forwardVel) : Vector3.zero;

        bool biasActive = hasInput && sideVel.magnitude > sideSpeedThreshold;

        // --- 기준 중심 갱신 로직 ---
        if (!hasInput || !biasActive)
        {
            // 입력이 없거나 쏠림이 사실상 없으면 플레이어 중심으로 리셋
            _neutralCenter = player.position;
        }
        else
        {
            // 쏠림 중에는 "입력 전방 성분만" 적분하여 편향 없는 경로를 따라감
            _neutralCenter += forwardVel * Time.deltaTime;
        }

        if (lockYToPlayer) _neutralCenter.y = player.position.y;

        // --- 링 위치 보간 ---
        Vector3 target = (!hasInput || !biasActive) ? player.position : _neutralCenter;
        if (!_initialized)
        {
            ring.position = target;
            _initialized = true;
        }
        else
        {
            ring.position = Vector3.Lerp(ring.position, target, Time.deltaTime * followSmoothing);
        }

        // --- 이탈 판정 (XZ 평면 거리) ---
        Vector2 p = new Vector2(player.position.x, player.position.z);
        Vector2 c = new Vector2(ring.position.x, ring.position.z);
        float dist = Vector2.Distance(p, c);

        if (!_isOutside && dist > ringRadius)
        {
            _isOutside = true;
            onExitRing?.Invoke();      // ← 미니게임 시작 연결
        }
        else if (_isOutside && dist <= ringRadius)
        {
            _isOutside = false;
            onReenterRing?.Invoke();   // ← 복귀 처리(선택)
        }
    }
}
