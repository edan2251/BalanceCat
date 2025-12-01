using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public int gridWidth = 6;
    public int gridHeight = 6;

    public float carryCapacity = 50f; // 제한 무게


    public InventoryGrid leftGrid { get; private set; }
    public InventoryGrid rightGrid { get; private set; }

    float _totalWeight = 0f;
    int _totalScore = 0;

    public event Action OnChanged;

    List<BagSide> _slotSides = new List<BagSide>();
    List<int> _slotXs = new List<int>();
    List<int> _slotYs = new List<int>();


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

    public float TotalWeight => _totalWeight;
    public int TotalScore => _totalScore;

    void AddItemToTotals(ItemInstance item)
    {
        if (item == null) return;
        _totalWeight += item.TotalWeight;
        _totalScore += item.Score;
    }

    void RemoveItemFromTotals(ItemInstance item)
    {
        if (item == null) return;
        _totalWeight -= item.TotalWeight;
        _totalScore -= item.Score;

        if (_totalWeight < 0f) _totalWeight = 0f;
        if (_totalScore < 0) _totalScore = 0;
    }

    public bool TryAddAuto(ItemInstance item, bool allowRotate = true)
    {
        if (item == null || item.data == null) return false;

        if (carryCapacity > 0f)
        {
            float newTotal = TotalWeight + (item.data != null ? item.data.weight : 0f);
            if (newTotal > carryCapacity)
                return false;
        }

        var order = new (BagSide side, Area area)[]
        {
            (BagSide.Left, Area.Top),
            (BagSide.Right, Area.Top),
            (BagSide.Left, Area.Bottom),
            (BagSide.Right, Area.Bottom),
        };

        bool placed = false;

        foreach (var (side, area) in order)
        {
            var g = (side == BagSide.Left) ? leftGrid : rightGrid;
            bool ltr = (side == BagSide.Left);
            if (g.TryPlaceInArea(item, area, ltr, allowRotate))
            {
                placed = true;
                break;
            }
        }

        if (!placed)
            return false;

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

    public bool RemovePlacement(ItemPlacement p, BagSide side)
    {
        EnsureReady();
        InventoryGrid grid = (side == BagSide.Left) ? leftGrid : rightGrid;
        if (p == null || grid == null) return false;

        grid.Remove(p);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemoveRandomSlot(out ItemInstance removedItem, out BagSide removedFromSide)
    {
        EnsureReady();

        removedItem = null;
        removedFromSide = BagSide.Left;

        _slotSides.Clear();
        _slotXs.Clear();
        _slotYs.Clear();

        // 왼쪽 그리드 후보 슬롯 수집
        if (leftGrid != null)
        {
            for (int y = 0; y < leftGrid.height; y++)
            {
                for (int x = 0; x < leftGrid.width; x++)
                {
                    if (leftGrid.IsCellBlocked(x, y)) continue; // 이미 부서진 슬롯 제외
                    _slotSides.Add(BagSide.Left);
                    _slotXs.Add(x);
                    _slotYs.Add(y);
                }
            }
        }

        // 오른쪽 그리드 후보 슬롯 수집
        if (rightGrid != null)
        {
            for (int y = 0; y < rightGrid.height; y++)
            {
                for (int x = 0; x < rightGrid.width; x++)
                {
                    if (rightGrid.IsCellBlocked(x, y)) continue;
                    _slotSides.Add(BagSide.Right);
                    _slotXs.Add(x);
                    _slotYs.Add(y);
                }
            }
        }

        // 부술 수 있는 슬롯이 없으면 실패
        if (_slotSides.Count == 0)
            return false;

        int idx = UnityEngine.Random.Range(0, _slotSides.Count);
        BagSide side = _slotSides[idx];
        int cx = _slotXs[idx];
        int cy = _slotYs[idx];

        InventoryGrid grid = (side == BagSide.Left) ? leftGrid : rightGrid;

        // 슬롯에 아이템이 있으면 그 아이템 전체 제거
        ItemPlacement p = grid.GetPlacementAt(cx, cy);
        if (p != null && p.item != null)
        {
            removedItem = p.item;
            removedFromSide = side;
            grid.Remove(p);
        }
        else
        {
            removedItem = null;
            removedFromSide = side;
        }

        // 슬롯 파괴
        grid.BlockCell(cx, cy);

        OnChanged?.Invoke();
        return true;
    }
}
