using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFollowAI : MonoBehaviour
{
    [Header("타겟 설정")]
    public Transform player;
    public float heightOffset = 2f;
    public float rightDistance = 1f;
    public float backwardDistance = 2f;

    [Header("움직임 설정")]
    public float smoothness = 3f;
    public float targetSmoothTime = 0.3f;

    [Header("둥둥 뜨기 (Idle Bobbing)")]
    public float bobSpeed = 2f;
    public float bobAmplitude = 0.2f;    
    public float idleThreshold = 0.1f;

    [Tooltip("둥둥 뜨기 효과가 시작/중지되는 데 걸리는 시간")]
    public float bobTransitionTime = 0.5f; 

    private Vector3 targetPos;
    private Vector3 targetPosVelocity = Vector3.zero;
    private Vector3 basePosition;

    private float currentBobAmplitude = 0f;
    private float bobAmplitudeVelocity = 0f;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("PetFollowAI: Player Transform이 할당되지 않았습니다!");
            return;
        }

        targetPos = player.position
                    + (player.right * rightDistance)
                    + (-player.forward * backwardDistance)
                    + (Vector3.up * heightOffset);

        transform.position = targetPos;
        basePosition = targetPos;
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 idealTarget = player.position
                              + (player.right * rightDistance)
                              + (-player.forward * backwardDistance)
                              + (Vector3.up * heightOffset);

        targetPos = Vector3.SmoothDamp(
            targetPos,
            idealTarget,
            ref targetPosVelocity,
            targetSmoothTime
        );

        basePosition = Vector3.Lerp(basePosition, targetPos, Time.deltaTime * smoothness);

        bool isTargetIdle = targetPosVelocity.magnitude < idleThreshold;
        float distanceToTarget = Vector3.Distance(basePosition, targetPos);
        bool hasArrived = distanceToTarget < idleThreshold;

        bool isIdle = isTargetIdle && hasArrived;

        float targetBobAmplitude = isIdle ? bobAmplitude : 0f;

        currentBobAmplitude = Mathf.SmoothDamp(
            currentBobAmplitude,     
            targetBobAmplitude,     
            ref bobAmplitudeVelocity, 
            bobTransitionTime      
        );

        Vector3 bobbingOffset = Vector3.zero;
        if (currentBobAmplitude > 0.001f) 
        {
            float bobValue = Mathf.Sin(Time.time * bobSpeed) * currentBobAmplitude;
            bobbingOffset = Vector3.up * bobValue;
        }

        transform.position = basePosition + bobbingOffset;

        transform.LookAt(player.position + Vector3.up * 1.5f);
    }
}