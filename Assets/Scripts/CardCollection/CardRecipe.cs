using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardGame;

/// <summary>
/// Defines a card merge recipe - 2 input cards combine to create 1 output card
/// Each input card is consumed (removed from collection) when merging
/// CRITICAL: Takes only ONE of each card (not all copies)
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Card Game/Card Recipe")]
public class CardRecipe : ScriptableObject
{
    [Header("Recipe Definition")]
    [Tooltip("First card required for this recipe")]
    public Card ingredient1;
    
    [Tooltip("Second card required for this recipe")]
    public Card ingredient2;
    
    [Tooltip("The resulting merged card")]
    public Card result;
    
    [Header("Display")]
    [Tooltip("Description shown in UI (e.g., 'Combine offense and bleed')")]
    [TextArea]
    public string recipeDescription;
    
    /// <summary>
    /// Check if player has the required cards in their collection
    /// </summary>
    public bool CanCraft(CardCollection collection)
    {
        if (collection == null || ingredient1 == null || ingredient2 == null)
            return false;
            
        // Check if player owns at least 1 copy of each ingredient
        bool hasIngredient1 = collection.OwnedCards.Contains(ingredient1);
        bool hasIngredient2 = collection.OwnedCards.Contains(ingredient2);
        
        return hasIngredient1 && hasIngredient2;
    }
    
    /// <summary>
    /// Get count of how many times this recipe can be crafted
    /// Based on minimum count of ingredient cards
    /// </summary>
    public int GetCraftableCount(CardCollection collection)
    {
        if (collection == null || ingredient1 == null || ingredient2 == null)
            return 0;
            
        int count1 = collection.OwnedCards.Count(c => c == ingredient1);
        int count2 = collection.OwnedCards.Count(c => c == ingredient2);
        
        // For recipes with same ingredient (e.g., Quick Slash + Quick Slash)
        if (ingredient1 == ingredient2)
        {
            return count1 / 2; // Need 2 copies
        }
        
        return Mathf.Min(count1, count2);
    }
    
    /// <summary>
    /// Validate recipe integrity (for debugging)
    /// </summary>
    public bool IsValid()
    {
        if (ingredient1 == null || ingredient2 == null || result == null)
        {
            Debug.LogWarning($"[CardRecipe] Invalid recipe '{name}': Missing ingredient or result cards");
            return false;
        }
        
        // Check if result is actually a merge-only card
        if (result.canAppearInStartingDecks || result.isStarterCard)
        {
            Debug.LogWarning($"[CardRecipe] Recipe '{name}' produces '{result.cardName}' which is not marked as merge-only!");
        }
        
        return true;
    }
    
    /// <summary>
    /// Get formatted recipe string for UI display
    /// </summary>
    public string GetRecipeString()
    {
        if (ingredient1 == null || ingredient2 == null || result == null)
            return "Invalid Recipe";
            
        return $"{ingredient1.cardName} + {ingredient2.cardName} = {result.cardName}";
    }
}
