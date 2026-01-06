using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private int stackSize;

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;

    public InventorySlot(InventoryItemData source, int amount)
    {
        itemData = source;
        stackSize = amount;
    }

    public InventorySlot(){
        Clear();
    }

    public void Clear()
    {
        itemData = null;
        stackSize = -1;
    }
    public void UpdateInventorySlot(InventoryItemData data, int amount)
    {
        itemData = data;
        stackSize = amount;
    }
    public bool RoomLeftInTheStack(int amountToAdd, out int remainingAmount)
    {
        remainingAmount = ItemData.maxStack - stackSize;
        return RoomLeftInTheStack(amountToAdd);

    }
    public bool RoomLeftInTheStack(int amountToAdd)
    {
        if (stackSize + amountToAdd <=itemData.maxStack) return true;
        else return false;
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
    }
    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
    }
    public void AssignItem(InventorySlot invSlot)
    {
        if (itemData == invSlot.ItemData)
        {
            AddToStack(invSlot.stackSize);
        }else{
            itemData = invSlot.ItemData;
            stackSize = 0;
            AddToStack(invSlot.stackSize);
        }
    }
}
