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
            Debug.Log("[HallwayDiscoveryTrigger] Attempting to find SimpleLevelTwoObjectives in scene...");
            var objectiveManager = FindFirstObjectByType<SimpleLevelTwoObjectives>();
            if (objectiveManager != null)
            {
                Debug.Log("[HallwayDiscoveryTrigger] Found SimpleLevelTwoObjectives, calling OnTunnelEntered()");
                objectiveManager.OnTunnelEntered();
                Debug.Log("[HallwayDiscoveryTrigger] Tunnel entered objective triggered");
            }
            else
            {
                Debug.LogError("[HallwayDiscoveryTrigger] SimpleLevelTwoObjectives NOT found in scene! Objective will not progress.");
            }

            if (destroyAfterTrigger)
            {
                Debug.Log("[HallwayDiscoveryTrigger] Destroying trigger after activation.");
                Destroy(gameObject);
            }
        }
    }
}
