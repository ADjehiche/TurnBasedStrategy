using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using CardGame;

/// <summary>
/// Handles card selection using New Input System.
/// Click 1: Select card (visual highlight)
/// Click 2: (Handled by TargetingSystem) Select target
/// </summary>
public class CardMovement : MonoBehaviour
{
    private CardInstance cardInstance;
    private RectTransform rectTransform;

    private Vector3 originalLocalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    // 0 = idle, 1 = hover, 2 = selected/targeting
    private int currentState = 0;

    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float selectedScale = 1.3f;
    [SerializeField] private Vector3 selectedPosition = new Vector3(0f, 100f, 0f); // Highlight position
    
    // NEW: Flag to disable clicking/playing while keeping hover active
    [HideInInspector] public bool isRewardCard = false; // Set by CardRewardUI
    
    private static CardMovement currentlySelectedCard = null;
    private Canvas canvas;

    void Awake()
    {
        cardInstance   = GetComponent<CardInstance>();
        rectTransform  = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        originalScale         = rectTransform.localScale;
        originalRotation      = rectTransform.localRotation;
        originalLocalPosition = rectTransform.localPosition;
        
        Debug.Log($"[CardMovement] Awake called on {gameObject.name}");
    }

    void OnDisable()
    {
        // Clear static reference if this card is selected
        if (currentlySelectedCard == this)
        {
            currentlySelectedCard = null;
        }
    }

    void Update()
    {
        // DEBUG: Log mouse position and state
        if (Input.GetMouseButtonUp(0))  // Changed from Down to Up to match Input System
        {
            Debug.Log($"[CardMovement] Mouse clicked! State: {currentState}, IsMouseOver: {IsMouseOverCard()}");
        }
        
        // Handle hover effect (check continuously for idle and hover states)
        if (currentState == 0 || currentState == 1)
        {
            CheckHover();
        }
        
        // Apply visual states
        if (currentState == 1)
        {
            rectTransform.localScale = originalScale * hoverScale;
        }
        else if (currentState == 2)
        {
            // Selected state - enlarged and centered
            rectTransform.localScale = originalScale * selectedScale;
        }
        else if (currentState == 0)
        {
            // Idle state - normal scale
            rectTransform.localScale = originalScale;
        }
        
        // Handle click input for card selection (CLICK 1)
        // CRITICAL: Use GetMouseButtonUp to match Input System's click behavior
        // This ensures CLICK 1 and CLICK 2 both happen on button release
        if (currentState == 1 && Input.GetMouseButtonUp(0))
        {
            if (IsMouseOverCard())
            {
                OnCardClicked();
            }
        }
    }

    /// <summary>
    /// Check if mouse is hovering over this card
    /// </summary>
    private void CheckHover()
    {
        if (IsMouseOverCard())
        {
            if (currentState == 0)
            {
                originalLocalPosition = rectTransform.localPosition;
                originalRotation      = rectTransform.localRotation;
                originalScale         = rectTransform.localScale;
                currentState          = 1; // hover
                Debug.Log($"[CardMovement] ✅ Hovering over card!");
            }
        }
        else
        {
            if (currentState == 1)
            {
                ResetVisual();
            }
        }
    }

