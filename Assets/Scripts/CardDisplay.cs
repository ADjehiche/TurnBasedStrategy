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
    public TMP_Text healthText;
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
        healthText.text = cardData.health.ToString();
        damageText.text = $"{cardData.damageMin} - {cardData.damageMax}";
        
       
    }

  
}
