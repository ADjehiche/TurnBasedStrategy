using UnityEngine;
using CardGame;

public class BattleBootstrap : MonoBehaviour
{
    [SerializeField] private DeckManager deckManager;   // drag from Canvas
    [SerializeField] private HandManager handManager;   // drag from Canvas

    // [SerializeField] private int openingHand = 3;

    void Start()
    {   
        BattleState.Reset();
        if (deckManager == null) deckManager = FindObjectOfType<DeckManager>();
        if (handManager == null) handManager = FindObjectOfType<HandManager>();
    }
}