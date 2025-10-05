using UnityEngine;

public class PlanetClicker : MonoBehaviour
{
    [Tooltip("이 행성의 챕터 인덱스 (0부터 시작)")]
    public int chapterIndex;

    private ChapterSelector chapterSelector;

    void Start()
    {
        // ChapterSelector 컴포넌트 찾기
        chapterSelector = FindObjectOfType<ChapterSelector>();

        // 행성에 Collider가 있는지 확인 (클릭 이벤트를 받기 위해 필수)
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError(gameObject.name + "에 Collider가 없습니다. 클릭이 작동하지 않습니다.");
        }
    }

    // 마우스 버튼이 이 오브젝트 위에서 눌렸을 때 호출됩니다.
    private void OnMouseDown()
    {
        if (chapterSelector != null)
        {
            int total = chapterSelector.GetTotalChapters();
            // ChapterSelector에게 클릭된 챕터 인덱스를 전달합니다.
            chapterSelector.HandlePlanetClick(chapterIndex);
        }
    }
}