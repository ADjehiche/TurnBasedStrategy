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
        
        if (debugMode)
        {
            Debug.Log("[TeleportBlueFragmentCollectable] Initialized for Guidance Trigger.");
        }
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
            Debug.Log("[TeleportBlueFragmentCollectable] ✅ Blue fragment collected - Triggering Guidance!");
        
        // Trigger Level Two objective - maze explored
        var objectiveManager = FindFirstObjectByType<SimpleLevelTwoObjectives>();
        if (objectiveManager != null)
        {
            objectiveManager.OnMazeExplored();
            Debug.Log("[TeleportBlueFragmentCollectable] Maze explored objective triggered");
        }
        
        hasBeenCollected = true;
        interactSuccessful = true;
        
        // 1. Set Game Session Flags
        GameSession.HasCollectedBlueFragment = true;
        GameSession.BlueCompanionActive = true;
        
        // 2. Check for Boss Door Unlock logic
        if (GameSession.CanUnlockBossDoor)
        {
            Debug.Log("[TeleportBlueFragmentCollectable] 🗝️ Both fragments collected! Attempting to trigger boss door cutscene...");
            LevelTwoReturnManager returnManager = FindFirstObjectByType<LevelTwoReturnManager>();
            if (returnManager != null)
            {
                returnManager.TriggerBossDoorCutscene();
            }
        }
        
        // 3. Become Follower IMMEDIATELY so we can pass reference
        CompanionFollower newFollower = BecomeFollower();
        
        // Disable CompanionInteraction to prevent it from overriding guidance behavior
        CompanionInteraction compInteraction = GetComponent<CompanionInteraction>();
        if (compInteraction != null)
        {
            compInteraction.enabled = false;
        }
        
        // 4. Trigger Guidance Controller to lead player out
        MazeGuidanceController guidance = FindFirstObjectByType<MazeGuidanceController>();
        if (guidance != null) 
        {
            guidance.OnBlueFragmentCollected(newFollower);
        }
        else
        {
             Debug.LogError("[TeleportBlueFragmentCollectable] MazeGuidanceController not found! Cannot guide out.");
        }
        
        // Play effects
        if (collectEffect != null) collectEffect.Play();
        if (collectSound != null) collectSound.Play();

        OnInteractionComplete?.Invoke(this);
    }
    
    public void EndInteraction()
    {
        // Nothing needed
    }
    
    // Add logic to become follower
    private CompanionFollower BecomeFollower()
    {
        // Add or enable CompanionFollower
        CompanionFollower follower = GetComponent<CompanionFollower>();
        if (follower == null) follower = gameObject.AddComponent<CompanionFollower>();
        
        follower.StartFollowing();
        
        // Disable this script
        this.enabled = false;
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        return follower;
    }
}