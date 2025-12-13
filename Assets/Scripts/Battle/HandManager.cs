using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGame; 
public class HandManager : MonoBehaviour
{
    // Start is called before the first frame update
    public DeckManager deckManager;

    public GameObject cardPrefab;
    public Transform handTransform;
    public float fanSpread = 5f;

    public float cardSpacing = 5f;

    public float verticalSpacing = 100f;
    public List<GameObject> cardsInHand = new List<GameObject>();
    
    void OnEnable()  => BattleEvents.OnCardResolved += HandleCardResolved;
    void OnDisable() => BattleEvents.OnCardResolved -= HandleCardResolved;

    void Start()
    {
        Debug.Log($"[HandManager] Awake called on {gameObject.name}");       
    }
    public void AddCardToHand(Card cardData)
    {
        Debug.Log($"[HandManager] AddCardToHand called for card: {cardData.name}. Current hand size: {cardsInHand.Count}");

        // 1) Instantiate under the hand transform (inactive to prevent OnEnable)
        GameObject newCard = Instantiate(cardPrefab, handTransform.position, Quaternion.identity, handTransform);
        newCard.SetActive(false); // Temporarily disable to prevent OnEnable before data is set
        cardsInHand.Add(newCard);

        // 2) Set the card data on the CardInstance component BEFORE enabling
        var instance = newCard.GetComponent<CardInstance>();
        if (instance != null)
        {
            instance.SetData(cardData);
        }
        else
        {
            // Fallback
            var display = newCard.GetComponent<CardDisplay>();
                         
            if (display != null)
            {
                 display.cardData = cardData;
            }
            else
            {
                Debug.LogError($"[{name}] Spawned card is missing CardInstance and CardDisplay.");
            }
        }

        // 3) Now enable the card (OnEnable will see the data)
        newCard.SetActive(true);

        // Manually refresh the display to ensure it's updated
        var cardDisplay = newCard.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.Refresh();
        }

        // 4) Layout
        UpdateHandVisuals();
    }

    private void HandleCardResolved(GameObject cardGO)
    {
        // 1) extract the Card data BEFORE destroying
        CardGame.Card cardData = null;

        var instance = cardGO.GetComponent<CardInstance>();
        if (instance != null) cardData = instance.Data;
        else
        {
            var display = cardGO.GetComponent<CardDisplay>();
            if (display != null) cardData = display.cardData;
        }

        // 2) remove from hand + destroy the UI
        if (cardsInHand.Remove(cardGO))
        {
            Destroy(cardGO);
            UpdateHandVisuals();
        }

        // 3) send to discard pile
        if (deckManager != null && cardData != null)
        {
            deckManager.Discard(cardData);
        }
    }


    
    // Discard every remaining (unplayed) card from the hand and clear the UI.
    public void DiscardAllInHand()
    {
        if (cardsInHand.Count == 0) return;

        // Work on a copy so we can safely Destroy & clear the original list.
        var snapshot = new List<GameObject>(cardsInHand);

        foreach (var go in snapshot)
        {
            
            CardGame.Card cardData = null;

            var instance = go.GetComponent<CardInstance>();
            if (instance != null) cardData = instance.Data;
            else
            {
                var display = go.GetComponent<CardDisplay>();
                if (display != null) cardData = display.cardData;
            }

            // Send to discard pile
            if (deckManager != null && cardData != null)
                deckManager.Discard(cardData);

            // Kill the UI object
            Destroy(go);
        }

        // Clear list and re-layout
        cardsInHand.Clear();
        UpdateHandVisuals();
    }

    void Update()
    {
      // UpdateHandVisuals(); 
    }

    private void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;

        if (cardCount == 1 )
        {

            cardsInHand[0].transform.localRotation = Quaternion.Euler(0, 0, 0f);
            cardsInHand[0].transform.localPosition = new Vector3(0f, 0f, 0f);
            return;

        }




        for (int i = 0; i < cardCount; i++)
        {
            float roationangle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].transform.localRotation = Quaternion.Euler(0, 0, roationangle);

            float horizontalOffset = (cardSpacing * (i - (cardCount - 1) / 2f)); 
            float normalizedPosition = (2f * i / (cardCount -1 ) -1f); //normalize card postion between -1, 1
            float verticalOffset = verticalSpacing * (1 - normalizedPosition * normalizedPosition);

            //set card postion
            cardsInHand[i].transform.localPosition = new Vector3(horizontalOffset, verticalOffset, 0f); 
                }
    
    }
}
