using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BalanceSafetyRing : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;            // �÷��̾� Transform
    [SerializeField] Rigidbody rb;                // �÷��̾� ������ٵ�
    [SerializeField] PlayerMovement movement;     // PlayerMovement (orientation, input ����)
    [Tooltip("�� ����(��) Transform. ���� �� ������Ʈ�� ���� ������Ʈ�� ���")]
    [SerializeField] Transform ring;

    [Header("Ring Settings")]
    [Tooltip("��Ż ���� ������(���� ��� XZ �� �Ÿ�)")]
    [SerializeField] float ringRadius = 2f;
    [Tooltip("�� �̵� ���� ����")]
    [SerializeField] float followSmoothing = 12f;
    [Tooltip("���� Y�� �׻� �÷��̾� ���̿� ����")]
    [SerializeField] bool lockYToPlayer = true;

    [Header("Bias Detection")]
    [Tooltip("��(���� �ӵ�) ���� �Ӱ谪 m/s")]
    [SerializeField] float sideSpeedThreshold = 0.15f;
    [Tooltip("�Է� ��ȿ ���� ������")]
    [SerializeField] float inputDeadZone = 0.08f;

    [Header("Events")]
    [Tooltip("�÷��̾ �� �ݰ��� ����� ���� 1ȸ ȣ��")]
    public UnityEvent onExitRing;
    [Tooltip("�÷��̾ �ٽ� �� �ݰ� ������ �����ϴ� ���� 1ȸ ȣ��(����)")]
    public UnityEvent onReenterRing;

    // --- ���� ���� ---
    Vector3 _neutralCenter;     // �� ���� ����� ���� �߽�
    bool _initialized;
    bool _isOutside;

    void Reset()
    {
        // �ڵ� ����
        if (!player) player = transform;
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!ring) ring = transform; // �ڱ� �ڽ��� ������ ���
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

        // --- �Է�/���� ��� ---
        var ori = movement.orientation;
        Vector3 camFwd = new Vector3(ori.forward.x, 0f, ori.forward.z).normalized;
        Vector3 camRight = new Vector3(ori.right.x, 0f, ori.right.z).normalized;

        float hx = movement.horizontalInput;
        float vz = movement.verticalInput;
        Vector2 in2 = new Vector2(hx, vz);
        bool hasInput = in2.sqrMagnitude > (inputDeadZone * inputDeadZone);

        Vector3 moveDir = hasInput ? (camFwd * vz + camRight * hx).normalized : Vector3.zero;

        // --- ���� �ӵ����� ����/���� ���� ---
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 forwardVel = hasInput ? Vector3.Project(flatVel, moveDir) : Vector3.zero;
        Vector3 sideVel = hasInput ? (flatVel - forwardVel) : Vector3.zero;

        bool biasActive = hasInput && sideVel.magnitude > sideSpeedThreshold;

        // --- ���� �߽� ���� ���� ---
        if (!hasInput || !biasActive)
        {
            // �Է��� ���ų� ���� ��ǻ� ������ �÷��̾� �߽����� ����
            _neutralCenter = player.position;
        }
        else
        {
            // �� �߿��� "�Է� ���� ���и�" �����Ͽ� ���� ���� ��θ� ����
            _neutralCenter += forwardVel * Time.deltaTime;
        }

        if (lockYToPlayer) _neutralCenter.y = player.position.y;

        // --- �� ��ġ ���� ---
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

        // --- ��Ż ���� (XZ ��� �Ÿ�) ---
        Vector2 p = new Vector2(player.position.x, player.position.z);
        Vector2 c = new Vector2(ring.position.x, ring.position.z);
        float dist = Vector2.Distance(p, c);

        if (!_isOutside && dist > ringRadius)
        {
            _isOutside = true;
            onExitRing?.Invoke();      // �� �̴ϰ��� ���� ����
        }
        else if (_isOutside && dist <= ringRadius)
        {
            _isOutside = false;
            onReenterRing?.Invoke();   // �� ���� ó��(����)
        }
    }
}