    /// <summary>
    /// Check if mouse is currently over this card using RectTransform bounds
    /// </summary>
    private bool IsMouseOverCard()
    {
        if (rectTransform == null) return false;
        
        // Get mouse position in screen space
        Vector2 mousePos = Input.mousePosition;
        
        // Check if mouse is within RectTransform bounds
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, canvas?.worldCamera);
    }

    /// <summary>
    /// Called when card is clicked (CLICK 1 - Select card)
    /// </summary>
    private void OnCardClicked()
    {
        // NEW: If this is a reward card, don't handle clicks (Button component handles it)
        if (isRewardCard)
        {
            Debug.Log($"[CardMovement] Click ignored - this is a reward card (handled by Button)");
            return;
        }
        
        // CRITICAL: Prevent clicking if a card is currently being played
        if (TargetingSystem.Instance != null && TargetingSystem.Instance.IsBusy)
        {
            Debug.Log($"[CardMovement] Click ignored - another card is being played");
            return;
        }

        Debug.Log($"[CardMovement] 🃏 CLICK 1: Card clicked!");
        
        // Deselect any previously selected card
        if (currentlySelectedCard != null && currentlySelectedCard != this)
        {
            currentlySelectedCard.ResetVisual();
        }

        // Get card data
        Card cardData = cardInstance != null
            ? cardInstance.Data
            : GetComponent<CardDisplay>()?.cardData;

        if (cardData == null)
        {
            Debug.LogError("[CardMovement] No Card data found on this card.");
            ResetVisual();
            return;
        }

        // Check stamina BEFORE doing anything
        if (PlayerStamina.Instance != null && PlayerStamina.Instance.currentStamina < cardData.staminaCost)
        {
            Debug.Log($"[CardMovement] Not enough stamina to play {cardData.cardName} (cost: {cardData.staminaCost}, have: {PlayerStamina.Instance.currentStamina})");
            ResetVisual();
            return;
        }

        // Check if this card needs targeting
        bool needsTargeting = cardData.targetType == TargetType.SingleEnemy ||
                              cardData.targetType == TargetType.SingleAlly;

        if (!needsTargeting)
        {
            // Cards with TargetType.None, AllEnemies, AllAllies, or Self play immediately
            Debug.Log($"[CardMovement] Card {cardData.cardName} plays immediately (type: {cardData.targetType})");
            PlayCardImmediately(cardData);
            return;
        }

        // Move to selected state
        currentState = 2;
        currentlySelectedCard = this;

        // VISUAL HIGHLIGHT: Move to center and enlarge
        rectTransform.SetAsLastSibling(); // bring to front
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = originalScale * selectedScale;
        rectTransform.localPosition = selectedPosition;

        // Start targeting - now player clicks on enemy/player (CLICK 2)
        if (TargetingSystem.Instance != null)
        {
            TargetingSystem.Instance.BeginTargeting(cardData, gameObject, () =>
            {
                // Callback when targeting is cancelled
                if (this != null && rectTransform != null)
                {
                    ResetVisual();
                }
            });
        }
        else
        {
            Debug.LogError("[CardMovement] No TargetingSystem.Instance found in scene.");
            ResetVisual();
        }

        Debug.Log($"[CardMovement] ✅ Card selected: {cardData.cardName}. Now waiting for CLICK 2 on target...");
    }

    private void ResetVisual()
    {
        currentState = 0;
        rectTransform.localScale    = originalScale;
        rectTransform.localRotation = originalRotation;
        rectTransform.localPosition = originalLocalPosition;
        
        // Clear static reference if this is the currently selected card
        if (currentlySelectedCard == this)
        {
            currentlySelectedCard = null;
        }
    }

    // Public method to deselect this card (can be called externally)
    public void Deselect()
    {
        if (currentState == 2)
        {
            ResetVisual();
        }
    }

    // Static method to deselect any currently selected card
    public static void DeselectCurrentCard()
    {
        if (currentlySelectedCard != null)
        {
            currentlySelectedCard.Deselect();
        }
    }

    private void PlayCardImmediately(Card cardData)
    {
        Debug.Log($"[CardMovement] Playing card immediately: {cardData.cardName} (type: {cardData.targetType})");
        
        // Check stamina
        if (PlayerStamina.Instance != null && !PlayerStamina.Instance.Spend(cardData.staminaCost))
        {
            Debug.Log($"[CardMovement] Not enough stamina to play {cardData.cardName} (cost: {cardData.staminaCost})");
            ResetVisual();
            return;
        }

        // For Self-targeting cards, directly resolve on player
        if (cardData.targetType == TargetType.Self && TargetingSystem.Instance != null)
        {
            Debug.Log($"[CardMovement] Self-targeting card, playing on player directly");
            TargetingSystem.Instance.BeginTargeting(cardData, gameObject, null);
            TargetingSystem.Instance.ResolveCard(cardData, null, PlayerHealth.Instance);
            BattleEvents.RaiseCardResolved(gameObject);
            ResetVisual();
            return;
        }

        // For other non-targeted cards (None, AllEnemies, AllAllies)
        if (TargetingSystem.Instance != null)
        {
            TargetingSystem.Instance.BeginTargeting(cardData, gameObject, null);
            
            // Use dummy position for cards that don't need specific targets
            Vector2 dummyPos = new Vector2(Screen.width / 2, Screen.height / 2);
            TargetingSystem.Instance.TryTargetAtScreenPoint(dummyPos, Camera.main);
        }
        
        ResetVisual();
    }
}
