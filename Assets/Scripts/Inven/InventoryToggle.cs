using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryToggle : MonoBehaviour
{
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] bool startHidden = true;
    [SerializeField] bool showCursorWhenOpen = true;
    [SerializeField] MonoBehaviour[] disableWhileOpen;
    [SerializeField] bool blockDuringMiniGame = true;

    [Header("Storage Near Spaceship")]
    [SerializeField] GameObject storagePanel;
    [SerializeField] StorageZone storageZone;
    [SerializeField] bool openBothWhenNear = true;

    [Header("Bag Layout Targets")]
    [SerializeField] InventoryUI playerInventoryUI;
    [SerializeField] InventoryUI storageInventoryUI;
    [SerializeField]
    PlayerMovement playerMovement;
    void Awake()
    {
        if (!inventoryPanel) inventoryPanel = GameObject.Find("Inventory");
        if (startHidden && inventoryPanel) inventoryPanel.SetActive(false);
        if (storagePanel && startHidden) storagePanel.SetActive(false);

        if (!playerInventoryUI && inventoryPanel) playerInventoryUI = inventoryPanel.GetComponentInChildren<InventoryUI>(true);
        if (!storageInventoryUI && storagePanel) storageInventoryUI = storagePanel.GetComponentInChildren<InventoryUI>(true);
        if (!playerMovement) playerMovement = FindObjectOfType<PlayerMovement>();
        ApplyState(inventoryPanel && inventoryPanel.activeSelf || (storagePanel && storagePanel.activeSelf));
        
        playerInventoryUI?.ApplyBagLayout(false);
        storageInventoryUI?.ApplyBagLayout(false);
    }

    void Update()
    {
        if (blockDuringMiniGame && BalanceMiniGame.IsRunning) return;

        if (Input.GetKeyDown(toggleKey))
        {
            bool nearStorage = storageZone && storageZone.IsPlayerInside && storagePanel;

            if (nearStorage && openBothWhenNear)
            {
                bool next = !((inventoryPanel && inventoryPanel.activeSelf) && (storagePanel && storagePanel.activeSelf));
                if (inventoryPanel) inventoryPanel.SetActive(next);
                if (storagePanel) storagePanel.SetActive(next);
                ApplyState(next);

                playerInventoryUI?.ApplyBagLayout(next);
                storageInventoryUI?.ApplyBagLayout(next);
            }
            else if (inventoryPanel)
            {
                bool next = !inventoryPanel.activeSelf;
                inventoryPanel.SetActive(next);
                if (storagePanel && storagePanel.activeSelf) storagePanel.SetActive(false);
                ApplyState(next);

                playerInventoryUI?.ApplyBagLayout(false);
                storageInventoryUI?.ApplyBagLayout(false);
            }
        }
    }

    void ApplyState(bool open)
    {
        if (disableWhileOpen != null)
            foreach (var c in disableWhileOpen)
                if (c) c.enabled = !open;

        if (showCursorWhenOpen)
        {
            Cursor.visible = open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        }
        if (EventSystem.current) EventSystem.current.sendNavigationEvents = open;

        playerMovement?.SetControlEnabled(!open);
    }

    public void ForceClose()
    {
        bool wasOpen = (inventoryPanel && inventoryPanel.activeSelf) || (storagePanel && storagePanel.activeSelf);

        if (inventoryPanel && inventoryPanel.activeSelf) inventoryPanel.SetActive(false);
        if (storagePanel && storagePanel.activeSelf) storagePanel.SetActive(false);

        if (wasOpen) ApplyState(false);

        playerInventoryUI?.ApplyBagLayout(false);
        storageInventoryUI?.ApplyBagLayout(false);
    }

    public bool IsOpen => (inventoryPanel && inventoryPanel.activeSelf) || (storagePanel && storagePanel.activeSelf);
}
