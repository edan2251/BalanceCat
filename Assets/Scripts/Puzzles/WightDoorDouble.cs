using UnityEngine;
using UnityEngine.Events;
using System.Collections; 

// WeightSensor의 이벤트에 반응하여 문을 회전하며 여닫는 컴포넌트
public class WeightDoorRotating : MonoBehaviour
{
    [Header("문 오브젝트 설정")]
    [Tooltip("왼쪽 문 오브젝트를 연결하세요. (피벗이 경첩 위치에 있어야 합니다)")]
    public Transform leftDoor;

    [Tooltip("오른쪽 문 오브젝트를 연결하세요. (피벗이 경첩 위치에 있어야 합니다)")]
    public Transform rightDoor;

    [Header("회전 설정")]
    [Tooltip("문이 열릴 때 회전할 각도 (예: 90.0)")]
    public float openAngle = 90.0f;

    [Tooltip("문이 회전하는 속도")]
    public float rotateSpeed = 3.0f;

    [Header("딜레이 설정")]
    [Tooltip("무게가 제거된 후 문이 닫히기까지의 딜레이 시간 (초)")]
    public float closeDelay = 3.0f; // 3초 딜레이 추가

    // 회전 목표 쿼터니언 (닫힘/열림 상태)
    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;
    private Quaternion leftTargetRotation;
    private Quaternion rightTargetRotation;

    private Coroutine closeCoroutine; // 닫기 코루틴 관리를 위한 변수

    private void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("[WeightDoorRotating] 좌/우 문 Transform을 모두 연결해야 합니다!");
            enabled = false;
            return;
        }

        // 초기 회전 상태를 닫힘 상태로 저장합니다.
        leftClosedRotation = leftDoor.localRotation;
        rightClosedRotation = rightDoor.localRotation;
        leftTargetRotation = leftClosedRotation;
        rightTargetRotation = rightClosedRotation;
    }

    private void Update()
    {
        // 왼쪽 문 부드러운 회전
        leftDoor.localRotation = Quaternion.Slerp(
            leftDoor.localRotation,
            leftTargetRotation,
            Time.deltaTime * rotateSpeed
        );

        // 오른쪽 문 부드러운 회전
        rightDoor.localRotation = Quaternion.Slerp(
            rightDoor.localRotation,
            rightTargetRotation,
            Time.deltaTime * rotateSpeed
        );
    }

    public void OpenDoor()
    {
        // 문이 열릴 때는 딜레이 없이 즉시 작동해야 합니다.
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine); // 닫기 코루틴이 진행 중이었다면 즉시 중단
            closeCoroutine = null;
        }

        // 닫힌 상태를 기준으로 회전 목표를 설정합니다.
        leftTargetRotation = leftClosedRotation * Quaternion.Euler(0, -openAngle, 0);
        rightTargetRotation = rightClosedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    public void CloseDoor()
    {
        // 이미 닫기 코루틴이 실행 중이 아니라면 새로 시작합니다.
        if (closeCoroutine == null)
        {
            closeCoroutine = StartCoroutine(CloseDoorWithDelay());
        }
    }

    // 닫기 딜레이를 처리하는 코루틴
    private IEnumerator CloseDoorWithDelay()
    {
        Debug.Log($"[WeightDoorRotating] 닫기 딜레이 시작: {closeDelay}초");
        yield return new WaitForSeconds(closeDelay);

        // 딜레이가 끝난 후 닫힘 목표 설정
        leftTargetRotation = leftClosedRotation;
        rightTargetRotation = rightClosedRotation;

        Debug.Log("[WeightDoorRotating] 문 닫기 시작.");

        closeCoroutine = null; // 코루틴이 완료되었음을 표시
    }
}