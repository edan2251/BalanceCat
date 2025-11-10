using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleWindZone : MonoBehaviour
{
    [Header("바람 설정")]
    public Vector3 windDirection = Vector3.right;
    public float windForce = 10.0f;

    [Header("이펙트 설정")]
    public ParticleSystem windParticleSystem;

    private BoxCollider _zoneCollider;
    private float _lastCheckedForce = float.MinValue; // [신규] 이전에 체크한 힘

    private void Awake()
    {
        _zoneCollider = GetComponent<BoxCollider>();
        if (_zoneCollider != null)
        {
            _zoneCollider.isTrigger = true;
        }

        // [신규] Awake에서는 트리거 설정만 하도록 변경
        if (windParticleSystem != null)
        {
            SetupParticleTriggers();
        }
    }

    private void Update()
    {
        if (windParticleSystem == null) return;

        // [수정] windForce가 0.01 이상 차이 날 때만 파티클 설정을 업데이트
        if (Mathf.Abs(windForce - _lastCheckedForce) > 0.01f)
        {
            // 1. 파티클 힘(Force) 업데이트
            SetupParticleForce();
            _lastCheckedForce = windForce; // 현재 힘을 저장

            // 2. 파티클 재생/정지
            if (Mathf.Abs(windForce) < 0.01f)
            {
                if (windParticleSystem.isPlaying)
                {
                    windParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            else
            {
                if (!windParticleSystem.isPlaying)
                {
                    windParticleSystem.Play();
                }
            }
        }
    }

    /// <summary>
    /// [신규] 파티클의 '힘'만 설정하는 함수 (Update에서 호출)
    /// </summary>
    private void SetupParticleForce()
    {
        var forceModule = windParticleSystem.forceOverLifetime;
        forceModule.enabled = true;
        forceModule.space = ParticleSystemSimulationSpace.World;

        // (0.1f 삭제한 버전)
        forceModule.x = windDirection.normalized.x * windForce * 0.3f;
        forceModule.y = windDirection.normalized.y * windForce * 0.3f;
        forceModule.z = windDirection.normalized.z * windForce * 0.3f;

        var mainModule = windParticleSystem.main;
        mainModule.startSpeed = 0;
    }

    /// <summary>
    /// [신규] 파티클의 '트리거'만 설정하는 함수 (Awake에서 한 번만 호출)
    /// </summary>
    private void SetupParticleTriggers()
    {
        var triggerModule = windParticleSystem.trigger;
        triggerModule.enabled = true;
        triggerModule.AddCollider(_zoneCollider);
        triggerModule.exit = ParticleSystemOverlapAction.Kill;
    }

    // --- (플레이어 속도 제어 로직 - 그대로 유지) ---
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.AddForce(windDirection.normalized * windForce, ForceMode.Force);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSpeedControl(false); // 속도 제한 끄기
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.SetSpeedControl(true); // 속도 제한 다시 켜기
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // ... (이전과 동일) ...
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        Vector3 direction = windDirection.normalized * 3;
        Gizmos.DrawRay(center, direction);
        Gizmos.DrawWireSphere(center + direction, 0.5f);
    }
}