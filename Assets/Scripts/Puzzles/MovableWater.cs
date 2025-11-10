using UnityEngine;

public class MovableWater : MonoBehaviour
{
    [Tooltip("물이 움직이는 속도")]
    public float moveSpeed = 1.0f;

    // 스크립트가 목표로 할 Y(높이) 값
    private float _targetY;
    // 물의 X, Z 좌표는 고정시키기 위함
    private float _originalX;
    private float _originalZ;

    void Start()
    {
        // 현재 위치를 첫 번째 목표로 설정
        _targetY = transform.position.y;
        _originalX = transform.position.x;
        _originalZ = transform.position.z;
    }

    void Update()
    {
        // 현재 높이와 목표 높이가 다르다면 부드럽게 이동(Lerp)
        if (Mathf.Abs(transform.position.y - _targetY) > 0.01f)
        {
            float newY = Mathf.Lerp(
                transform.position.y,
                _targetY,
                Time.deltaTime * moveSpeed
            );

            // Y축으로만 이동
            transform.position = new Vector3(_originalX, newY, _originalZ);
        }
    }

    // WaterLevelSwitch가 이 함수를 호출할 것입니다.
    public void MoveToHeight(float newHeight)
    {
        _targetY = newHeight;
    }
}