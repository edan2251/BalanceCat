using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlotBreaker : MonoBehaviour
{
    [Header("Refs")]
    public Inventory inventory;                 // 대상 인벤토리
    public InventoryDropController dropController; // 드랍 담당

    private void Awake()
    {
        if (inventory == null)
            inventory = GetComponent<Inventory>();
    }

    public void BreakRandomSlotAndDrop()
    {
        if (inventory == null)
        {
            Debug.LogWarning("InventorySlotBreaker: inventory가 설정되지 않음");
            return;
        }

        ItemInstance removedItem;
        BagSide removedSide;
        bool ok = inventory.TryRemoveRandomSlot(out removedItem, out removedSide);

        if (!ok) return; // 부술 슬롯 없음

        if (removedItem != null && dropController != null)
        {
            dropController.Drop(removedItem);
        }
    }
}
