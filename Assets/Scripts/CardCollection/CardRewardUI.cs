using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame;

/// <summary>
/// Manages the post-battle card reward selection UI
/// Displays 2 random cards for the player to choose from
/// </summary>
public class CardRewardUI : MonoBehaviour
{
    public static CardRewardUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject cardPrefab; // Same prefab used in HandManager
    [SerializeField] private Transform cardOptionsContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button skipButton;

    [Header("Card Display Settings")]
    [SerializeField] private float cardScale = 1.2f; // Make reward cards bigger than hand cards
    [SerializeField] private float cardSpacing = 400f; // Space between cards

    [Header("Card Display")]
    private List<GameObject> currentCardDisplays = new List<GameObject>();
    private List<Card> currentOptions = new List<Card>();
    private bool isExplorationReward = false; // Track if this is exploration or battle reward
    private bool keyboardShortcutsEnabled = false; // Track if keyboard shortcuts are active

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Keyboard shortcuts for card selection (workaround for Level Two click issues)
        if (keyboardShortcutsEnabled && rewardPanel != null && rewardPanel.activeSelf)
        {
            // Press 1 to select first card
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                if (currentOptions.Count > 0)
                {
                    Debug.Log("[CardRewardUI] ⌨️ Keyboard shortcut: Selected card 1");
                    OnCardSelected(currentOptions[0]);
                }
            }
            // Press 2 to select second card
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                if (currentOptions.Count > 1)
                {
                    Debug.Log("[CardRewardUI] ⌨️ Keyboard shortcut: Selected card 2");
                    OnCardSelected(currentOptions[1]);
                }
            }
            // Press Escape to skip
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("[CardRewardUI] ⌨️ Keyboard shortcut: Skipped reward");
                OnSkipReward();
            }
        }
    }

    private void Start()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipReward);
        }

        // Setup the layout group for proper card spacing
        SetupCardContainerLayout();
    }

    /// <summary>
    /// Configure the card container with proper layout settings
    /// </summary>
    private void SetupCardContainerLayout()
    {
        if (cardOptionsContainer == null) return;

        // Force the container to have proper size
        RectTransform containerRect = cardOptionsContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            // Set anchors to center
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Force a good size for 2 cards side-by-side
            containerRect.sizeDelta = new Vector2(700f, 450f); // Width x Height
            containerRect.anchoredPosition = Vector2.zero; // Center it
            
            Debug.Log($"[CardRewardUI] Container size forced to: {containerRect.sizeDelta}");
        }

        // Add HorizontalLayoutGroup if not present
        var layoutGroup = cardOptionsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = cardOptionsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        // Configure spacing and alignment
        layoutGroup.spacing = cardSpacing;
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        // Add padding around the cards
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);

        Debug.Log($"[CardRewardUI] Layout configured: spacing={cardSpacing}, scale={cardScale}");
    }

    /// <summary>
    /// Update card scale - can be called from inspector or code
    /// </summary>
    public void UpdateCardScale(float newScale)
    {
        cardScale = newScale;
        Debug.Log($"[CardRewardUI] Card scale updated to: {cardScale}");
        
        // Update existing displayed cards if any
        foreach (var cardObj in currentCardDisplays)
        {
            if (cardObj != null)
            {
                cardObj.transform.localScale = Vector3.one * cardScale;
            }
        }
    }

    /// <summary>
    /// Update card spacing - can be called from inspector or code
    /// </summary>
    public void UpdateCardSpacing(float newSpacing)
    {
        cardSpacing = newSpacing;
        Debug.Log($"[CardRewardUI] Card spacing updated to: {cardSpacing}");
        
        var layoutGroup = cardOptionsContainer?.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = cardSpacing;
        }
    }

    // Called when inspector values change in editor
    private void OnValidate()
    {
        // Update layout if in editor mode
        if (!Application.isPlaying) return;
        
        var layoutGroup = cardOptionsContainer?.GetComponent<HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = cardSpacing;
        }
        
        // Update scale on existing cards
        foreach (var cardObj in currentCardDisplays)
        {
            if (cardObj != null)
            {
                cardObj.transform.localScale = Vector3.one * cardScale;
            }
        }
    }

    /// <summary>
    /// Show the reward selection UI with 2 random card options
    /// </summary>
    public void ShowRewardSelection()
    {
        isExplorationReward = false; // This is a battle reward
        
        if (CardCollection.Instance == null)
        {
            Debug.LogError("[CardRewardUI] CardCollection.Instance is null!");
            return;
        }

        if (cardOptionsContainer == null)
        {
            Debug.LogError("[CardRewardUI] cardOptionsContainer is not assigned in inspector!");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("[CardRewardUI] cardPrefab is not assigned in inspector!");
            return;
        }

        Debug.Log("[CardRewardUI] Starting reward selection...");

        // Get 2 random reward cards
        currentOptions = CardCollection.Instance.GetRandomRewardOptions(2);

        if (currentOptions.Count < 2)
        {
            Debug.LogWarning("[CardRewardUI] Not enough cards for reward selection!");
            return;
        }

        Debug.Log($"[CardRewardUI] Got {currentOptions.Count} reward options: {currentOptions[0].cardName}, {currentOptions[1].cardName}");

        // Clear previous card DISPLAYS (but NOT currentOptions!)
        foreach (var cardDisplay in currentCardDisplays)
        {
            if (cardDisplay != null)
            {
                Destroy(cardDisplay);
            }
        }
        currentCardDisplays.Clear();

        // Hide the player's hand during reward selection
        if (HandManager.Instance != null)
        {
            HandManager.Instance.HideHand();
            Debug.Log("[CardRewardUI] Hand hidden during reward selection");
        }

        // Create card option displays
        try
        {
            Debug.Log($"[CardRewardUI] About to create {currentOptions.Count} card displays...");
            foreach (var card in currentOptions)
            {
                Debug.Log($"[CardRewardUI] Creating display for: {card.cardName}");
                GameObject cardDisplay = CreateCardOption(card);
                if (cardDisplay != null)
                {
                    currentCardDisplays.Add(cardDisplay);
                    Debug.Log($"[CardRewardUI] Successfully added card display for {card.cardName}");
                }
                else
                {
                    Debug.LogError($"[CardRewardUI] CreateCardOption returned null for {card.cardName}!");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CardRewardUI] Exception while creating card displays: {ex.Message}\n{ex.StackTrace}");
        }

        Debug.Log($"[CardRewardUI] Created {currentCardDisplays.Count} card displays");

        // Show the panel
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "Choose Your Reward";
        }

        Debug.Log("[CardRewardUI] Reward panel shown");
    }

    /// <summary>
    /// Create a clickable card option display (same method as HandManager)
    /// </summary>
    private GameObject CreateCardOption(Card card)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[CardRewardUI] Card prefab is not assigned!");
            return null;
        }

        Debug.Log($"[CardRewardUI] Creating card option for: {card.cardName}");

        // Instantiate the same card prefab used in battle (inactive to prevent OnEnable)
        GameObject cardObj = Instantiate(cardPrefab, cardOptionsContainer);
        cardObj.SetActive(false);

        // Scale the card to make it more prominent
        cardObj.transform.localScale = Vector3.one * cardScale;
        
        // Reset rotation (in case HandManager uses rotation)
        cardObj.transform.localRotation = Quaternion.identity;

        // Set the card data on CardInstance component BEFORE enabling (same as HandManager)
        var instance = cardObj.GetComponent<CardInstance>();
        if (instance != null)
        {
            instance.SetData(card);
            Debug.Log($"[CardRewardUI] Set data on CardInstance for {card.cardName}");
        }
        else
        {
            // Fallback
            var display = cardObj.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.cardData = card;
                Debug.Log($"[CardRewardUI] Set data on CardDisplay (fallback) for {card.cardName}");
            }
            else
            {
                Debug.LogError("[CardRewardUI] Card prefab is missing CardInstance and CardDisplay components!");
            }
        }

        // CRITICAL: Mark as reward card to disable click-to-play while keeping hover
        // Reward cards should only use Button component for selection, not battle card interactions
        var cardMovement = cardObj.GetComponent<CardMovement>();
        if (cardMovement != null)
        {
            cardMovement.isRewardCard = true; // Disable clicking but keep hover active
            Debug.Log($"[CardRewardUI] Marked {card.cardName} as reward card (hover enabled, click disabled)");
        }

        // Now enable the card (OnEnable will see the data)
        cardObj.SetActive(true);
        Debug.Log($"[CardRewardUI] Card GameObject enabled for {card.cardName}");

        // Manually refresh the display to ensure it's updated
        var cardDisplay = cardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.Refresh();
            Debug.Log($"[CardRewardUI] Manually refreshed CardDisplay for {card.cardName}");
        }

        // Make it clickable - add Button if not already present
        Button button = cardObj.GetComponent<Button>();
        if (button == null)
        {
            button = cardObj.AddComponent<Button>();
            // Setup button visuals
            button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 0.8f); // Light yellow
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f); // Light gray
            button.colors = colors;
        }

        // Setup click handler
        Card selectedCard = card; // Capture for lambda
        button.onClick.RemoveAllListeners(); // Clear any existing listeners
        button.onClick.AddListener(() => OnCardSelected(selectedCard));

        return cardObj;
    }

    /// <summary>
    /// Called when player selects a reward card (routes to battle or exploration flow)
    /// </summary>
    private void OnCardSelected(Card selectedCard)
    {
        Debug.Log($"[CardRewardUI] Player selected: {selectedCard.cardName}");

        // Route to appropriate flow based on context
        if (isExplorationReward)
        {
            OnExplorationCardSelected(selectedCard);
        }
        else
        {
            OnBattleCardSelected(selectedCard);
        }
    }

    /// <summary>
    /// Handle battle reward selection (original flow)
    /// </summary>
    private void OnBattleCardSelected(Card selectedCard)
    {
        Debug.Log($"[CardRewardUI] Battle reward selected: {selectedCard.cardName}");

        // Add card to collection
        if (CardCollection.Instance != null)
        {
            CardCollection.Instance.AddCard(selectedCard);
        }

        // Hide the reward panel
        HideRewardPanel();

        // Return to exploration scene
        ReturnToExploration();
    }

    /// <summary>
    /// Called when player skips the reward
    /// </summary>
    private void OnSkipReward()
    {
        Debug.Log("[CardRewardUI] Player skipped reward");

        // Check if this is an exploration reward (different flow than battle reward)
        if (isExplorationReward)
        {
            Debug.Log("[CardRewardUI] Skipping exploration reward - hiding panel and unlocking movement");
            HideExplorationRewardPanel();

            // Notify ExplorationRewardManager that reward was handled (skipped)
            if (ExplorationRewardManager.Instance != null)
            {
                ExplorationRewardManager.Instance.OnRewardClaimed();
            }
        }
        else
        {
            // Battle reward skip - hide and return to exploration scene
            HideRewardPanel();
            ReturnToExploration();
        }
    }

    /// <summary>
    /// Show reward selection for exploration (chests, etc.) with ONLY starter cards
    /// Similar to ShowRewardSelection but doesn't return to battle scene
    /// </summary>
    public void ShowExplorationReward(int numberOfOptions = 2)
    {
        isExplorationReward = true; // This is an exploration reward

        if (CardCollection.Instance == null)
        {
            Debug.LogError("[CardRewardUI] CardCollection.Instance is null!");
            return;
        }

        if (cardOptionsContainer == null)
        {
            Debug.LogError("[CardRewardUI] cardOptionsContainer is not assigned in inspector!");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("[CardRewardUI] cardPrefab is not assigned in inspector!");
            return;
        }

        Debug.Log("[CardRewardUI] Starting exploration reward selection (starter cards only)...");

        // Get random STARTER cards only (no rare/advanced cards)
        currentOptions = CardCollection.Instance.GetRandomStarterCards(numberOfOptions);

        if (currentOptions.Count < numberOfOptions)
        {
            Debug.LogWarning($"[CardRewardUI] Not enough starter cards! Got {currentOptions.Count} instead of {numberOfOptions}");
            // Continue anyway with what we have
        }

        Debug.Log($"[CardRewardUI] Got {currentOptions.Count} starter card options: {string.Join(", ", currentOptions.ConvertAll(c => c.cardName))}");

        // Clear previous card displays
        foreach (var cardDisplay in currentCardDisplays)
        {
            if (cardDisplay != null)
            {
                Destroy(cardDisplay);
            }
        }
        currentCardDisplays.Clear();

        // No need to hide hand in exploration (player doesn't have cards in hand during exploration)

        // Create card option displays
        try
        {
            foreach (var card in currentOptions)
            {
                GameObject cardDisplay = CreateCardOption(card);
                if (cardDisplay != null)
                {
                    currentCardDisplays.Add(cardDisplay);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[CardRewardUI] Exception while creating card displays: {ex.Message}\n{ex.StackTrace}");
        }

        Debug.Log($"[CardRewardUI] Created {currentCardDisplays.Count} card displays for exploration reward");

        // Show the panel
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
            
            // CRITICAL: Ensure reward panel is on top of all other UI
            Canvas rewardCanvas = rewardPanel.GetComponent<Canvas>();
            if (rewardCanvas != null)
            {
                rewardCanvas.overrideSorting = true;
                rewardCanvas.sortingOrder = 1000; // Very high to be on top
                Debug.Log("[CardRewardUI] Set reward canvas sorting order to 1000");
            }
            else
            {
                // If no Canvas, add one
                rewardCanvas = rewardPanel.AddComponent<Canvas>();
                rewardCanvas.overrideSorting = true;
                rewardCanvas.sortingOrder = 1000;
                
                // Also add GraphicRaycaster for button clicks
                if (rewardPanel.GetComponent<GraphicRaycaster>() == null)
                {
                    rewardPanel.AddComponent<GraphicRaycaster>();
                }
                
                Debug.Log("[CardRewardUI] Added Canvas and GraphicRaycaster to reward panel");
            }
        }

        if (titleText != null)
        {
            titleText.text = "Choose Your Card (Press 1 or 2)";
        }

        // CRITICAL: Ensure cursor is unlocked for card selection
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // CRITICAL: Disable any UI that might block raycasts
        DisableBlockingUI();
        
        // Enable keyboard shortcuts for Level Two workaround
        keyboardShortcutsEnabled = true;
        
        Debug.Log("[CardRewardUI] ✨ Cursor unlocked for exploration reward selection");
        Debug.Log("[CardRewardUI] ⌨️ Keyboard shortcuts enabled: Press 1 or 2 to select, ESC to skip");

        Debug.Log("[CardRewardUI] Exploration reward panel shown");
    }

    /// <summary>
    /// Handle card selection for exploration rewards (doesn't return to battle scene)
    /// </summary>
    private void OnExplorationCardSelected(Card selectedCard)
    {
        Debug.Log($"[CardRewardUI] Exploration reward selected: {selectedCard.cardName}");

        // Add to collection
        if (CardCollection.Instance != null)
        {
            CardCollection.Instance.AddCard(selectedCard);
            Debug.Log($"[CardRewardUI] {selectedCard.cardName} added to collection");
        }

        // Hide reward panel
        HideExplorationRewardPanel();

        // Notify ExplorationRewardManager that reward was claimed
        if (ExplorationRewardManager.Instance != null)
        {
            ExplorationRewardManager.Instance.OnRewardClaimed();
        }
    }

    /// <summary>
    /// Hide the reward panel for exploration (doesn't return to battle scene)
    /// </summary>
    private void HideExplorationRewardPanel()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        // Disable keyboard shortcuts
        keyboardShortcutsEnabled = false;

        // Re-enable any UI that was disabled
        EnableBlockingUI();

        ClearCardDisplays();
    }

    /// <summary>
    /// Return to exploration after reward selection
    /// </summary>
    private void ReturnToExploration()
    {
        // Call BattleManager to return to level
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager != null)
        {
            battleManager.ReturnToLevelOne();
        }
        else
        {
            Debug.LogWarning("[CardRewardUI] BattleManager not found - cannot return to exploration");
        }
    }

    /// <summary>
    /// Hide the reward panel and cleanup
    /// </summary>
    private void HideRewardPanel()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        // Show the hand again
        if (HandManager.Instance != null)
        {
            HandManager.Instance.ShowHand();
            Debug.Log("[CardRewardUI] Hand shown after reward selection");
        }

        ClearCardDisplays();
    }

    /// <summary>
    /// Temporarily disable UI elements that might block raycasts to reward cards
    /// </summary>
    private void DisableBlockingUI()
    {
        Debug.Log("[CardRewardUI] 🔍 Disabling ALL potentially blocking UI...");
        
        // Method 1: Disable caption panel if it exists (common in Level Two)
        GameObject captionPanel = GameObject.Find("CaptionPanel");
        if (captionPanel != null)
        {
            var canvasGroup = captionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                Debug.Log("[CardRewardUI] ✓ Disabled caption panel raycasts");
            }
            
            // Also disable Canvas if present
            var captionCanvas = captionPanel.GetComponent<Canvas>();
            if (captionCanvas != null)
            {
                captionCanvas.enabled = false;
                Debug.Log("[CardRewardUI] ✓ Disabled caption Canvas");
            }
        }
        
        // Method 2: Disable ALL other Canvas GraphicRaycasters except reward panel
        GraphicRaycaster[] allRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        Debug.Log($"[CardRewardUI] Found {allRaycasters.Length} GraphicRaycasters in scene");
        
        foreach (var raycaster in allRaycasters)
        {
            // Don't disable the reward panel's raycaster
            if (rewardPanel != null && raycaster.transform.IsChildOf(rewardPanel.transform))
            {
                raycaster.enabled = true;
                Debug.Log($"[CardRewardUI] ✓ Keeping raycaster ENABLED: {raycaster.gameObject.name} (reward panel)");
            }
            else if (rewardPanel != null && raycaster.gameObject == rewardPanel)
            {
                raycaster.enabled = true;
                Debug.Log($"[CardRewardUI] ✓ Keeping raycaster ENABLED: {raycaster.gameObject.name} (reward panel root)");
            }
            else
            {
                raycaster.enabled = false;
                Debug.Log($"[CardRewardUI] ✓ DISABLED raycaster: {raycaster.gameObject.name}");
            }
        }
        
        // Method 3: Disable all CanvasGroups except reward panel hierarchy
        CanvasGroup[] allCanvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        Debug.Log($"[CardRewardUI] Found {allCanvasGroups.Length} CanvasGroups in scene");
        
        foreach (var group in allCanvasGroups)
        {
            // Don't disable reward panel's canvas group
            if (rewardPanel != null && group.transform.IsChildOf(rewardPanel.transform))
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                Debug.Log($"[CardRewardUI] ✓ Keeping CanvasGroup ENABLED: {group.gameObject.name} (reward panel)");
            }
            else if (rewardPanel != null && group.gameObject == rewardPanel)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                Debug.Log($"[CardRewardUI] ✓ Keeping CanvasGroup ENABLED: {group.gameObject.name} (reward panel root)");
            }
            else
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                Debug.Log($"[CardRewardUI] ✓ DISABLED CanvasGroup: {group.gameObject.name}");
            }
        }
        
        Debug.Log("[CardRewardUI] ✅ Finished disabling blocking UI");
    }

    /// <summary>
    /// Re-enable UI elements that were temporarily disabled
    /// </summary>
    private void EnableBlockingUI()
    {
        Debug.Log("[CardRewardUI] 🔍 Re-enabling all UI elements...");
        
        // Re-enable caption panel
        GameObject captionPanel = GameObject.Find("CaptionPanel");
        if (captionPanel != null)
        {
            var canvasGroup = captionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                Debug.Log("[CardRewardUI] ✓ Re-enabled caption panel raycasts");
            }
            
            var captionCanvas = captionPanel.GetComponent<Canvas>();
            if (captionCanvas != null)
            {
                captionCanvas.enabled = true;
                Debug.Log("[CardRewardUI] ✓ Re-enabled caption Canvas");
            }
        }
        
        // Re-enable ALL GraphicRaycasters
        GraphicRaycaster[] allRaycasters = FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None);
        foreach (var raycaster in allRaycasters)
        {
            raycaster.enabled = true;
            Debug.Log($"[CardRewardUI] ✓ Re-enabled raycaster: {raycaster.gameObject.name}");
        }
        
        // Re-enable ALL CanvasGroups
        CanvasGroup[] allCanvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        foreach (var group in allCanvasGroups)
        {
            group.blocksRaycasts = true;
            group.interactable = true;
            Debug.Log($"[CardRewardUI] ✓ Re-enabled CanvasGroup: {group.gameObject.name}");
        }
        
        Debug.Log("[CardRewardUI] ✅ Finished re-enabling all UI");
    }

    /// <summary>
    /// Clear all card display objects
    /// </summary>
    private void ClearCardDisplays()
    {
        foreach (var cardDisplay in currentCardDisplays)
        {
            if (cardDisplay != null)
            {
                Destroy(cardDisplay);
            }
        }
        currentCardDisplays.Clear();
        currentOptions.Clear();
    }

    private void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
        }
    }
}
