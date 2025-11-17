using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemInstance
{
    public ItemData data;
    public bool rotated90 = false;
    public string guid;


    public ItemInstance(ItemData data)
    {
        this.data = data;
        this.guid = System.Guid.NewGuid().ToString("N");
    }


    public int Width => rotated90 ? data.sizeH : data.sizeW;
    public int Height => rotated90 ? data.sizeW : data.sizeH;
    public float TotalWeight => (data != null ? data.weight : 0f);
}