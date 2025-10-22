using System.Collections.Generic;
using UnityEngine;
using CardGame;

public class DeckManager : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("If empty, will load all Cards from Resources/Cards")]
    [SerializeField] private List<Card> startingDeck = new List<Card>();

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

        if (startingDeck == null || startingDeck.Count == 0)
        {
            var all = Resources.LoadAll<Card>("Cards");
            drawPile.AddRange(all);
        }
        else
        {
            drawPile.AddRange(startingDeck);
        }
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