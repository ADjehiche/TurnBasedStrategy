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

    private void OnEnable()
    {
        Refresh();
    }

    public void SetCard(Card newCard)
    {
        cardData = newCard;
        Refresh();
    }

    public void Refresh()
    {
        if (cardData == null)
        {
            if (staminaText)     staminaText.text     = "";
            if (descriptionText) descriptionText.text = "";
            if (nameText)        nameText.text        = "";
            if (cardImage)       cardImage.sprite     = null;
            Debug.LogWarning($"[{name}] No card data assigned to CardDisplay!");
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