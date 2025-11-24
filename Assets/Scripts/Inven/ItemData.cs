using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;

    public Sprite icon;

    [TextArea]
    public string description;

    [Min(1)] public int sizeW = 1;
    [Min(1)] public int sizeH = 1;

    public float weight = 0.5f;
}
