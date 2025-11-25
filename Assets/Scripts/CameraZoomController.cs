using UnityEngine;
using System.Collections;

/// <summary>
/// Handles camera zoom and lock-on effects when enemies are detected.
/// Smoothly zooms in, locks onto target, then returns to normal after a duration.
/// </summary>
public class CameraZoomController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float zoomedFOV = 40f;
    [SerializeField] private float zoomDuration = 0.5f;
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnDuration = 2.5f;
    [SerializeField] private float returnToNormalDuration = 0.8f;
    [SerializeField] private bool smoothLookAt = true;
    [SerializeField] private float lookAtSpeed = 3f;
    
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private MonoBehaviour playerController; // Your player movement script
    
    [Header("Debug")]
    [SerializeField] private bool allowZoom = true;
    
    // State tracking
    private bool isZooming = false;
    private bool isLockedOn = false;
    private Transform currentTarget = null;
    private Coroutine zoomCoroutine = null;
    
    // Store original camera state
    private float originalFOV;
    private Quaternion originalRotation;
    private bool wasPlayerControlEnabled = true;
    
    void Start()
    {
        // Auto-find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("CameraZoomController: No camera found! Please assign the target camera.");
                return;
            }
        }
        
        // Store original FOV
        originalFOV = targetCamera.fieldOfView;
        normalFOV = originalFOV; // Use current FOV as normal
        
        Debug.Log($"CameraZoomController: Initialized with FOV {originalFOV}");
    }
    
    /// <summary>
    /// Trigger zoom effect on a specific target
    /// </summary>
    public void ZoomOnTarget(Transform target)
    {
        if (!allowZoom || target == null)
        {
            Debug.LogWarning("CameraZoomController: Zoom not allowed or target is null");
            return;
        }
        
        // Don't interrupt an ongoing zoom
        if (isZooming)
        {
            Debug.Log("CameraZoomController: Already zooming, ignoring request");
            return;
        }
        
        // Stop any existing coroutine
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
        }
        
        // Start the zoom sequence
        currentTarget = target;
        zoomCoroutine = StartCoroutine(ZoomSequence());
    }
    
    /// <summary>
    /// Main zoom sequence: zoom in -> lock on -> return to normal
    /// </summary>
    private IEnumerator ZoomSequence()
    {
        isZooming = true;
        Debug.Log("CameraZoomController: Starting zoom sequence");
        
        // Store original state
        originalRotation = targetCamera.transform.rotation;
        
        // Disable player control during zoom
        DisablePlayerControl();
        
        // Phase 1: Zoom in
        yield return StartCoroutine(ZoomToFOV(normalFOV, zoomedFOV, zoomDuration));
        
        // Phase 2: Lock on to target
        isLockedOn = true;
        float lockOnTimer = 0f;
        
        while (lockOnTimer < lockOnDuration)
        {
            if (currentTarget != null)
            {
                // Look at the target
                Vector3 directionToTarget = currentTarget.position - targetCamera.transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                
                if (smoothLookAt)
                {
                    targetCamera.transform.rotation = Quaternion.Slerp(
                        targetCamera.transform.rotation, 
                        targetRotation, 
                        Time.deltaTime * lookAtSpeed
                    );
                }
                else
                {
                    targetCamera.transform.rotation = targetRotation;
                }
            }
            
            lockOnTimer += Time.deltaTime;
            yield return null;
        }
        
        isLockedOn = false;
        Debug.Log("CameraZoomController: Lock-on complete, returning to normal");
        
        // Phase 3: Return to normal
        yield return StartCoroutine(ReturnToNormal());
        
        // Re-enable player control
        EnablePlayerControl();
        
        isZooming = false;
        currentTarget = null;
        zoomCoroutine = null;
        
        Debug.Log("CameraZoomController: Zoom sequence complete");
    }
    
    /// <summary>
    /// Smoothly transition between FOV values
    /// </summary>
    private IEnumerator ZoomToFOV(float fromFOV, float toFOV, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            float curveValue = zoomCurve.Evaluate(progress);
            
            targetCamera.fieldOfView = Mathf.Lerp(fromFOV, toFOV, curveValue);
            
            yield return null;
        }
        
        // Ensure final value is set
        targetCamera.fieldOfView = toFOV;
    }
    
    /// <summary>
    /// Return camera to normal state
    /// </summary>
    private IEnumerator ReturnToNormal()
    {
        // Zoom back to normal FOV
        yield return StartCoroutine(ZoomToFOV(zoomedFOV, normalFOV, returnToNormalDuration));
        
        // Smoothly return rotation to original (or forward)
        float elapsed = 0f;
        Quaternion startRotation = targetCamera.transform.rotation;
        
        while (elapsed < returnToNormalDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / returnToNormalDuration;
            
            targetCamera.transform.rotation = Quaternion.Slerp(
                startRotation, 
                originalRotation, 
                progress
            );
            
            yield return null;
        }
        
        // Ensure final rotation is set
        targetCamera.transform.rotation = originalRotation;
    }
    
    /// <summary>
    /// Disable player movement during zoom effect
    /// </summary>
    private void DisablePlayerControl()
    {
        if (playerController != null)
        {
            wasPlayerControlEnabled = playerController.enabled;
            playerController.enabled = false;
            Debug.Log("CameraZoomController: Player control disabled");
        }
    }
    
    /// <summary>
    /// Re-enable player movement after zoom effect
    /// </summary>
    private void EnablePlayerControl()
    {
        if (playerController != null && wasPlayerControlEnabled)
        {
            playerController.enabled = true;
            Debug.Log("CameraZoomController: Player control enabled");
        }
    }
    
    /// <summary>
    /// Cancel zoom effect immediately (emergency stop)
    /// </summary>
    [ContextMenu("Cancel Zoom")]
    public void CancelZoom()
    {
        if (zoomCoroutine != null)
        {
            StopCoroutine(zoomCoroutine);
            zoomCoroutine = null;
        }
        
        // Reset state
        isZooming = false;
        isLockedOn = false;
        currentTarget = null;
        
        // Reset camera
        targetCamera.fieldOfView = normalFOV;
        targetCamera.transform.rotation = originalRotation;
        
        // Re-enable player control
        EnablePlayerControl();
        
        Debug.Log("CameraZoomController: Zoom cancelled");
    }
    
    /// <summary>
    /// Test the zoom effect (for debugging)
    /// </summary>
    [ContextMenu("Test Zoom Effect")]
    public void TestZoomEffect()
    {
        // Create a temporary target in front of camera
        GameObject tempTarget = new GameObject("TempZoomTarget");
        tempTarget.transform.position = targetCamera.transform.position + targetCamera.transform.forward * 5f;
        
        ZoomOnTarget(tempTarget.transform);
        
        // Destroy temp target after zoom completes
        StartCoroutine(DestroyAfterDelay(tempTarget, lockOnDuration + zoomDuration + returnToNormalDuration + 0.5f));
    }
    
    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            Destroy(obj);
        }
    }
    
    /// <summary>
    /// Check if currently zooming or locked on
    /// </summary>
    public bool IsActive()
    {
        return isZooming || isLockedOn;
    }
    
    // Visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 0.3f);
        }
    }
}
