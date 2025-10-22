using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardGame;

public class CardDisplay : MonoBehaviour
{
    public Card cardData;

    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text staminaText;
    public TMP_Text damageText;

    public TMP_Text typeText;
  
    void Start()
    {
        UpdateCardDisplay();


    }

    public void UpdateCardDisplay()
    {
        nameText.text = cardData.cardName;
        typeText.text = cardData.cardType.ToString();
        staminaText.text = cardData.staminaCost.ToString();
        damageText.text = $"{cardData.damageMin} - {cardData.damageMax}";


    }
    
    public void Refresh()
    {
        if (cardData == null)
        {
            Debug.LogWarning($"[{name}] No card data assigned to CardDisplay!");
            return;
        }

        // Update all the displayed values based on the ScriptableObject data
        nameText.text = cardData.cardName;
        staminaText.text = cardData.staminaCost.ToString();
        damageText.text = $"{cardData.damageMin} - {cardData.damageMax}";
        typeText.text = cardData.cardType.ToString();

        // for later if we add images
        // if (cardImage != null)
        //     cardImage.sprite = cardData.cardSprite;
    }

  
}
