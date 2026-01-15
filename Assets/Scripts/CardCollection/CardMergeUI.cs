using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using CardGame;

/// <summary>
/// UI for card merging/crafting system
/// Shows available recipes and allows player to merge cards
/// CRITICAL: Only consumes ONE copy of each ingredient card
/// </summary>
public class CardMergeUI : MonoBehaviour
{
    public static CardMergeUI Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject mergePanel;
    [SerializeField] private Transform recipeListContainer;
    [SerializeField] private GameObject recipeItemPrefab;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button closeButton;
    
    [Header("Selected Recipe Display")]
    [SerializeField] private GameObject selectedRecipePanel;
    [SerializeField] private Image ingredient1Image;
    [SerializeField] private Image ingredient2Image;
    [SerializeField] private Image resultImage;
    [SerializeField] private TextMeshProUGUI ingredient1Text;
    [SerializeField] private TextMeshProUGUI ingredient2Text;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI recipeDescriptionText;
    [SerializeField] private Button mergeButton;
    
    [Header("Settings")]
    [SerializeField] private bool showOnlyAvailable = true;
    
    private CardMergeManager mergeManager;
    private CardRecipe selectedRecipe;
    private List<GameObject> recipeItems = new List<GameObject>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        mergeManager = CardMergeManager.Instance;
        
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
        
        if (selectedRecipePanel != null)
        {
            selectedRecipePanel.SetActive(false);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(ClosePanel);
        }
        
