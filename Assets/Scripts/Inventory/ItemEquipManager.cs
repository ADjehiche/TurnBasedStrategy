using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the physical display of inventory items in the player's hand.
/// Bridges the inventory UI hotbar with the 3D item display system.
/// When a hotbar slot is selected (1-9 keys), this instantiates the item's 3D prefab at the hold position.
/// </summary>
public class ItemEquipManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPos; // Where items appear (from PickUpScript)
    [SerializeField] private InventoryHolder inventoryHolder; // Reference to player's inventory
    [SerializeField] private StaticInventoryDisplay inventoryDisplay; // Reference to UI display
    
    [Header("Settings")]
    [SerializeField] private int defaultSelectedSlot = 0; // Which slot is selected by default (0-based)
    [SerializeField] private bool debugMode = true;
    [SerializeField] private float throwForce = 500f; // Force applied when throwing items
    [SerializeField] private Vector3 throwOffset = new Vector3(0, 0.5f, 1f); // Offset from player when spawning thrown item
    
    // State tracking
    private InventorySystem inventorySystem;
    private int currentlySelectedSlotIndex = 0;
    private GameObject currentlyEquippedItem;
    private InventorySlot currentlySelectedSlot;
    
    // Input
    private PlayerInput playerInput;
    private InputAction[] hotbarActions;
    private InputAction fireAction;
    
    void Awake()
    {
        // Get player input component
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("ItemEquipManager: PlayerInput component not found. Make sure it's on this GameObject or a parent.");
            return;
        }
        
        SetupHotbarInput();
        
        // Setup fire/throw action
        fireAction = playerInput.actions["Fire"];
        if (fireAction != null)
        {
            fireAction.performed += ctx => OnFirePerformed();
        }
    }
    
    void Start()
    {
        // Get inventory system reference
        if (inventoryHolder != null)
        {
            inventorySystem = inventoryHolder.InventorySystem;
            
            // Subscribe to inventory changes
            inventorySystem.OnInventorySlotChanged += OnInventorySlotChanged;
            
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Connected to inventory with {inventorySystem.InventorySize} slots");
            }
        }
        else
        {
            Debug.LogError("ItemEquipManager: InventoryHolder not assigned!");
            return;
        }
        
        if (holdPos == null)
        {
            Debug.LogError("ItemEquipManager: Hold position not assigned!");
            return;
        }
        
        // Select default slot and equip if it has an item
        currentlySelectedSlotIndex = defaultSelectedSlot;
        UpdateEquippedItem();
        
        if (debugMode)
        {
            Debug.Log("ItemEquipManager: Initialized successfully");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inventorySystem != null)
        {
            inventorySystem.OnInventorySlotChanged -= OnInventorySlotChanged;
        }
        

        if (hotbarActions != null)
        {
            foreach (var action in hotbarActions)
            {
                if (action != null)
                {
                    action.Disable();
                }
            }
        }
        
        if (fireAction != null)
        {
            fireAction.performed -= ctx => OnFirePerformed();
        }
    }
    
    /// <summary>
    /// Set up input for hotbar slot selection (keys 1-9)
    /// </summary>
    private void SetupHotbarInput()
    {

    }
    
    void Update()
    {
        // Check for hotbar key presses (1-9 for slots 0-8, 0 for slot 9)
        if (Keyboard.current != null)
        {
            // Check keys 1-9 (map to slots 0-8)
            for (int i = 1; i <= 9; i++)
            {
                Key key = GetNumberKey(i);
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    SelectHotbarSlot(i - 1); // 1 -> slot 0, 2 -> slot 1, etc.
                    break;
                }
            }
            
            // Check key 0 (maps to slot 9, the 10th slot)
            if (Keyboard.current[Key.Digit0].wasPressedThisFrame)
            {
                SelectHotbarSlot(9); // 0 -> slot 9
            }
        }
    }
    
    /// <summary>
    /// Get the keyboard key for a number (1-9)
    /// </summary>
    private Key GetNumberKey(int number)
    {
        switch (number)
        {
            case 1: return Key.Digit1;
            case 2: return Key.Digit2;
            case 3: return Key.Digit3;
            case 4: return Key.Digit4;
            case 5: return Key.Digit5;
            case 6: return Key.Digit6;
            case 7: return Key.Digit7;
            case 8: return Key.Digit8;
            case 9: return Key.Digit9;
            default: return Key.Digit1;
        }
    }
    
    /// <summary>
    /// Select a hotbar slot by index (0-based)
    /// </summary>
    public void SelectHotbarSlot(int slotIndex)
    {
        // Validate slot index
        if (slotIndex < 0 || slotIndex >= inventorySystem.InventorySize)
        {
            Debug.LogWarning($"ItemEquipManager: Invalid slot index {slotIndex}");
            return;
        }
        
        // If already selected, do nothing
        if (slotIndex == currentlySelectedSlotIndex)
        {
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Slot {slotIndex} already selected");
            }
            return;
        }
        
        // Update selection
        int previousSlot = currentlySelectedSlotIndex;
        currentlySelectedSlotIndex = slotIndex;
        
        if (debugMode)
        {
            Debug.Log($"ItemEquipManager: Switched from slot {previousSlot} to slot {slotIndex}");
        }
        
        // Update the equipped item
        UpdateEquippedItem();
        
        // Notify UI to update highlight (we'll implement this later)
        NotifySlotSelectionChanged(previousSlot, slotIndex);
    }
    
    /// <summary>
    /// Update the physically equipped item based on currently selected slot
    /// </summary>
    private void UpdateEquippedItem()
    {
        // Get the currently selected slot
        currentlySelectedSlot = inventorySystem.InventorySlots[currentlySelectedSlotIndex];
        
        // Clear current equipped item
        ClearEquippedItem();
        
        // Check if slot has an item
        if (currentlySelectedSlot.ItemData != null)
        {
            // Equip the new item
            EquipItem(currentlySelectedSlot.ItemData);
        }
        else
        {
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Slot {currentlySelectedSlotIndex} is empty - no item equipped");
            }
        }
    }
    
    /// <summary>
    /// Equip an item (instantiate its 3D prefab at hold position)
    /// </summary>
    private void EquipItem(InventoryItemData itemData)
    {
        // Check if item has a prefab assigned
        if (itemData.itemPrefab == null)
        {
            if (debugMode)
            {
                Debug.LogWarning($"ItemEquipManager: Item '{itemData.itemName}' has no prefab assigned!");
            }
            return;
        }
        
        // Instantiate the item prefab at hold position
        currentlyEquippedItem = Instantiate(itemData.itemPrefab, holdPos);
        
        // Reset local position and rotation to match hold position exactly
        currentlyEquippedItem.transform.localPosition = Vector3.zero;
        currentlyEquippedItem.transform.localRotation = Quaternion.identity;
        
        // Disable physics on equipped item (it's just for display)
        Rigidbody rb = currentlyEquippedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Disable collider on equipped item (prevent interference)
        Collider col = currentlyEquippedItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Add tracker component to remove from inventory if this item gets destroyed
        InventoryItemTracker tracker = currentlyEquippedItem.GetComponent<InventoryItemTracker>();
        if (tracker == null)
        {
            tracker = currentlyEquippedItem.AddComponent<InventoryItemTracker>();
        }
        tracker.SetItemData(itemData);
        
        if (debugMode)
        {
            Debug.Log($"ItemEquipManager: Equipped '{itemData.itemName}' from slot {currentlySelectedSlotIndex} (added InventoryItemTracker)");
        }
    }
    
    /// <summary>
    /// Clear the currently equipped item (destroy the 3D model)
    /// </summary>
    private void ClearEquippedItem()
    {
        if (currentlyEquippedItem != null)
        {
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Clearing equipped item (switching slots)");
            }
            
            InventoryItemTracker tracker = currentlyEquippedItem.GetComponent<InventoryItemTracker>();
            if (tracker != null)
            {
                tracker.DisableRemovalOnDestroy();
                if (debugMode)
                {
                    Debug.Log($"ItemEquipManager: Disabled tracker removal to prevent inventory removal on slot switch");
                }
            }
            
            Destroy(currentlyEquippedItem);
            currentlyEquippedItem = null;
        }
    }
    
    /// <summary>
    /// Called when any inventory slot changes (item added/removed/updated)
    /// </summary>
    private void OnInventorySlotChanged(InventorySlot updatedSlot)
    {
        // Check if the changed slot is the currently selected one
        // Compare both by reference AND by checking if it's in the currently selected index
        if (updatedSlot == currentlySelectedSlot || 
            (inventorySystem != null && 
             currentlySelectedSlotIndex >= 0 && 
             currentlySelectedSlotIndex < inventorySystem.InventorySize &&
             inventorySystem.InventorySlots[currentlySelectedSlotIndex] == updatedSlot))
        {
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Currently selected slot (index {currentlySelectedSlotIndex}) changed - updating equipped item");
            }
            
            // Refresh the reference to the current slot in case it changed
            currentlySelectedSlot = inventorySystem.InventorySlots[currentlySelectedSlotIndex];
            
            UpdateEquippedItem();
        }
    }
    
    /// <summary>
    /// Notify the UI that slot selection has changed (for visual highlight)
    /// </summary>
    private void NotifySlotSelectionChanged(int previousSlot, int newSlot)
    {
        // TODO: Implement UI notification for highlight
        // We'll add this in Step 6
        if (inventoryDisplay != null)
        {
            // For now, just log it
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Should update UI highlight from slot {previousSlot} to {newSlot}");
            }
        }
    }
    
    /// <summary>
    /// Get the currently selected slot index (for UI)
    /// </summary>
    public int GetCurrentlySelectedSlotIndex()
    {
        return currentlySelectedSlotIndex;
    }
    
    /// <summary>
    /// Get the currently equipped item GameObject (for external scripts)
    /// </summary>
    public GameObject GetCurrentlyEquippedItem()
    {
        return currentlyEquippedItem;
    }
    
    /// <summary>
    /// Force refresh the equipped item (useful for testing)
    /// </summary>
    [ContextMenu("Refresh Equipped Item")]
    public void RefreshEquippedItem()
    {
        UpdateEquippedItem();
    }
    
    /// <summary>
    /// Called when Fire button is pressed - throws the currently equipped item
    /// </summary>
    private void OnFirePerformed()
    {
        //nothing
    }
    
    /// <summary>
    /// Throw the currently equipped item into the world
    /// </summary>
    private void ThrowEquippedItem()
    {
        if (currentlySelectedSlot == null || currentlySelectedSlot.ItemData == null)
        {
            Debug.LogWarning("ItemEquipManager: Cannot throw - no item in selected slot");
            return;
        }
        
        InventoryItemData itemData = currentlySelectedSlot.ItemData;
        
        if (itemData.itemPrefab == null)
        {
            Debug.LogWarning($"ItemEquipManager: Cannot throw '{itemData.itemName}' - no prefab assigned");
            return;
        }
        
        // Calculate spawn position (in front of player)
        Vector3 spawnPosition = transform.position + transform.TransformDirection(throwOffset);
        
        // Instantiate the item in the world
        GameObject thrownItem = Instantiate(itemData.itemPrefab, spawnPosition, Quaternion.identity);
        
        // Re-enable physics and collider
        Rigidbody rb = thrownItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * throwForce);
        }
        
        Collider col = thrownItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Make sure it has ItemPickUp component so it can be picked up again
        if (thrownItem.GetComponent<ItemPickUp>() == null)
        {
            ItemPickUp pickupComponent = thrownItem.AddComponent<ItemPickUp>();
            pickupComponent.ItemData = itemData;
        }
        
        // Remove any InventoryItemTracker from thrown item (we're already removing it from inventory)
        InventoryItemTracker tracker = thrownItem.GetComponent<InventoryItemTracker>();
        if (tracker != null)
        {
            Destroy(tracker);
            if (debugMode)
            {
                Debug.Log("ItemEquipManager: Removed InventoryItemTracker from thrown item (already removed from inventory)");
            }
        }
        
        // Add tag so it can be picked up
        if (!thrownItem.CompareTag("canPickUp"))
        {
            thrownItem.tag = "canPickUp";
        }
        
        if (debugMode)
        {
            Debug.Log($"ItemEquipManager: Threw '{itemData.itemName}' with force {throwForce}");
        }
        
        // Remove item from inventory AFTER spawning (so thrown item doesn't trigger tracker)
        RemoveItemFromCurrentSlot();
    }
    
    /// <summary>
    /// Remove one item from the currently selected slot
    /// </summary>
    private void RemoveItemFromCurrentSlot()
    {
        if (currentlySelectedSlot == null)
        {
            Debug.LogWarning("ItemEquipManager: Cannot remove item - no slot selected");
            return;
        }
        
        InventoryItemData itemData = currentlySelectedSlot.ItemData;
        
        if (itemData == null)
        {
            Debug.LogWarning("ItemEquipManager: Cannot remove item - slot is empty");
            return;
        }
        
        // Decrease stack size
        if (currentlySelectedSlot.StackSize > 1)
        {
            currentlySelectedSlot.RemoveFromStack(1);
            inventorySystem.OnInventorySlotChanged?.Invoke(currentlySelectedSlot);
            
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Removed 1x '{itemData.itemName}' from slot {currentlySelectedSlotIndex}. Remaining: {currentlySelectedSlot.StackSize}");
            }
        }
        else
        {
            // Last item in stack - clear the slot
            currentlySelectedSlot.Clear();
            inventorySystem.OnInventorySlotChanged?.Invoke(currentlySelectedSlot);
            
            // Clear equipped item
            ClearEquippedItem();
            
            if (debugMode)
            {
                Debug.Log($"ItemEquipManager: Removed last '{itemData.itemName}' from slot {currentlySelectedSlotIndex}");
            }
        }
    }
    
    /// <summary>
    /// Debug: Print current inventory state
    /// </summary>
    [ContextMenu("Debug: Print Inventory State")]
    private void DebugPrintInventory()
    {
        if (inventorySystem == null)
        {
            Debug.Log("ItemEquipManager: No inventory system!");
            return;
        }
        
        Debug.Log("=== INVENTORY STATE ===");
        for (int i = 0; i < inventorySystem.InventorySize; i++)
        {
            var slot = inventorySystem.InventorySlots[i];
            string selected = (i == currentlySelectedSlotIndex) ? " [SELECTED]" : "";
            
            if (slot.ItemData != null)
            {
                Debug.Log($"Slot {i}: {slot.ItemData.itemName} x{slot.StackSize}{selected}");
            }
            else
            {
                Debug.Log($"Slot {i}: Empty{selected}");
            }
        }
        Debug.Log("======================");
    }
}
