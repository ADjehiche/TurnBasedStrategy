using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardGame;

/// <summary>
/// Manages card merging/crafting system
/// Loads all recipes from Resources and handles merge validation/execution
/// CRITICAL: Only removes ONE copy of each ingredient card
/// </summary>
public class CardMergeManager : MonoBehaviour
{
    public static CardMergeManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private CardCollection cardCollection;
    
    [Header("Recipe Database")]
    private List<CardRecipe> allRecipes = new List<CardRecipe>();
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Find CardCollection if not assigned
        if (cardCollection == null)
        {
            cardCollection = CardCollection.Instance;
        }
    }
    
    private void Start()
    {
        LoadAllRecipes();
    }
    
    /// <summary>
    /// Load all card recipes from Resources/Recipes folder
    /// </summary>
    private void LoadAllRecipes()
    {
        CardRecipe[] recipes = Resources.LoadAll<CardRecipe>("Recipes");
        allRecipes.Clear();
        allRecipes.AddRange(recipes);
        
        // Validate all recipes
        int validCount = 0;
        foreach (var recipe in allRecipes)
        {
            if (recipe.IsValid())
                validCount++;
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"[CardMergeManager] Loaded {validCount}/{allRecipes.Count} valid recipes");
        }
    }
    
    /// <summary>
    /// Get all available recipes (regardless of whether player can craft)
    /// </summary>
    public List<CardRecipe> GetAllRecipes()
    {
        return new List<CardRecipe>(allRecipes);
    }
    
    /// <summary>
    /// Get only recipes that player can currently craft
    /// </summary>
    public List<CardRecipe> GetCraftableRecipes()
    {
        if (cardCollection == null)
        {
            Debug.LogError("[CardMergeManager] CardCollection is null!");
            return new List<CardRecipe>();
        }
        
        return allRecipes.Where(r => r.CanCraft(cardCollection)).ToList();
    }
    
    /// <summary>
    /// Attempt to merge two cards using a specific recipe
    /// CRITICAL: Only removes ONE copy of each ingredient
    /// </summary>
    public bool TryMergeCards(CardRecipe recipe)
    {
        if (recipe == null || !recipe.IsValid())
        {
            Debug.LogError("[CardMergeManager] Invalid recipe!");
            return false;
        }
        
        if (cardCollection == null)
        {
            Debug.LogError("[CardMergeManager] CardCollection is null!");
            return false;
        }
        
        // Validate player has the required cards
        if (!recipe.CanCraft(cardCollection))
        {
            Debug.LogWarning($"[CardMergeManager] Cannot craft '{recipe.result.cardName}' - missing ingredients");
            return false;
        }
        
        // Remove ONE copy of each ingredient card
        bool removed1 = RemoveOneCard(recipe.ingredient1);
        bool removed2 = RemoveOneCard(recipe.ingredient2);
        
        if (!removed1 || !removed2)
        {
            Debug.LogError($"[CardMergeManager] Failed to remove ingredient cards! Restore attempted.");
            
            // Attempt to restore if something went wrong
            if (removed1) cardCollection.AddCard(recipe.ingredient1);
            if (removed2) cardCollection.AddCard(recipe.ingredient2);
            
            return false;
        }
        
        // Add the result card
        cardCollection.AddCard(recipe.result);
        
        if (enableDebugLogs)
        {
            Debug.Log($"[CardMergeManager] ✨ Merged '{recipe.ingredient1.cardName}' + '{recipe.ingredient2.cardName}' = '{recipe.result.cardName}'");
        }
        
        // Play merge success audio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("CardMerge");
        }
        
        return true;
    }
    
    /// <summary>
    /// Remove ONE copy of a specific card from collection
    /// Returns true if successful
    /// </summary>
    private bool RemoveOneCard(Card card)
    {
        if (cardCollection.OwnedCards.Contains(card))
        {
            cardCollection.OwnedCards.Remove(card); // Removes only first occurrence
            
            if (enableDebugLogs)
            {
                int remaining = cardCollection.OwnedCards.Count(c => c == card);
                Debug.Log($"[CardMergeManager] Removed 1x '{card.cardName}' ({remaining} remaining)");
            }
            
            return true;
        }
        
        Debug.LogError($"[CardMergeManager] Failed to remove '{card.cardName}' - not found in collection!");
        return false;
    }
    
    /// <summary>
    /// Find recipe that matches two specific cards
    /// Order doesn't matter (Quick Slash + Stab = Stab + Quick Slash)
    /// </summary>
    public CardRecipe FindRecipe(Card card1, Card card2)
    {
        if (card1 == null || card2 == null)
            return null;
            
        return allRecipes.FirstOrDefault(r =>
            (r.ingredient1 == card1 && r.ingredient2 == card2) ||
            (r.ingredient1 == card2 && r.ingredient2 == card1)
        );
    }
    
    /// <summary>
    /// Get all recipes that use a specific card as ingredient
    /// </summary>
    public List<CardRecipe> GetRecipesUsingCard(Card card)
    {
        if (card == null)
            return new List<CardRecipe>();
            
        return allRecipes.Where(r =>
            r.ingredient1 == card || r.ingredient2 == card
        ).ToList();
    }
    
    /// <summary>
    /// Debug method to print all recipes
    /// </summary>
    [ContextMenu("Debug: Print All Recipes")]
    public void DebugPrintRecipes()
    {
        Debug.Log("=== CARD MERGE RECIPES ===");
        foreach (var recipe in allRecipes)
        {
            if (recipe.IsValid())
            {
                int craftable = recipe.GetCraftableCount(cardCollection);
                bool canCraft = recipe.CanCraft(cardCollection);
                Debug.Log($"{recipe.GetRecipeString()} | Craftable: {canCraft} ({craftable}x)");
            }
        }
        Debug.Log("===========================");
    }
    
    /// <summary>
    /// Reload recipes from Resources (for testing)
    /// </summary>
    [ContextMenu("Reload Recipes")]
    public void ReloadRecipes()
    {
        LoadAllRecipes();
    }
}
