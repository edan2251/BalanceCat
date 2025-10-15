using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 6;

    public float carryCapacity = 999f; // ¹«°Ô


    public InventoryGrid leftGrid { get; private set; }
    public InventoryGrid rightGrid { get; private set; }


    public event Action OnChanged;


    void Awake()
    {
        leftGrid = new InventoryGrid(gridWidth, gridHeight);
        rightGrid = new InventoryGrid(gridWidth, gridHeight);
    }

    public void EnsureReady()
    {
        if (leftGrid == null) leftGrid = new InventoryGrid(gridWidth, gridHeight);
        if (rightGrid == null) rightGrid = new InventoryGrid(gridWidth, gridHeight);
    }


    public float TotalWeight
    {
        get
        {
            float sum = 0f;
            foreach (var p in leftGrid.placements) 
                sum += p.item.TotalWeight;
            foreach (var p in rightGrid.placements) 
                sum += p.item.TotalWeight;
            return sum;
        }
    }
    public bool TryAddAuto(ItemInstance item, bool allowRotate = true)
    {
        if (item == null || item.data == null) return false;
        int count = Mathf.Max(1, item.quantity);
                
        var order = new (BagSide side, Area area)[]
        {
            (BagSide.Left, Area.Top),
            (BagSide.Right, Area.Top),
            (BagSide.Left, Area.Bottom),
            (BagSide.Right, Area.Bottom),
        };


        int placedCount = 0;

        for (int i = 0; i < count; i++)
        {
            var unit = new ItemInstance(item.data, 1);

            bool placed = false;
            foreach (var (side, area) in order)
            {
                var g = (side == BagSide.Left) ? leftGrid : rightGrid;
                bool ltr = (side == BagSide.Left);
                if (g.TryPlaceInArea(unit, area, ltr, allowRotate))
                {
                    placed = true;
                    placedCount++;
                    break;
                }
            }

            if (!placed)
            {
                if (placedCount > 0) OnChanged?.Invoke();
                return false;
            }
        }

        OnChanged?.Invoke();
        return true;
    }
}