        if (mergeButton != null)
        {
            mergeButton.onClick.RemoveAllListeners();
            mergeButton.onClick.AddListener(OnMergeButtonClicked);
        }
    }
    
    /// <summary>
    /// Open the merge panel and display recipes
    /// </summary>
    public void OpenMergePanel()
    {
        if (mergePanel == null)
        {
            Debug.LogError("[CardMergeUI] Merge panel not assigned!");
            return;
        }
        
        if (mergeManager == null)
        {
            mergeManager = CardMergeManager.Instance;
            if (mergeManager == null)
            {
                Debug.LogError("[CardMergeUI] CardMergeManager not found!");
                return;
            }
        }
        
        // Show panel
        mergePanel.SetActive(true);
        
        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Lock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("CardMerge");
        }
        
        // Refresh recipe list
        RefreshRecipeList();
        
        Debug.Log("[CardMergeUI] Merge panel opened");
    }
    
    /// <summary>
    /// Close the merge panel
    /// </summary>
    public void ClosePanel()
    {
        if (mergePanel != null)
        {
            mergePanel.SetActive(false);
        }
        
        if (selectedRecipePanel != null)
        {
            selectedRecipePanel.SetActive(false);
        }
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Unlock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("CardMerge");
        }
        
        selectedRecipe = null;
        
        Debug.Log("[CardMergeUI] Merge panel closed");
    }
    
    /// <summary>
    /// Refresh the list of available recipes
    /// </summary>
    private void RefreshRecipeList()
    {
        // Clear existing items
        foreach (var item in recipeItems)
        {
            if (item != null)
                Destroy(item);
        }
        recipeItems.Clear();
        
        // Get recipes to display
        List<CardRecipe> recipes = showOnlyAvailable 
            ? mergeManager.GetCraftableRecipes() 
            : mergeManager.GetAllRecipes();
        
        if (recipes.Count == 0)
        {
            Debug.Log("[CardMergeUI] No recipes to display");
            if (titleText != null)
            {
                titleText.text = showOnlyAvailable 
                    ? "No Recipes Available (Collect More Cards!)" 
                    : "No Recipes Loaded";
            }
            return;
        }
        
        // Update title
        if (titleText != null)
        {
            titleText.text = $"Card Merge ({recipes.Count} Recipe{(recipes.Count != 1 ? "s" : "")} Available)";
        }
        
        // Create recipe items
        foreach (var recipe in recipes)
        {
            if (recipe == null || !recipe.IsValid())
                continue;
                
            CreateRecipeItem(recipe);
        }
        
        Debug.Log($"[CardMergeUI] Displayed {recipes.Count} recipes");
    }
    
    /// <summary>
    /// Create a recipe item in the scroll view
    /// </summary>
    private void CreateRecipeItem(CardRecipe recipe)
    {
        if (recipeItemPrefab == null || recipeListContainer == null)
        {
            Debug.LogError("[CardMergeUI] Recipe item prefab or container not assigned!");
            return;
        }
        
        GameObject item = Instantiate(recipeItemPrefab, recipeListContainer);
        recipeItems.Add(item);
        
        // Set recipe data (assumes prefab has specific components)
        var button = item.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnRecipeSelected(recipe));
        }
        
        // Find text components (customize based on your prefab structure)
        var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
        if (texts.Length > 0)
        {
            // First text = recipe name/description
            texts[0].text = recipe.GetRecipeString();
        }
        
        // Find images for ingredient/result preview (customize based on prefab)
        var images = item.GetComponentsInChildren<Image>();
        if (images.Length >= 3)
        {
            if (recipe.ingredient1 != null && recipe.ingredient1.artwork != null)
                images[0].sprite = recipe.ingredient1.artwork;
            if (recipe.ingredient2 != null && recipe.ingredient2.artwork != null)
                images[1].sprite = recipe.ingredient2.artwork;
            if (recipe.result != null && recipe.result.artwork != null)
                images[2].sprite = recipe.result.artwork;
        }
        
        // Visual feedback for craftable vs locked
        bool canCraft = recipe.CanCraft(CardCollection.Instance);
        if (!canCraft && button != null)
        {
            var colors = button.colors;
            colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Grayed out
            button.colors = colors;
        }
    }
    
    /// <summary>
    /// Called when player selects a recipe from the list
    /// </summary>
    private void OnRecipeSelected(CardRecipe recipe)
    {
        if (recipe == null)
            return;
            
        selectedRecipe = recipe;
        
        // Show selected recipe panel
        if (selectedRecipePanel != null)
        {
            selectedRecipePanel.SetActive(true);
        }
        
        // Update ingredient 1
        if (ingredient1Image != null && recipe.ingredient1 != null)
        {
            ingredient1Image.sprite = recipe.ingredient1.artwork;
        }
        if (ingredient1Text != null && recipe.ingredient1 != null)
        {
            int count = CardCollection.Instance.OwnedCards.Count(c => c == recipe.ingredient1);
            ingredient1Text.text = $"{recipe.ingredient1.cardName}\n(x{count})";
        }
        
        // Update ingredient 2
        if (ingredient2Image != null && recipe.ingredient2 != null)
        {
            ingredient2Image.sprite = recipe.ingredient2.artwork;
        }
        if (ingredient2Text != null && recipe.ingredient2 != null)
        {
            int count = CardCollection.Instance.OwnedCards.Count(c => c == recipe.ingredient2);
            ingredient2Text.text = $"{recipe.ingredient2.cardName}\n(x{count})";
        }
        
        // Update result
        if (resultImage != null && recipe.result != null)
        {
            resultImage.sprite = recipe.result.artwork;
        }
        if (resultText != null && recipe.result != null)
        {
            resultText.text = $"{recipe.result.cardName}\n{recipe.result.description}";
        }
        
        // Update description
        if (recipeDescriptionText != null)
        {
            recipeDescriptionText.text = string.IsNullOrEmpty(recipe.recipeDescription) 
                ? "Combine two cards to create something new!" 
                : recipe.recipeDescription;
        }
        
        // Update merge button
        if (mergeButton != null)
        {
            bool canCraft = recipe.CanCraft(CardCollection.Instance);
            mergeButton.interactable = canCraft;
            
            var buttonText = mergeButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canCraft ? "MERGE CARDS" : "MISSING INGREDIENTS";
            }
        }
        
        Debug.Log($"[CardMergeUI] Selected recipe: {recipe.GetRecipeString()}");
    }
    
    /// <summary>
    /// Called when player clicks the merge button
    /// </summary>
    private void OnMergeButtonClicked()
    {
        if (selectedRecipe == null)
        {
            Debug.LogWarning("[CardMergeUI] No recipe selected!");
            return;
        }
        
        // Attempt merge
        bool success = mergeManager.TryMergeCards(selectedRecipe);
        
        if (success)
        {
            Debug.Log($"[CardMergeUI] ✨ Successfully merged: {selectedRecipe.GetRecipeString()}");
            
            // Refresh UI
            RefreshRecipeList();
            
            // Clear selection or update it
            if (selectedRecipe.CanCraft(CardCollection.Instance))
            {
                // Can still craft - update counts
                OnRecipeSelected(selectedRecipe);
            }
            else
            {
                // Can't craft anymore - hide selection panel
                if (selectedRecipePanel != null)
                {
                    selectedRecipePanel.SetActive(false);
                }
                selectedRecipe = null;
            }
        }
        else
        {
            Debug.LogWarning($"[CardMergeUI] Failed to merge: {selectedRecipe.GetRecipeString()}");
        }
    }
    
    /// <summary>
    /// Toggle between showing all recipes vs only available ones
    /// </summary>
    public void ToggleShowOnlyAvailable(bool value)
    {
        showOnlyAvailable = value;
        RefreshRecipeList();
    }
    
    /// <summary>
    /// Static helper to open merge panel from anywhere
    /// </summary>
    public static void OpenMerge()
    {
        if (Instance != null)
        {
            Instance.OpenMergePanel();
        }
        else
        {
            Debug.LogError("[CardMergeUI] Instance not found! Add CardMergeUI to your scene.");
        }
    }
}
