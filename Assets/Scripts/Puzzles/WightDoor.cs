using UnityEngine;

// WeightSensor의 이벤트에 반응하여 문을 여닫는 컴포넌트
public class WeightDoor : MonoBehaviour
{
    [Header("문 설정")]
    public Vector3 openPositionOffset = new Vector3(0, 5, 0); // 열릴 때 움직이는 거리 (Y축으로 5 유닛 위로)
    public float moveSpeed = 2.0f; // 문이 움직이는 속도

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        closedPosition = transform.position; // 시작 위치를 닫힌 위치로 저장
        targetPosition = closedPosition;
    }

    private void Update()
    {
        // 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    // WeightSensor의 onWeightMet 이벤트에 연결할 함수
    public void OpenDoor()
    {
        targetPosition = closedPosition + openPositionOffset;
        Debug.Log("[WeightDoor] 문이 열립니다.");
    }

    // WeightSensor의 onWeightUnmet 이벤트에 연결할 함수
    public void CloseDoor()
    {
        targetPosition = closedPosition;
        Debug.Log("[WeightDoor] 문이 닫힙니다.");
    }
}