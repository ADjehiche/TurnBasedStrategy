using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System / Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    [Header("UI Display")]
    public Sprite icon;
    public string itemName;
    [TextArea(2, 6)]
    public string itemDescription;
    
    [Header("Item Properties")]
    public int itemID;
    public int maxStack;
    public bool isStackable;
    
    [Header("3D Representation")]
    [Tooltip("The 3D prefab to display when this item is equipped in the player's hand")]
    public GameObject itemPrefab;
}
