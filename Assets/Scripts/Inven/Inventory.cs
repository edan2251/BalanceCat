using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 6;

    public float carryCapacity = 999f; // 무게


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

    public bool TryMove(ItemPlacement p, BagSide fromSide, BagSide toSide, int x, int y, bool rotated)
    {
        if (p == null) return false;
        EnsureReady();

        var from = (fromSide == BagSide.Left) ? leftGrid : rightGrid;
        var to = (toSide == BagSide.Left) ? leftGrid : rightGrid;

        int w = rotated ? p.item.data.sizeH : p.item.data.sizeW;
        int h = rotated ? p.item.data.sizeW : p.item.data.sizeH;

        // === 같은 그리드 내 이동: 자기 자신과의 겹침 허용 ===
        if (from == to)
        {
            // 1) 선검사: 자기 자신 무시하고 배치 가능 여부 체크 (상태 변경 X)
            bool can = to.CanPlaceIgnoring(x, y, w, h, p);
            if (!can) return false;

            // 2) 실제 이동(이 시점에만 상태 변경)
            from.Remove(p);
            bool ok = to.Place(p.item, x, y, rotated);
            if (!ok)
            {
                // 이론상 거의 없음: 안전 복구
                to.Place(p.item, p.x, p.y, p.item.rotated90);
                return false;
            }
            OnChanged?.Invoke();
            return true;
        }

        // === 서로 다른 그리드 간 이동 ===
        if (!to.CanPlace(x, y, w, h)) return false;

        from.Remove(p);
        bool placed = to.Place(p.item, x, y, rotated);
        if (!placed)
        {
            from.Place(p.item, p.x, p.y, p.item.rotated90);
            return false;
        }

        OnChanged?.Invoke();
        return true;
    }

    public bool TryMoveToOtherInventory(ItemPlacement p, BagSide fromSide, Inventory other, BagSide toSide, int x, int y, bool rotated)
    {
        if (p == null || other == null) return false;
        EnsureReady();
        other.EnsureReady();

        var from = (fromSide == BagSide.Left) ? leftGrid : rightGrid;
        var to = (toSide == BagSide.Left) ? other.leftGrid : other.rightGrid;

        int w = rotated ? p.item.data.sizeH : p.item.data.sizeW;
        int h = rotated ? p.item.data.sizeW : p.item.data.sizeH;

        if (!to.CanPlace(x, y, w, h)) return false;

        // 원복용 좌표/회전 백업
        int ox = p.x, oy = p.y; bool orot = p.item.rotated90;

        from.Remove(p);
        bool placed = to.Place(p.item, x, y, rotated);
        if (!placed)
        {
            // 실패시 원상복구
            from.Place(p.item, ox, oy, orot);
            return false;
        }

        // 양쪽 UI 갱신
        OnChanged?.Invoke();
        other.OnChanged?.Invoke();
        return true;
    }
}