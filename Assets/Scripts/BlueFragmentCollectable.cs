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
        
        // Spawn the follower version
        SpawnBlueFragmentFollower();
        
        // Notify guidance controller
        if (guidanceController != null)
        {
            guidanceController.OnBlueFragmentCollected();
        }
        
        // Destroy this collectible
        Destroy(gameObject);
        
        OnInteractionComplete?.Invoke(this);
    }
    
    /// <summary>
    /// Spawn the blue fragment as a follower
    /// </summary>
    private void SpawnBlueFragmentFollower()
    {
        if (blueFragmentFollowerPrefab == null)
        {
            Debug.LogError("[BlueFragmentCollectable] Blue fragment follower prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos = spawnPosition != null ? spawnPosition.position : transform.position;
        
        // Spawn slightly to the side of the player
        spawnPos += Vector3.right * 2f + Vector3.up * 0.5f;
        
        GameObject follower = Instantiate(blueFragmentFollowerPrefab, spawnPos, Quaternion.identity);
        
        // Make sure it starts following
        CompanionFollower followerScript = follower.GetComponent<CompanionFollower>();
        if (followerScript != null)
        {
            followerScript.StartFollowing();
        }
        
        if (debugMode)
            Debug.Log("[BlueFragmentCollectable] Spawned blue fragment follower");
    }
    
    /// <summary>
    /// Get the guidance controller reference (for MazeGuidanceController to use)
    /// </summary>
    public MazeGuidanceController GetGuidanceController()
    {
        return guidanceController;
    }
}