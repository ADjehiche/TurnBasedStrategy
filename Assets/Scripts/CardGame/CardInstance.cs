
using UnityEngine;
using CardGame;

[DisallowMultipleComponent]
public class CardInstance : MonoBehaviour
{
    [SerializeField] private Card cardData;   
    public Card Data => cardData;

    [HideInInspector] public string InstanceId;

    void Awake()
    {
        if (string.IsNullOrEmpty(InstanceId))
            InstanceId = System.Guid.NewGuid().ToString();
    }

    // Call this right after instantiating the card
    public void SetData(Card data)
    {
        cardData = data;

        // keep CardDisplay in sync
        var display = GetComponent<CardDisplay>();
        if (display != null)
        {
            display.cardData = data;   
            display.Refresh(); 
        }
    }
}