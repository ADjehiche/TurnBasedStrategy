using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] private MouseItemData mouseInventoryItem;
    protected InventorySystem inventorySystem;
    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary;

    protected virtual void Start()
    {
        
    }
    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    public abstract void AssignSlot(InventorySystem invToDisplay);

    protected virtual void UpdateSlot(InventorySlot updatedSlot)
    {
        foreach (var slot in SlotDictionary)
        {
            if(slot.Value == updatedSlot)
            {
                slot.Key.UpdateUISlot(updatedSlot);
            }
        }
    }

    public void SlotClicked(InventorySlot_UI clickedUISlot)
    {
        // Picking up an item from a slot
        if(clickedUISlot.AssignedInventorySlot.ItemData!=null && mouseInventoryItem.AssignedInventorySlot.ItemData==null)
        {
            mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
            clickedUISlot.ClearSlot();
            
            // Notify that the slot changed
            inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
            return;
        }
        // Placing an item into an empty slot
        if(clickedUISlot.AssignedInventorySlot.ItemData == null && mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
            clickedUISlot.UpdateUISlot();
            mouseInventoryItem.ClearSlot();
            
            // Notify that the slot changed
            inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
        }
    }

}
