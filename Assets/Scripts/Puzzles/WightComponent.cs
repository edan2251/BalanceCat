using UnityEngine;

// 오브젝트의 무게를 정의하는 컴포넌트
// 추후 플레이어 무게 연결 하면 됨.
public class WeightComponent : MonoBehaviour
{
    [Header("오브젝트 무게")]
    [Tooltip("이 오브젝트가 WeightSensor에 가하는 무게 값")]
    public float objectWeight = 1.0f;
}