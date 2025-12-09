using UnityEngine;

/// <summary>
/// Zone 2: Acquisition (Interact)
/// Player must pick up the key from the pedestal
/// Caption shows when player gets close
/// </summary>
public class Zone2_Interact : TutorialZone
{
    [Header("Zone 2 Specific")]
    public GameObject keyObject; // The key on the pedestal (public so Zone3 can destroy it)
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private float captionTriggerRange = 5f; // Distance to show caption
    
    private Transform player;
    private bool playerInRange = false;
    private bool captionShown = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Find player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogError("[Zone2_Interact] Player not found!");
        }
        
        if (keyObject == null)
        {
            Debug.LogError("[Zone2_Interact] Key object not assigned!");
        }
        
        // Don't show caption yet - wait for player to get close
    }
    
    void Update()
    {
        if (isComplete || player == null || keyObject == null) return;
        
        // Check if player is close enough to show caption
        float distanceToKey = Vector3.Distance(player.position, keyObject.transform.position);
        
        if (!captionShown && distanceToKey <= captionTriggerRange)
        {
            // Show caption when player gets close
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowInstruction("Press E to acquire the object", 999f);
                Debug.Log($"[Zone2] Showing caption - Distance: {distanceToKey}");
            }
            else
            {
                Debug.LogWarning("[Zone2] CaptionManager not found!");
            }
            captionShown = true;
        }
        
        // Check if key was picked up by PickUpScript
        // PickUpScript sets Rigidbody.isKinematic = true when picking up
        Rigidbody keyRb = keyObject.GetComponent<Rigidbody>();
        if (keyRb != null && keyRb.isKinematic)
        {
            Debug.Log("[Zone2] Key picked up by player (Rigidbody is kinematic)!");
            OnKeyPickedUp();
        }
    }
    
    private void OnKeyPickedUp()
    {
        // Key was picked up by PickUpScript naturally
        // Just show the completion message and complete the zone
        
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage("Key acquired - Use it on the door ahead", 4f);
        }
        
        CompleteZone();
    }
    
    public bool HasKey()
    {
        return isComplete; // Player has key if Zone 2 is complete
    }
    
    void OnDrawGizmosSelected()
    {
        if (keyObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(keyObject.transform.position, interactionRange);
        }
    }
}
