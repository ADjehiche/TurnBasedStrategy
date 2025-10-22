using UnityEngine;
using UnityEngine.EventSystems; 

public class DragUIObject : MonoBehaviour, IDragHandler, IPointerDownHandler 
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
    public float movementSensitivity = 1.0f; // mouse sens

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); 
        canvas = GetComponentInParent<Canvas>(); 
    }

    public void OnPointerDown(PointerEventData eventData) 
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out originalLocalPointerPosition); //Using the event system to detect what is clicked on
        originalPanelLocalPosition = rectTransform.localPosition;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition))
        {
            localPointerPosition /= canvas.scaleFactor;

            
            Vector3 offsetToOriginal = (localPointerPosition - originalLocalPointerPosition) * movementSensitivity;
            rectTransform.localPosition = originalPanelLocalPosition + offsetToOriginal;

            // for debugging purposes, will be commented out later
            Debug.Log($"Drag - LocalPointerPosition: {localPointerPosition}, Offset: {offsetToOriginal}, New Position: {rectTransform.localPosition}"); 
        }
    }
}