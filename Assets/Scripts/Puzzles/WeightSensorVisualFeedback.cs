using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeightSensorVisualFeedback : MonoBehaviour
{
    [Header("시각적 피드백 설정")]
    [Tooltip("센서가 눌릴 때 아래로 내려가는 거리 (Y축)")]
    public float pressDistance = 0.2f;

    [Tooltip("눌림/복귀 애니메이션 속도")]
    public float moveSpeed = 5.0f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        // 컴포넌트가 붙은 오브젝트의 초기 위치를 저장합니다.
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    private void Update()
    {
        // 목표 위치로 부드럽게 이동합니다.
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    // (무게 충족 시: 발판이 눌립니다)
    public void PressSensor()
    {
        // 원래 위치에서 pressDistance만큼 아래로 이동하는 위치를 목표로 설정
        targetPosition = originalPosition - new Vector3(0, pressDistance, 0);
    }

    // (무게 미충족 시: 발판이 복귀합니다)
    public void ReleaseSensor()
    {
        // 원래 위치로 복귀
        targetPosition = originalPosition;
    }
}
