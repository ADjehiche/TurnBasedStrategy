using UnityEngine;
using CardGame;

public class BattleBootstrap : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;   // drag from Canvas
    [SerializeField] private HandManager handManager;   // drag from Canvas
    [SerializeField] private int openingHand = 3;

    void Start()
    {   
        BattleState.Reset();
        if (deckManager == null) deckManager = FindObjectOfType<DeckManager>();
        if (handManager == null) handManager = FindObjectOfType<HandManager>();

        for (int i = 0; i < openingHand; i++)
        {
            var card = deckManager.DrawOne();
            if (card != null) handManager.AddCardToHand(card);
        }
    }
}