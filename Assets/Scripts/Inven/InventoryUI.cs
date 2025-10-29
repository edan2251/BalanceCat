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

    public Canvas dragCanvas;              // 비워두면 부모 Canvas 자동 탐색
    RectTransform dragGhost;
    Image dragGhostImg;
    ItemPlacement dragging;
    BagSide draggingFromSide;

    Camera UICam => dragCanvas ? dragCanvas.worldCamera : null;

    [Header("Preview")]
    public GameObject previewCellPrefab;                 // 투명 이미지 프리팹(없으면 런타임 생성)
    public Color previewOK = new Color(0f, 1f, 0f, 0.25f);
    public Color previewBad = new Color(1f, 0f, 0f, 0.35f);
    List<Image> previewLeft = new List<Image>();
    List<Image> previewRight = new List<Image>();

    List<GameObject> slotPool = new List<GameObject>();
    List<GameObject> itemPool = new List<GameObject>();

    bool _slotsBuilt = false;     // 슬롯 한 번만 빌드
    bool _previewsBuilt = false;

    int Idx(int x, int y, int w) => y * w + x;

    void OnEnable()
    {
        StartCoroutine(InitOnce());
    }

    IEnumerator InitOnce()
    {
        inventory.EnsureReady();
        yield return null;

        if (!_slotsBuilt)
        {
            BuildSlots();
            _slotsBuilt = true;
        }
        else
        {
            ClearPreview();
            EnsurePreviewOnTop();
        }

        Refresh();
        inventory.OnChanged += Refresh;
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        ClearPreview();
    }

    void BuildSlots()
    {
        if (inventory == null) return;
                
        if (slotPool.Count == 0)
        {
            BuildGridSlots(leftRoot, inventory.leftGrid.width, inventory.leftGrid.height);
            BuildGridSlots(rightRoot, inventory.rightGrid.width, inventory.rightGrid.height, alignRight: true);
        }

        if (!_previewsBuilt)
        {
            BuildPreviewGrid(leftRoot, inventory.leftGrid.width, inventory.leftGrid.height, false, previewLeft);
            BuildPreviewGrid(rightRoot, inventory.rightGrid.width, inventory.rightGrid.height, true, previewRight);
            _previewsBuilt = true;
        }
        else
        {
            ClearPreview();
            EnsurePreviewOnTop();
        }
    }

    void ClearPreview()
    {
        foreach (var i in previewLeft) { i.enabled = false; }
        foreach (var i in previewRight) { i.enabled = false; }
    }

    void EnsurePreviewOnTop()
    {
        foreach (var i in previewLeft) i.transform.SetAsLastSibling();
        foreach (var i in previewRight) i.transform.SetAsLastSibling();
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

    void BuildPreviewGrid(RectTransform root, int w, int h, bool alignRight, List<Image> store)
    {
        if (store != null && store.Count >= w * h)
        {
            ClearPreview();
            EnsurePreviewOnTop();
            return;
        }

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                GameObject go;
                if (previewCellPrefab) go = Instantiate(previewCellPrefab, root);
                else
                {
                    go = new GameObject("PreviewCell", typeof(RectTransform), typeof(Image));
                    go.GetComponent<Image>().raycastTarget = false;
                }
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = CellToPos(x, y, alignRight, w); // 슬롯과 동일 좌표계
                rt.sizeDelta = cellSize;

                var img = go.GetComponent<Image>();
                img.enabled = false; // 기본 숨김
                go.transform.SetAsLastSibling(); // 아이템 위에 보이도록

                store.Add(img);
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

        EnsurePreviewOnTop();
    }

    void SpawnItemIcon(ItemPlacement p, BagSide side)
    {
        RectTransform root = (side == BagSide.Left) ? leftRoot : rightRoot;
        bool alignRight = (side == BagSide.Right);

        var go = Instantiate(itemPrefab, root);
        var rt = go.transform as RectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);

        int gridW = (side == BagSide.Left ? inventory.leftGrid.width : inventory.rightGrid.width);
        float stepX = cellSize.x + cellSpacing.x;
        float stepY = cellSize.y + cellSpacing.y;
                
        float px = alignRight ? (gridW - (p.x + p.w)) * stepX : p.x * stepX;
        float py = p.y * stepY;

        rt.anchoredPosition = new Vector2(px, -py);
        rt.sizeDelta = new Vector2(p.w * cellSize.x + (p.w - 1) * cellSpacing.x,
                                   p.h * cellSize.y + (p.h - 1) * cellSpacing.y);

        var img = go.GetComponentInChildren<Image>();
        if (img && p.item.data.icon) img.sprite = p.item.data.icon;

        var icon = go.GetComponent<ItemIconUI>() ?? go.AddComponent<ItemIconUI>();
        icon.Setup(this, p, side, img);

        itemPool.Add(go);
    }

    public void BeginDrag(ItemPlacement p, BagSide side, Sprite iconSprite, Vector2 pointerScreenPos, string guid)
    {
        dragging = p;
        draggingFromSide = side;

        if (!dragCanvas) dragCanvas = GetComponentInParent<Canvas>();
        if (!dragCanvas)
        {
            Debug.LogError("InventoryUI: Drag ghost not ready (Canvas missing?)");
            return;
        }

        if (!dragGhost)
        {
            var go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
            dragGhost = go.GetComponent<RectTransform>();
            dragGhostImg = go.GetComponent<Image>();
            dragGhostImg.raycastTarget = false;
            dragGhost.SetParent(dragCanvas.transform, false);
        }

        dragGhostImg.sprite = iconSprite;
        dragGhost.sizeDelta = new Vector2(p.w * cellSize.x + (p.w - 1) * cellSpacing.x,
                                          p.h * cellSize.y + (p.h - 1) * cellSpacing.y);
        dragGhost.gameObject.SetActive(true);
        UpdateDrag(pointerScreenPos);
        UpdatePreview(pointerScreenPos);
        EnsurePreviewOnTop();
    }

    public void UpdateDrag(Vector2 pointerScreenPos)
    {
        if (dragGhost) dragGhost.position = pointerScreenPos;
        UpdatePreview(pointerScreenPos);
    }

    public void EndDrag(Vector2 pointerScreenPos, bool cancel = false)
    {
        if (dragGhost) dragGhost.gameObject.SetActive(false);
        ClearPreview();
        if (!cancel && dragging != null) TryDropAt(pointerScreenPos);
        dragging = null;
    }

    bool TryDropAt(Vector2 screenPos)
    {
        if (inventory == null || dragging == null) return false;

        var cam = UICam;
        bool inLeft = RectTransformUtility.RectangleContainsScreenPoint(leftRoot, screenPos, cam);
        bool inRight = RectTransformUtility.RectangleContainsScreenPoint(rightRoot, screenPos, cam);
        if (!inLeft && !inRight) return false; // 중앙 빈영역 등은 무시

        BagSide targetSide = inLeft ? BagSide.Left : BagSide.Right;
        var root = inLeft ? leftRoot : rightRoot;
        var grid = inLeft ? inventory.leftGrid : inventory.rightGrid;
        bool alignRight = (targetSide == BagSide.Right);

        if (!ScreenToCell(root, grid.width, grid.height, alignRight, screenPos, dragging.item, out int gx, out int gy))
            return false;

        // 최종 이동(경계/겹침 자동 검사 → 중앙선 걸침 불가)
        bool ok = inventory.TryMove(dragging, draggingFromSide, targetSide, gx, gy, dragging.item.rotated90);
        return ok;
    }

    public bool ScreenToCell(RectTransform root, int gridW, int gridH, bool alignRight,
                             Vector2 screenPos, ItemInstance item, out int cellX, out int cellY)
    {
        cellX = cellY = -1;
        var cam = UICam;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPos, cam, out var lp))
            return false;

        // 루트 로컬공간에서 좌상단 기준 픽셀 오프셋 계산
        Vector2 TL = new Vector2(-root.rect.width * root.pivot.x, root.rect.height * (1 - root.pivot.y));
        float pixelX = lp.x - TL.x;             // 좌->우 양수
        float pixelY = TL.y - lp.y;             // 위->아래 양수

        float stepX = cellSize.x + cellSpacing.x;
        float stepY = cellSize.y + cellSpacing.y;

        int ixFromLeft = Mathf.FloorToInt(pixelX / stepX);
        int iy = Mathf.FloorToInt(pixelY / stepY);

        // 아이템 폭/높이 고려하여 경계 클램프(중앙선 걸침 방지)
        int wItem = item.rotated90 ? item.data.sizeH : item.data.sizeW;
        int hItem = item.rotated90 ? item.data.sizeW : item.data.sizeH;

        if (alignRight)
        {
            int gx = gridW - wItem - ixFromLeft;
            gx = Mathf.Clamp(gx, 0, Mathf.Max(0, gridW - wItem));
            iy = Mathf.Clamp(iy, 0, Mathf.Max(0, gridH - hItem));
            cellX = gx; cellY = iy; return true;
        }
        else
        {
            int gx = Mathf.Clamp(ixFromLeft, 0, Mathf.Max(0, gridW - wItem));
            iy = Mathf.Clamp(iy, 0, Mathf.Max(0, gridH - hItem));
            cellX = gx; cellY = iy; return true;
        }
    }

    void UpdatePreview(Vector2 screenPos)
    {
        if (inventory == null || dragging == null) { ClearPreview(); return; }

        var cam = UICam;
        bool inLeft = RectTransformUtility.RectangleContainsScreenPoint(leftRoot, screenPos, cam);
        bool inRight = RectTransformUtility.RectangleContainsScreenPoint(rightRoot, screenPos, cam);
        ClearPreview();
        if (!inLeft && !inRight) return;

        BagSide side = inLeft ? BagSide.Left : BagSide.Right;
        var root = inLeft ? leftRoot : rightRoot;
        var grid = inLeft ? inventory.leftGrid : inventory.rightGrid;
        bool alignRight = (side == BagSide.Right);

        if (!ScreenToCell(root, grid.width, grid.height, alignRight, screenPos, dragging.item, out int gx, out int gy))
            return;

        int w = dragging.item.rotated90 ? dragging.item.data.sizeH : dragging.item.data.sizeW;
        int h = dragging.item.rotated90 ? dragging.item.data.sizeW : dragging.item.data.sizeH;

        bool can =
            (side == draggingFromSide)
            ? grid.CanPlaceIgnoring(gx, gy, w, h, dragging)    // 같은 그리드 → 자기 자신 무시
            : grid.CanPlace(gx, gy, w, h);                     // 다른 그리드
        var pool = inLeft ? previewLeft : previewRight;
        var color = can ? previewOK : previewBad;

        for (int yy = gy; yy < gy + h; yy++)
            for (int xx = gx; xx < gx + w; xx++)
            {
                int idx = Idx(xx, yy, grid.width);
                var img = pool[idx];
                img.color = color;
                img.enabled = true;
            }
    }
}