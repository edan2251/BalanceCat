using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerFreezeStatus : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public PlayerRespawn playerRespawn;
    [Tooltip("씬에 있는 Global Volume 오브젝트 (Priority 높게 설정 필수!)")]
    public Volume globalVolume;

    [Header("Freeze Settings")]
    [Tooltip("얼어붙는 속도 (초당)")]
    public float freezeRate = 0.2f;
    [Tooltip("녹는 속도 (초당)")]
    public float thawRate = 0.3f;

    [Tooltip("물에서 나온 후 녹기 시작하기까지 걸리는 대기 시간 (초)")]
    public float thawDelay = 3.0f; // [신규] 3초 대기

    [Tooltip("이 값 이상 얼면 사망 (0.0 ~ 1.0)")]
    public float deathThreshold = 0.95f;

    [Header("Visual Settings")]
    [Tooltip("비네팅 최대 강도")]
    public float maxVignetteIntensity = 0.6f;

    // --- 상태 변수 ---
    [SerializeField, Range(0f, 1f)]
    private float _currentFreezeAmount = 0f;
    private bool _isInIceWater = false;

    // [신규] 녹는 대기 시간을 체크할 타이머
    private float _thawTimer = 0f;

    private Vignette _vignette;

    void Start()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (playerRespawn == null) playerRespawn = GetComponent<PlayerRespawn>();

        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette vig))
        {
            _vignette = vig;
        }
        else
        {
            Debug.LogWarning("Global Volume이 연결되지 않았거나 Vignette를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        // 사망/리스폰 중이면 로직 중단
        if (playerRespawn.IsRespawning || playerRespawn.IsLavaDying) return;

        // 1. 얼림/녹임 상태 계산
        if (_isInIceWater)
        {
            // 물 안에 있을 때는:
            // 1. 계속 얼어붙음
            _currentFreezeAmount += freezeRate * Time.deltaTime;

            // 2. 타이머 초기화 (나가는 순간 0부터 다시 세기 위해)
            _thawTimer = 0f;
        }
        else
        {
            // 물 밖에 있을 때는:
            if (_currentFreezeAmount > 0)
            {
                // 1. 타이머를 잰다
                _thawTimer += Time.deltaTime;

                // 2. 지정한 시간(3초)이 지났는지 확인
                if (_thawTimer >= thawDelay)
                {
                    // 시간이 지났으면 녹이기 시작
                    _currentFreezeAmount -= thawRate * Time.deltaTime;
                }
                // 시간이 안 지났으면? _currentFreezeAmount 유지 (한기 지속)
            }
            else
            {
                // 완전히 다 녹았으면 타이머 리셋
                _thawTimer = 0f;
            }
        }

        // 0~1 사이로 값 제한
        _currentFreezeAmount = Mathf.Clamp01(_currentFreezeAmount);


        // 2. 이동 속도 반영
        float speedMultiplier = 1.0f - (_currentFreezeAmount * 0.9f);
        playerMovement.SetSpeedMultiplier(speedMultiplier);


        // 3. 화면 효과(비네팅) 반영
        if (_vignette != null)
        {
            _vignette.intensity.value = _currentFreezeAmount * maxVignetteIntensity;
        }


        // 4. 사망 체크
        if (_currentFreezeAmount >= deathThreshold)
        {
            FreezeDeath();
        }
    }

    private void FreezeDeath()
    {
        Debug.Log("동사했습니다!");
        playerRespawn.TriggerRespawn();

        // 상태 초기화
        _currentFreezeAmount = 0f;
        _thawTimer = 0f; // 타이머도 초기화
        if (_vignette != null) _vignette.intensity.value = 0f;
        playerMovement.SetSpeedMultiplier(1.0f);
    }

    // --- 충돌 감지 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("IceWater"))
        {
            _isInIceWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("IceWater"))
        {
            _isInIceWater = false;
            // 여기서 타이머를 0으로 확실히 초기화
            _thawTimer = 0f;
        }
    }
}