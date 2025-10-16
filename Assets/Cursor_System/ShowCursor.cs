using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    [SerializeField]
    private Texture2D ReleasedState;

    [SerializeField]
    private Texture2D PressedState;

    // Hotspot ��ǥ�� (3, 32)�� �����߽��ϴ�.
    private Vector2 _hotspot = new Vector2(3, 32);

    [SerializeField]
    private CursorMode _cursorMode = CursorMode.Auto;

    // Event Function
    void Start()
    {
        // Start ������ ������ _hotspot ���� ����մϴ�.
        Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);
    }

    // Event Function
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Ŭ�� �ÿ��� ������ _hotspot ���� ����մϴ�.
            Cursor.SetCursor(PressedState, _hotspot, _cursorMode);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);
        }
    }
}