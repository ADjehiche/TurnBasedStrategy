using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardGame;

/// <summary>
/// Manages the player's persistent card collection throughout the game.
/// Cards are added at game start and through rewards (battle victories, chests).
/// </summary>
public class CardCollection : MonoBehaviour
{
    public static CardCollection Instance { get; private set; }

    [Header("Player's Card Collection")]
    [SerializeField] private List<Card> ownedCards = new List<Card>();

    [Header("Starting Deck Configuration")]
    [SerializeField] private int startingAttackCards = 8;
    [SerializeField] private int startingDefenseCards = 4;
    [SerializeField] private int startingUtilityCards = 3;

    [Header("Hand Composition Rules")]
    [SerializeField] private int guaranteedAttackInHand = 1;
    [SerializeField] private int guaranteedDefenseInHand = 1;
    [SerializeField] private int maxUtilityInHand = 1;

    private List<Card> starterPool;

    public List<Card> OwnedCards => new List<Card>(ownedCards);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadStarterCardPool();
    }

    /// <summary>
    /// Load all cards marked as starter cards (canAppearInStartingDecks = true)
    /// Also includes merge-only cards (rarely) for rewards
    /// </summary>
    private void LoadStarterCardPool()
    {
        starterPool = new List<Card>();
        
        // Load all cards from Resources/Cards folders
        Card[] allCards = Resources.LoadAll<Card>("Cards");
        
        foreach (var card in allCards)
        {
            if (card.canAppearInStartingDecks)
            {
                starterPool.Add(card);
            }
        }

        Debug.Log($"[CardCollection] Loaded {starterPool.Count} starter cards from pool");
    }

    /// <summary>
    /// Initialize player's collection with 15 random starter cards
    /// 8 Attack, 4 Defense, 3 Utility (can include duplicates)
    /// </summary>
    public void InitializeStartingCollection()
    {
        ownedCards.Clear();

        // Ensure starter pool is loaded before initializing
        if (starterPool == null || starterPool.Count == 0)
        {
            LoadStarterCardPool();
        }

        // Get starter cards by category
        var attackCards = starterPool.Where(c => c.category == CardCategory.Attack).ToList();
        var defenseCards = starterPool.Where(c => c.category == CardCategory.Defense).ToList();
        var utilityCards = starterPool.Where(c => c.category == CardCategory.Utility).ToList();

        // Add 8 random Attack cards (duplicates allowed)
        for (int i = 0; i < startingAttackCards; i++)
        {
            if (attackCards.Count > 0)
            {
                Card randomCard = attackCards[Random.Range(0, attackCards.Count)];
                ownedCards.Add(randomCard);
            }
        }

        // Add 4 random Defense cards
        for (int i = 0; i < startingDefenseCards; i++)
        {
            if (defenseCards.Count > 0)
            {
                Card randomCard = defenseCards[Random.Range(0, defenseCards.Count)];
                ownedCards.Add(randomCard);
            }
        }

        // Add 3 random Utility cards
        for (int i = 0; i < startingUtilityCards; i++)
        {
            if (utilityCards.Count > 0)
            {
                Card randomCard = utilityCards[Random.Range(0, utilityCards.Count)];
                ownedCards.Add(randomCard);
            }
        }

        Debug.Log($"[CardCollection] Starting collection initialized with {ownedCards.Count} cards");
        LogCollectionSummary();
    }

    /// <summary>
    /// Add a card to the player's collection (from rewards, chests, etc.)
    /// </summary>
    public void AddCard(Card card)
    {
        if (card != null)
        {
            ownedCards.Add(card);
            Debug.Log($"[CardCollection] Added {card.cardName} to collection. Total: {ownedCards.Count}");
        }
    }

    /// <summary>
    /// Remove a card from the collection (if you add card removal mechanics)
    /// </summary>
    public void RemoveCard(Card card)
    {
        if (ownedCards.Contains(card))
        {
            ownedCards.Remove(card);
            Debug.Log($"[CardCollection] Removed {card.cardName} from collection");
        }
    }

    /// <summary>
    /// Get 2 random reward cards from the starter pool for post-battle selection
    /// 10% chance to include a rare merge-only card if available
    /// </summary>
    public List<Card> GetRandomRewardOptions(int count = 2)
    {
        List<Card> options = new List<Card>();
        List<Card> availablePool = new List<Card>(starterPool);

        // Load all cards including merge-only (for rare rewards)
        Card[] allCards = Resources.LoadAll<Card>("Cards");
        var mergeOnlyCards = allCards.Where(c => !c.canAppearInStartingDecks && !c.isStarterCard).ToList();

        for (int i = 0; i < count && availablePool.Count > 0; i++)
        {
            Card selectedCard;

            // 10% chance to offer a merge-only card (if available)
            if (mergeOnlyCards.Count > 0 && Random.value < 0.10f)
            {
                selectedCard = mergeOnlyCards[Random.Range(0, mergeOnlyCards.Count)];
                mergeOnlyCards.Remove(selectedCard); // Prevent duplicates in same choice
                Debug.Log($"[CardCollection] Rare merge-only card offered: {selectedCard.cardName}");
            }
            else
            {
                // Normal starter pool card
                int randomIndex = Random.Range(0, availablePool.Count);
                selectedCard = availablePool[randomIndex];
                availablePool.RemoveAt(randomIndex);
            }

            options.Add(selectedCard);
        }

        return options;
    }

    /// <summary>
    /// Get random STARTER cards only (for exploration rewards, chests, etc.)
    /// NO rare/merge-only cards - only basic starter pool cards
    /// </summary>
    public List<Card> GetRandomStarterCards(int count = 2)
    {
        List<Card> options = new List<Card>();
        
        // Ensure starter pool is loaded
        if (starterPool == null || starterPool.Count == 0)
        {
            LoadStarterCardPool();
        }

        List<Card> availablePool = new List<Card>(starterPool);

        for (int i = 0; i < count && availablePool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availablePool.Count);
            Card selectedCard = availablePool[randomIndex];
            availablePool.RemoveAt(randomIndex); // Prevent duplicates in same selection
            options.Add(selectedCard);
        }

        Debug.Log($"[CardCollection] Selected {options.Count} starter cards for exploration reward");
        return options;
    }

    /// <summary>
    /// Build a battle deck from owned cards with hand composition rules
    /// This returns the deck that DeckManager will use for battle
    /// </summary>
    public List<Card> BuildBattleDeck()
    {
        // For now, just return all owned cards
        // DeckManager will handle drawing with composition rules
        return new List<Card>(ownedCards);
    }

    /// <summary>
    /// Draw a hand of cards following composition rules:
    /// - At least 1 Attack
    /// - At least 1 Defense
    /// - Max 1 Utility
    /// - Remaining slots filled randomly
    /// </summary>
    public List<Card> DrawHandWithRules(List<Card> drawPile, int handSize = 4)
    {
        List<Card> hand = new List<Card>();
        List<Card> availableCards = new List<Card>(drawPile);

        if (availableCards.Count == 0)
        {
            Debug.LogWarning("[CardCollection] Draw pile is empty!");
            return hand;
        }

        // Step 1: Guarantee 1 Attack card
        var attackCards = availableCards.Where(c => c.category == CardCategory.Attack).ToList();
        if (attackCards.Count > 0)
        {
            Card attackCard = attackCards[Random.Range(0, attackCards.Count)];
            hand.Add(attackCard);
            availableCards.Remove(attackCard);
        }

        // Step 2: Guarantee 1 Defense card
        var defenseCards = availableCards.Where(c => c.category == CardCategory.Defense).ToList();
        if (defenseCards.Count > 0)
        {
            Card defenseCard = defenseCards[Random.Range(0, defenseCards.Count)];
            hand.Add(defenseCard);
            availableCards.Remove(defenseCard);
        }

        // Step 3: Add max 1 Utility card
        var utilityCards = availableCards.Where(c => c.category == CardCategory.Utility || c.category == CardCategory.Tactical).ToList();
        if (utilityCards.Count > 0 && Random.value > 0.3f) // 70% chance to include utility
        {
            Card utilityCard = utilityCards[Random.Range(0, utilityCards.Count)];
            hand.Add(utilityCard);
            availableCards.Remove(utilityCard);
        }

        // Step 4: Fill remaining slots - PREFER ATTACK CARDS
        while (hand.Count < handSize && availableCards.Count > 0)
        {
            // Try to add attack cards first (if available)
            var remainingAttacks = availableCards.Where(c => c.category == CardCategory.Attack).ToList();
            
            if (remainingAttacks.Count > 0)
            {
                Card attackCard = remainingAttacks[Random.Range(0, remainingAttacks.Count)];
                hand.Add(attackCard);
                availableCards.Remove(attackCard);
            }
            else
            {
                // No more attacks, add any random card
                int randomIndex = Random.Range(0, availableCards.Count);
                hand.Add(availableCards[randomIndex]);
                availableCards.RemoveAt(randomIndex);
            }
        }

        return hand;
    }

    private void LogCollectionSummary()
    {
        int attackCount = ownedCards.Count(c => c.category == CardCategory.Attack);
        int defenseCount = ownedCards.Count(c => c.category == CardCategory.Defense);
        int utilityCount = ownedCards.Count(c => c.category == CardCategory.Utility);
        int tacticalCount = ownedCards.Count(c => c.category == CardCategory.Tactical);

        Debug.Log($"[CardCollection] Collection: {attackCount} Attack, {defenseCount} Defense, {utilityCount} Utility, {tacticalCount} Tactical");
    }

    /// <summary>
    /// Clear the entire collection (for new game)
    /// </summary>
    public void ClearCollection()
    {
        ownedCards.Clear();
        Debug.Log("[CardCollection] Collection cleared");
    }
}
