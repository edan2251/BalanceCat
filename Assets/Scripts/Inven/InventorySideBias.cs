using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySideBias : MonoBehaviour
{
    public Inventory inventory;

    public float leftWeight;
    public float rightWeight;
    public float weightAmount;
    public float maxWeight;

    public float tilt;

    private void OnEnable()
    {
        if (!inventory) inventory = GetComponent<Inventory>();
        inventory.EnsureReady();
        inventory.OnChanged += RefreshWeight;
        RefreshWeight();
    }

    private void OnDisable()
    {
        if (inventory) inventory.OnChanged -= RefreshWeight;
    }

    public void RefreshWeight()
    {
        if (inventory.leftGrid == null || inventory.rightGrid == null)
        {
            inventory.EnsureReady();
            if (inventory.leftGrid == null || inventory.rightGrid == null)
                return;
        }

        leftWeight = 0f;
        rightWeight = 0f;

        var leftPlacements = inventory.leftGrid.placements;
        if (leftPlacements != null)
        {
            foreach (var p in leftPlacements)
            {
                if (p != null && p.item != null)
                    leftWeight += p.item.TotalWeight;
            }
        }

        var rightPlacements = inventory.rightGrid.placements;
        if (rightPlacements != null)
        {
            foreach (var p in rightPlacements)
            {
                if (p != null && p.item != null)
                    rightWeight += p.item.TotalWeight;
            }
        }

        weightAmount = leftWeight + rightWeight;

        if (weightAmount <= 0f)
        {
            tilt = 0f;
            return;
        }

        if (maxWeight <= 0f)
        {
            maxWeight = (inventory.carryCapacity > 0f) ? inventory.carryCapacity : weightAmount;
        }

        float diff = rightWeight - leftWeight;
        float ratioDiff = diff / weightAmount;

        float heavyFactor = Mathf.Clamp01(weightAmount / maxWeight);

        float rawTilt = ratioDiff * heavyFactor;

        tilt = Mathf.Clamp(rawTilt, -1f, 1f);
    }
}