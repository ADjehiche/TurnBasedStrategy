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

        // CRITICAL: Disable CardMovement component to prevent card from playing on click
        // Reward cards should only use Button component, not battle card interactions
        var cardMovement = cardObj.GetComponent<CardMovement>();
        if (cardMovement != null)
        {
            cardMovement.enabled = false;
            Debug.Log($"[CardRewardUI] Disabled CardMovement component on {card.cardName}");
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
        HideRewardPanel();
        
        // Return to exploration scene
        ReturnToExploration();
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
        }

        if (titleText != null)
        {
            titleText.text = "Choose Your Card";
        }

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
