using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BreakingPlatform : MonoBehaviour
{
    [Header("파괴 설정")]
    public float timeToBreak = 3.0f;
    public bool resetOnExit = true;

    [Tooltip("플레이어가 나간 후 리셋되기까지 걸리는 시간")]
    public float resetDelay = 4.0f; // (기본 4초, 인스펙터에서 수정 가능)

    // --- [신규] 서서히 돌아오는 기능 복구 ---
    [Tooltip("색상이 원래대로 돌아오는 데 걸리는 시간")]
    public float visualResetDuration = 1.0f;
    // --- [신규] 끝 ---

    [Tooltip("점프/달리기 시 헛밟는 것을 무시할 유예 시간(초)")]
    public float collisionGracePeriod = 0.1f;

    [Header("재생성(리스폰) 설정")]
    public bool canRespawn = false;
    public float respawnTime = 10.0f;

    [Header("시각 효과 설정")]
    public Color breakingColor = Color.red;

    // --- 내부 변수들 ---
    private float _timer = 0f;
    private bool _isPlayerOn = false;
    private bool _isBreaking = false;

    private MeshRenderer _renderer;
    private Rigidbody _rb;
    private Color _originalColor;

    private float _weightMultiplier = 1.0f;

    private Coroutine _resetCoroutine;
    private Coroutine _gracePeriodCoroutine;

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public void SetWeightMultiplier(float multiplier)
    {
        _weightMultiplier = multiplier;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponentInChildren<MeshRenderer>();

        _rb.isKinematic = true;
        _rb.useGravity = false;

        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }

        _originalPosition = transform.position;
        _originalRotation = transform.rotation;

        GetComponent<Collider>().isTrigger = false;
    }

    void Update()
    {
        if (_isPlayerOn && !_isBreaking)
        {
            _timer += Time.deltaTime * _weightMultiplier;
            UpdateBreakingVisuals();

            if (_timer >= timeToBreak)
            {
                StartCoroutine(BreakPlatform());
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !_isBreaking)
        {
            _isPlayerOn = true;

            if (_resetCoroutine != null)
            {
                StopCoroutine(_resetCoroutine);
                _resetCoroutine = null;
            }

            if (_gracePeriodCoroutine != null)
            {
                StopCoroutine(_gracePeriodCoroutine);
                _gracePeriodCoroutine = null;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (!_isBreaking && _gracePeriodCoroutine == null)
            {
                _gracePeriodCoroutine = StartCoroutine(GracePeriodExitCoroutine());
            }
        }
    }

    private IEnumerator GracePeriodExitCoroutine()
    {
        yield return new WaitForSeconds(collisionGracePeriod);

        _isPlayerOn = false;
        SetWeightMultiplier(1.0f);

        if (resetOnExit && !_isBreaking)
        {
            _resetCoroutine = StartCoroutine(ResetTimerCoroutine());
        }

        _gracePeriodCoroutine = null;
    }


    /// <summary>
    /// [수정] 설정된 시간(resetDelay) 후에 타이머를 '서서히' 리셋하는 코루틴
    /// </summary>
    private IEnumerator ResetTimerCoroutine()
    {
        // 1. 설정한 4초(resetDelay) 동안 기다림
        yield return new WaitForSeconds(resetDelay);

        // 2. 4초가 지났는데도 플레이어가 여전히 발판 위에 없다면
        if (!_isPlayerOn)
        {
            // [수정] 즉시 리셋하는 대신, '서서히' _timer를 0으로 줄임
            float currentTimer = _timer; // 현재 타이머 값 저장
            float elapsed = 0f;

            // visualResetDuration (예: 1초)에 걸쳐서 타이머를 0으로 내림
            while (elapsed < visualResetDuration)
            {
                // (플레이어가 다시 밟으면 OnCollisionEnter가 이 코루틴을 중단시킴)
                if (_isPlayerOn)
                {
                    _resetCoroutine = null;
                    yield break;
                }

                elapsed += Time.deltaTime;
                _timer = Mathf.Lerp(currentTimer, 0f, elapsed / visualResetDuration);
                UpdateBreakingVisuals(); // 시각 효과(색상)도 서서히 업데이트

                yield return null; // 다음 프레임까지 대기
            }

            // 루프가 끝나면 확실하게 0으로 설정
            if (!_isPlayerOn)
            {
                _timer = 0f;
                UpdateBreakingVisuals();
            }
        }

        // 3. 코루틴 종료
        _resetCoroutine = null;
    }

    private void UpdateBreakingVisuals()
    {
        if (_renderer == null) return;
        float breakPercent = Mathf.Clamp01(_timer / timeToBreak);
        _renderer.material.color = Color.Lerp(_originalColor, breakingColor, breakPercent);
    }

    private IEnumerator BreakPlatform()
    {
        _isBreaking = true;

        if (_resetCoroutine != null)
        {
            StopCoroutine(_resetCoroutine);
            _resetCoroutine = null;
        }

        if (_gracePeriodCoroutine != null)
        {
            StopCoroutine(_gracePeriodCoroutine);
            _gracePeriodCoroutine = null;
        }

        _rb.isKinematic = false;
        _rb.useGravity = true;

        GetComponent<Collider>().enabled = false;

        if (canRespawn)
        {
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            yield return new WaitForSeconds(5.0f);
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTime);

        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
        _rb.useGravity = false;

        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
        GetComponent<Collider>().enabled = true;

        _isBreaking = false;
        _isPlayerOn = false;
        _timer = 0f;

        SetWeightMultiplier(1.0f);
        UpdateBreakingVisuals();
    }
}