using UnityEngine;
using System.Collections;

/// <summary>
/// Creates a zoom/focus effect on a target object
/// Used for highlighting important objects like the key on the dead skeleton
/// Pans camera to look at target during zoom
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
    
    [Tooltip("Vertical offset to aim higher on target")]
    [SerializeField] private float verticalOffset = 0.5f;
    
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
    private Quaternion originalCameraRotation;
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
        
        // Store original camera rotation
        originalCameraRotation = playerCamera.transform.rotation;
        
        // Show caption
        if (showCaption && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(captionMessage, captionDuration);
        }
        
        // Zoom in with rotation
        float elapsed = 0f;
        float zoomInDuration = zoomDuration / 2f;
        
        while (elapsed < zoomInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomInDuration;
            
            // Zoom FOV
            playerCamera.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            
            // Rotate camera to look at target (with vertical offset)
            Vector3 targetPosition = focusTarget.position + Vector3.up * verticalOffset;
            Vector3 directionToTarget = targetPosition - playerCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            playerCamera.transform.rotation = Quaternion.Slerp(originalCameraRotation, targetRotation, t);
            
            yield return null;
        }
        
        // Hold zoom
        yield return new WaitForSeconds(zoomDuration / 2f);
        
        // Zoom out with rotation back
        elapsed = 0f;
        float zoomOutDuration = zoomDuration / 2f;
        Quaternion currentRotation = playerCamera.transform.rotation;
        
        while (elapsed < zoomOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomOutDuration;
            
            // Restore FOV
            playerCamera.fieldOfView = Mathf.Lerp(zoomFOV, originalFOV, t);
            
            // Restore rotation
            playerCamera.transform.rotation = Quaternion.Slerp(currentRotation, originalCameraRotation, t);
            
            yield return null;
        }
        
        // Ensure exact restoration
        playerCamera.fieldOfView = originalFOV;
        playerCamera.transform.rotation = originalCameraRotation;
        
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