using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] bool startHidden = true;
    
    [SerializeField] bool showCursorWhenOpen = true;   // ����

    // �� �߰�: �κ��丮 ���� �� ��Ȱ��ȭ�� ������Ʈ��(ī�޶� ��Ʈ�ѷ� ����)
    [SerializeField] MonoBehaviour[] disableWhileOpen; // ��: ThirdPersonCam, CameraFollow2D ��

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
        // �� ���⼭ ī�޶� ������ ������ ��� OFF
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
