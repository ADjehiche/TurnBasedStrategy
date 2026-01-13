using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChestInventory : InventoryHolder,IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(InventorySystem);
        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        
    }
    // Helper to add items to chest inventory (e.g. for potions)
    public void AddItemToChest(InventoryItemData item, int amount = 1)
    {
        PrimaryInventorySystem.AddToInventory(item, amount);
    }
}
