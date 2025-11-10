using UnityEngine;

public class MovableWater : MonoBehaviour
{
    [Header("Water Settings")]
    [Tooltip("물이 움직이는 속도")]
    public float moveSpeed = 1.0f;

    [Tooltip("리스폰 시 돌아올 기본 Y(높이) 값")]
    public float defaultWaterHeight;

    private float _targetY;
    private float _originalX;
    private float _originalZ;

    void Start()
    {
        _targetY = defaultWaterHeight;
        transform.position = new Vector3(transform.position.x, defaultWaterHeight, transform.position.z);

        // X, Z축은 고정
        _originalX = transform.position.x;
        _originalZ = transform.position.z;
    }

    void Update()
    {
        if (Mathf.Abs(transform.position.y - _targetY) > 0.01f)
        {
            float newY = Mathf.Lerp(
                transform.position.y,
                _targetY,
                Time.deltaTime * moveSpeed
            );
            transform.position = new Vector3(_originalX, newY, _originalZ);
        }
    }

    public void MoveToHeight(float newHeight)
    {
        _targetY = newHeight;
    }

    public void ResetToDefaultHeight()
    {
        _targetY = defaultWaterHeight;
        transform.position = new Vector3(_originalX, defaultWaterHeight, _originalZ);
        
    }
}