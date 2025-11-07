using UnityEngine;

public class GlobalCurvedManager : MonoBehaviour
{
    [Range(-1f, 1f)] // 슬라이더로 조절하기 쉽게 Range 추가
    public float globalCurvedStrength;

    // C#에서 사용할 프로퍼티 이름 (셰이더 그래프의 Reference와 일치해야 함)
    private readonly int _globalCurveID = Shader.PropertyToID("_CurvedStrength");

    void Update()
    {
        // 매 프레임 모든 셰이더에 전역 변수 값을 설정합니다.
        Shader.SetGlobalFloat(_globalCurveID, globalCurvedStrength);
    }
}
