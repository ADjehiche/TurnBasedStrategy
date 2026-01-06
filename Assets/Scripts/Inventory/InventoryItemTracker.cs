using UnityEngine;

/// <summary>
/// Attach this to items in the world that are linked to inventory.
/// When the item is destroyed, it will be removed from the player's inventory.
/// </summary>
public class InventoryItemTracker : MonoBehaviour
{
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private bool removeFromInventoryOnDestroy = true;
    
    private InventoryHolder playerInventory;
    
    /// <summary>
    /// Disable the automatic removal behavior (used when switching equipped items)
    /// </summary>
    public void DisableRemovalOnDestroy()
    {
        removeFromInventoryOnDestroy = false;
    }
    
    void Start()
    {
        // Find the player's inventory holder
        playerInventory = FindFirstObjectByType<InventoryHolder>();
        
        if (playerInventory == null)
        {
            Debug.LogWarning($"InventoryItemTracker on {gameObject.name}: No InventoryHolder found in scene");
        }
    }
    
    /// <summary>
    /// Set the item data this tracker should monitor
    /// </summary>
    public void SetItemData(InventoryItemData data)
    {
        itemData = data;
    }
    
    void OnDestroy()
    {
        // Only remove from inventory if enabled and we have valid references
        if (!removeFromInventoryOnDestroy || itemData == null || playerInventory == null)
        {
            return;
        }
        
        // Find the inventory slot containing this item and remove one
        RemoveItemFromInventory();
    }
    
    /// <summary>
    /// Remove this item from the player's inventory
    /// </summary>
    private void RemoveItemFromInventory()
    {
        InventorySystem inventory = playerInventory.InventorySystem;
        
        // Find a slot that contains this item
        foreach (var slot in inventory.InventorySlots)
        {
            if (slot.ItemData == itemData)
            {
                // Found it - remove one from stack
                if (slot.StackSize > 1)
                {
                    slot.RemoveFromStack(1);
                    inventory.OnInventorySlotChanged?.Invoke(slot);
                    Debug.Log($"InventoryItemTracker: Removed 1x {itemData.itemName} from inventory (destroyed in world). Remaining: {slot.StackSize}");
                }
                else
                {
                    // Last one in stack - clear the slot
                    slot.Clear();
                    inventory.OnInventorySlotChanged?.Invoke(slot);
                    Debug.Log($"InventoryItemTracker: Removed last {itemData.itemName} from inventory (destroyed in world)");
                }
                
                return; // Only remove from one slot
            }
        }
        
        Debug.LogWarning($"InventoryItemTracker: Could not find {itemData.itemName} in inventory to remove");
    }
}
