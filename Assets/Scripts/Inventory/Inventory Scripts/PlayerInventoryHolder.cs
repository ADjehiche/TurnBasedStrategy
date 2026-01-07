using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInventoryHolder : InventoryHolder
{
    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] protected InventorySystem secondaryInventorySystem;

    public InventorySystem SecondaryInventorySystem => secondaryInventorySystem;
    
    protected override void Awake()
    {
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            OnDynamicInventoryDisplayRequested?.Invoke(secondaryInventorySystem);
        }
    }
    public bool AddToInventory(InventoryItemData data, int amount)
    {
        if(PrimaryInventorySystem.AddToInventory(data, amount)){
            return true;
        } else if(secondaryInventorySystem.AddToInventory(data, amount)){
            return true;
        }
        return false;
    }
}
