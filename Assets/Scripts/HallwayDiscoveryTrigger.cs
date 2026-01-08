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
        if (hasTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            
            if (captionController != null)
            {
                captionController.TriggerHallwayDiscovery();
                Debug.Log("[HallwayDiscoveryTrigger] Player entered hallway area - triggering discovery sequence");
            }
            else
            {
                Debug.LogWarning("[HallwayDiscoveryTrigger] No LevelTwoCaptionController found!");
            }
            
            if (destroyAfterTrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
