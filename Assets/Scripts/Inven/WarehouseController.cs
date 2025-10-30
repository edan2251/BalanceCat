using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarehouseController : MonoBehaviour
{
    [Header("Refs")]
    public Inventory warehouseInventory;      // â��� Inventory ������Ʈ
    public GameObject clearTextGO;            // "CLEAR" �ؽ�Ʈ ������Ʈ

    [Header("Rule")]
    public int clearThreshold = 3;

    public static bool Cleared { get; private set; }

    void Awake()
    {
        if (!warehouseInventory) warehouseInventory = GetComponentInChildren<Inventory>();
        if (clearTextGO) clearTextGO.SetActive(false);
    }

    void OnEnable()
    {
        if (warehouseInventory != null)
            warehouseInventory.OnChanged += CheckClear;
        CheckClear();
    }
    void OnDisable()
    {
        if (warehouseInventory != null)
            warehouseInventory.OnChanged -= CheckClear;
    }

    void CheckClear()
    {
        if (warehouseInventory == null) return;

        int count = warehouseInventory.leftGrid.placements.Count + warehouseInventory.rightGrid.placements.Count;
        bool ok = count >= clearThreshold;
        Cleared = ok;
        if (clearTextGO) clearTextGO.SetActive(ok);
    }
}
