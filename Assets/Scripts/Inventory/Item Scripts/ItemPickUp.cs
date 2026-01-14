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
        var inventory = other.transform.GetComponent<PlayerInventoryHolder>();
        if (!inventory) return;
       if(inventory.AddToInventory(ItemData, 1))
       {
        // Track if this is the original key being collected
        if (gameObject.name == "Key")
        {
            GameSession.OriginalKeyCollected = true;
            Debug.Log("[ItemPickUp] Original key collected - marked in GameSession");
        }
        
        // Check if item data is specifically the cell key (not skeleton key)
        if (ItemData != null && (ItemData.name.ToLower().Contains("cell key") || 
                                (ItemData.name.ToLower().Contains("key") && !ItemData.name.ToLower().Contains("skeleton"))))
        {
            Debug.Log("[ItemPickUp] Cell key picked up - triggering objectives");
            
            // Find and trigger the objective system
            var objectiveManager = FindFirstObjectByType<SimpleLevelOneObjectives>();
            if (objectiveManager != null)
            {
                objectiveManager.StartObjectives(); // Show first objective
                objectiveManager.OnKeyPickedUp(); // Complete "Find Key", show "Escape Cell"
            }
            else
            {
                Debug.LogWarning("[ItemPickUp] SimpleLevelOneObjectives not found in scene!");
            }
        }
        
        Destroy(this.gameObject);
       }
    }
    
    void Update()
    {
        
    }
}
