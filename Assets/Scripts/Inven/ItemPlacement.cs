using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPlacement
{
    public ItemInstance item;
    public int x, y, w, h; // origin + size


    public ItemPlacement(ItemInstance item, int x, int y, int w, int h)
    {
        this.item = item; this.x = x; this.y = y; this.w = w; this.h = h;
    }
}

