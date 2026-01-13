using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class MouseItemData : MonoBehaviour
{
    public Image ItemSprite;
    public TextMeshProUGUI ItemCount;
    public InventorySlot AssignedInventorySlot;
    
    private RectTransform rectTransform;
    private Canvas canvas;

    public void Awake()
    {
        ItemSprite.color = Color.clear;
        ItemCount.text = "";
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // Initialize the inventory slot for holding dragged items
        AssignedInventorySlot = new InventorySlot();
        
        // IMPORTANT: Disable raycast on the mouse item so it doesn't block clicks on slots
        ItemSprite.raycastTarget = false;
        if (ItemCount != null)
        {
            ItemCount.raycastTarget = false;
        }
    }

    private void Update()
    {
        if(AssignedInventorySlot.ItemData!=null)
        {
            // Convert screen position to canvas position
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = mousePos;
            }
            else if (canvas != null)
            {
                // For ScreenSpaceCamera or WorldSpace canvas
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    mousePos,
                    canvas.worldCamera,
                    out Vector2 localPoint);
                rectTransform.position = canvas.transform.TransformPoint(localPoint);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject()){
                DropItemInWorld();
            }
        }
    }
    
    /// <summary>
    /// Spawn the held item as a prefab in the world instead of discarding it
    /// </summary>
    private void DropItemInWorld()
    {
        if (AssignedInventorySlot.ItemData == null) return;
        
        InventoryItemData itemData = AssignedInventorySlot.ItemData;
        int stackSize = AssignedInventorySlot.StackSize;
        
        // Check if item has a prefab
        if (itemData.itemPrefab != null)
        {
            // Find player and spawn in front of them
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 spawnPos = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 0.5f;
                GameObject droppedItem = Instantiate(itemData.itemPrefab, spawnPos, Quaternion.identity);
                
                // Enable physics
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.AddForce(player.transform.forward * 100f + Vector3.up * 50f);
                }
                
                // Enable collider
                Collider col = droppedItem.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = true;
                }
                
                // Ensure it can be picked up again
                ItemPickUp pickup = droppedItem.GetComponent<ItemPickUp>();
                if (pickup == null)
                {
                    pickup = droppedItem.AddComponent<ItemPickUp>();
                }
                pickup.ItemData = itemData;
                
                Debug.Log($"[MouseItemData] Dropped {itemData.itemName} x{stackSize} into the world");
            }
            else
            {
                Debug.LogWarning("[MouseItemData] No player found to drop item near!");
            }
        }
        else
        {
            Debug.LogWarning($"[MouseItemData] Item '{itemData.itemName}' has no prefab - cannot drop in world");
        }
        
        ClearSlot();
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        AssignedInventorySlot.AssignItem(invSlot);
        ItemSprite.sprite = invSlot.ItemData.icon;
        ItemCount.text = invSlot.StackSize.ToString();
        ItemSprite.color = Color.white;
    }
    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
    public void ClearSlot()
    {
        AssignedInventorySlot.Clear();
        ItemSprite.sprite = null;
        ItemSprite.color = Color.clear;
        ItemCount.text = "";
    }
}
