using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] slots;
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
