using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Detects when the player looks at enemies and triggers camera zoom/caption effects.
/// Uses raycasting from the camera to detect enemy objects within view.
/// </summary>
public class EnemyLookDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionDistance = 50f;
    [SerializeField] private float detectionAngle = 30f; // How centered the enemy needs to be
    [SerializeField] private string enemyTag = "Enemy"; // Tag to identify enemies
    [SerializeField] private float detectionCooldown = 0.5f; // Time between detection checks
    
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CameraZoomController zoomController;
    [SerializeField] private LevelOneCaptionController captionController;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private bool enableDetection = true;
    [SerializeField] private bool verboseLogging = true; // Enable detailed per-frame logging
    
    // Track which enemies have already been spotted
    private HashSet<GameObject> spottedEnemies = new HashSet<GameObject>();
    private float lastDetectionTime = 0f;
    private int frameCounter = 0;
    
    void Start()
    {
        Debug.Log("=== ENEMY LOOK DETECTOR START ===");
        
        // Auto-find components if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("EnemyLookDetector: No camera found! Please assign the player camera.");
            }
            else
            {
                Debug.Log($"EnemyLookDetector: Found camera: {playerCamera.name}");
            }
        }
        else
        {
            Debug.Log($"EnemyLookDetector: Using assigned camera: {playerCamera.name}");
        }
        
        if (zoomController == null)
        {
            zoomController = FindFirstObjectByType<CameraZoomController>();
            if (zoomController == null)
            {
                Debug.LogWarning("EnemyLookDetector: No CameraZoomController found in scene.");
            }
            else
            {
                Debug.Log($"EnemyLookDetector: Found CameraZoomController on: {zoomController.gameObject.name}");
            }
        }
        
        if (captionController == null)
        {
            captionController = FindFirstObjectByType<LevelOneCaptionController>();
            if (captionController == null)
            {
                Debug.LogWarning("EnemyLookDetector: No LevelOneCaptionController found in scene.");
            }
            else
            {
                Debug.Log($"EnemyLookDetector: Found LevelOneCaptionController on: {captionController.gameObject.name}");
            }
        }
        
        Debug.Log($"EnemyLookDetector: Settings - Distance: {detectionDistance}m, Angle: {detectionAngle}°, Tag: '{enemyTag}', Cooldown: {detectionCooldown}s");
        Debug.Log($"EnemyLookDetector: Detection enabled: {enableDetection}, Verbose logging: {verboseLogging}");
        Debug.Log("=== INITIALIZATION COMPLETE ===");
    }
    
    void Update()
    {
        if (playerCamera == null)
            return;
        
        // Always do detection for logging, but only trigger events if enabled
        // Cooldown check to avoid spamming raycasts
        if (Time.time - lastDetectionTime < detectionCooldown)
            return;
        
        DetectEnemyInView();
        lastDetectionTime = Time.time;
    }
    
    /// <summary>
    /// Perform raycast detection to check if player is looking at an enemy
    /// </summary>
    private void DetectEnemyInView()
    {
        frameCounter++;
        
        // Raycast from the center of the screen (where player is looking / mouse cursor)
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        
        // Debug visualization
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * detectionDistance, Color.yellow, detectionCooldown);
        }
        
        // Check if we hit something within detection distance
        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            // ===== ALWAYS LOG WHAT WE'RE LOOKING AT (color-coded for visibility) =====
            string logMessage = $"<color=cyan>[Looking At]</color> <b>{hit.collider.gameObject.name}</b> | Tag: <color=orange>'{hit.collider.tag}'</color> | Distance: <color=lime>{hit.distance:F2}m</color> | Layer: <color=yellow>{LayerMask.LayerToName(hit.collider.gameObject.layer)}</color>";
            Debug.Log(logMessage);
            
            // Check if the object has the enemy tag (only trigger events if detection enabled)
            if (hit.collider.CompareTag(enemyTag))
            {
                GameObject hitObject = hit.collider.gameObject;
                
                // Additional angle check to ensure enemy is centered enough
                Vector3 directionToEnemy = (hit.point - ray.origin).normalized;
                float angle = Vector3.Angle(ray.direction, directionToEnemy);
                
                Debug.Log($"<color=green>[ENEMY FOUND!]</color> Name: {hitObject.name} | Angle: {angle:F2}° (max: {detectionAngle}°) | Already spotted: {spottedEnemies.Contains(hitObject)} | Detection enabled: {enableDetection}");
                
                if (enableDetection && angle <= detectionAngle && !spottedEnemies.Contains(hitObject))
                {
                    Debug.Log($"<color=lime>>>> TRIGGERING ENEMY SPOTTED EVENT! <<<</color>");
                    OnEnemySpotted(hitObject, hit.point);
                }
                else if (angle > detectionAngle)
                {
                    Debug.LogWarning($"Enemy found but angle too wide! {angle:F2}° > {detectionAngle}° (not triggering)");
                }
                else if (spottedEnemies.Contains(hitObject))
                {
                    Debug.Log($"Enemy already spotted previously, not triggering again.");
                }
                else if (!enableDetection)
                {
                    Debug.LogWarning($"Enemy found but detection is DISABLED in Inspector!");
                }
            }
        }
        else
        {
            // ===== ALWAYS LOG WHEN NOT LOOKING AT ANYTHING =====
            Debug.Log($"<color=grey>[Looking At] <b>Nothing</b> (no object within {detectionDistance}m)</color>");
        }
    }
    
    /// <summary>
    /// Called when an enemy is spotted for the first time
    /// </summary>
    private void OnEnemySpotted(GameObject enemy, Vector3 hitPoint)
    {
        // Mark this enemy as spotted so we don't trigger again
        spottedEnemies.Add(enemy);
        
        Debug.Log($"EnemyLookDetector: Enemy spotted! {enemy.name}");
        
        // Trigger camera zoom and lock
        if (zoomController != null)
        {
            zoomController.ZoomOnTarget(enemy.transform);
        }
        else
        {
            Debug.LogWarning("EnemyLookDetector: No zoom controller to trigger zoom effect");
        }
        
        // Show caption
        if (captionController != null)
        {
            captionController.OnEnemySpotted();
        }
        else
        {
            Debug.LogWarning("EnemyLookDetector: No caption controller to show message");
        }
        
        // Play suspenseful audio if you have it
        // AudioManager.Instance?.Play("EnemySpotted");
    }
    
    /// <summary>
    /// Reset spotted enemies (useful for testing or level restart)
    /// </summary>
    [ContextMenu("Reset Spotted Enemies")]
    public void ResetSpottedEnemies()
    {
        spottedEnemies.Clear();
        Debug.Log("EnemyLookDetector: Spotted enemies list cleared");
    }
    
    /// <summary>
    /// Manually trigger enemy detection for a specific enemy (for testing)
    /// </summary>
    public void ManuallySpotEnemy(GameObject enemy)
    {
        if (enemy != null && !spottedEnemies.Contains(enemy))
        {
            OnEnemySpotted(enemy, enemy.transform.position);
        }
    }
    
    /// <summary>
    /// Enable or disable detection at runtime
    /// </summary>
    public void SetDetectionEnabled(bool enabled)
    {
        enableDetection = enabled;
        Debug.Log($"EnemyLookDetector: Detection {(enabled ? "enabled" : "disabled")}");
    }
    
    // Visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
            return;
        
        // Draw detection cone
        Gizmos.color = Color.yellow;
        Vector3 forward = playerCamera.transform.forward;
        Vector3 origin = playerCamera.transform.position;
        
        // Draw the center ray
        Gizmos.DrawRay(origin, forward * detectionDistance);
        
        // Draw detection angle cone
        Vector3 rightBoundary = Quaternion.Euler(0, detectionAngle, 0) * forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -detectionAngle, 0) * forward;
        
        Gizmos.color = Color.yellow * 0.5f;
        Gizmos.DrawRay(origin, rightBoundary * detectionDistance);
        Gizmos.DrawRay(origin, leftBoundary * detectionDistance);
        
        // Draw spotted enemies
        Gizmos.color = Color.red;
        foreach (GameObject enemy in spottedEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
            }
        }
        
        // Draw crosshair at center to show what we're looking at
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            Gizmos.color = hit.collider.CompareTag(enemyTag) ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(hit.point, 0.2f);
            Gizmos.DrawLine(origin, hit.point);
        }
    }
}
