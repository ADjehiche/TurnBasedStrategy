using UnityEngine;

/// <summary>
/// Zone 1: Motor Reflexes (WASD)
/// Player must walk into the spotlight to complete
/// </summary>
public class Zone1_Movement : TutorialZone
{
    [Header("Zone 1 Specific")]
    [SerializeField] private Transform targetSpotlight; // The spotlight the player must reach
    [SerializeField] private float completionRadius = 2f; // How close to get to spotlight
    
    private Transform player;
    
    protected override void Start()
    {
        base.Start();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("[Zone1_Movement] Player not found!");
        }
        
        if (targetSpotlight == null)
        {
            Debug.LogError("[Zone1_Movement] Target spotlight not assigned!");
        }
        
        // Show movement instruction caption
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowInstruction("Use WASD to move towards the light", 999f);
        }
    }
    
    void Update()
    {
        if (isComplete || player == null || targetSpotlight == null) return;
        
        // Check if player reached the spotlight
        float distance = Vector3.Distance(player.position, targetSpotlight.position);
        
        if (distance <= completionRadius)
        {
            CompleteZone();
        }
    }
    
    protected override void CompleteZone()
    {
        // Hide the instruction caption
        if (CaptionManager.Instance != null)
        {
            // Hide current caption before showing completion message
            CaptionManager.Instance.HideCaption();
            
            // Show completion message
            CaptionManager.Instance.ShowSystemMessage("Movement initialized", 2f);
        }
        
        base.CompleteZone();
    }
    
    void OnDrawGizmosSelected()
    {
        if (targetSpotlight != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetSpotlight.position, completionRadius);
        }
    }
}
