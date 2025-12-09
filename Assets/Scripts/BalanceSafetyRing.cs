using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class BalanceSafetyRing : MonoBehaviour
{
    [Header("Refs")]
    public Transform ringCenter;     // 링 중심(비우면 자기 transform)
    public Transform player;         // 플레이어 Transform (부모)
    public LineRenderer line;        // 아크 LineRenderer

    [Header("Ring Settings")]
    public float ringRadius = 0.8f;      // 로컬 XZ 거리 기준 링 반지름
    public float dangerStart = 0.4f;     // 이 거리부터 아크 표시
    public float yOffset = 0.05f;        // 약간 띄우기

    [Header("Arc Shape")]
    public int segmentCount = 32;        // 아크 세그먼트 수
    public float minArcAngle = 20f;      // 최소 아크 각도
    public float maxArcAngle = 80f;      // 최대 아크 각도

    [Header("MiniGame")]
    public bool autoStartMiniGame = false;
    public UnityEvent onExitRing;        // 링을 넘었을 때

    [Header("Bias Follow Settings")]
    public PlayerMovement movement;          // 플레이어 이동 스크립트
    public InventorySideBias sideBias;       // 편향 값

    public float tiltDeadZone = 0.02f;       // 이 값 이하면 편향 무시(아크 그리기용)
    public float inputDeadZone = 0.08f;      // 입력 데드존

    [Header("Bias Accumulation")]
    [Tooltip("tilt=1일 때 초당 링 로컬 x 이동량")]
    public float biasAccumulationSpeed = 1.0f;
    [Tooltip("쏠림/입력 없을 때 중심으로 되돌아가는 속도")]
    public float biasReturnSpeed = 3.0f;

    [Header("Smoothing")]
    [Tooltip("기울기 값 변화를 부드럽게 만드는 시간 (초 단위)")]
    public float tiltSmoothTime = 0.15f;

    Vector3 _localOffset;

    float _currentSmoothTilt;
    float _tiltVelocity;

    bool _triggered;

    void Reset()
    {
        ringCenter = transform;
        if (!player && transform.parent != null)
            player = transform.parent;

        line = GetComponent<LineRenderer>();

        ringRadius = 0.8f;
        dangerStart = 0.4f;
        yOffset = 0.05f;

        segmentCount = 32;
        minArcAngle = 20f;
        maxArcAngle = 80f;

        tiltDeadZone = 0.02f;
        inputDeadZone = 0.08f;

        biasAccumulationSpeed = 1.0f;
        biasReturnSpeed = 3.0f;
        tiltSmoothTime = 0.15f;

        if (line)
        {
            line.useWorldSpace = false;
            line.loop = false;
            line.enabled = false;
        }

        _localOffset = Vector3.zero;
    }

    void Awake()
    {
        if (!ringCenter) ringCenter = transform;
        if (!player && transform.parent != null) player = transform.parent;
        if (!line) line = GetComponent<LineRenderer>();

        if (line)
        {
            line.useWorldSpace = false;
            line.loop = false;
            line.enabled = false;
        }

        if (!movement && player) movement = player.GetComponent<PlayerMovement>();
        if (!sideBias && movement) sideBias = movement.sideBias;

        _localOffset = Vector3.zero;
        ringCenter.localPosition = Vector3.zero;
        _triggered = false;

        _currentSmoothTilt = 0f;
        _tiltVelocity = 0f;
    }

    float GetTilt()
    {
        if (movement != null)
            return movement.GetEffectiveTilt();
        if (sideBias != null)
            return sideBias.tilt;
        return 0f;
    }

    void Update()
    {
        if (!ringCenter || !player || !line) return;

        if (!movement && player) movement = player.GetComponent<PlayerMovement>();
        if (!sideBias && movement) sideBias = movement.sideBias;

        if (movement != null && !movement.enabled)
        {
            _localOffset = Vector3.zero;
            ringCenter.localPosition = Vector3.zero;
            _triggered = false;
            line.enabled = false;
            return;
        }

        float targetTilt = GetTilt();
        _currentSmoothTilt = Mathf.SmoothDamp(_currentSmoothTilt, targetTilt, ref _tiltVelocity, tiltSmoothTime);

        UpdateRingOffsetLocal();

        Vector3 local = ringCenter.localPosition;
        Vector2 localXZ = new Vector2(local.x, local.z);
        float dist = localXZ.magnitude;

        if (dist < dangerStart * 0.5f)
        {
            _triggered = false;
        }

        if (dist <= dangerStart)
        {
            line.enabled = false;
            return;
        }

        if (dist >= ringRadius)
        {
            if (!_triggered)
            {
                _triggered = true;
                line.enabled = false;

                if (autoStartMiniGame)
                {
                    onExitRing?.Invoke();
                }
            }
            return;
        }

        DrawArcLocal(dist);
    }

    void UpdateRingOffsetLocal()
    {
        float hx = 0f, vz = 0f;
        if (movement)
        {
            hx = movement.horizontalInput;
            vz = movement.verticalInput;
        }
        Vector2 in2 = new Vector2(hx, vz);
        bool hasInput = in2.sqrMagnitude > (inputDeadZone * inputDeadZone);

        float tilt = _currentSmoothTilt;

        if (hasInput)
        {
            float moveSpeed = 0f;
            if (movement != null)
                moveSpeed = movement.CurrentMoveSpeed;

            if (moveSpeed > 0.001f)
            {
                float lateralRatio = tilt / Mathf.Sqrt(1f + tilt * tilt);
                float lateralSpeed = lateralRatio * moveSpeed * biasAccumulationSpeed;

                _localOffset.x -= lateralSpeed * Time.deltaTime;
            }
        }
        else
        {
            _localOffset = Vector3.Lerp(_localOffset, Vector3.zero, biasReturnSpeed * Time.deltaTime);
        }

        _localOffset.y = 0f;
        ringCenter.localPosition = _localOffset;
    }

    void DrawArcLocal(float dist)
    {
        Vector3 local = ringCenter.localPosition;
        bool isRight = local.x < 0f;

        float t = Mathf.InverseLerp(dangerStart, ringRadius, dist);
        t = Mathf.Clamp01(t);

        float arcAngle = Mathf.Lerp(minArcAngle, maxArcAngle, t);
        float halfArc = arcAngle * 0.5f;

        float sideAngle = isRight ? 90f : -90f;
        float startAngle = sideAngle - halfArc;
        float endAngle = sideAngle + halfArc;

        int seg = Mathf.Max(3, segmentCount);
        line.positionCount = seg + 1;
        line.enabled = true;

        for (int i = 0; i <= seg; i++)
        {
            float f = (seg == 0) ? 0f : (i / (float)seg);
            float angRad = Mathf.Deg2Rad * Mathf.Lerp(startAngle, endAngle, f);

            float x = Mathf.Sin(angRad) * ringRadius;
            float z = Mathf.Cos(angRad) * ringRadius;

            Vector3 localPos = new Vector3(x, yOffset, z);
            line.SetPosition(i, localPos);
        }
    }
}