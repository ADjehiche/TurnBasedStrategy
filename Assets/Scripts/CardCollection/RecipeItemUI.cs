using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame;
using System.Linq;

/// <summary>
/// UI component for individual recipe items in the merge panel scroll view
/// Displays recipe ingredients, result, and craftability status
/// </summary>
public class RecipeItemUI : MonoBehaviour
{
    [Header("Card Sprites")]
    [SerializeField] private Image ingredient1Image;
    [SerializeField] private Image ingredient2Image;
    [SerializeField] private Image resultImage;
    [SerializeField] private Image arrowIcon;
    
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI recipeName;
    [SerializeField] private TextMeshProUGUI ingredient1Count;
    [SerializeField] private TextMeshProUGUI ingredient2Count;
    [SerializeField] private TextMeshProUGUI lockedOverlay;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color craftableColor = new Color(0.2f, 0.8f, 0.3f, 0.3f);
    [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private GameObject newBadge;
    
    [Header("Hover Animation")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationSpeed = 10f;
    
    private CardRecipe recipe;
    private Button button;
    private Vector3 originalScale;
    private bool isHovering = false;
    private bool isCraftable = false;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        originalScale = transform.localScale;
    }
    
    private void Update()
    {
        // Smooth hover animation
        Vector3 targetScale = isHovering ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
    }
    
    /// <summary>
    /// Initialize the recipe item with data
    /// </summary>
    public void Setup(CardRecipe recipeData, System.Action<CardRecipe> onSelected)
    {
        recipe = recipeData;
        
        if (recipe == null || !recipe.IsValid())
        {
            Debug.LogError("[RecipeItemUI] Invalid recipe data!");
            return;
        }
        
        // Setup button click listener
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected?.Invoke(recipe));
        }
        
        // Update visuals
        UpdateVisuals();
    }
    
    /// <summary>
    /// Update all visual elements based on recipe and player's collection
    /// </summary>
    public void UpdateVisuals()
    {
        if (recipe == null) return;
        
        CardCollection collection = CardCollection.Instance;
        
        // Check if craftable
        isCraftable = recipe.CanCraft(collection);
        
        // Set card artwork
        if (ingredient1Image != null && recipe.ingredient1 != null)
        {
            ingredient1Image.sprite = recipe.ingredient1.artwork;
            ingredient1Image.enabled = recipe.ingredient1.artwork != null;
        }
        
        if (ingredient2Image != null && recipe.ingredient2 != null)
        {
            ingredient2Image.sprite = recipe.ingredient2.artwork;
            ingredient2Image.enabled = recipe.ingredient2.artwork != null;
        }
        
        if (resultImage != null && recipe.result != null)
        {
            resultImage.sprite = recipe.result.artwork;
            resultImage.enabled = recipe.result.artwork != null;
        }

        // If an arrow icon Image exists but has no sprite assigned, hide it to avoid white boxes
        if (arrowIcon != null && arrowIcon.sprite == null)
        {
            arrowIcon.enabled = false;
        }
        
        // Update counts
        if (collection != null)
        {
            int count1 = collection.OwnedCards.Count(c => c == recipe.ingredient1);
            int count2 = collection.OwnedCards.Count(c => c == recipe.ingredient2);
            
            if (ingredient1Count != null)
            {
                ingredient1Count.text = $"x{count1}";
                ingredient1Count.color = count1 > 0 ? Color.white : Color.red;
            }
            
            if (ingredient2Count != null)
            {
                ingredient2Count.text = $"x{count2}";
                ingredient2Count.color = count2 > 0 ? Color.white : Color.red;
            }
        }
        
        // Update recipe name
        if (recipeName != null && recipe.result != null)
        {
            recipeName.text = recipe.result.cardName;
        }
        
        // Visual feedback for craftable status
        UpdateCraftableStatus();
    }
    
    /// <summary>
    /// Update visual feedback based on craftability
    /// </summary>
    private void UpdateCraftableStatus()
    {
        // Background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isCraftable ? craftableColor : lockedColor;
        }
        
        // Glow effect
        if (glowEffect != null)
        {
            glowEffect.SetActive(isCraftable);
        }
        
        // Locked overlay text
        if (lockedOverlay != null)
        {
            lockedOverlay.gameObject.SetActive(!isCraftable);
            lockedOverlay.text = "LOCKED";
        }
        
        // Keep recipe items clickable even when not craftable.
        // The merge button in the details panel is what should be disabled.
        if (button != null)
        {
            button.interactable = true;
        }
        
        // Dim images if locked
        float alpha = isCraftable ? 1f : 0.5f;
        SetImageAlpha(ingredient1Image, alpha);
        SetImageAlpha(ingredient2Image, alpha);
        SetImageAlpha(resultImage, alpha);
        SetImageAlpha(arrowIcon, alpha);
    }
    
    /// <summary>
    /// Helper to set image alpha
    /// </summary>
    private void SetImageAlpha(Image img, float alpha)
    {
        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
    
    /// <summary>
    /// Show "NEW" badge for recently unlocked recipes
    /// </summary>
    public void ShowNewBadge(bool show)
    {
        if (newBadge != null)
        {
            newBadge.SetActive(show);
        }
    }
    
    // UI Event handlers
    public void OnPointerEnter()
    {
        isHovering = true;
    }
    
    public void OnPointerExit()
    {
        isHovering = false;
    }
    
    public CardRecipe GetRecipe()
    {
        return recipe;
    }
}
