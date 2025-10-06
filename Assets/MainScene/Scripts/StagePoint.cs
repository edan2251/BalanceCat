using UnityEngine;

[ExecuteInEditMode] // 에디터에서 바로 결과를 볼 수 있게 합니다.
public class StagePoint : MonoBehaviour
{
    [Tooltip("스테이지 순서 (1부터 시작)")]
    public int stageID;

    [Tooltip("정렬할 행성 오브젝트 (StageSelector가 붙은 부모)")]
    public Transform planetObject;

    // 행성 반지름 (Start에서 자동으로 계산됩니다)
    private float planetRadius;

    void Start()
    {
        // 부모 행성을 찾거나, 인스펙터에 연결된 행성 오브젝트를 사용합니다.
        if (planetObject == null && transform.parent != null)
        {
            planetObject = transform.parent;
        }

        if (planetObject != null)
        {
            // 행성 스케일의 절반을 반지름으로 가정
            planetRadius = planetObject.localScale.x * 0.5f;
            AlignToSurface();
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        // 에디터 모드에서 위치를 변경할 때마다 자동으로 표면에 붙입니다.
        if (!Application.isPlaying && planetObject != null)
        {
            AlignToSurface();
        }
    }
#endif

    private void AlignToSurface()
    {
        if (planetObject == null || planetRadius == 0) return;

        // 1. 현재 로컬 회전값(오일러 각)을 저장합니다.
        Vector3 currentLocalEuler = transform.localEulerAngles;

        // 2. 현재 회전값에 해당하는 표면 위치를 계산합니다.
        // Quaternion.Euler로 회전된 로컬 Z축(Vector3.forward)을 기준으로 반지름만큼 떨어진 위치를 찾습니다.
        Vector3 position = Quaternion.Euler(currentLocalEuler) * Vector3.forward * planetRadius;

        // 3. 계산된 위치로 Local Position을 설정합니다.
        // 이렇게 하면 위치는 표면에 고정되고, 로컬 회전값(Rotation)만 조절하면
        // 자유롭게 스테이지의 위도와 경도를 바꿀 수 있습니다.
        transform.localPosition = position;
    }
}