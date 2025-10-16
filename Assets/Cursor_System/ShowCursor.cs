using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    [SerializeField]
    private Texture2D ReleasedState;

    [SerializeField]
    private Texture2D PressedState;

    // Hotspot 좌표를 (3, 32)로 설정했습니다.
    private Vector2 _hotspot = new Vector2(3, 32);

    [SerializeField]
    private CursorMode _cursorMode = CursorMode.Auto;

    // Event Function
    void Start()
    {
        // Start 시점에 설정된 _hotspot 값을 사용합니다.
        Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);
    }

    // Event Function
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 클릭 시에도 동일한 _hotspot 값을 사용합니다.
            Cursor.SetCursor(PressedState, _hotspot, _cursorMode);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);
        }
    }
}