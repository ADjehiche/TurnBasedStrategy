using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System / Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    public Sprite icon;
    public int maxStack;
    public string itemName;
    [TextArea(2, 6)]
    public string itemDescription;
    public int itemID;
    public bool isStackable;
}
