using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public ItemData data;
    public int quantity = 1;
    public bool rotated90 = false; // true => swap W/H
    public string guid;


    public ItemInstance(ItemData data, int quantity = 1)
    {
        this.data = data;
        this.quantity = Mathf.Max(1, quantity);
        this.guid = System.Guid.NewGuid().ToString("N");
    }


    public int Width => rotated90 ? data.sizeH : data.sizeW;
    public int Height => rotated90 ? data.sizeW : data.sizeH;
    public float TotalWeight => (data != null ? data.weight : 0f) * quantity;
}