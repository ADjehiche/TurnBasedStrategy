using UnityEngine;
using System.Collections;

/// <summary>
/// Triggers a warning sequence before the skeleton battle
/// Only triggers once - checks if skeleton already defeated
/// Automatically starts battle after warning sequence
/// Pans camera to look at skeleton during warning
/// Destroys itself after skeleton is defeated
/// </summary>
public class SkeletonWarningTrigger : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private float warningDuration = 3f;
    
    [Header("Captions")]
    [SerializeField] private string warningMessage1 = "[You] Wait... movement ahead.";
    [SerializeField] private string warningMessage2 = "[System] Warning: Guardian detected";
    [SerializeField] private string warningMessage3 = "[System] Prepare for battle!";
    
    [Header("Camera Zoom")]
    [SerializeField] private Transform skeletonTarget;
    [SerializeField] private float zoomFOV = 45f;
    [SerializeField] private float zoomSpeed = 3f;
    [SerializeField] private float verticalOffset = 1.5f; // Height offset to aim at skeleton's upper body
    [SerializeField] private bool enableCameraZoom = true;
    
    [Header("Battle")]
    [SerializeField] private GameManager gameManager;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasTriggered = false;
    private Camera playerCamera;
    private float originalFOV;
    private Quaternion originalCameraRotation;
    
    void Start()
    {
        // Get player camera
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            originalFOV = playerCamera.fieldOfView;
        }
        
        // Auto-find GameManager
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("[SkeletonWarningTrigger] No GameManager found!");
            }
        }
        
        // Auto-find skeleton if not assigned
        if (skeletonTarget == null && enableCameraZoom)
        {
            // Try to find by tag or name
            GameObject skeleton = GameObject.FindGameObjectWithTag("Enemy");
            if (skeleton == null)
            {
                skeleton = GameObject.Find("Skeleton");
            }
            
            if (skeleton != null)
            {
                skeletonTarget = skeleton.transform;
                if (showDebugLogs)
                    Debug.Log($"[SkeletonWarningTrigger] Auto-found skeleton: {skeleton.name}");
            }
            else
            {
                Debug.LogWarning("[SkeletonWarningTrigger] Skeleton target not found! Zoom disabled.");
                enableCameraZoom = false;
            }
        }
        
        // If skeleton already defeated, destroy this trigger immediately
        if (GameSession.EnemyDefeated)
        {
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] Skeleton already defeated - destroying trigger");
            
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Don't trigger if skeleton already defeated
        if (GameSession.EnemyDefeated)
        {
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] Skeleton defeated - ignoring trigger");
            return;
        }
        
        if (hasTriggered || !other.CompareTag("Player"))
            return;
        
        hasTriggered = true;
        
        if (showDebugLogs)
            Debug.Log("[SkeletonWarningTrigger] Player entered warning zone");
        
        StartCoroutine(WarningSequence());
    }
    
    private IEnumerator WarningSequence()
    {
        // Lock movement during warning
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.LockMovement("Skeleton warning");
        
        // Store original camera rotation
        if (playerCamera != null)
        {
            originalCameraRotation = playerCamera.transform.rotation;
        }
        
        // Start camera zoom to skeleton
        if (enableCameraZoom && playerCamera != null && skeletonTarget != null)
        {
            yield return StartCoroutine(ZoomToSkeleton());
        }
        
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(warningMessage1, 1f);
            yield return new WaitForSeconds(1.2f);
            
            CaptionManager.Instance.ShowSystemMessage(warningMessage2, 1f);
            yield return new WaitForSeconds(1.2f);
            
            CaptionManager.Instance.ShowInstruction(warningMessage3, 0.8f);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            Debug.Log(warningMessage1);
            Debug.Log(warningMessage2);
            Debug.Log(warningMessage3);
            yield return new WaitForSeconds(warningDuration);
        }
        
        // Zoom camera back out
        if (enableCameraZoom && playerCamera != null)
        {
            yield return StartCoroutine(ZoomOut());
        }
        
        // Unlock movement before battle (battle system will handle its own locks)
        if (PlayerMovementLock.Instance != null)
            PlayerMovementLock.Instance.UnlockMovement("Warning complete - starting battle");
        
        // Save checkpoint before battle starts
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            GameSession.SaveCheckpoint(
                player.transform.position,
                player.transform.rotation
            );
            
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] Checkpoint saved before battle");
        }
        
        // Small pause before battle starts
        yield return new WaitForSeconds(0.5f);
        
        // Automatically start battle
        if (gameManager != null)
        {
            if (showDebugLogs)
                Debug.Log("[SkeletonWarningTrigger] ⚔️ Starting battle!");
            
            gameManager.StartBattle();
        }
        else
        {
            Debug.LogError("[SkeletonWarningTrigger] Cannot start battle - GameManager is null!");
        }
        
        // Disable collider so it won't trigger again
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }
    
    private IEnumerator ZoomToSkeleton()
    {
        float elapsedTime = 0f;
        float zoomDuration = 1.5f;
        
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            
            // Zoom FOV
            playerCamera.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            
            // Rotate camera to look at skeleton (with vertical offset for better framing)
            Vector3 targetPosition = skeletonTarget.position + Vector3.up * verticalOffset;
            Vector3 directionToSkeleton = targetPosition - playerCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(directionToSkeleton);
            playerCamera.transform.rotation = Quaternion.Slerp(originalCameraRotation, targetRotation, t);
            
            yield return null;
        }
    }
    
    private IEnumerator ZoomOut()
    {
        float elapsedTime = 0f;
        float zoomDuration = 0.5f;
        float startFOV = playerCamera.fieldOfView;
        Quaternion startRotation = playerCamera.transform.rotation;
        
        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            
            // Restore FOV
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            
            // Restore rotation
            playerCamera.transform.rotation = Quaternion.Slerp(startRotation, originalCameraRotation, t);
            
            yield return null;
        }
        
        // Ensure exact restoration
        playerCamera.fieldOfView = originalFOV;
        playerCamera.transform.rotation = originalCameraRotation;
    }
}
