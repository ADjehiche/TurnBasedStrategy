using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame;

public class DeckManager : MonoBehaviour
{
    public List<Card> allCards = new List<Card>();

    private int currentIndex = 0;

    void Start()
    {
        //load cards assests from resources folder
        Card[] cards = Resources.LoadAll<Card>("Cards");
        ///Add loaded cards to the allCards list
        allCards.AddRange(cards);


    }


    public void DrawCard(HandManager handManager)
    {
        if (allCards.Count == 0)
            return;

        Card nextCard = allCards[currentIndex];
        handManager.AddCardToHand(nextCard);
        currentIndex = (currentIndex + 1) % allCards.Count;


    }
}

        

