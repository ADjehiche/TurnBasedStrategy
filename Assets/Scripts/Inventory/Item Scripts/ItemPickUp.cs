using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class ItemPickUp : MonoBehaviour
{
    public InventoryItemData ItemData;
    private SphereCollider myCollider;
    public float pickupRadius = 1f;

    private void Awake()
    {
        myCollider = GetComponent<SphereCollider>();
        myCollider.isTrigger = true;
        myCollider.radius = pickupRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        var inventory = other.transform.GetComponent<InventoryHolder>();
        if (!inventory) return;
       if(inventory.InventorySystem.AddToInventory(ItemData, 1))
       {
        Destroy(this.gameObject);
       }
    }
}
