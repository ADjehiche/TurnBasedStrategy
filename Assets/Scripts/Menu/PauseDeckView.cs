using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using CardGame;
using UnityEngine.UI;

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
        // Grid cell size (must match GridLayoutGroup)
        float cellW = 180f;
        float cellH = 252f;

        // 1) Slot = the grid item
        var slot = new GameObject("CardSlot", typeof(RectTransform), typeof(LayoutElement));
        var slotRT = slot.GetComponent<RectTransform>();
        slotRT.SetParent(contentRoot, false);
        slotRT.localScale = Vector3.one;
        slotRT.sizeDelta = new Vector2(cellW, cellH);

        var le = slot.GetComponent<LayoutElement>();
        le.preferredWidth = cellW;
        le.preferredHeight = cellH;

        // 2) Card inside slot
        var go = Instantiate(cardUiPrefab, slotRT, false);

        // CRITICAL: neutralize prefab’s battle scaling
        go.transform.localScale = Vector3.one;

        // If the card prefab has its own Canvas, it will ignore parent scaling/layout → disable it in pause UI
        var extraCanvas = go.GetComponentInChildren<Canvas>(true);
        if (extraCanvas != null) extraCanvas.enabled = false;

        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -10f); // small down nudge

            // Force UI to update so bounds are correct
            Canvas.ForceUpdateCanvases();

            // Fit based on actual rendered bounds (works even if prefab sizeDelta is weird)
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(slotRT, rt);
            float sx = cellW / Mathf.Max(1f, bounds.size.x);
            float sy = cellH / Mathf.Max(1f, bounds.size.y);
            float s = Mathf.Min(sx, sy);

            rt.localScale = Vector3.one * s;
        }

        // Bind data
        var display = go.GetComponent<CardDisplay>();
        if (display != null) display.SetCard(card);

        var countText = go.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(t => t.name == "CountText");
        if (countText != null) countText.text = count > 1 ? $"x{count}" : "";
    }

}
