using UnityEngine;

/// <summary>
/// Add this to any chest prefab to auto-populate it with items at runtime.
/// Works with chests that use ChestInventory component.
/// </summary>
public class ChestLootInitializer : MonoBehaviour
{
    [Header("Items to Add")]
    [SerializeField] private InventoryItemData[] itemsToSpawn;
    [SerializeField] private int[] quantities; // Optional: amount of each item (defaults to 1)
    
    void Start()
    {
        ChestInventory chest = GetComponent<ChestInventory>();
        if (chest == null)
        {
            Debug.LogWarning($"[ChestLootInitializer] No ChestInventory found on {gameObject.name}");
            return;
        }
        
        if (itemsToSpawn == null || itemsToSpawn.Length == 0)
        {
            return; // No items configured
        }
        
        for (int i = 0; i < itemsToSpawn.Length; i++)
        {
            if (itemsToSpawn[i] != null)
            {
                int amount = (quantities != null && i < quantities.Length && quantities[i] > 0) 
                    ? quantities[i] 
                    : 1;
                    
                chest.AddItemToChest(itemsToSpawn[i], amount);
                Debug.Log($"[ChestLootInitializer] Added {amount}x {itemsToSpawn[i].itemName} to {gameObject.name}");
            }
        }
    }
}
