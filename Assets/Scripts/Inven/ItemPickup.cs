using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : InteractableObject
{
    [Header("Pickup Item")] public ItemData itemData;

    public override void Interact()
    {
        var inv = FindObjectOfType<Inventory>();
        var inst = new ItemInstance(itemData);
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