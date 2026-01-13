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
    [SerializeField] private GameObject cardOptionPrefab;
    [SerializeField] private Transform cardOptionsContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button skipButton;

    [Header("Card Display")]
    private List<GameObject> currentCardDisplays = new List<GameObject>();
    private List<Card> currentOptions = new List<Card>();

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
    }

    /// <summary>
    /// Show the reward selection UI with 2 random card options
    /// </summary>
    public void ShowRewardSelection()
    {
        if (CardCollection.Instance == null)
        {
            Debug.LogError("[CardRewardUI] CardCollection.Instance is null!");
            return;
        }

        // Get 2 random reward cards
        currentOptions = CardCollection.Instance.GetRandomRewardOptions(2);

        if (currentOptions.Count < 2)
        {
            Debug.LogWarning("[CardRewardUI] Not enough cards for reward selection!");
            return;
        }

        // Clear previous displays
        ClearCardDisplays();

        // Create card option displays
        foreach (var card in currentOptions)
        {
            GameObject cardDisplay = CreateCardOption(card);
            currentCardDisplays.Add(cardDisplay);
        }

        // Show the panel
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = "Choose Your Reward";
        }

        Debug.Log("[CardRewardUI] Showing reward selection");
    }

    /// <summary>
    /// Create a clickable card option display
    /// </summary>
    private GameObject CreateCardOption(Card card)
    {
        GameObject cardObj;

        if (cardOptionPrefab != null)
        {
            // Use prefab if provided
            cardObj = Instantiate(cardOptionPrefab, cardOptionsContainer);
        }
        else
        {
            // Create simple UI if no prefab
            cardObj = new GameObject(card.cardName);
            cardObj.transform.SetParent(cardOptionsContainer);
            
            // Add components for display
            var rectTransform = cardObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 300);
            
            var image = cardObj.AddComponent<Image>();
            image.color = Color.white;
            
            // Add card name text
            GameObject textObj = new GameObject("CardName");
            textObj.transform.SetParent(cardObj.transform);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = card.cardName;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.black;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        // Make it clickable
        Button button = cardObj.GetComponent<Button>();
        if (button == null)
        {
            button = cardObj.AddComponent<Button>();
        }

        // Setup click handler
        Card selectedCard = card; // Capture for lambda
        button.onClick.AddListener(() => OnCardSelected(selectedCard));

        // Try to set card data if CardDisplay component exists
        CardDisplay cardDisplay = cardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            CardInstance cardInstance = cardObj.GetComponent<CardInstance>();
            if (cardInstance == null)
            {
                cardInstance = cardObj.AddComponent<CardInstance>();
            }
            cardInstance.SetData(card);
        }

        return cardObj;
    }

    /// <summary>
    /// Called when player selects a reward card
    /// </summary>
    private void OnCardSelected(Card selectedCard)
    {
        Debug.Log($"[CardRewardUI] Player selected: {selectedCard.cardName}");

        // Add card to collection
        if (CardCollection.Instance != null)
        {
            CardCollection.Instance.AddCard(selectedCard);
        }

        // Hide the reward panel
        HideRewardPanel();

        // Continue to next scene or gameplay
        // You can add scene transition logic here
    }

    /// <summary>
    /// Called when player skips the reward
    /// </summary>
    private void OnSkipReward()
    {
        Debug.Log("[CardRewardUI] Player skipped reward");
        HideRewardPanel();
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
