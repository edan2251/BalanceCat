using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterLevelSwitch : MonoBehaviour
{
    [Header("연결 설정")]
    [Tooltip("움직일 물 오브젝트 (MovableWater 스크립트가 있는)")]
    public MovableWater waterMover;

    [Tooltip("이 발판을 밟았을 때 설정될 물의 Y(높이) 값")]
    public float targetWaterHeight;

    [Header("시각 효과 설정")]
    [Tooltip("시각적으로 눌릴 발판의 '버튼' 부분 (자식 오브젝트)")]
    public Transform buttonVisual;

    [Tooltip("얼마나 깊이 눌릴지 (미터 단위)")]
    public float pressDownAmount = 0.1f;

    [Tooltip("버튼이 눌리고 올라오는 속도")]
    public float pressSpeed = 5.0f;

    private Vector3 _originalButtonPos;
    private Vector3 _targetButtonPos;
    private int _triggerCount = 0; // 여러 오브젝트가 올라가도 감지

    void Start()
    {
        if (buttonVisual != null)
        {
            // 버튼의 원래 위치 저장
            _originalButtonPos = buttonVisual.localPosition;
            _targetButtonPos = _originalButtonPos;
        }

        // 이 스크립트는 트리거 콜라이더가 필수
        GetComponent<Collider>().isTrigger = true;
    }

    void Update()
    {
        // 버튼의 시각적 위치를 목표 위치로 부드럽게 이동
        if (buttonVisual != null)
        {
            buttonVisual.localPosition = Vector3.Lerp(
                buttonVisual.localPosition,
                _targetButtonPos,
                Time.deltaTime * pressSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 누군가 밟으면 카운트 증가
            _triggerCount++;

            // 처음 밟힌 순간 (카운트가 1일 때) 버튼을 누름
            if (_triggerCount == 1)
            {
                PressButton();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 밟고 있던 플레이어가 나가면 카운트 감소
            _triggerCount--;

            // 아무도 밟고 있지 않으면 (카운트가 0일 때) 버튼을 올림
            if (_triggerCount == 0)
            {
                ReleaseButton();
            }
        }
    }

    private void PressButton()
    {
        // 1. 버튼 시각 효과: 목표 위치를 '아래'로 설정
        if (buttonVisual != null)
        {
            _targetButtonPos = _originalButtonPos + (Vector3.down * pressDownAmount);
        }

        // 2. 물 높이 조절: 연결된 물에게 명령
        if (waterMover != null)
        {
            waterMover.MoveToHeight(targetWaterHeight);
        }
        else
        {
            Debug.LogWarning("WaterMover가 연결되지 않았습니다!", this);
        }
    }

    private void ReleaseButton()
    {
        // 1. 버튼 시각 효과: 목표 위치를 '원래' 위치로 설정
        if (buttonVisual != null)
        {
            _targetButtonPos = _originalButtonPos;
        }
    }
}