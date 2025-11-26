using GLTFast.Schema;
using UnityEngine;

public class ShowCursor : MonoBehaviour
{
    public static bool IsLocked { get; private set; } = true;

    // NEW: Public field to force the cursor to stay unlocked
    [Tooltip("If true, the cursor will always be visible and unlocked, and the Escape key toggle will be disabled.")]
    public bool ForceUnlocked = false; 

    [SerializeField]
    private Texture2D ReleasedState;

    [SerializeField]
    private Texture2D PressedState;

    private Vector2 _hotspot = new Vector2(3, 32);

    [SerializeField]
    private CursorMode _cursorMode = CursorMode.Auto;

    [SerializeField]
    private Animator menuAnimator; 


    void Start()
    {
        
        if (ForceUnlocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void Update()
    {
        if (ForceUnlocked)
        {
            if (Cursor.visible)
            {
                ApplyCustomCursorTexture();
            }
            return; 
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsLocked)
            {
                if (menuAnimator != null)
                {
                    menuAnimator.SetBool("isOpen", true);
                }

                UnlockCursor();
                
            }
            else
            {
                if (menuAnimator != null)
                {
                    menuAnimator.SetBool("isOpen", false);
                }

                LockCursor();
                
            }
        }

        if (Cursor.visible)
        {
            ApplyCustomCursorTexture();
        }
    }

    private void ApplyCustomCursorTexture()
    {
        Cursor.SetCursor(ReleasedState, _hotspot, _cursorMode);

        if (Input.GetMouseButton(0))
        {
            Cursor.SetCursor(PressedState, _hotspot, _cursorMode);
        }
    }

    public void LockCursor()
    {
        
        if (!ForceUnlocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            IsLocked = true;
        }
    }

    public void UnlockCursor()
    {
        

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        IsLocked = false;

        ApplyCustomCursorTexture();
    }
}