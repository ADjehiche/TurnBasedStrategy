using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject door;
    [SerializeField] private string keyTag = "key";
    [SerializeField] private string playerTag = "Player"; // Tag for player
    [SerializeField] private InventoryItemData requiredKeyItemData; // The key item required to open

    [Header("Key Type Restriction")]
    [Tooltip("Which type of key can open this door")]
    [SerializeField] private KeyType requiredKeyType = KeyType.AnyKey;
    [Tooltip("If true, any key can open this door (ignores requiredKeyType)")]
    [SerializeField] private bool acceptAnyKey = false;

    [Header("Hinge")]
    [SerializeField] private Transform hingePoint;
    [SerializeField] private Vector3 hingeLocalOffset = Vector3.zero;

    [Header("Open settings")]
    [SerializeField] private float openSpeed = 90f;
    [SerializeField] private float openAngle = -90f;
    [SerializeField] private bool useYAxis = true;

    private bool isOpening = false;
    private GameObject key;

    private const String DOOR_NAME = "Door";
    private const String KEY_NAME = "Key";

    void Start()
    {
        if (door == null)
            door = GameObject.Find(DOOR_NAME);

        key = GameObject.Find(KEY_NAME);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DoorTrigger] Something entered trigger: {other.gameObject.name} with tag '{other.tag}'");
        
        if (isOpening) return;

        // Check if it's a physical key in the world (old system)
        if (other.CompareTag(keyTag))
        {
            Debug.Log("[DoorTrigger] Physical key detected (old system)");
            // Check if this key is allowed to open this door
            if (!IsKeyAllowed(other.gameObject))
            {
                Debug.Log($"[DoorTrigger] Wrong key type! This door requires: {requiredKeyType}");
                return;
            }

            // Destroy the colliding object (child collider)
            if (other.gameObject != null)
                Destroy(other.gameObject);
            
            // Also destroy the parent key GameObject (the actual key prefab root)
            if (other.transform.parent != null)
                Destroy(other.transform.parent.gameObject);
            
            // Destroy the original key reference if it exists
            if (key != null)
                Destroy(key);

            if (door != null)
            {
                StartCoroutine(OpenDoorCoroutine());
            }
        }
        // Check if it's the player with key in inventory (new system)
        else if (other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))
        {
            Debug.Log($"[DoorTrigger] Player detected! Tag: {other.tag}, Root tag: {other.transform.root.tag}");
            TryOpenDoorWithInventory(other.gameObject);
        }
        else
        {
            Debug.Log($"[DoorTrigger] Object doesn't match key tag ('{keyTag}') or player tag ('{playerTag}')");
        }
    }
    
    /// <summary>
    /// Try to open the door if player has the required key in their inventory
    /// </summary>
    private void TryOpenDoorWithInventory(GameObject playerObject)
    {
        Debug.Log($"[DoorTrigger] Player entered door trigger. Checking inventory...");
        
        // Find the player's inventory
        InventoryHolder inventoryHolder = playerObject.GetComponent<InventoryHolder>();
        if (inventoryHolder == null)
        {
            inventoryHolder = playerObject.GetComponentInParent<InventoryHolder>();
        }
        
        if (inventoryHolder == null)
        {
            Debug.LogWarning("[DoorTrigger] Player has no InventoryHolder component!");
            return;
        }
        
        Debug.Log("[DoorTrigger] Found InventoryHolder");
        
        // Check if player has the required key in inventory
        if (requiredKeyItemData == null)
        {
            Debug.LogWarning("[DoorTrigger] No requiredKeyItemData assigned! Cannot check inventory. Please assign the Key ItemData in the Inspector.");
            return;
        }
        
        Debug.Log($"[DoorTrigger] Looking for '{requiredKeyItemData.itemName}' in inventory...");
        
        InventorySystem inventory = inventoryHolder.InventorySystem;
        InventorySlot keySlot = null;
        
        // Find the key in inventory
        foreach (var slot in inventory.InventorySlots)
        {
            if (slot.ItemData != null)
            {
                Debug.Log($"[DoorTrigger] Checking slot: {slot.ItemData.itemName}");
            }
            
            if (slot.ItemData == requiredKeyItemData)
            {
                keySlot = slot;
                Debug.Log($"[DoorTrigger] FOUND KEY in inventory! Stack size: {slot.StackSize}");
                break;
            }
        }
        
        if (keySlot == null || keySlot.ItemData == null)
        {
            Debug.Log("[DoorTrigger] Player doesn't have the required key in inventory!");
            return;
        }
        
        Debug.Log($"[DoorTrigger] Player has {requiredKeyItemData.itemName} in inventory! Opening door and removing key.");
        
        // Remove key from inventory
        if (keySlot.StackSize > 1)
        {
            keySlot.RemoveFromStack(1);
            inventory.OnInventorySlotChanged?.Invoke(keySlot);
            Debug.Log($"[DoorTrigger] Removed 1x key from stack. Remaining: {keySlot.StackSize}");
        }
        else
        {
            keySlot.Clear();
            inventory.OnInventorySlotChanged?.Invoke(keySlot);
            Debug.Log($"[DoorTrigger] Cleared key from inventory slot.");
        }
        
        // Force the ItemEquipManager to refresh (clear the equipped item)
        ItemEquipManager equipManager = playerObject.GetComponentInChildren<ItemEquipManager>();
        if (equipManager == null)
        {
            equipManager = playerObject.GetComponentInParent<ItemEquipManager>();
        }
        
        if (equipManager != null)
        {
            equipManager.RefreshEquippedItem();
            Debug.Log("[DoorTrigger] Forced ItemEquipManager to refresh.");
        }
        
        // Open the door
        if (door != null)
        {
            StartCoroutine(OpenDoorCoroutine());
        }
    }

    /// <summary>
    /// Check if the given key object is allowed to open this door
    /// </summary>
    private bool IsKeyAllowed(GameObject keyObject)
    {
        // If door accepts any key, allow it
        if (acceptAnyKey || requiredKeyType == KeyType.AnyKey)
        {
            return true;
        }

        // Check if key has SkeletonKeyBehavior component
        // Check both the object itself and its parent (since key uses child collider)
        var skeletonKey = keyObject.GetComponent<SkeletonKeyBehavior>();
        if (skeletonKey == null && keyObject.transform.parent != null)
        {
            skeletonKey = keyObject.transform.parent.GetComponent<SkeletonKeyBehavior>();
        }

        if (skeletonKey != null)
        {
            // Key has a type marker, check if it matches
            Debug.Log($"[DoorTrigger] Found {skeletonKey.keyType} key, door requires: {requiredKeyType}");
            return skeletonKey.keyType == requiredKeyType;
        }
        else
        {
            // No SkeletonKeyBehavior component means it's the original key
            // Original key can only open doors that require OriginalKey
            Debug.Log($"[DoorTrigger] Found original key, door requires: {requiredKeyType}");
            return requiredKeyType == KeyType.OriginalKey;
        }
    }

    private IEnumerator OpenDoorCoroutine()
    {
        isOpening = true;
        
        // Play door unlock sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("DoorUnlock");
        }
        
        // Mark door as opened (for first door only)
        if (gameObject.name == "Door" || gameObject.name.Contains("Door") && !gameObject.name.Contains("Door_2"))
        {
            GameSession.DoorOpened = true;
            Debug.Log("[DoorTrigger] First door opened - marked in GameSession");
            
            // Trigger cell escaped objective
            var objectiveManager = FindFirstObjectByType<SimpleLevelOneObjectives>();
            if (objectiveManager != null)
            {
                objectiveManager.OnCellEscaped();
                Debug.Log("[DoorTrigger] Cell escape objective triggered");
            }
        }
        
        // Trigger celebration monologue when door starts opening
        TriggerDoorOpenCaption();
        
        Transform doorT = door.transform;

        Vector3 hingeWorldPos = (hingePoint != null) ? hingePoint.position : doorT.TransformPoint(hingeLocalOffset);
        Vector3 axis = useYAxis ? Vector3.up : Vector3.right;

        float totalRotated = 0f;
        float targetRotation = Mathf.Abs(openAngle);

        while (Mathf.Abs(totalRotated) < targetRotation)
        {
            float step = openSpeed * Time.deltaTime;
            if (Mathf.Abs(totalRotated + step) > targetRotation)
            {
                step = targetRotation - Mathf.Abs(totalRotated);
            }

            float rotationStep = Mathf.Sign(openAngle) * step;
            doorT.RotateAround(hingeWorldPos, axis, rotationStep);
            
            totalRotated += step;
            yield return null;
        }

        isOpening = false;

        // Destroy only the DoorTrigger component, not the entire GameObject
        Destroy(this);
    }
    
    private void TriggerDoorOpenCaption()
    {
        // Find and trigger the caption controller when door opens
        var levelController = FindFirstObjectByType<LevelOneCaptionController>();
        if (levelController != null)
        {
            levelController.OnDoorOpened();
            Debug.Log("DoorTrigger: Door open celebration caption triggered!");
        }
        else
        {
            Debug.LogWarning("DoorTrigger: LevelOneCaptionController not found in scene");
        }
    }
    
    /// <summary>
    /// Public method to open the door immediately (used for symbol-activated doors)
    /// </summary>
    public void OpenDoorImmediately()
    {
        if (!isOpening && door != null)
        {
            StartCoroutine(OpenDoorCoroutine());
        }
    }
}
