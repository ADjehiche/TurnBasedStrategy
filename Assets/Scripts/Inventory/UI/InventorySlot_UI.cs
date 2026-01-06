
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics.CodeAnalysis;
public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] private Image itemSprite;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] private InventorySlot assignedInventorySlot;
    
    [Header("Selection Highlight")]
    [SerializeField] private GameObject selectionHighlight; // UI element to show when selected
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Image slotBackground; // Optional: background image to tint

    private Button button;
    private bool isSelected = false;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay {get; private set;}

private void Awake()
    {
        ClearSlot();

        button = GetComponent<Button>();
        button?.onClick.AddListener(OnUISlotClicked);
        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
        
        // Hide selection highlight by default
        SetSelected(false);
    }

    public void Init(InventorySlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot)
    {
        if (slot.ItemData != null) {
            itemSprite.sprite = slot.ItemData.icon;
            itemSprite.color = Color.white;
        }
        else { 
            ClearSlot();
        }
        if (slot.StackSize > 1) { 
            itemCount.text = slot.StackSize.ToString();
        }
        else { 
            itemCount.text = ""; 
        }
    }
    public void UpdateUISlot()
    {
        if (assignedInventorySlot != null) UpdateUISlot(AssignedInventorySlot);
    }
    public void ClearSlot()
    {
        assignedInventorySlot.Clear();
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    private void OnUISlotClicked()
    {
        ParentDisplay?.SlotClicked(this);
    }
    
    /// <summary>
    /// Set whether this slot is currently selected (for hotbar)
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        // Show/hide highlight object if assigned
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(selected);
        }
        
        // Tint background if assigned
        if (slotBackground != null)
        {
            slotBackground.color = selected ? selectedColor : normalColor;
        }
    }
    
    /// <summary>
    /// Check if this slot is currently selected
    /// </summary>
    public bool IsSelected()
    {
        return isSelected;
    }

}
