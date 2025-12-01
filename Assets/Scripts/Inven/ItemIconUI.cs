using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    InventoryUI ui;
    public ItemPlacement placement;
    public BagSide side;
    Image img;          // 아이콘 이미지(고스트 스프라이트용)
    CanvasGroup cg;     // 드래그 중 원본 희미하게

    public void Setup(InventoryUI ui, ItemPlacement p, BagSide side, Image iconImage)
    {
        this.ui = ui;
        this.placement = p;
        this.side = side;
        this.img = iconImage ?? GetComponentInChildren<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ui == null || placement == null) return;

        ui.HideItemTooltip();

        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0.35f;          // 원본 반투명
        cg.blocksRaycasts = false; // 드래그 중 레이캐스트 통과

        ui.BeginDrag(placement, side, img ? img.sprite : null, eventData.position, placement.item.guid);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ui?.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cg) { cg.alpha = 1f; cg.blocksRaycasts = true; }

        if (ui == null || placement == null)
            return;

        Vector2 pos = eventData.position;

        bool overAnyInventory = false;

        var cam = ui.dragCanvas ? ui.dragCanvas.worldCamera : null;
        if (ui.leftRoot && RectTransformUtility.RectangleContainsScreenPoint(ui.leftRoot, pos, cam))
        {
            overAnyInventory = true;
        }
        else if (!ui.singleSide && ui.rightRoot &&
                 RectTransformUtility.RectangleContainsScreenPoint(ui.rightRoot, pos, cam))
        {
            overAnyInventory = true;
        }

        if (!overAnyInventory && ui.externalUI && ui.externalUI.inventory != null && ui.externalUI.isActiveAndEnabled)
        {
            var ext = ui.externalUI;
            var cam2 = ext.dragCanvas ? ext.dragCanvas.worldCamera : null;

            if (ext.leftRoot && RectTransformUtility.RectangleContainsScreenPoint(ext.leftRoot, pos, cam2))
            {
                overAnyInventory = true;
            }
            else if (!ext.singleSide && ext.rightRoot &&
                     RectTransformUtility.RectangleContainsScreenPoint(ext.rightRoot, pos, cam2))
            {
                overAnyInventory = true;
            }
        }

        ui.EndDrag(pos, cancel: false);

        if (!overAnyInventory && ui.dropController != null)
        {
            ItemInstance inst = placement.item;
            if (inst != null && ui.inventory != null)
            {
                ui.inventory.RemovePlacement(placement, side);
                ui.dropController.Drop(inst);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ui == null || placement == null) return;
        ui.ShowItemTooltip(placement.item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ui == null) return;
        ui.HideItemTooltip();
    }
}
