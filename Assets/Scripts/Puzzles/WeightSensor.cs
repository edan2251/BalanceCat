using UnityEngine;
using UnityEngine.Events; 

public class WeightSensor : MonoBehaviour
{
    [Header("감지 설정")]
    [Tooltip("문이 열리거나 플랫폼이 작동하기 위해 필요한 최소 무게")]
    public float requiredWeight = 10.0f;

    // 현재 발판 위에 있는 총 무게
    [Header("현재 상태")]
    [Tooltip("현재 센서 위에 있는 총 무게 (Read Only)")]
    [SerializeField] private float currentWeight = 0.0f;

    [Tooltip("최소 무게를 충족했는지 여부 (Read Only)")]
    public bool isWeightMet = false;

    // 무게 충족 시/미충족 시 외부에 알리는 이벤트
    [Header("이벤트")]
    [Tooltip("요구 무게를 충족했을 때 발생")]
    public UnityEvent onWeightMet;
    [Tooltip("요구 무게 미만으로 떨어졌을 때 발생")]
    public UnityEvent onWeightUnmet;


    // --- 기능 구현 ---

    // 발판에 물체가 올라왔을 때
    private void OnTriggerEnter(Collider other)
    {
        //WeightComponent라는 스크립트가 붙은 오브젝트만 무게로 인정
        WeightComponent wc = other.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeight += wc.objectWeight;
            CheckWeightStatus();
        }
    }

    // 발판에서 물체가 내려갔을 때
    private void OnTriggerExit(Collider other)
    {
        WeightComponent wc = other.GetComponent<WeightComponent>();
        if (wc != null)
        {
            currentWeight -= wc.objectWeight;

            // 무게가 음수가 되는 것을 방지 (안전 장치)
            if (currentWeight < 0) currentWeight = 0;

            CheckWeightStatus();
        }
    }

    // 현재 무게를 확인하고 상태 변화를 체크하는 함수
    private void CheckWeightStatus()
    {
        bool newStatus = currentWeight >= requiredWeight;

        // 상태가 '미충족 -> 충족'으로 바뀌었을 때 (문 열림)
        if (newStatus && !isWeightMet)
        {
            isWeightMet = true;
            onWeightMet.Invoke(); // 이벤트 발생
            Debug.Log($"[WeightSensor] 무게 충족! 현재 무게: {currentWeight}");
        }
        // 상태가 '충족 -> 미충족'으로 바뀌었을 때 (문 닫힘)
        else if (!newStatus && isWeightMet)
        {
            isWeightMet = false;
            onWeightUnmet.Invoke(); // 이벤트 발생
            Debug.Log($"[WeightSensor] 무게 미충족. 현재 무게: {currentWeight}");
        }
    }
}