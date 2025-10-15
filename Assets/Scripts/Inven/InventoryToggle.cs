using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] bool startHidden = true;
    
    [SerializeField] bool showCursorWhenOpen = true;   // 기존

    // ▼ 추가: 인벤토리 열릴 때 비활성화할 컴포넌트들(카메라 컨트롤러 포함)
    [SerializeField] MonoBehaviour[] disableWhileOpen; // 예: ThirdPersonCam, CameraFollow2D 등

    void Awake()
    {
        if (!inventoryPanel) inventoryPanel = GameObject.Find("Inventory");
        if (startHidden && inventoryPanel) inventoryPanel.SetActive(false);
        ApplyState(inventoryPanel && inventoryPanel.activeSelf);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && inventoryPanel)
        {
            bool next = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(next);
            ApplyState(next);
        }
    }

    void ApplyState(bool open)
    {        
        // ▼ 여기서 카메라 움직임 포함해 묶어서 OFF
        if (disableWhileOpen != null)
            foreach (var c in disableWhileOpen)
                if (c) c.enabled = !open;

        if (showCursorWhenOpen)
        {
            Cursor.visible = open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        }
        if (EventSystem.current) EventSystem.current.sendNavigationEvents = open;
    }
}
