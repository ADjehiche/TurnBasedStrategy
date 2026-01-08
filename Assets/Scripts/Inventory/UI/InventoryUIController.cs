using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{

    public DynamicInventoryDisplay chestPanel;
    public DynamicInventoryDisplay playerBackPackPanel;

    
    // Public property to check if inventory is currently open
    public bool IsInventoryOpen => 
        (chestPanel != null && chestPanel.gameObject.activeInHierarchy) ||
        (playerBackPackPanel != null && playerBackPackPanel.gameObject.activeInHierarchy);

    void Awake()
    {
        chestPanel.gameObject.SetActive(false);
        playerBackPackPanel.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;
    }

    void Update()
    {
        if (chestPanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            chestPanel.gameObject.SetActive(false);
        }
        if (playerBackPackPanel.gameObject.activeInHierarchy && Keyboard.current.escapeKey.wasPressedThisFrame) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerBackPackPanel.gameObject.SetActive(false);
        }
    }

    void DisplayInventory(InventorySystem invToDisplay)
    {   
        chestPanel.gameObject.SetActive(true);
        chestPanel.RefreshDynamicInventory(invToDisplay);
    }
    void DisplayPlayerBackpack(InventorySystem invToDisplay)
    {   
        playerBackPackPanel.gameObject.SetActive(true);
        playerBackPackPanel.RefreshDynamicInventory(invToDisplay);
    }
}
