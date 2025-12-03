using UnityEngine;
using UnityEngine.EventSystems;
using CardGame;

public class CardMovement : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler
{
    private CardInstance cardInstance;
    private RectTransform rectTransform;

    private Vector3 originalLocalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    // 0 = idle, 1 = hover, 2 = locked/targeting
    private int currentState = 0;

    [SerializeField] private float selectScale = 1.1f;

    void Awake()
    {
        cardInstance   = GetComponent<CardInstance>();
        rectTransform  = GetComponent<RectTransform>();

        originalScale         = rectTransform.localScale;
        originalRotation      = rectTransform.localRotation;
        originalLocalPosition = rectTransform.localPosition;
    }

    void Update()
    {
        if (currentState == 1)
        {
            rectTransform.localScale = originalScale * selectScale;
        }
    }

    private void ResetVisual()
    {
        currentState = 0;
        rectTransform.localScale    = originalScale;
        rectTransform.localRotation = originalRotation;
        rectTransform.localPosition = originalLocalPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == 0)
        {
            originalLocalPosition = rectTransform.localPosition;
            originalRotation      = rectTransform.localRotation;
            originalScale         = rectTransform.localScale;
            currentState          = 1; // hover
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == 1)
        {
            ResetVisual();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // only allow from hover → lock
        if (currentState != 1) return;

        // grab card data
        Card cardData = cardInstance != null
            ? cardInstance.Data
            : GetComponent<CardDisplay>()?.cardData;

        if (cardData == null)
        {
            Debug.LogError("[CardMovement] No Card data found on this card.");
            ResetVisual();
            return;
        }

        if (TargetingSystem.Instance == null)
        {
            Debug.LogError("[CardMovement] No TargetingSystem.Instance found in scene.");
            ResetVisual();
            return;
        }

        currentState = 2; // locked / targeting

        // VISUAL LOCK:
        rectTransform.SetAsLastSibling(); // bring to front
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale    = originalScale * 1.15f;
        rectTransform.localPosition = originalLocalPosition + new Vector3(0f, 150f, 0f); 
        // tweak this offset to taste

        // start targeting – onComplete will be called after play/cancel
        TargetingSystem.Instance.BeginTargeting(cardData, gameObject, () =>
        {
            // if card still exists, snap back visuals
            if (this != null && rectTransform != null)
            {
                ResetVisual();
            }
        });

        Debug.Log($"[CardMovement] Locked card for targeting: {cardData.cardName}");
    }
}