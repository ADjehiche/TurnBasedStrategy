using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PotionEffectType { None, Speed, Stamina }

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

    [Header("Potion Properties")]
    public PotionEffectType potionEffectType = PotionEffectType.None;
    public float effectDuration; // For speed
    public float speedMultiplier; // For speed
    public int staminaIncrease;   // For stamina
}
