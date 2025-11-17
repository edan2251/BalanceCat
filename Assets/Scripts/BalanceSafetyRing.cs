using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BalanceSafetyRing : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerMovement movement;
    [SerializeField] Transform ring;
    [SerializeField] InventorySideBias sideBias;

    [SerializeField] float ringRadius = 2f;
    [SerializeField] float followSmoothing = 12f;
    [SerializeField] bool lockYToPlayer = true;

    [SerializeField] float sideSpeedThreshold = 0.15f;
    [SerializeField] float inputDeadZone = 0.08f;

    [SerializeField] float tiltDeadZone = 0.02f;
    [SerializeField] float maxRingOffset = 3.0f;

    public UnityEvent onExitRing;
    public UnityEvent onReenterRing;

    Vector3 _neutralCenter;
    bool _initialized;
    bool _isOutside;

    void Reset()
    {
        if (!player) player = transform;
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!ring) ring = transform;
        if (!sideBias && movement != null) sideBias = movement.sideBias;
    }

    void Awake()
    {
        if (!player) player = transform;
        if (!ring) ring = transform;
        _neutralCenter = player.position;
        if (!movement) movement = GetComponent<PlayerMovement>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!sideBias && movement != null) sideBias = movement.sideBias;
    }

    void Update()
    {
        if (!player || !rb || !movement) return;

        float hx = movement.horizontalInput;
        float vz = movement.verticalInput;
        Vector2 in2 = new Vector2(hx, vz);
        bool hasInput = in2.sqrMagnitude > (inputDeadZone * inputDeadZone);

        Transform ori = movement.orientation != null ? movement.orientation : player;
        Vector3 camForward = new Vector3(ori.forward.x, 0f, ori.forward.z).normalized;
        Vector3 camRight = new Vector3(ori.right.x, 0f, ori.right.z).normalized;
        Vector3 inputDir = hasInput ? (camForward * vz + camRight * hx).normalized : Vector3.zero;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 forwardVel = hasInput && inputDir.sqrMagnitude > 0.0001f
            ? Vector3.Project(flatVel, inputDir)
            : Vector3.zero;
        Vector3 sideVel = hasInput ? (flatVel - forwardVel) : Vector3.zero;

        bool biasActive;
        if (sideBias != null)
        {
            float tiltAbs = Mathf.Abs(sideBias.tilt);
            bool tiltActive = tiltAbs > tiltDeadZone;
            biasActive = hasInput && tiltActive;
        }
        else
        {
            biasActive = hasInput && sideVel.magnitude > sideSpeedThreshold;
        }

        if (!hasInput || !biasActive)
        {
            _neutralCenter = player.position;
        }
        else
        {
            _neutralCenter += forwardVel * Time.deltaTime;
        }

        if (lockYToPlayer)
            _neutralCenter.y = player.position.y;

        Vector3 target = (!hasInput || !biasActive) ? player.position : _neutralCenter;

        Vector3 toTarget = target - player.position;
        Vector3 toTargetXZ = new Vector3(toTarget.x, 0f, toTarget.z);
        float offsetDist = toTargetXZ.magnitude;
        if (offsetDist > maxRingOffset)
        {
            Vector3 clamped = toTargetXZ.normalized * maxRingOffset;
            target = new Vector3(player.position.x + clamped.x, target.y, player.position.z + clamped.z);
        }

        if (!_initialized)
        {
            ring.position = target;
            _initialized = true;
        }
        else
        {
            ring.position = Vector3.Lerp(ring.position, target, Time.deltaTime * followSmoothing);
        }

        if (lockYToPlayer)
        {
            Vector3 p = ring.position;
            p.y = player.position.y;
            ring.position = p;
        }

        Vector2 p2 = new Vector2(player.position.x, player.position.z);
        Vector2 c2 = new Vector2(ring.position.x, ring.position.z);
        float dist = Vector2.Distance(p2, c2);

        if (!_isOutside && dist > ringRadius)
        {
            _isOutside = true;
            onExitRing?.Invoke();
        }
        else if (_isOutside && dist <= ringRadius)
        {
            _isOutside = false;
            onReenterRing?.Invoke();
        }
    }
}