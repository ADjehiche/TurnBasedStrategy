using UnityEngine;
using System.Collections;

/// <summary>
/// Ultra-simple blue fragment collection that just teleports player to maze entrance
/// No complex guidance, no spawning, just direct teleportation
/// </summary>
public class TeleportBlueFragmentCollectable : MonoBehaviour, IInteractable
{
    [Header("Collection Settings")]
    [SerializeField] private float collectDelay = 1f;
    [SerializeField] private bool debugMode = true;
    
    [Header("Teleportation")]
    [SerializeField] private Transform teleportTarget; // Manual assignment option
    [SerializeField] private float teleportDelay = 2f; // Delay after dialogue before teleport
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioSource collectSound;
    
    private bool hasBeenCollected = false;
    private LevelTwoCaptionController captionController;
    
    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }
    
    void Start()
    {
        // Find caption controller
        captionController = FindFirstObjectByType<LevelTwoCaptionController>();
        
        if (captionController == null)
        {
            Debug.LogError("[TeleportBlueFragmentCollectable] LevelTwoCaptionController not found!");
        }
        
        // Auto-find teleport target if not manually assigned
        if (teleportTarget == null)
        {
            FindTeleportTarget();
        }
        
        if (debugMode)
        {
            Debug.Log($"[TeleportBlueFragmentCollectable] Initialized. Teleport target: {(teleportTarget != null ? teleportTarget.position.ToString() : "NULL")}");
        }
    }
    
    private void FindTeleportTarget()
    {
        // Look for maze entrance marker
        GameObject entrance = GameObject.Find("MazeEntrance");
        if (entrance != null)
        {
            teleportTarget = entrance.transform;
            if (debugMode)
                Debug.Log("[TeleportBlueFragmentCollectable] Found MazeEntrance marker");
            return;
        }
        
        // Look for maze detection trigger (created automatically by MazeGenerator)
        GameObject triggerObj = GameObject.Find("MazeDetectionTrigger");
        if (triggerObj != null)
        {
            teleportTarget = triggerObj.transform;
            if (debugMode)
                Debug.Log("[TeleportBlueFragmentCollectable] Using MazeDetectionTrigger as teleport target");
            return;
        }
        
        // Look for MazeGenerator as fallback
        MazeGenerator mazeGen = FindFirstObjectByType<MazeGenerator>();
        if (mazeGen != null)
        {
            teleportTarget = mazeGen.transform;
            if (debugMode)
                Debug.Log("[TeleportBlueFragmentCollectable] Using MazeGenerator position as teleport target");
            return;
        }
        
        Debug.LogWarning("[TeleportBlueFragmentCollectable] No teleport target found - assign manually in inspector");
    }
    
    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        
        if (hasBeenCollected)
        {
            if (debugMode)
                Debug.Log("[TeleportBlueFragmentCollectable] Already collected");
            return;
        }
        
        if (debugMode)
            Debug.Log("[TeleportBlueFragmentCollectable] ✅ Blue fragment collected - starting teleport sequence!");
        
        // Trigger Level Two objective - maze explored
        var objectiveManager = FindFirstObjectByType<SimpleLevelTwoObjectives>();
        if (objectiveManager != null)
        {
            objectiveManager.OnMazeExplored();
            Debug.Log("[TeleportBlueFragmentCollectable] Maze explored objective triggered");
        }
        
        hasBeenCollected = true;
        interactSuccessful = true;
        
        StartCoroutine(TeleportSequence());
    }
    
    public void EndInteraction()
    {
        // Nothing needed
    }
    
    /// <summary>
    /// Simple sequence: effects → dialogue → teleport
    /// </summary>
    private IEnumerator TeleportSequence()
    {
        if (debugMode)
            Debug.Log("[TeleportBlueFragmentCollectable] Starting teleport sequence");
        
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
        yield return new WaitForSeconds(collectDelay);
        
        // Show dialogue
        if (captionController != null)
        {
            if (debugMode)
                Debug.Log("[TeleportBlueFragmentCollectable] Starting dialogue");
            
            yield return captionController.ShowDialogue("Blue Fragment", "Thank you for freeing me from this maze!", 3f);
            yield return new WaitForSeconds(0.5f);
            yield return captionController.ShowDialogue("Player", "We should get out of here quickly!", 2f);
            yield return new WaitForSeconds(0.5f);
            yield return captionController.ShowDialogue("Blue Fragment", "I'll transport us to the exit!", 2f);
            
            if (debugMode)
                Debug.Log("[TeleportBlueFragmentCollectable] Dialogue complete");
        }
        
        // Wait a moment before teleport
        yield return new WaitForSeconds(teleportDelay);
        
        // TELEPORT!
        TeleportPlayerToExit();
        
        // Destroy this collectible
        Destroy(gameObject);
        
        OnInteractionComplete?.Invoke(this);
        
        if (debugMode)
            Debug.Log("[TeleportBlueFragmentCollectable] Teleport sequence complete");
    }
    
    /// <summary>
    /// Teleport player to maze exit
    /// </summary>
    private void TeleportPlayerToExit()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[TeleportBlueFragmentCollectable] ❌ Player not found for teleportation!");
            return;
        }
        
        if (teleportTarget == null)
        {
            Debug.LogError("[TeleportBlueFragmentCollectable] ❌ Teleport target not set!");
            return;
        }
        
        // Calculate teleport position (slightly offset from target to avoid being inside walls)
        Vector3 teleportPosition = teleportTarget.position;
        
        // If using trigger, teleport to the edge (outside the maze)
        if (teleportTarget.name.Contains("Trigger"))
        {
            // Move player to the "entrance" side of the trigger (assuming entrance is at negative Z)
            teleportPosition += Vector3.back * 5f; // 5 units back from trigger center
        }
        
        // Ensure player is on ground level
        teleportPosition.y = player.transform.position.y;
        
        if (debugMode)
            Debug.Log($"[TeleportBlueFragmentCollectable] 🚀 Teleporting player from {player.transform.position} to {teleportPosition}");
        
        // Teleport!
        player.transform.position = teleportPosition;
        
        // Trigger Level Two objective - returned to archive
        var objectiveManager = FindFirstObjectByType<SimpleLevelTwoObjectives>();
        if (objectiveManager != null)
        {
            objectiveManager.OnReturnedToArchive();
            Debug.Log("[TeleportBlueFragmentCollectable] Returned to archive objective triggered");
        }
        
        // Optional: Show completion message
        if (captionController != null)
        {
            StartCoroutine(ShowTeleportCompleteMessage());
        }
        
        Debug.Log("[TeleportBlueFragmentCollectable] ✅ Player teleported to maze exit!");
    }
    
    /// <summary>
    /// Show message after teleportation
    /// </summary>
    private IEnumerator ShowTeleportCompleteMessage()
    {
        yield return new WaitForSeconds(1f);
        yield return captionController.ShowDialogue("Blue Fragment", "Here we are - outside the maze! You're free!", 3f);
    }
}