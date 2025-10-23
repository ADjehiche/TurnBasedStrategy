// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor.Search;
using UnityEngine;
using UnityEngine.EventSystems;
using CardGame;

public class CardMovement : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler
{
    private CardInstance cardInstance;
    private RectTransform rectTransform;

    private Canvas canvas;

    private Vector2 originalLocalPointerPosition;

    private Vector3 originalPanelLocalPosition;

    private Vector3 originalScale;

    private int currentState = 0;

    private Quaternion originalRotation;

    private Vector3 originalLocalPosition;


    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private Vector2 cardPlay;

    [SerializeField] private Vector3 playPostion;

    // [SerializeField] private GameObject glowEffect;
    // [SerializeField] private GameObject playArrow;


    void Awake()
    {
        cardInstance = GetComponent<CardInstance>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;
        originalLocalPosition = rectTransform.localPosition;
    }

    void Update()
    {
        switch (currentState)
        {
            case 1:


                HandleHoverState();
                break;


            case 2:
                HandleDragState(); 
                break;


            case 3:
                HandlePlayState();
                break;

        }
    }

    private void TransistionToState0()
    {
        //reseting to original state
        currentState = 0;
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;
        rectTransform.localPosition = originalLocalPosition;
        // if (glowEffect) glowEffect.SetActive(false);
        // if (playArrow) playArrow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalLocalPosition = rectTransform.localPosition;
            originalRotation = rectTransform.localRotation;
            originalScale = rectTransform.localScale;
            currentState = 1; // hover state
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            TransistionToState0();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            currentState = 2;

            var parentRect = (RectTransform)rectTransform.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out originalLocalPointerPosition
            );

            originalPanelLocalPosition = rectTransform.localPosition;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
       
        if (currentState == 3) return;
        TransistionToState0();
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (currentState == 2)
        {
            var parentRect = (RectTransform)rectTransform.parent;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPointerPosition))
            {
                Vector3 offsetToOriginal = (localPointerPosition - originalLocalPointerPosition);
                rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;

                if (rectTransform.localPosition.y > cardPlay.y)
                {
                    // 1) Get the Card data (prefer CardInstance; fallback to CardDisplay)
                    Card cardData = cardInstance != null
                        ? cardInstance.Data
                        : GetComponent<CardDisplay>()?.cardData;

                    if (cardData == null)
                    {
                        Debug.LogError("No Card data found on this card (CardInstance or CardDisplay).");
                        TransistionToState0();
                        return;
                    }

                    // 2) Stamina check BEFORE locking the card
                    if (PlayerStamina.Instance != null && !PlayerStamina.Instance.CanAfford(cardData.staminaCost))
                    {
                        Debug.Log("Not enough stamina to play this card.");
                        TransistionToState0(); // snap back to hand
                        return;
                    }

                    // 3) Lock and start targeting with CARD DATA
                    currentState = 3; // play state
                    rectTransform.localPosition = playPostion;

                    if (TargetingSystem.Instance != null)
                    {
                        TargetingSystem.Instance.BeginTargeting(cardData, gameObject, () =>
                        {
                            Debug.Log("Card played and resolved.");
                            TransistionToState0();
                            
                        });
                    }
                    else
                    {
                        Debug.LogError("TargetingSystem.Instance is null. Add a TargetingSystem object to the scene.");
                    }
                }
            }
        }
    }

    private void HandleHoverState()
    {
        // if (glowEffect && !glowEffect.activeSelf) glowEffect.SetActive(true);
        rectTransform.localScale = originalScale * selectScale;
    }
    
    private void HandleDragState()
    {
        rectTransform.localRotation = Quaternion.identity;
    }

    private void HandlePlayState()
    {
        rectTransform.localPosition = playPostion;
        rectTransform.localRotation = Quaternion.identity;

        
    }


}
