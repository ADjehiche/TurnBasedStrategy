using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using CardGame;

public class PauseDeckView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentRoot;      // ScrollView/Viewport/Content
    [SerializeField] private GameObject cardUiPrefab;    // Your Card UI prefab (with CardDisplay)
    [SerializeField] private bool groupDuplicates = true;

    private void OnEnable()
    {
        // When pause menu (or this panel) becomes active, rebuild the deck UI
        Rebuild();
    }
    private CardCollection GetCollection()
    {
        if (CardCollection.Instance != null) return CardCollection.Instance;

        // Unity 2022+: FindFirstObjectByType
        var found = FindFirstObjectByType<CardCollection>();
        if (found != null) return found;

        // fallback for older Unity:
        return FindObjectOfType<CardCollection>();
    }

    public void Rebuild()
    {
        if (contentRoot == null || cardUiPrefab == null)
        {
            Debug.LogWarning("[PauseDeckView] Missing references.");
            return;
        }

        var collection = GetCollection();
        Debug.Log($"[PauseDeckView] CardCollection exists? {collection != null}");
        if (collection == null) return;

        Debug.Log($"[PauseDeckView] OwnedCards: {collection.OwnedCards.Count}");

        // Clear old
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        // Use the found collection, NOT CardCollection.Instance
        List<Card> cards = collection.OwnedCards;
        if (cards == null || cards.Count == 0) return;

        if (!groupDuplicates)
        {
            foreach (var card in cards)
                SpawnCard(card, 1);
            return;
        }

        var grouped = cards
            .GroupBy(c => c)
            .OrderBy(g => g.Key.category)
            .ThenBy(g => g.Key.rarity)
            .ThenBy(g => g.Key.cardName);

        foreach (var g in grouped)
            SpawnCard(g.Key, g.Count());
    }


    private void SpawnCard(Card card, int count)
    {
        var go = Instantiate(cardUiPrefab, contentRoot, false);

        // Option A: Use CardDisplay directly (recommended for UI prefabs)
        var display = go.GetComponent<CardDisplay>();
        if (display != null)
        {
            display.SetCard(card); // sets + Refresh :contentReference[oaicite:5]{index=5}
        }
        else
        {
            // Option B: If your prefab uses CardInstance to drive CardDisplay
            var instance = go.GetComponent<CardInstance>();
            if (instance != null) instance.SetData(card); // keeps CardDisplay synced :contentReference[oaicite:6]{index=6}
        }

        // Optional: show stack count if you added a TMP child named "CountText"
        var countText = go.GetComponentsInChildren<TMP_Text>(true)
                          .FirstOrDefault(t => t.name == "CountText");
        if (countText != null)
            countText.text = count > 1 ? $"x{count}" : "";
    }
}
