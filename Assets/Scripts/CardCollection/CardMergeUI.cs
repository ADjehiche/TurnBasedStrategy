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

    [Header("Cursor")]
    [Tooltip("If true, closes will lock/hide the cursor during gameplay. If the game is paused (Time.timeScale == 0), the cursor will remain unlocked/visible regardless.")]
    [SerializeField] private bool lockCursorOnCloseWhenNotPaused = true;
    
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
        
        // Make sure selection panel is hidden
        if (selectedRecipePanel != null)
        {
            selectedRecipePanel.SetActive(false);
        }
        
        selectedRecipe = null;
        
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
        
        // Cursor handling:
        // - If paused (common for menus), keep cursor available.
        // - Otherwise restore gameplay cursor lock (optional).
        bool isPaused = Time.timeScale <= 0.0001f;
        bool shouldLock = !isPaused && lockCursorOnCloseWhenNotPaused;

        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
        
        // Unlock player movement
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.UnlockMovement("CardMerge");
        }
        
        selectedRecipe = null;
        
        Debug.Log("[CardMergeUI] Merge panel closed");
    }
    
    /// <summary>
    /// Ensure the recipe list container has proper layout components for scrolling
    /// Also positions the ScrollView (or container) to avoid overlapping main buttons
    /// </summary>
    private void EnsureLayoutComponents()
    {
        if (recipeListContainer == null)
            return;
        
        // 1. Identify the root layout object to position
        // This should be the child of "MergePanel" that leads to our list items.
        // We walk up the tree until we find the transform just below 'mergePanel'.
        RectTransform targetRect = null;
        
        if (mergePanel != null)
        {
            Transform current = recipeListContainer;
            while (current.parent != null && current.parent != mergePanel.transform)
            {
                current = current.parent;
            }
            
            // If current.parent is mergePanel, then 'current' is the object we want to resize (e.g. Scroll View)
            if (current.parent == mergePanel.transform)
            {
                targetRect = current.GetComponent<RectTransform>();
                Debug.Log($"[CardMergeUI] Identified root list object: {targetRect.name}");
            }
        }
        
        // Fallback: If logic failed, try finding ScrollRect
        if (targetRect == null)
        {
            var scrollRect = recipeListContainer.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                targetRect = scrollRect.GetComponent<RectTransform>();
            }
        }
        
        // 2. Apply Safe Zone Anchors to the target
        if (targetRect != null)
        {
            // SAFE ZONE CONFIGURATION:
            // Left (X Min): 0.4 (40%) - Clears left navigation column
            // Right (X Max): 0.95 (95%) - Slight padding from screen edge
            // Bottom (Y Min): 0.2 (20%) - Clears 'Done' button area
            // Top (Y Max): 0.85 (85%) - Clears Header area
            
            targetRect.anchorMin = new Vector2(0.4f, 0.2f);
            targetRect.anchorMax = new Vector2(0.95f, 0.85f);
            
            // Important: Reset offsets so the rect snaps effectively to the anchors
            targetRect.offsetMin = Vector2.zero;
            targetRect.offsetMax = Vector2.zero;
            
            // Ensure pivot is standard
            targetRect.pivot = new Vector2(0.5f, 0.5f);
            
            Debug.Log("[CardMergeUI] Applied Safe Zone Anchors to List Container");
        }
        else
        {
            Debug.LogError("[CardMergeUI] Could not find root container to resize! List may overlap UI.");
        }

        // 3. Ensure Content RectTransform is properly configured for vertical list
        var contentRect = recipeListContainer.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            // Anchor to top-stretch of the viewport
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
        }
        
        // 4. Add VerticalLayoutGroup
        var layoutGroup = recipeListContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = recipeListContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        // Configure layout settings
        layoutGroup.childControlHeight = false; // Allow items to set their own height
        layoutGroup.childControlWidth = true;   // Force items to expand width-wise
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 20f; // Wide spacing
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        
        // 5. Add ContentSizeFitter
        var sizeFitter = recipeListContainer.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = recipeListContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        
        // Configure size fitter
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }
    
    /// <summary>
    /// Refresh the list of available recipes
    /// </summary>
    private void RefreshRecipeList()
    {
        if (recipeListContainer == null || recipeItemPrefab == null)
        {
            Debug.LogError("[CardMergeUI] Missing references. Assign 'recipeListContainer' to ScrollView/Viewport/Content and assign 'recipeItemPrefab' to a prefab asset (not a scene object).");
            if (titleText != null)
            {
                titleText.text = "Card Merge (UI not wired)";
            }
            return;
        }

        // Ensure the recipeListContainer has a VerticalLayoutGroup for proper layout
        EnsureLayoutComponents();

        // Clear existing items
        foreach (var item in recipeItems)
        {
            if (item != null)
                Destroy(item);
        }
        recipeItems.Clear();
        
        // Get all recipes
        List<CardRecipe> recipes = mergeManager.GetAllRecipes();
        
        if (recipes.Count == 0)
        {
            Debug.Log("[CardMergeUI] No recipes to display");
            if (titleText != null)
            {
                titleText.text = "No Recipes Loaded";
            }
            return;
        }
        
        // Update title
        if (titleText != null)
        {
            titleText.text = $"Card Merge ({recipes.Count} Recipe{(recipes.Count != 1 ? "s" : "")} Available)";
        }
        
        int createdCount = 0;

        // Create recipe items
        foreach (var recipe in recipes)
        {
            if (recipe == null || !recipe.IsValid())
                continue;

            if (CreateRecipeItem(recipe))
            {
                createdCount++;
            }
        }

        Debug.Log($"[CardMergeUI] Created {createdCount} recipe UI items (recipes loaded: {recipes.Count})");
        
        // Force layout rebuild
        Canvas.ForceUpdateCanvases();
        if (recipeListContainer != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(recipeListContainer.GetComponent<RectTransform>());
        }
    }
    
    /// <summary>
    /// Create a recipe item in the scroll view
    /// </summary>
    private bool CreateRecipeItem(CardRecipe recipe)
    {
        GameObject item = Instantiate(recipeItemPrefab, recipeListContainer);
        recipeItems.Add(item);
        
        // Configure RectTransform
        var rectTransform = item.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.localScale = Vector3.one;
        }
        
        // Add LayoutElement to control sizing
        var layoutElement = item.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = item.AddComponent<LayoutElement>();
        }
        layoutElement.preferredHeight = 80f;
        layoutElement.minHeight = 60f;
        layoutElement.flexibleHeight = 0f;
        
        // Try to use RecipeItemUI component if available
        var recipeItemUI = item.GetComponent<RecipeItemUI>();
        if (recipeItemUI != null)
        {
            recipeItemUI.Setup(recipe, OnRecipeSelected);
        }
        else
        {
            // Fallback to old setup method
            SetupRecipeItemFallback(item, recipe);
        }

        return true;
    }
    
    /// <summary>
    /// Fallback setup method for recipe items without RecipeItemUI component
    /// </summary>
    private void SetupRecipeItemFallback(GameObject item, CardRecipe recipe)
    {
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
        
        // Only allow interaction with craftable recipes
        bool canCraft = recipe.CanCraft(CardCollection.Instance);
        if (button != null)
        {
            button.interactable = canCraft;
            if (!canCraft)
            {
                var colors = button.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Grayed out
                button.colors = colors;
            }
        }
    }
    
    /// <summary>
    /// Called when player selects a recipe from the list
    /// Only craftable recipes can be selected
    /// </summary>
    private void OnRecipeSelected(CardRecipe recipe)
    {
        if (recipe == null)
            return;
        
        // Only allow selection of craftable recipes
        if (!recipe.CanCraft(CardCollection.Instance))
        {
            Debug.LogWarning($"[CardMergeUI] Cannot select non-craftable recipe: {recipe.GetRecipeString()}");
            return;
        }
            
        selectedRecipe = recipe;
        
        // Show selected recipe panel
        if (selectedRecipePanel != null)
        {
            selectedRecipePanel.SetActive(true);
        }
        
        // Update ingredient 1
        if (ingredient1Image != null)
        {
            var sprite = recipe.ingredient1 != null ? recipe.ingredient1.artwork : null;
            ingredient1Image.sprite = sprite;
            ingredient1Image.enabled = sprite != null;
        }
        if (ingredient1Text != null && recipe.ingredient1 != null)
        {
            int count = CardCollection.Instance.OwnedCards.Count(c => c == recipe.ingredient1);
            ingredient1Text.text = recipe.ingredient1.cardName;
            ingredient1Text.color = count > 0 ? Color.white : Color.red;
        }
        
        // Update ingredient 2
        if (ingredient2Image != null)
        {
            var sprite = recipe.ingredient2 != null ? recipe.ingredient2.artwork : null;
            ingredient2Image.sprite = sprite;
            ingredient2Image.enabled = sprite != null;
        }
        if (ingredient2Text != null && recipe.ingredient2 != null)
        {
            int count = CardCollection.Instance.OwnedCards.Count(c => c == recipe.ingredient2);
            ingredient2Text.text = recipe.ingredient2.cardName;
            ingredient2Text.color = count > 0 ? Color.white : Color.red;
        }
        
        // Update result
        if (resultImage != null)
        {
            var sprite = recipe.result != null ? recipe.result.artwork : null;
            resultImage.sprite = sprite;
            resultImage.enabled = sprite != null;
        }
        if (resultText != null && recipe.result != null)
        {
            resultText.text = recipe.result.cardName;
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
            
            // Refresh the recipe list to update craftable states
            RefreshRecipeList();
            
            // Hide selection panel
            if (selectedRecipePanel != null)
            {
                selectedRecipePanel.SetActive(false);
            }
            selectedRecipe = null;
        }
        else
        {
            Debug.LogWarning($"[CardMergeUI] Failed to merge: {selectedRecipe.GetRecipeString()}");
        }
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