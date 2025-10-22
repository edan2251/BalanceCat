using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    public static bool IsLocked { get; private set; } = true;

    [SerializeField]
    private Texture2D ReleasedState;

    [SerializeField]
    private Texture2D PressedState;

    private Vector2 _hotspot = new Vector2(3, 32);

    [SerializeField]
    private CursorMode _cursorMode = CursorMode.Auto;

    void Start()
    {
        LockCursor();
    }

    // Event Function
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        if (Cursor.visible)
        {
            Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);

            if (Input.GetMouseButton(0))
            {
                Cursor.SetCursor(PressedState, _hotspot, _cursorMode);
            }
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IsLocked = true; 
    }

    public void UnlockCursor() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsLocked = false; 
        Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);
    }
}