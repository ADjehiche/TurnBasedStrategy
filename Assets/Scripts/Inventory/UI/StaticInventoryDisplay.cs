using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] slots;
    [SerializeField] private ItemEquipManager equipManager; // Reference to equip manager for slot selection
    
    private int currentlySelectedSlotIndex = 0;
    protected override void Start()
    {
        base.Start();

        if (inventoryHolder != null) { 
            inventorySystem = inventoryHolder.InventorySystem;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
        }else{
            Debug.LogWarning("InventoryHolder is null");
        }
        AssignSlot(inventorySystem);
        
        // Auto-find equip manager if not assigned
        if (equipManager == null)
        {
            equipManager = FindFirstObjectByType<ItemEquipManager>();
        }
        
        // Set initial selection highlight
        UpdateSlotSelection(0);
    }
    
    void Update()
    {
        // Sync with ItemEquipManager's selected slot
        if (equipManager != null)
        {
            int selectedSlot = equipManager.GetCurrentlySelectedSlotIndex();
            if (selectedSlot != currentlySelectedSlotIndex)
            {
                UpdateSlotSelection(selectedSlot);
            }
        }
    }
    
    /// <summary>
    /// Update which slot is visually selected
    /// </summary>
    private void UpdateSlotSelection(int newSlotIndex)
    {
        // Deselect old slot
        if (currentlySelectedSlotIndex >= 0 && currentlySelectedSlotIndex < slots.Length)
        {
            slots[currentlySelectedSlotIndex].SetSelected(false);
        }
        
        // Select new slot
        currentlySelectedSlotIndex = newSlotIndex;
        if (currentlySelectedSlotIndex >= 0 && currentlySelectedSlotIndex < slots.Length)
        {
            slots[currentlySelectedSlotIndex].SetSelected(true);
        }
    }
    public override void AssignSlot(InventorySystem invToDisplay)
    {
        inventorySystem = invToDisplay;
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();
        for (int i = 0; i < inventorySystem.InventorySize;i++)
        {
            slotDictionary.Add(slots[i], inventorySystem.InventorySlots[i]);
            slots[i].Init(inventorySystem.InventorySlots[i]);
        }
    }
}
