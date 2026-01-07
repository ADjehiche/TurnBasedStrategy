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
                ClearSlot();
            }
        }
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
