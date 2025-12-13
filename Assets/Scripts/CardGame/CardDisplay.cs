using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame;

public class CardDisplay : MonoBehaviour
{
    [Header("Data")]
    public Card cardData;

    [Header("UI References")]
    public Image cardImage;          // big art image
    public TMP_Text nameText;        // card name
    public TMP_Text staminaText;     // number in the stamina shield
    public TMP_Text descriptionText; // bottom description box

    private bool hasRefreshed = false;

    private void OnEnable()
    {
        // Only refresh if we haven't already and have data
        if (!hasRefreshed && cardData != null)
        {
            Refresh();
        }
    }

    private void Start()
    {
        // Ensure we refresh at least once
        if (!hasRefreshed)
        {
            Refresh();
        }
    }

    public void SetCard(Card newCard)
    {
        cardData = newCard;
        Refresh();
    }

    public void Refresh()
    {
        hasRefreshed = true;
        
        if (cardData == null)
        {
            if (staminaText)     staminaText.text     = "";
            if (descriptionText) descriptionText.text = "";
            if (nameText)        nameText.text        = "";
            if (cardImage)       cardImage.sprite     = null;
            // Only warn if the card has been enabled (not during instantiation)
            if (gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[{name}] No card data assigned to CardDisplay!");
            }
            return;
        }

        // Cost
        if (staminaText)
            staminaText.text = cardData.staminaCost.ToString();

        // Art
        if (cardImage)
            cardImage.sprite = cardData.artwork;

        // Text – exactly like before: just use what you wrote on the card
        if (descriptionText)
            descriptionText.text = cardData.description;

        if (nameText)
            nameText.text = cardData.cardName;
    }
}