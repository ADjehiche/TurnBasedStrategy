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
        UnityEngine.Debug.Log($"[InventoryDisplay] SlotClicked called! Slot has item: {clickedUISlot.AssignedInventorySlot.ItemData != null}");
        UnityEngine.Debug.Log($"[InventoryDisplay] MouseItemData has item: {mouseInventoryItem?.AssignedInventorySlot?.ItemData != null}");

        // Picking up an item from a slot
        if(clickedUISlot.AssignedInventorySlot.ItemData!=null && mouseInventoryItem.AssignedInventorySlot.ItemData==null)
        {
            UnityEngine.Debug.Log($"[InventoryDisplay] Picking up item: {clickedUISlot.AssignedInventorySlot.ItemData.itemName}");
            mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
            clickedUISlot.ClearSlot();
            
            // Notify that the slot changed
            inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
            return;
        }
        // Placing an item into an empty slot
        if(clickedUISlot.AssignedInventorySlot.ItemData == null && mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            UnityEngine.Debug.Log($"[InventoryDisplay] Placing item: {mouseInventoryItem.AssignedInventorySlot.ItemData.itemName}");
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
            clickedUISlot.UpdateUISlot();
            mouseInventoryItem.ClearSlot();
            
            // Notify that the slot changed
            inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
        }
        // Swapping items (both have items)
        if(clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            // If same item type and stackable, try to stack
            if (clickedUISlot.AssignedInventorySlot.ItemData == mouseInventoryItem.AssignedInventorySlot.ItemData &&
                clickedUISlot.AssignedInventorySlot.ItemData.isStackable)
            {
                // Try to add to stack
                int remaining = clickedUISlot.AssignedInventorySlot.ItemData.maxStack - clickedUISlot.AssignedInventorySlot.StackSize;
                if (remaining > 0)
                {
                    int toAdd = Mathf.Min(remaining, mouseInventoryItem.AssignedInventorySlot.StackSize);
                    clickedUISlot.AssignedInventorySlot.AddToStack(toAdd);
                    mouseInventoryItem.AssignedInventorySlot.RemoveFromStack(toAdd);
                    
                    if (mouseInventoryItem.AssignedInventorySlot.StackSize <= 0)
                        mouseInventoryItem.ClearSlot();
                    else
                        mouseInventoryItem.UpdateMouseSlot(mouseInventoryItem.AssignedInventorySlot);
                    
                    clickedUISlot.UpdateUISlot();
                    inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
                    return;
                }
            }
            
            // Swap items
            UnityEngine.Debug.Log($"[InventoryDisplay] Swapping items");
            InventorySlot tempSlot = new InventorySlot();
            tempSlot.AssignItem(clickedUISlot.AssignedInventorySlot);
            
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
            mouseInventoryItem.UpdateMouseSlot(tempSlot);
            
            clickedUISlot.UpdateUISlot();
            inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
        }
    }

}
