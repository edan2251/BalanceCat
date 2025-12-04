using System.Collections;
using UnityEngine;

public class WindCycler : MonoBehaviour
{
    [Header("시간 설정")]
    public float onDuration = 3.0f;
    public float offDuration = 2.0f;
    public float startDelay = 0.0f;

    [Header("참조")]
    public Collider windCollider;
    public ParticleSystem windParticles;
    public SimpleWindZone windScript;

    private void Start()
    {
        if (windCollider == null) windCollider = GetComponent<Collider>();
        if (windParticles == null) windParticles = GetComponent<ParticleSystem>();
        if (windScript == null) windScript = GetComponent<SimpleWindZone>();

        StartCoroutine(CycleRoutine());
    }

    private IEnumerator CycleRoutine()
    {
        if (startDelay > 0)
        {
            SetWindState(false);
            yield return new WaitForSeconds(startDelay);
        }

        while (true)
        {
            // 1. 바람 켜기 (ON)
            SetWindState(true);
            yield return new WaitForSeconds(onDuration);

            // 2. 바람 끄기 (OFF)
            SetWindState(false);
            yield return new WaitForSeconds(offDuration);
        }
    }

    private void SetWindState(bool isOn)
    {
        // 1. 물리(Collider) 제어
        if (windCollider != null)
        {
            windCollider.enabled = isOn;
        }

        // 2. 스크립트 제어
        if (windScript != null)
        {
            windScript.enabled = isOn;
        }

        // 3. 파티클 제어 (핵심 수정)
        if (windParticles != null)
        {
            if (isOn)
            {
                // [수정] 조건문(if !isPlaying) 삭제! 
                // 꺼져있든 켜져있든 무조건 다시 재생(Play)하라고 강제 명령
                windParticles.Play();
            }
            else
            {
                // [수정] 끄는 즉시 화면에서 지움
                windParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}