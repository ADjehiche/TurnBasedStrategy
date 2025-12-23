using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame;

[ExecuteAlways]
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

    private void OnValidate()
    {
        // Runs in the editor when you change values in the Inspector (no Play mode needed)
        if (cardData != null)
        {
            Refresh();
        }
    }

    private void OnEnable()
    {
        if (!hasRefreshed && cardData != null)
        {
            Refresh();
        }
    }

    private void Start()
    {
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

            if (gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[{name}] No card data assigned to CardDisplay!");
            }
            return;
        }

        if (staminaText)
            staminaText.text = cardData.staminaCost.ToString();

        if (cardImage)
            cardImage.sprite = cardData.artwork;

        if (descriptionText)
            descriptionText.text = cardData.description;

        if (nameText)
            nameText.text = cardData.cardName;
    }
}
