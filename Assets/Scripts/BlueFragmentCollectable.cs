using UnityEngine;

/// <summary>
/// Attach to the blue fragment collectible in the maze
/// Handles collection and notifies the MazeGuidanceController
/// </summary>
public class BlueFragmentCollectable : MonoBehaviour, IInteractable
{
    [Header("Collection Settings")]
    [SerializeField] private GameObject blueFragmentFollowerPrefab; // Prefab to spawn as follower
    [SerializeField] private Transform spawnPosition; // Where to spawn the follower (near player)
    [SerializeField] private float collectDelay = 1f; // Delay before collection completes
    [SerializeField] private bool debugMode = false;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem collectEffect; // Optional particle effect
    [SerializeField] private AudioSource collectSound; // Optional sound effect
    
    private bool hasBeenCollected = false;
    private MazeGuidanceController guidanceController;
    
    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }
    
    void Start()
    {
        // If already collected previously, destroy this (prevents duplicates in maze)
        if (GameSession.HasCollectedBlueFragment)
        {
            if (debugMode) Debug.Log("[BlueFragmentCollectable] Already collected, destroying duplicate from maze!");
            Destroy(gameObject);
            return;
        }
        
        // Find the guidance controller
        guidanceController = FindFirstObjectByType<MazeGuidanceController>();
        
        if (guidanceController == null)
        {
            Debug.LogWarning("[BlueFragmentCollectable] MazeGuidanceController not found!");
        }
        
        // Auto-find spawn position if not set (use player position)
        if (spawnPosition == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                spawnPosition = player.transform;
            }
        }
    }
    
    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        
        if (hasBeenCollected)
        {
            if (debugMode)
                Debug.Log("[BlueFragmentCollectable] Already collected");
            return;
        }
        
        if (debugMode)
            Debug.Log("[BlueFragmentCollectable] Blue fragment collected!");
        
        hasBeenCollected = true;
        interactSuccessful = true;
        
        StartCoroutine(CollectionSequence());
    }
    
    public void EndInteraction()
    {
        // Nothing needed for end interaction
    }
    
    /// <summary>
    /// Handle the collection sequence with effects and spawning
    /// </summary>
    private System.Collections.IEnumerator CollectionSequence()
    {
        // Play effects
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        if (collectSound != null)
        {
            collectSound.Play();
        }
        
        // Wait for effects
        yield return new UnityEngine.WaitForSeconds(collectDelay);
        
        // Mark as collected for persistence
        GameSession.HasCollectedBlueFragment = true;
        
        // Become follower (same object, just change behavior)
        BecomeFollower();
        
        // Notify guidance controller
        if (guidanceController != null)
        {
            guidanceController.OnBlueFragmentCollected();
        }
        
        OnInteractionComplete?.Invoke(this);
    }
    
    /// <summary>
    /// Transform this collectible into a follower
    /// </summary>
    private void BecomeFollower()
    {
        // Get the CompanionFollower on this same object
        CompanionFollower follower = GetComponent<CompanionFollower>();
        if (follower != null)
        {
            follower.StartFollowing();
            GameSession.BlueCompanionActive = true;
            
            if (debugMode)
                Debug.Log("[BlueFragmentCollectable] Became follower (same object)");
        }
        else
        {
            Debug.LogError("[BlueFragmentCollectable] No CompanionFollower on this object!");
        }
        
        // Disable this collectible script (no longer interactable)
        this.enabled = false;
        
        // Remove collider so player can't interact again
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }
    
    /// <summary>
    /// Get the guidance controller reference (for MazeGuidanceController to use)
    /// </summary>
    public MazeGuidanceController GetGuidanceController()
    {
        return guidanceController;
    }
}