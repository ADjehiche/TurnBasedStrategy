using System.Collections.Generic;
using UnityEngine;
using CardGame;

public class DeckManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("If empty, will load all Cards from Resources/Cards")]
    [SerializeField] private List<Card> startingDeck = new List<Card>();
    [SerializeField] private int autoDeckSize = 20;

    [Header("Runtime")]
    [SerializeField] private List<Card> drawPile = new List<Card>();
    [SerializeField] private List<Card> discardPile = new List<Card>();

    void Awake()
    {
        BuildStartingDeck();
        Shuffle(drawPile);
    }

    private void BuildStartingDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        // If a starting deck is assigned in the inspector, use that directly.
        if (startingDeck != null && startingDeck.Count > 0)
        {
            drawPile.AddRange(startingDeck);
            Debug.Log($"[DeckManager] Using startingDeck from inspector, count = {drawPile.Count}");
            return;
        }

        // Auto-build from Resources using the 2 Attack / 1 Defense / 1 Utility-or-Tactical pattern.
        var allCards = new List<Card>();
        allCards.AddRange(Resources.LoadAll<Card>("Cards"));
        allCards.AddRange(Resources.LoadAll<Card>("Cards/Attack"));
        allCards.AddRange(Resources.LoadAll<Card>("Cards/Defense"));
        allCards.AddRange(Resources.LoadAll<Card>("Cards/Tactical"));
        allCards.AddRange(Resources.LoadAll<Card>("Cards/Utility"));

        if (allCards.Count == 0)
        {
            Debug.LogError("[DeckManager] No cards found in Resources/Cards – cannot build deck.");
            return;
        }

        // Split by category
        var attacks   = allCards.FindAll(c => c.category == CardCategory.Attack);
        var defenses  = allCards.FindAll(c => c.category == CardCategory.Defense);
        var utilities = allCards.FindAll(c => c.category == CardCategory.Utility);
        var tacticals = allCards.FindAll(c => c.category == CardCategory.Tactical);

        if (attacks.Count == 0 || defenses.Count == 0 || (utilities.Count == 0 && tacticals.Count == 0))
        {
            Debug.LogError("[DeckManager] Not enough cards in one or more categories to build auto deck.");
            return;
        }

        void AddRandom(List<Card> pool)
        {
            if (pool == null || pool.Count == 0) return;
            var card = pool[Random.Range(0, pool.Count)];
            drawPile.Add(card);
        }

        // Build deck: 2 Attack → 1 Defense → 1 Utility/Tactical, repeated until autoDeckSize is reached
        while (drawPile.Count < autoDeckSize)
        {
            // 2 attacks
            AddRandom(attacks);
            if (drawPile.Count >= autoDeckSize) break;
            AddRandom(attacks);
            if (drawPile.Count >= autoDeckSize) break;

            // 1 defense
            AddRandom(defenses);
            if (drawPile.Count >= autoDeckSize) break;

            // 1 utility OR tactical
            bool useUtility = utilities.Count > 0 && (tacticals.Count == 0 || Random.value < 0.5f);
            if (useUtility)
                AddRandom(utilities);
            else
                AddRandom(tacticals);
        }

        Debug.Log($"[DeckManager] Auto-built deck with {drawPile.Count} cards (2A/1D/1U-T pattern).");
    }

    public static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// Draw 1 (reshuffle discards if needed). Returns null if deck truly empty.
    public Card DrawOne()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count > 0)
            {
                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }
        }
        if (drawPile.Count == 0) return null;

        var c = drawPile[0];
        drawPile.RemoveAt(0);
        return c;
    }

    public List<Card> Draw(int count)
    {
        var result = new List<Card>(count);
        for (int i = 0; i < count; i++)
        {
            var c = DrawOne();
            if (c != null) result.Add(c);
            else break;
        }
        return result;
    }

    public void Discard(Card card)
    {
        if (card != null) discardPile.Add(card);
    }

    // Backward-compat if something calls this
    public Card GetNextCard() => DrawOne();
}