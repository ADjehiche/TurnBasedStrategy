using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;
    public float throwForce = 500f;
    public float pickUpRange = 5f;
    
    [Header("Inventory Integration")]
    [SerializeField] private InventoryHolder inventoryHolder; // Reference to player's inventory
    [SerializeField] private ItemEquipManager equipManager; // Reference to equip manager
    [SerializeField] private bool addToInventoryOnPickup = true; // Toggle inventory integration
    
    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private int LayerNumber;
    
    private PlayerInput playerInput;
    private InputAction pickUpAction;
    private InputAction fireAction;
    private const String PICKUP_ACTION_NAME = "PickUp";
    private const String FIRE_ACTION_NAME = "Fire";
    private const String CAN_PICKUP_TAG = "canPickUp";
    private const String HOLD_LAYER_NAME = "holdLayer";
    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found. Make sure it's on this GameObject or a parent.");
            return;
        }

        // Get references to the actions we need
        pickUpAction = playerInput.actions[PICKUP_ACTION_NAME];
        fireAction = playerInput.actions[FIRE_ACTION_NAME];

        // Setup callbacks for the actions
        // Pickup action DISABLED - using inventory system instead
        // pickUpAction.performed += ctx => OnPickUpPerformed();
        
        // Fire/throw action DISABLED - ItemEquipManager handles throwing now
        // fireAction.performed += ctx => OnFirePerformed();
        
        Debug.Log("PickUpScript: Old pickup and throw actions DISABLED. Using inventory system only.");
    }

    void OnEnable()
    {
        // Old pickup and throw actions disabled - using inventory system only
        // pickUpAction?.Enable();
        // fireAction?.Enable();
    }

    void OnDisable()
    {
        // Old pickup and throw actions disabled - using inventory system only
        // pickUpAction?.Disable();
        // fireAction?.Disable();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from input action callbacks to prevent errors
        // Old actions were never subscribed, so no need to unsubscribe
        // if (pickUpAction != null)
        // {
        //     pickUpAction.performed -= ctx => OnPickUpPerformed();
        // }
        // if (fireAction != null)
        // {
        //     fireAction.performed -= ctx => OnFirePerformed();
        // }
    }

    void Start()
    {
        LayerNumber = LayerMask.NameToLayer(HOLD_LAYER_NAME);
        
        // Auto-find inventory holder if not assigned
        if (inventoryHolder == null)
        {
            inventoryHolder = player.GetComponent<InventoryHolder>();
            if (inventoryHolder == null)
            {
                Debug.LogWarning("PickUpScript: No InventoryHolder found. Inventory integration disabled.");
                addToInventoryOnPickup = false;
            }
        }
        
        // Auto-find equip manager if not assigned
        if (equipManager == null)
        {
            equipManager = player.GetComponentInChildren<ItemEquipManager>();
            if (equipManager == null && addToInventoryOnPickup)
            {
                Debug.LogWarning("PickUpScript: No ItemEquipManager found. Items will be added to inventory but not auto-equipped.");
            }
        }
    }
    void Update()
    {
        if (heldObj != null)
        {
            MoveObject();
        }
    }
    
    private void OnPickUpPerformed()
    {
        if (heldObj == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
            {
                if (hit.transform.gameObject.CompareTag(CAN_PICKUP_TAG))
                {
                    PickUpObject(hit.transform.gameObject);
                }
            }
        }
        else
        {
            StopClipping();
            DropObject();
        }
    }

    private void OnFirePerformed()
    {
        // Only throw if using old pickup system (holding object directly in hand)
        // If using inventory system, ItemEquipManager handles throwing
        if (heldObj != null)
        {
            StopClipping();
            ThrowObject();
        }
    }
    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            // Check if this object has an ItemPickUp component with inventory data
            ItemPickUp itemPickup = pickUpObj.GetComponent<ItemPickUp>();
            
            // If inventory integration is enabled and item has inventory data
            if (addToInventoryOnPickup && itemPickup != null && itemPickup.ItemData != null && inventoryHolder != null)
            {
                // Try to add to inventory
                if (inventoryHolder.InventorySystem.AddToInventory(itemPickup.ItemData, 1))
                {
                    Debug.Log($"PickUpScript: Added {itemPickup.ItemData.itemName} to inventory");
                    
                    // Destroy the world object since it's now in inventory
                    Destroy(pickUpObj);
                    
                    // Optional: Auto-equip to hand if equip manager exists
                    if (equipManager != null)
                    {
                        // The ItemEquipManager will automatically show it if the current slot gets the item
                        // Or you can manually trigger a refresh
                        equipManager.RefreshEquippedItem();
                    }
                    
                    return;
                }
                else
                {
                    Debug.LogWarning("PickUpScript: Inventory full! Could not pick up item.");
                    return;
                }
            }
            
            // Fallback to old pickup behavior (hold in hand directly without inventory)
            // This is for objects that don't have ItemPickUp component
            
            // Check if this is the key and trigger caption
            if (pickUpObj.name == "Key")
            {
                TriggerKeyPickupCaption();
                // Mark original key as collected
                GameSession.OriginalKeyCollected = true;
                Debug.Log("[PickUpScript] Original key collected - marked in GameSession");
            }
            
            heldObj = pickUpObj;
            heldObjRb = pickUpObj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform;
            heldObj.layer = LayerNumber;
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }
    void DropObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObj = null;
    }
    void MoveObject()
    {
        heldObj.transform.position = holdPos.transform.position;
    }
    
    void ThrowObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }
    void StopClipping()
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f);
        }
    }
    
    private void TriggerKeyPickupCaption()
    {
        // Find and trigger the caption controller when key is picked up
        var levelController = FindFirstObjectByType<LevelOneCaptionController>();
        if (levelController != null)
        {
            levelController.OnKeyPickedUp();
            Debug.Log("PickUpScript: Key pickup caption triggered!");
        }
        else
        {
            Debug.LogWarning("PickUpScript: LevelOneCaptionController not found in scene");
        }
    }
}