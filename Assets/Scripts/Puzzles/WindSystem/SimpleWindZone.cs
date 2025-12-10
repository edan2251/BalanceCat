using UnityEngine;
using System.Collections.Generic; // 리스트 사용을 위해 추가

[RequireComponent(typeof(Collider))]
public class SimpleWindZone : MonoBehaviour
{
    [Header("바람 설정")]
    public Vector3 windDirection = Vector3.right;
    public float windForce = 10.0f;

    [Header("이펙트 설정")]
    public ParticleSystem windParticleSystem;

    private BoxCollider _zoneCollider;
    private float _lastCheckedForce = float.MinValue;

    // [신규] 파티클 데이터를 처리하기 위한 리스트
    private List<ParticleSystem.Particle> _exitParticles = new List<ParticleSystem.Particle>();

    private void Awake()
    {
        _zoneCollider = GetComponent<BoxCollider>();
        if (_zoneCollider != null)
        {
            _zoneCollider.isTrigger = true;
        }

        if (windParticleSystem != null)
        {
            SetupParticleTriggers();
        }
    }

    private void Update()
    {
        if (windParticleSystem == null) return;

        if (Mathf.Abs(windForce - _lastCheckedForce) > 0.01f)
        {
            SetupParticleForce();
            _lastCheckedForce = windForce;

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

    // --- [신규] 파티클이 트리거(박스) 이벤트를 겪을 때 유니티가 자동으로 호출하는 함수 ---
    private void OnParticleTrigger()
    {
        if (windParticleSystem == null) return;

        // 1. 박스를 '나가는(Exit)' 파티클들을 리스트에 담아옵니다.
        int numExit = windParticleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, _exitParticles);

        // 2. 나가는 파티클들의 수명을 짧게 줄여서 '자연스럽게 죽게' 만듭니다.
        for (int i = 0; i < numExit; i++)
        {
            ParticleSystem.Particle p = _exitParticles[i];

            // 남은 수명을 0.3초~0.5초 정도로 설정
            // (이 시간 동안 Color over Lifetime의 투명해지는 구간이 재생됩니다)
            p.remainingLifetime = 1f;

            _exitParticles[i] = p;
        }

        // 3. 변경된 정보를 파티클 시스템에 다시 적용합니다.
        windParticleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, _exitParticles);
    }
    // --- [신규] 끝 ---

    private void SetupParticleForce()
    {
        var forceModule = windParticleSystem.forceOverLifetime;
        forceModule.enabled = true;
        forceModule.space = ParticleSystemSimulationSpace.World;

        forceModule.x = windDirection.normalized.x * windForce * 0.1f;
        forceModule.y = windDirection.normalized.y * windForce * 0.1f;
        forceModule.z = windDirection.normalized.z * windForce * 0.1f;

        var mainModule = windParticleSystem.main;
        mainModule.startSpeed = 0;
    }

    private void SetupParticleTriggers()
    {
        var triggerModule = windParticleSystem.trigger;
        triggerModule.enabled = true;
        triggerModule.AddCollider(_zoneCollider);

        // [핵심 수정] 즉시 삭제(Kill)가 아니라, 스크립트에게 알려달라(Callback)고 설정
        triggerModule.exit = ParticleSystemOverlapAction.Callback;
    }

    // --- (플레이어 속도 제어 로직 - 변경 없음) ---
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
                player.SetSpeedControl(false);
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
                player.SetSpeedControl(true);
            }
        }
    }
    // --- (여기까지) ---

    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        // 1. 바람 영역 그리기 (하늘색 박스)
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // 반투명 하늘색
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = Color.cyan; // 외곽선
        Gizmos.DrawWireCube(box.center, box.size);

        // 2. 바람 방향 화살표 그리기 (노란색)
        Gizmos.matrix = Matrix4x4.identity; // 월드 좌표계로 복귀

        // 박스의 실제 월드 중심점 계산
        Vector3 worldCenter = transform.TransformPoint(box.center);
        Vector3 dir = windDirection.normalized;
        Vector3 arrowEnd = worldCenter + dir * 3.0f; // 화살표 길이 3

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(worldCenter, arrowEnd); // 몸통
        Gizmos.DrawSphere(arrowEnd, 0.2f);      // 화살표 끝 (구)
    }
}