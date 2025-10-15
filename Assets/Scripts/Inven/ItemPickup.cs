using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : InteractableObject
{
    [Header("Pickup Item")] public ItemData itemData;
    [Min(1)] public int quantity = 1;


    public override void Interact()
    {
        var inv = FindObjectOfType<Inventory>();
        if (inv == null || itemData == null) { Debug.LogWarning("Inventory/ItemData missing"); return; }


        var inst = new ItemInstance(itemData, quantity);
        bool ok = inv.TryAddAuto(inst, allowRotate: true);
        if (ok)
        {
            Destroy(gameObject);
        }
    }


    private void Reset()
    {
        interactionType = InteractionType.Item;
        interactionText = "[E] È¹µæ";
    }
}