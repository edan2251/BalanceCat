using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SafeRangeTracker : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("안전 반경(유닛)")]
    public float radius = 2f;

    [Tooltip("입력 적분 속도(초당 유닛) - 기본 이동만 반영")]
    public float trackSpeedPerSec = 3f;

    [Tooltip("Raw 입력 축 이름")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";

    [Header("State (ReadOnly)")]
    [SerializeField] bool inRange = true;
    [SerializeField] Vector2 localPos; // 중심(0,0) 기준 가상 위치(입력 적분)

    public bool InRange => inRange;
    public Vector2 LocalPos => localPos;

    [Header("Events")]
    public UnityEvent OnExited;   // 범위 밖으로 처음 나갈 때
    public UnityEvent OnReturned; // 범위 안으로 다시 들어올 때

    void Update()
    {
        // 기본 이동만: Raw 입력 축만 사용(편향/물리 무시)
        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);
        Vector2 input = new Vector2(x, y);
        if (input.sqrMagnitude > 1f) input.Normalize();

        localPos += input * (trackSpeedPerSec * Time.deltaTime);

        // 반경 체크
        bool nowIn = (localPos.sqrMagnitude <= radius * radius);
        if (inRange != nowIn)
        {
            inRange = nowIn;
            if (inRange) OnReturned?.Invoke();
            else OnExited?.Invoke();
        }
    }

    public void ResetToCenter() => localPos = Vector2.zero;
}
