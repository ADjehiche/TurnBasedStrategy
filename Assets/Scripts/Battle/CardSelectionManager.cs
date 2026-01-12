using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Manages card selection in battle. Detects clicks outside of cards to deselect.
/// Attach this to a UI element that covers the whole screen (like the battle canvas).
/// </summary>
public class CardSelectionManager : MonoBehaviour, IPointerDownHandler
{
    [Header("Input")]
    [SerializeField] private InputActionReference cancelAction;
    
    private void OnEnable()
    {
        // Subscribe to cancel action (Escape key or right-click)
        if (cancelAction != null && cancelAction.action != null)
        {
            cancelAction.action.performed += OnCancelPerformed;
            cancelAction.action.Enable();
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from cancel action
        if (cancelAction != null && cancelAction.action != null)
        {
            cancelAction.action.performed -= OnCancelPerformed;
            cancelAction.action.Disable();
        }
    }
    
    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        // Deselect the current card when Escape or right-click is pressed
        CardMovement.DeselectCurrentCard();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if we clicked on a UI element that's a card
        if (eventData.pointerEnter != null)
        {
            // If we clicked on a card, let the card handle it
            var cardMovement = eventData.pointerEnter.GetComponent<CardMovement>();
            if (cardMovement != null)
            {
                return; // Card will handle this click
            }
            
            // Also check parents in case we clicked on a child element of the card
            cardMovement = eventData.pointerEnter.GetComponentInParent<CardMovement>();
            if (cardMovement != null)
            {
                return; // Card will handle this click
            }
        }
        
        // We clicked somewhere that's not a card, so deselect
        CardMovement.DeselectCurrentCard();
    }
}
