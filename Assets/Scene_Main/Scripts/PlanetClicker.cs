using UnityEngine;

public class PlanetClicker : MonoBehaviour
{
    [Tooltip("�� �༺�� é�� �ε��� (0���� ����)")]
    public int chapterIndex;

    private ChapterSelector chapterSelector;

    void Start()
    {
        // ChapterSelector ������Ʈ ã��
        chapterSelector = FindObjectOfType<ChapterSelector>();

        // �༺�� Collider�� �ִ��� Ȯ�� (Ŭ�� �̺�Ʈ�� �ޱ� ���� �ʼ�)
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError(gameObject.name + "�� Collider�� �����ϴ�. Ŭ���� �۵����� �ʽ��ϴ�.");
        }
    }

    // ���콺 ��ư�� �� ������Ʈ ������ ������ �� ȣ��˴ϴ�.
    private void OnMouseDown()
    {
        if (chapterSelector != null)
        {
            int total = chapterSelector.GetTotalChapters();
            // ChapterSelector���� Ŭ���� é�� �ε����� �����մϴ�.
            chapterSelector.HandlePlanetClick(chapterIndex);
        }
    }
}