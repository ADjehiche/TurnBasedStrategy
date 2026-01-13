using UnityEngine;

/// <summary>
/// Trigger zone that calls LevelTwoCaptionController when player enters
/// Attach this to a GameObject with a Collider set to "Is Trigger"
/// </summary>
[RequireComponent(typeof(Collider))]
public class HallwayDiscoveryTrigger : MonoBehaviour
{
    [SerializeField] private LevelTwoCaptionController captionController;
    [SerializeField] private bool destroyAfterTrigger = true;
    
    private bool hasTriggered = false;
    
    void Start()
    {
        // Auto-find caption controller if not assigned
        if (captionController == null)
        {
            captionController = FindFirstObjectByType<LevelTwoCaptionController>();
        }
        
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[HallwayDiscoveryTrigger] Collider should be set as 'Is Trigger'!");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[HallwayDiscoveryTrigger] OnTriggerEnter called by: {other.gameObject.name}, tag: {other.tag}, hasTriggered: {hasTriggered}");
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("[HallwayDiscoveryTrigger] Player trigger detected, proceeding...");

            if (captionController != null)
            {
                captionController.TriggerHallwayDiscovery();
                Debug.Log("[HallwayDiscoveryTrigger] Player entered hallway area - triggering discovery sequence");
            }
            else
            {
                Debug.LogWarning("[HallwayDiscoveryTrigger] No LevelTwoCaptionController found!");
            }

            // Trigger Level Two objective - tunnel entered
            Debug.Log("[HallwayDiscoveryTrigger] ===== OBJECTIVE TRIGGER DEBUG START =====");
            Debug.Log("[HallwayDiscoveryTrigger] Attempting to find SimpleLevelTwoObjectives in scene...");
            
            SimpleLevelTwoObjectives objectiveManager = FindFirstObjectByType<SimpleLevelTwoObjectives>();
            
            Debug.Log($"[HallwayDiscoveryTrigger] FindFirstObjectByType result: {(objectiveManager != null ? "FOUND" : "NULL")}");
            
            if (objectiveManager != null)
            {
                Debug.Log("[HallwayDiscoveryTrigger] Calling objectiveManager.OnTunnelEntered() NOW...");
                objectiveManager.OnTunnelEntered();
                Debug.Log("[HallwayDiscoveryTrigger] OnTunnelEntered() call completed");
            }
            else
            {
                Debug.LogError("[HallwayDiscoveryTrigger] SimpleLevelTwoObjectives NOT found in scene! Objective will not progress.");
                
                // Extra debug: List all SimpleLevelTwoObjectives in scene (including inactive)
                var allObjectives = FindObjectsByType<SimpleLevelTwoObjectives>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                Debug.LogError($"[HallwayDiscoveryTrigger] Total SimpleLevelTwoObjectives in scene (including inactive): {allObjectives.Length}");
            }
            Debug.Log("[HallwayDiscoveryTrigger] ===== OBJECTIVE TRIGGER DEBUG END =====");

            if (destroyAfterTrigger)
            {
                Debug.Log("[HallwayDiscoveryTrigger] Destroying trigger after activation.");
                Destroy(gameObject);
            }
        }
    }
}
