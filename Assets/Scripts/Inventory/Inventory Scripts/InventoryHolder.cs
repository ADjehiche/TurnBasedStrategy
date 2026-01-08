using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    [SerializeField] protected InventorySystem PrimaryInventorySystem;

    public InventorySystem InventorySystem => PrimaryInventorySystem;
    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    protected virtual void Awake()
    {
        PrimaryInventorySystem = new InventorySystem(inventorySize);
    }
}
