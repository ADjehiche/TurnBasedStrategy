using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardGame;
using System.Linq;

/// <summary>
/// Reusable UI component for displaying a card slot in the merge interface
/// Shows card artwork, name, count, and supports visual feedback
/// </summary>
public class CardSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card Display")]
    [SerializeField] private Image cardArtwork;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Image cardBackground;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardCountText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    
    [Header("Visual States")]
    [SerializeField] private GameObject emptySlotIndicator;
    [SerializeField] private GameObject highlightEffect;
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Image rarityGlow;
    
    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private Color filledColor = Color.white;
    [SerializeField] private Color insufficientColor = new Color(1f, 0.3f, 0.3f, 0.7f);
    [SerializeField] private Color sufficientColor = new Color(0.3f, 1f, 0.3f, 0.7f);
    
    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseAmount = 0.1f;
    
    private Card currentCard;
    private int cardCount;
    private bool isEmpty = true;
    private bool isInsufficient = false;
    private Vector3 originalScale;
    
    private void Awake()
    {
        originalScale = transform.localScale;
        ClearSlot();
    }
    
    private void Update()
    {
        // Pulse animation when insufficient
        if (isInsufficient && !isEmpty)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
        else
        {
            transform.localScale = originalScale;
        }
    }
    
    /// <summary>
    /// Set the card to display in this slot
    /// </summary>
    public void SetCard(Card card, int count = -1)
    {
        currentCard = card;
        isEmpty = card == null;
        
        // Auto-detect count from collection if not provided
        if (count < 0 && card != null && CardCollection.Instance != null)
        {
            cardCount = CardCollection.Instance.OwnedCards.Count(c => c == card);
        }
        else
        {
            cardCount = count;
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Clear the slot (show empty state)
    /// </summary>
    public void ClearSlot()
    {
        currentCard = null;
        cardCount = 0;
        isEmpty = true;
        isInsufficient = false;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Mark this slot as insufficient (player doesn't have this card)
    /// </summary>
    public void SetInsufficient(bool insufficient)
    {
        isInsufficient = insufficient;
        UpdateVisuals();
    }
    
    /// <summary>
    /// Update all visual elements
    /// </summary>
    private void UpdateVisuals()
    {
        if (isEmpty)
        {
            ShowEmptyState();
        }
        else
        {
            ShowFilledState();
        }
    }
    
    /// <summary>
    /// Show empty slot state
    /// </summary>
    private void ShowEmptyState()
    {
        // Hide card elements
        if (cardArtwork != null)
        {
            cardArtwork.enabled = false;
        }
        
        if (cardNameText != null)
        {
            cardNameText.text = "Empty Slot";
            cardNameText.color = emptyColor;
        }
        
        if (cardCountText != null)
        {
            cardCountText.gameObject.SetActive(false);
        }
        
        if (cardDescriptionText != null)
        {
            cardDescriptionText.gameObject.SetActive(false);
        }
        
        // Show empty indicator
        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(true);
        }
        
        // Background
        if (cardBackground != null)
        {
            cardBackground.color = emptyColor;
        }
        
        // Disable effects
        if (highlightEffect != null) highlightEffect.SetActive(false);
        if (glowEffect != null) glowEffect.SetActive(false);
        if (rarityGlow != null) rarityGlow.enabled = false;
    }
    
    /// <summary>
    /// Show filled slot with card data
    /// </summary>
    private void ShowFilledState()
    {
        if (currentCard == null) return;
        
        // Show card artwork
        if (cardArtwork != null)
        {
            cardArtwork.sprite = currentCard.artwork;
            cardArtwork.enabled = currentCard.artwork != null;
        }
        
        // Card name
        if (cardNameText != null)
        {
            cardNameText.text = currentCard.cardName;
            cardNameText.color = filledColor;
        }
        
        // Card count
        if (cardCountText != null)
        {
            cardCountText.gameObject.SetActive(true);
            cardCountText.text = $"x{cardCount}";
            
            // Color based on availability
            if (cardCount <= 0)
            {
                cardCountText.color = insufficientColor;
            }
            else
            {
                cardCountText.color = sufficientColor;
            }
        }
        
        // Card description (optional)
        if (cardDescriptionText != null && !string.IsNullOrEmpty(currentCard.description))
        {
            cardDescriptionText.gameObject.SetActive(true);
            cardDescriptionText.text = currentCard.description;
        }
        
        // Hide empty indicator
        if (emptySlotIndicator != null)
        {
            emptySlotIndicator.SetActive(false);
        }
        
        // Background color based on sufficiency
        if (cardBackground != null)
        {
            if (isInsufficient || cardCount <= 0)
            {
                cardBackground.color = insufficientColor;
            }
            else
            {
                cardBackground.color = filledColor;
            }
        }
        
        // Glow based on card rarity (if applicable)
        SetRarityGlow();
        
        // Highlight if sufficient
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(cardCount > 0 && !isInsufficient);
        }
    }
    
    /// <summary>
    /// Set glow effect based on card rarity
    /// </summary>
    private void SetRarityGlow()
    {
        if (rarityGlow == null || currentCard == null) return;
        
        // Check if card has rarity field (adjust based on your Card class)
        // For now, using a simple detection based on card category
        Color glowColor = Color.white;
        
        // You can customize this based on your card rarity system
        if (currentCard.category == CardCategory.Tactical)
        {
            glowColor = new Color(1f, 0.84f, 0f); // Gold
        }
        else if (currentCard.category == CardCategory.Attack)
        {
            glowColor = new Color(1f, 0.3f, 0.3f); // Red
        }
        else if (currentCard.category == CardCategory.Defense)
        {
            glowColor = new Color(0.3f, 0.3f, 1f); // Blue
        }
        
        rarityGlow.color = glowColor;
        rarityGlow.enabled = !isEmpty;
    }
    
    /// <summary>
    /// Trigger a shine/flash effect (for merge success)
    /// </summary>
    public void PlayShineEffect()
    {
        if (glowEffect != null)
        {
            glowEffect.SetActive(true);
            // Disable after delay
            Invoke(nameof(DisableGlow), 1f);
        }
    }
    
    private void DisableGlow()
    {
        if (glowEffect != null)
        {
            glowEffect.SetActive(false);
        }
    }
    
    /// <summary>
    /// Get the current card
    /// </summary>
    public Card GetCard()
    {
        return currentCard;
    }
    
    /// <summary>
    /// Get the card count
    /// </summary>
    public int GetCount()
    {
        return cardCount;
    }
    
    // IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEmpty && highlightEffect != null)
        {
            highlightEffect.SetActive(true);
        }
        
        // Play hover sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("UIHover");
        }
    }
    
    // IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEmpty && highlightEffect != null && (cardCount <= 0 || isInsufficient))
        {
            highlightEffect.SetActive(false);
        }
    }
}
