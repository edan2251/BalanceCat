using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BagSide { Left, Right }
public enum Area { Top, Bottom }


public class InventoryGrid
{
    public readonly int width;
    public readonly int height;


    // 각 셀에 배치된 아이템 참조(없으면 null)
    private ItemPlacement[,] owner;
    public readonly List<ItemPlacement> placements = new List<ItemPlacement>();


    public int HalfY => height / 2; // 상/하 절반 경계


    public InventoryGrid(int w, int h)
    {
        width = Mathf.Max(1, w);
        height = Mathf.Max(1, h);
        owner = new ItemPlacement[width, height];
    }


    public bool InBounds(int x, int y) => (x >= 0 && y >= 0 && x < width && y < height);


    public bool CanPlace(int x, int y, int w, int h)
    {
        if (x < 0 || y < 0 || (x + w) > width || (y + h) > height) return false;
        for (int yy = y; yy < y + h; yy++)
            for (int xx = x; xx < x + w; xx++)
                if (owner[xx, yy] != null) return false;
        return true;
    }
    public bool Place(ItemInstance item, int x, int y, bool rotated)
    {
        int w = rotated ? item.data.sizeH : item.data.sizeW;
        int h = rotated ? item.data.sizeW : item.data.sizeH;
        if (!CanPlace(x, y, w, h)) return false;


        item.rotated90 = rotated;
        var p = new ItemPlacement(item, x, y, w, h);
        placements.Add(p);
        for (int yy = y; yy < y + h; yy++)
            for (int xx = x; xx < x + w; xx++)
                owner[xx, yy] = p;
        return true;
    }


    public ItemPlacement GetPlacementAt(int x, int y)
    {
        if (!InBounds(x, y)) return null;
        return owner[x, y];
    }


    public void Remove(ItemPlacement p)
    {
        if (p == null) return;
        for (int yy = p.y; yy < p.y + p.h; yy++)
            for (int xx = p.x; xx < p.x + p.w; xx++)
                if (InBounds(xx, yy) && owner[xx, yy] == p) owner[xx, yy] = null;
        placements.Remove(p);
    }
    public bool TryPlaceInArea(ItemInstance item, Area area, bool scanLeftToRight, bool allowRotate)
    {
        int yStart = (area == Area.Top) ? 0 : HalfY;
        int yEnd = (area == Area.Top) ? (HalfY - 1) : (height - 1);
        if (HalfY == 0) { yStart = 0; yEnd = height - 1; }


        // x 열 순회자
        System.Func<IEnumerable<int>> XSeq = () =>
        {
            if (scanLeftToRight)
            {
                List<int> xs = new List<int>();
                for (int i = 0; i < width; i++) xs.Add(i);
                return xs;
            }
            else
            {
                List<int> xs = new List<int>();
                for (int i = width - 1; i >= 0; i--) xs.Add(i);
                return xs;
            }
        };


        // 회전 후보
        IEnumerable<bool> RotOptions()
        {
            if (!allowRotate) { yield return item.rotated90; yield break; }
            yield return false; // 0°
            if (item.data.sizeW != item.data.sizeH) yield return true; // 90° (정사각이면 중복)
        }


        foreach (var rot in RotOptions())
        {
            int w = rot ? item.data.sizeH : item.data.sizeW;
            int h = rot ? item.data.sizeW : item.data.sizeH;


            for (int y = yStart; y <= yEnd; y++)
            {
                foreach (int x in XSeq())
                {
                    if (CanPlace(x, y, w, h))
                        if (Place(item, x, y, rot)) return true;
                }
            }
        }
        return false;
    }
}