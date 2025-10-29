using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIconUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    InventoryUI ui;
    public ItemPlacement placement;
    public BagSide side;
    Image img;          // ������ �̹���(��Ʈ ��������Ʈ��)
    CanvasGroup cg;     // �巡�� �� ���� ����ϰ� (����)

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
        if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0.35f;          // ���� ������
        cg.blocksRaycasts = false; // �巡�� �� ����ĳ��Ʈ ���

        ui.BeginDrag(placement, side, img ? img.sprite : null, eventData.position, placement.item.guid);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ui?.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cg) { cg.alpha = 1f; cg.blocksRaycasts = true; }
        ui?.EndDrag(eventData.position, cancel: false);
    }
}
