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

    [Header("External Drop")]
    public InventoryUI externalUI;

    [Header("Visual")]
    public Vector2 cellSize = new Vector2(64, 64);
    public Vector2 cellSpacing = new Vector2(2, 2);
    public GameObject slotPrefab;
    public GameObject itemPrefab;

    public Canvas dragCanvas;
    RectTransform dragGhost;
    Image dragGhostImg;
    ItemPlacement dragging;
    BagSide draggingFromSide;
    Camera UICam => dragCanvas ? dragCanvas.worldCamera : null;

    [Header("Preview")]
    public GameObject previewCellPrefab;
    public Color previewOK = new Color(0f, 1f, 0f, 0.25f);
    public Color previewBad = new Color(1f, 0f, 0f, 0.35f);
    List<Image> previewLeft = new List<Image>();
    List<Image> previewRight = new List<Image>();

    List<GameObject> slotPool = new List<GameObject>();
    List<GameObject> itemPool = new List<GameObject>();

    bool _slotsBuilt = false;
    bool _previewsBuilt = false;
    bool _dropGuard = false;

    [Header("Single Grid Mode (for Storage)")]
    public bool singleSide = false;

   
   
    [Header("Dynamic Bag Layout")]
    public bool enableDynamicLayout = false;
    public Vector2 leftPos_Default;
    public Vector2 rightPos_Default;
    public Vector2 leftPos_WithStorage;
    public Vector2 rightPos_WithStorage;

    int Idx(int x, int y, int w) => y * w + x;

    void OnEnable() { StartCoroutine(InitOnce()); }

    IEnumerator InitOnce()
    {
        if (inventory != null) inventory.EnsureReady();
        yield return null;

        if (!_slotsBuilt) { BuildSlots(); _slotsBuilt = true; }
        else { ClearPreview(); EnsurePreviewOnTop(); }

        Refresh();
        if (inventory != null) inventory.OnChanged += Refresh;
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
        ClearPreview();
        if (externalUI) externalUI.ClearPreviewEx();
    }

    void BuildSlots()
    {
        if (inventory == null) return;

        if (slotPool.Count == 0)
        {
            BuildGridSlots(leftRoot, inventory.leftGrid.width, inventory.leftGrid.height);
            if (!singleSide && rightRoot)
                BuildGridSlots(rightRoot, inventory.rightGrid.width, inventory.rightGrid.height, alignRight: true);
        }

        if (!_previewsBuilt)
        {
            BuildPreviewGrid(leftRoot, inventory.leftGrid.width, inventory.leftGrid.height, false, previewLeft);
            if (!singleSide && rightRoot)
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
        foreach (var i in previewLeft) { if (i) i.enabled = false; }
        foreach (var i in previewRight) { if (i) i.enabled = false; }
    }

    public void ClearPreviewEx() => ClearPreview();
    public void EnsurePreviewOnTopEx() => EnsurePreviewOnTop();

    void EnsurePreviewOnTop()
    {
        foreach (var i in previewLeft) if (i) i.transform.SetAsLastSibling();
        foreach (var i in previewRight) if (i) i.transform.SetAsLastSibling();
    }

    void BuildGridSlots(RectTransform root, int w, int h, bool alignRight = false)
    {
        if (!root) return;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var go = Instantiate(slotPrefab, root);
                var rt = go.transform as RectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = CellToPos(x, y, alignRight, w);
                rt.sizeDelta = cellSize;
                slotPool.Add(go);
            }
    }

    void BuildPreviewGrid(RectTransform root, int w, int h, bool alignRight, List<Image> store)
    {
        if (!root) return;

        if (store != null && store.Count >= w * h)
        {
            ClearPreview();
            EnsurePreviewOnTop();
            return;
        }

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                GameObject go = previewCellPrefab
                    ? Instantiate(previewCellPrefab, root)
                    : new GameObject("PreviewCell", typeof(RectTransform), typeof(Image));
                if (!previewCellPrefab)
                {
                    var im = go.GetComponent<Image>();
                    im.raycastTarget = false;
                }
                var rt = (RectTransform)go.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = CellToPos(x, y, alignRight, w);
                rt.sizeDelta = cellSize;

                var img = go.GetComponent<Image>();
                img.enabled = false;
                go.transform.SetAsLastSibling();
                store.Add(img);
            }
    }

    Vector2 CellToPos(int x, int y, bool alignRight, int w)
    {
        if (alignRight) x = (w - 1 - x);
        float px = x * (cellSize.x + cellSpacing.x);
        float py = y * (cellSize.y + cellSpacing.y);
        return new Vector2(px, -py);
    }

    public void Refresh()
    {
        if (inventory == null) return;
        foreach (var go in itemPool) if (go) Destroy(go);
        itemPool.Clear();

        foreach (var p in inventory.leftGrid.placements)
            SpawnItemIcon(p, BagSide.Left);

        if (!singleSide)
            foreach (var p in inventory.rightGrid.placements)
                SpawnItemIcon(p, BagSide.Right);

        EnsurePreviewOnTop();
    }

    void SpawnItemIcon(ItemPlacement p, BagSide side)
    {
        RectTransform root = (side == BagSide.Left) ? leftRoot : rightRoot;
        if (!root) return;

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
        if (_dropGuard) return;
        _dropGuard = true;

        if (dragGhost) dragGhost.gameObject.SetActive(false);

        ClearPreview();
        if (externalUI) externalUI.ClearPreviewEx();

        var moving = dragging;
        var fromSide = draggingFromSide;
        dragging = null;

        if (!cancel && moving != null)
        {
            dragging = moving;
            draggingFromSide = fromSide;
            TryDropAt(pointerScreenPos);
            dragging = null;
        }

        _dropGuard = false;
    }

    bool TryDropAt(Vector2 screenPos)
    {
        if (inventory == null || dragging == null) return false;

        var cam = UICam;
        bool inLeft = leftRoot && RectTransformUtility.RectangleContainsScreenPoint(leftRoot, screenPos, cam);
        bool inRight = (!singleSide) && rightRoot && RectTransformUtility.RectangleContainsScreenPoint(rightRoot, screenPos, cam);

        if (inLeft || inRight)
        {
            BagSide targetSide = inLeft ? BagSide.Left : BagSide.Right;
            var root = inLeft ? leftRoot : rightRoot;
            var grid = inLeft ? inventory.leftGrid : inventory.rightGrid;
            bool alignRight = (targetSide == BagSide.Right);

            if (!ScreenToCell(root, grid.width, grid.height, alignRight, screenPos, dragging.item, out int gx, out int gy))
                return false;

            bool ok = inventory.TryMove(dragging, draggingFromSide, targetSide, gx, gy, dragging.item.rotated90);
            return ok;
        }

        if (externalUI && externalUI.inventory != null)
        {
            var cam2 = externalUI.dragCanvas ? externalUI.dragCanvas.worldCamera : null;
            bool inExtLeft = externalUI.leftRoot && RectTransformUtility.RectangleContainsScreenPoint(externalUI.leftRoot, screenPos, cam2);
            bool inExtRight = (!externalUI.singleSide) && externalUI.rightRoot && RectTransformUtility.RectangleContainsScreenPoint(externalUI.rightRoot, screenPos, cam2);

            if (inExtLeft || inExtRight)
            {
                BagSide targetSide = inExtLeft ? BagSide.Left : BagSide.Right;
                var root = inExtLeft ? externalUI.leftRoot : externalUI.rightRoot;
                var grid = inExtLeft ? externalUI.inventory.leftGrid : externalUI.inventory.rightGrid;
                bool alignRight = (targetSide == BagSide.Right);

                if (!externalUI.ScreenToCell(root, grid.width, grid.height, alignRight, screenPos, dragging.item, out int gx, out int gy))
                    return false;

                bool ok = inventory.TryMoveToOtherInventory(
                    dragging, draggingFromSide, externalUI.inventory,
                    targetSide, gx, gy, dragging.item.rotated90
                );
                return ok;
            }
        }

        return false;
    }

    public bool ScreenToCell(RectTransform root, int gridW, int gridH, bool alignRight,
                             Vector2 screenPos, ItemInstance item, out int cellX, out int cellY)
    {
        cellX = cellY = -1;
        if (!root) return false;
        var cam = UICam;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPos, cam, out var lp))
            return false;

        Vector2 TL = new Vector2(-root.rect.width * root.pivot.x, root.rect.height * (1 - root.pivot.y));
        float pixelX = lp.x - TL.x;
        float pixelY = TL.y - lp.y;

        float stepX = cellSize.x + cellSpacing.x;
        float stepY = cellSize.y + cellSpacing.y;

        int ixFromLeft = Mathf.FloorToInt(pixelX / stepX);
        int iy = Mathf.FloorToInt(pixelY / stepY);

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
        if (inventory == null || dragging == null)
        {
            ClearPreview();
            if (externalUI) externalUI.ClearPreviewEx();
            return;
        }

        var cam = UICam;
        bool inLeft = leftRoot && RectTransformUtility.RectangleContainsScreenPoint(leftRoot, screenPos, cam);
        bool inRight = (!singleSide) && rightRoot && RectTransformUtility.RectangleContainsScreenPoint(rightRoot, screenPos, cam);

        if (inLeft || inRight)
        {
            if (externalUI) externalUI.ClearPreviewEx();

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
                ? grid.CanPlaceIgnoring(gx, gy, w, h, dragging)
                : grid.CanPlace(gx, gy, w, h);

            var pool = inLeft ? previewLeft : previewRight;
            var color = can ? previewOK : previewBad;

            ClearPreview();
            for (int yy = gy; yy < gy + h; yy++)
                for (int xx = gx; xx < gx + w; xx++)
                {
                    int idx = Idx(xx, yy, grid.width);
                    if (idx < 0) continue;
                    var img = pool[idx];
                    img.color = color;
                    img.enabled = true;
                }
            return;
        }

        if (externalUI)
        {
            ClearPreview();
            externalUI.ShowPreviewForExternal(screenPos, dragging.item);
        }
    }

    public void ShowPreviewForExternal(Vector2 screenPos, ItemInstance item)
    {
        if (!leftRoot && !rightRoot) return;

        var cam = dragCanvas ? dragCanvas.worldCamera : null;

        bool inLeft = leftRoot && RectTransformUtility.RectangleContainsScreenPoint(leftRoot, screenPos, cam);
        bool inRight = (!singleSide) && rightRoot && RectTransformUtility.RectangleContainsScreenPoint(rightRoot, screenPos, cam);
        if (!inLeft && !inRight)
        {
            ClearPreview();
            return;
        }

        BagSide side = inLeft ? BagSide.Left : BagSide.Right;
        var root = inLeft ? leftRoot : rightRoot;
        var grid = inLeft ? inventory.leftGrid : inventory.rightGrid;
        bool alignRight = (side == BagSide.Right);

        if (!ScreenToCell(root, grid.width, grid.height, alignRight, screenPos, item, out int gx, out int gy))
            return;

        int w = item.rotated90 ? item.data.sizeH : item.data.sizeW;
        int h = item.rotated90 ? item.data.sizeW : item.data.sizeH;

        bool can = grid.CanPlace(gx, gy, w, h);

        var pool = inLeft ? previewLeft : previewRight;
        var color = can ? previewOK : previewBad;

        ClearPreview();
        for (int yy = gy; yy < gy + h; yy++)
            for (int xx = gx; xx < gx + w; xx++)
            {
                int idx = Idx(xx, yy, grid.width);
                if (idx < 0) continue;
                var img = pool[idx];
                img.color = color;
                img.enabled = true;
            }
        EnsurePreviewOnTop();
    }

    public void ApplyBagLayout(bool withStorage)
    {
        if (!enableDynamicLayout) return;
        if (leftRoot)
            leftRoot.anchoredPosition = withStorage ? leftPos_WithStorage : leftPos_Default;

        if (rightRoot && !singleSide)
            rightRoot.anchoredPosition = withStorage ? rightPos_WithStorage : rightPos_Default;

        EnsurePreviewOnTop();
    }
}
