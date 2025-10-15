using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public Inventory inventory;
    public RectTransform leftRoot;
    public RectTransform rightRoot;


    [Header("Visual")]
    public Vector2 cellSize = new Vector2(64, 64);
    public Vector2 cellSpacing = new Vector2(2, 2);
    public GameObject slotPrefab; // 슬롯 프리팹
    public GameObject itemPrefab; // 아이템 아이콘 프리팹


    List<GameObject> slotPool = new List<GameObject>();
    List<GameObject> itemPool = new List<GameObject>();

    void OnEnable()
    {
        StartCoroutine(InitOnce());
    }

    IEnumerator InitOnce()
    {
        inventory.EnsureReady();

        yield return null;

        BuildSlots();
        Refresh();
        inventory.OnChanged += Refresh;
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    void BuildSlots()
    {
        if (inventory == null) return;
        slotPool.Clear();


        BuildGridSlots(leftRoot, inventory.leftGrid.width, inventory.leftGrid.height);
        BuildGridSlots(rightRoot, inventory.rightGrid.width, inventory.rightGrid.height, alignRight: true);
    }
    void BuildGridSlots(RectTransform root, int w, int h, bool alignRight = false)
    {
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var go = Instantiate(slotPrefab, root);
                var rt = go.transform as RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);


                Vector2 pos = CellToPos(x, y, alignRight, w);
                rt.anchoredPosition = pos;
                rt.sizeDelta = cellSize;
                slotPool.Add(go);
            }
    }


    Vector2 CellToPos(int x, int y, bool alignRight, int w)
    {
        // 좌상단(Left) 기준. Right는 우상단 기준으로 반전 배치
        if (alignRight) x = (w - 1 - x);
        float px = x * (cellSize.x + cellSpacing.x);
        float py = y * (cellSize.y + cellSpacing.y);
        return new Vector2(px, -py);
    }

    public void Refresh()
    {
        if (inventory == null) return;
        // 아이템 아이콘 초기화
        foreach (var go in itemPool) Destroy(go);
        itemPool.Clear();


        foreach (var p in inventory.leftGrid.placements)
            SpawnItemIcon(p, BagSide.Left);
        foreach (var p in inventory.rightGrid.placements)
            SpawnItemIcon(p, BagSide.Right);
    }

    void SpawnItemIcon(ItemPlacement p, BagSide side)
    {
        RectTransform root = (side == BagSide.Left) ? leftRoot : rightRoot;
        bool alignRight = (side == BagSide.Right);


        var go = Instantiate(itemPrefab, root);
        var rt = go.transform as RectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);


        Vector2 pos = CellToPos(p.x, p.y, alignRight, (side == BagSide.Left ? inventory.leftGrid.width : inventory.rightGrid.width));
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(p.w * cellSize.x + (p.w - 1) * cellSpacing.x,
        p.h * cellSize.y + (p.h - 1) * cellSpacing.y);


        var img = go.GetComponentInChildren<Image>();
        if (img != null && p.item.data.icon != null) img.sprite = p.item.data.icon;

        itemPool.Add(go);
    }
}