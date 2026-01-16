using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CardGame;
using System.Linq;

/// <summary>
/// Displays detailed tooltips when hovering over cards in the merge UI
/// Shows card stats, effects, and additional information
/// </summary>
public class CardTooltipUI : MonoBehaviour
{
    public static CardTooltipUI Instance { get; private set; }
    
    [Header("Tooltip Panel")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Content")]
    [SerializeField] private Image cardArtwork;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardTypeText;
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    [SerializeField] private TextMeshProUGUI cardStatsText;
    [SerializeField] private Image rarityBorder;
    
    [Header("Settings")]
    [SerializeField] private float showDelay = 0.5f;
    [SerializeField] private float fadeSpeed = 10f;
    [SerializeField] private Vector2 offset = new Vector2(20f, -20f);
    [SerializeField] private bool followMouse = true;
    
    private Card currentCard;
    private bool isShowing = false;
    private float showTimer = 0f;
    private bool isVisible = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    private void Update()
    {
        // Handle show delay
        if (isShowing && !isVisible)
        {
            showTimer += Time.deltaTime;
            if (showTimer >= showDelay)
            {
                ShowTooltip();
            }
        }
        
        // Fade in/out
        if (canvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
        
        // Follow mouse
        if (isVisible && followMouse && tooltipRect != null)
        {
            UpdateTooltipPosition();
        }
    }
    
    /// <summary>
    /// Show tooltip for a specific card
    /// </summary>
    public void ShowCardTooltip(Card card)
    {
        if (card == null) return;
        
        currentCard = card;
        isShowing = true;
        showTimer = 0f;
    }
    
    /// <summary>
    /// Hide the tooltip
    /// </summary>
    public void HideTooltip()
    {
        isShowing = false;
        isVisible = false;
        showTimer = 0f;
        
        if (canvasGroup != null && canvasGroup.alpha <= 0.01f)
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Actually display the tooltip
    /// </summary>
    private void ShowTooltip()
    {
        if (currentCard == null) return;
        
        isVisible = true;
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
        }
        
        UpdateTooltipContent();
        UpdateTooltipPosition();
    }
    
    /// <summary>
    /// Update tooltip content with card data
    /// </summary>
    private void UpdateTooltipContent()
    {
        if (currentCard == null) return;
        
        // Card artwork
        if (cardArtwork != null)
        {
            cardArtwork.sprite = currentCard.artwork;
            cardArtwork.enabled = currentCard.artwork != null;
        }
        
        // Card name
        if (cardNameText != null)
        {
            cardNameText.text = currentCard.cardName;
        }
        
        // Card type
        if (cardTypeText != null)
        {
            cardTypeText.text = GetCardTypeString(currentCard.category);
            cardTypeText.color = GetCardTypeColor(currentCard.category);
        }
        
        // Card description
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = currentCard.description;
        }
        
        // Card stats (if applicable)
        if (cardStatsText != null)
        {
            cardStatsText.text = GetCardStatsString(currentCard);
        }
        
        // Rarity border color
        if (rarityBorder != null)
        {
            rarityBorder.color = GetCardTypeColor(currentCard.category);
        }
    }
    
    /// <summary>
    /// Update tooltip position relative to mouse
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (tooltipRect == null) return;
        
        Vector2 mousePosition = Input.mousePosition;
        Vector2 tooltipPosition = mousePosition + offset;
        
        // Keep tooltip on screen
        RectTransform canvasRect = tooltipRect.root.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            // Check right edge
            if (tooltipPosition.x + tooltipRect.rect.width > canvasRect.rect.width)
            {
                tooltipPosition.x = mousePosition.x - offset.x - tooltipRect.rect.width;
            }
            
            // Check bottom edge
            if (tooltipPosition.y - tooltipRect.rect.height < 0)
            {
                tooltipPosition.y = mousePosition.y - offset.y + tooltipRect.rect.height;
            }
            
            // Check left edge
            if (tooltipPosition.x < 0)
            {
                tooltipPosition.x = 10f;
            }
            
            // Check top edge
            if (tooltipPosition.y > canvasRect.rect.height)
            {
                tooltipPosition.y = canvasRect.rect.height - 10f;
            }
        }
        
        tooltipRect.position = tooltipPosition;
    }
    
    /// <summary>
    /// Get card type as readable string
    /// </summary>
    private string GetCardTypeString(CardCategory type)
    {
        switch (type)
        {
            case CardCategory.Attack:
                return "⚔️ Attack";
            case CardCategory.Defense:
                return "🛡️ Defense";
            case CardCategory.Tactical:
                return "✨ Tactical";
            case CardCategory.Utility:
                return "🔧 Utility";
            default:
                return type.ToString();
        }
    }
    
    /// <summary>
    /// Get color based on card type
    /// </summary>
    private Color GetCardTypeColor(CardCategory type)
    {
        switch (type)
        {
            case CardCategory.Attack:
                return new Color(1f, 0.3f, 0.3f); // Red
            case CardCategory.Defense:
                return new Color(0.3f, 0.5f, 1f); // Blue
            case CardCategory.Tactical:
                return new Color(1f, 0.84f, 0f); // Gold
            case CardCategory.Utility:
                return new Color(0.5f, 1f, 0.5f); // Green
            default:
                return Color.white;
        }
    }
    
    /// <summary>
    /// Get card stats formatted string
    /// </summary>
    private string GetCardStatsString(Card card)
    {
        if (card == null) return "";
        
        string stats = "";
        
        // Add card-specific stats based on your Card class
        // This is a placeholder - adjust based on your actual Card properties
        
        if (card.category == CardCategory.Attack)
        {
            // Example: stats += $"Damage: {card.damage}\n";
        }
        else if (card.category == CardCategory.Defense)
        {
            // Example: stats += $"Block: {card.blockAmount}\n";
        }
        
        // Check player's collection count
        if (CardCollection.Instance != null)
        {
            int count = CardCollection.Instance.OwnedCards.Count(c => c == card);
            stats += $"\n<color=#FFD700>Owned: {count}</color>";
        }
        
        return stats;
    }
    
    /// <summary>
    /// Static helper to show tooltip from anywhere
    /// </summary>
    public static void Show(Card card)
    {
        if (Instance != null)
        {
            Instance.ShowCardTooltip(card);
        }
    }
    
    /// <summary>
    /// Static helper to hide tooltip
    /// </summary>
    public static void Hide()
    {
        if (Instance != null)
        {
            Instance.HideTooltip();
        }
    }
}
