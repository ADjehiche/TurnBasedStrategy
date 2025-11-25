using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a zoom/focus effect on a target object
/// Used for highlighting important objects like the key on the dead skeleton
/// </summary>
public class CameraZoomFocus : MonoBehaviour
{
    [Header("Zoom Settings")]
    [Tooltip("Target to zoom towards")]
    [SerializeField] private Transform focusTarget;
    
    [Tooltip("How much to zoom in (lower = more zoom)")]
    [SerializeField] private float zoomFOV = 40f;
    
    [Tooltip("How long the zoom lasts")]
    [SerializeField] private float zoomDuration = 2f;
    
    [Tooltip("Speed of zoom in/out")]
    [SerializeField] private float zoomSpeed = 3f;
    
    [Header("Trigger Settings")]
    [Tooltip("Auto-trigger when player enters this trigger zone")]
    [SerializeField] private bool autoTrigger = true;
    
    [Header("Caption")]
    [SerializeField] private bool showCaption = true;
    [SerializeField] private string captionMessage = "[You] A key... on that skeleton.";
    [SerializeField] private float captionDuration = 2.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private Camera playerCamera;
    private float originalFOV;
    private bool hasTriggered = false;
    
    void Start()
    {
        // Find main camera
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!autoTrigger || hasTriggered || !other.CompareTag("Player"))
            return;
        
        hasTriggered = true;
        
        if (showDebugLogs)
            Debug.Log("[CameraZoomFocus] Player triggered zoom effect");
        
        StartCoroutine(ZoomSequence());
    }
    
    private IEnumerator ZoomSequence()
    {
        if (playerCamera == null || focusTarget == null)
        {
            Debug.LogWarning("[CameraZoomFocus] Missing camera or focus target!");
            yield break;
        }
        
        // Lock movement during zoom
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Camera zoom");
        
        // Show caption
        if (showCaption && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(captionMessage, captionDuration);
        }
        
        // Zoom in
        float elapsed = 0f;
        while (elapsed < zoomDuration / 2f)
        {
            elapsed += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                zoomFOV,
                Time.deltaTime * zoomSpeed
            );
            yield return null;
        }
        
        // Hold zoom
        yield return new WaitForSeconds(zoomDuration / 2f);
        
        // Zoom out
        elapsed = 0f;
        while (elapsed < zoomDuration / 2f)
        {
            elapsed += Time.deltaTime;
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                originalFOV,
                Time.deltaTime * zoomSpeed
            );
            yield return null;
        }
        
        // Ensure we're back to original
        playerCamera.fieldOfView = originalFOV;
        
        // Unlock movement after zoom
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Zoom complete");
        
        if (showDebugLogs)
            Debug.Log("[CameraZoomFocus] Zoom sequence complete");
    }
    
    /// <summary>
    /// Manually trigger the zoom effect (can be called from other scripts)
    /// </summary>
    public void TriggerZoom()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ZoomSequence());
        }
    }
}
