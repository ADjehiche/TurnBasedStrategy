using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public float cardHoverHeight = 4f;

    public float hoverSpeed = 10f;

    Vector3 originalPosition;
    Vector3 targetPosition;

    void Start()
    {
        // Store the original world position so we can return to it
        originalPosition = transform.position;
        targetPosition = originalPosition;
    }

    void OnMouseEnter()
    {
        targetPosition = originalPosition + Vector3.up * cardHoverHeight;
    }

    void OnMouseExit()
    {
        targetPosition = originalPosition;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * hoverSpeed);
    }
}
