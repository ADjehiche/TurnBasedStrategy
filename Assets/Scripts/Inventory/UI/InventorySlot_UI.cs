
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics.CodeAnalysis;
public class InventorySlot_UI : MonoBehaviour
{
    [SerializeField] private Image itemSprite;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] private InventorySlot assignedInventorySlot;

    private Button button;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay {get; private set;}

private void Awake()
    {
        ClearSlot();

        button = GetComponent<Button>();
        button?.onClick.AddListener(OnUISlotClicked);
        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();

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

}
