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

        // Use potion if clicked and not holding anything with mouse
        if (clickedUISlot.AssignedInventorySlot.ItemData != null &&
            mouseInventoryItem.AssignedInventorySlot.ItemData == null)
        {
            var item = clickedUISlot.AssignedInventorySlot.ItemData;
            if (item.potionEffectType != PotionEffectType.None)
            {
                // Use the potion
                var handler = GameObject.FindObjectOfType<PotionUseHandler>();
                if (handler != null)
                {
                    handler.UsePotion(item);
                    // Remove one from stack or clear slot
                    clickedUISlot.AssignedInventorySlot.RemoveFromStack(1);
                    if (clickedUISlot.AssignedInventorySlot.StackSize <= 0)
                        clickedUISlot.ClearSlot();
                    clickedUISlot.UpdateUISlot();
                    inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
                    return;
                }
            }
        }

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
    }

}
