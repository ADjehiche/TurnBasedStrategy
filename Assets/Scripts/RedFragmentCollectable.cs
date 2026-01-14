using UnityEngine;
using System.Collections;

/// <summary>
/// Attach to the red fragment collectible in the Combat Wing
/// Handles collection, triggers the Rage flashback, and spawns the follower
/// Based on BlueFragmentCollectable pattern
/// </summary>
public class RedFragmentCollectable : MonoBehaviour, IInteractable
{
    [Header("Collection Settings")]
    [Tooltip("Prefab to spawn as follower companion after collection")]
    [SerializeField] private GameObject redFragmentFollowerPrefab;
    [Tooltip("Where to spawn the follower (defaults to player position)")]
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private float collectDelay = 0.5f;
    [SerializeField] private bool debugMode = true;
    
    [Header("Flashback Settings")]
    [Tooltip("Name of the flashback scene to load")]
    [SerializeField] private string flashbackSceneName = "RageFlashback";
    [Tooltip("If true, use scene-based flashback. If false, use overlay-only.")]
    [SerializeField] private bool useSceneFlashback = true;
    
    [Header("Overlay Flashback Dialogue (if not using scene)")]
    [SerializeField] private string[] flashbackDialogue = new string[]
    {
        "...Blood. So much blood...",
        "They screamed my name. Not in fear—in reverence.",
        "I held lightning in my hands. I decided who lived.",
        "This place... I built it all.",
        "What... what have I done?"
    };
    [SerializeField] private float dialogueLineDuration = 3f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle effect to play on collection")]
    [SerializeField] private ParticleSystem collectEffect;
    [Tooltip("Sound effect to play on collection")]
    [SerializeField] private AudioSource collectSound;
    [Tooltip("Optional glow/pulse effect while uncollected")]
    [SerializeField] private Light glowLight;
    
    private bool hasBeenCollected = false;
    private bool playerInRange = false;
    
    [Header("Interaction Prompt")]
    [SerializeField] private string interactionPrompt = "[System] Press E to interact";
    [SerializeField] private float promptDuration = 5f;
    
    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }
    
    void Start()
    {
        // Check if already collected (from GameSession)
        if (GameSession.HasCollectedRedFragment)
        {
            if (debugMode) Debug.Log("[RedFragmentCollectable] Already collected - destroying self");
            Destroy(gameObject);
            return;
        }
        
        // Auto-find spawn position if not set
        if (spawnPosition == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                spawnPosition = player.transform;
            }
        }
        
        // Start glow animation if present
        if (glowLight != null)
        {
            StartCoroutine(PulseGlow());
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (hasBeenCollected) return;
        
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            
            // Show interaction prompt
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowSystemMessage(interactionPrompt, promptDuration);
            }
            
            if (debugMode) Debug.Log("[RedFragmentCollectable] Player in range - showing prompt");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (debugMode) Debug.Log("[RedFragmentCollectable] Player left range");
        }
    }
    
    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        
        if (hasBeenCollected)
        {
            if (debugMode) Debug.Log("[RedFragmentCollectable] Already collected");
            return;
        }
        
        if (debugMode) Debug.Log("[RedFragmentCollectable] 🔴 Red Fragment collected! Triggering Rage flashback...");
        
        hasBeenCollected = true;
        interactSuccessful = true;
        
        StartCoroutine(CollectionSequence());
    }
    
    public void EndInteraction()
    {
        // Nothing needed for end interaction
    }
    
    /// <summary>
    /// Handle the collection sequence with effects and flashback trigger
    /// </summary>
    private IEnumerator CollectionSequence()
    {
        // Play collection effects immediately
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        if (collectSound != null)
        {
            collectSound.Play();
        }
        
        // Brief delay for effects
        yield return new WaitForSeconds(collectDelay);
        
        // Mark as collected FIRST (like yellow fragment)
        GameSession.HasCollectedRedFragment = true;
        GameSession.HasPlayedRageFlashback = true; // Will be set to true after flashback, but set now for safety
        
        // Start following FIRST (like yellow fragment)
        BecomeFollower();
        
        // Show intro dialogue
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowFlashback("[Red Fragment] I am your rage.", 2.5f);
        }
        
        // Lock player movement during dialogue
        if (PlayerMovementLock.Instance != null)
        {
            PlayerMovementLock.Instance.LockMovement("Red Fragment intro");
        }
        
        // Wait for dialogue to display
        yield return new WaitForSeconds(3f);
        
        // Notify any listening systems (like boss door glyph counter)
        NotifyGlyphSystems();
        
        // Complete interaction
        OnInteractionComplete?.Invoke(this);
        
        // THEN trigger flashback (happens after follower is already active)
        if (FlashbackManager.Instance != null && useSceneFlashback)
        {
            FlashbackManager.Instance.StartRageFlashback(flashbackSceneName, null);
        }
        else if (FlashbackManager.Instance != null)
        {
            FlashbackManager.Instance.StartOverlayFlashback(flashbackDialogue, dialogueLineDuration, null);
        }
        else
        {
            // No flashback manager - just unlock
            if (PlayerMovementLock.Instance != null)
            {
                PlayerMovementLock.Instance.UnlockMovement("No flashback");
            }
        }
    }
    
    /// <summary>
    /// Called when flashback sequence completes
    /// </summary>
    private void OnFlashbackComplete()
    {
        if (debugMode) Debug.Log("[RedFragmentCollectable] Flashback complete - becoming follower");
        
        // Mark as collected in GameSession
        GameSession.HasCollectedRedFragment = true;
        
        // Transform this object into a follower (single prefab approach)
        BecomeFollower();
        
        // Notify any listening systems (like boss door glyph counter)
        NotifyGlyphSystems();
        
        // Complete interaction
        OnInteractionComplete?.Invoke(this);
        
        // DON'T destroy - we're now a follower!
    }
    
    /// <summary>
    /// Transform this collectible into a following companion
    /// </summary>
    private void BecomeFollower()
    {
        // Re-enable visuals
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in renderers) r.enabled = true;
        
        if (glowLight != null) glowLight.enabled = true;
        
        // Get or add CompanionFollower component
        CompanionFollower followerScript = GetComponent<CompanionFollower>();
        if (followerScript == null)
        {
            Debug.LogWarning("[RedFragmentCollectable] No CompanionFollower on this object - add the component!");
            return;
        }
        
        // Start following the player
        followerScript.StartFollowing();
        
        // Mark red companion as active (for persistence across scenes)
        GameSession.RedCompanionActive = true;
        
        // Disable this collectible script (no longer needed)
        this.enabled = false;
        
        // Remove collider so player doesn't keep interacting
        Collider col = GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        if (debugMode) Debug.Log("[RedFragmentCollectable] ✅ Now following player as companion!");
    }
    
    /// <summary>
    /// Notify glyph/boss door systems that a fragment has been collected
    /// </summary>
    private void NotifyGlyphSystems()
    {
        if (debugMode) Debug.Log($"[RedFragmentCollectable] Fragment count: {GameSession.CollectedFragmentCount}");
        
        // Play objective complete sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("ObjectiveComplete");
        }
        
        // Check if both fragments are collected (boss door can unlock)
        if (GameSession.CanUnlockBossDoor)
        {
            if (debugMode) Debug.Log("[RedFragmentCollectable] Both fragments collected - boss door can now unlock!");
            
            // Show boss door unlock message
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowSystemMessage("[Boss Door Unsealed]", 3f);
            }
            
            // Play special unlock sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play("DoorUnlock");
            }
        }
        else
        {
            // Show fragment acquired message
            if (CaptionManager.Instance != null)
            {
                CaptionManager.Instance.ShowSystemMessage("[Rage Fragment Acquired]", 2f);
            }
        }
    }
    
    /// <summary>
    /// Pulse the glow light for visual effect
    /// </summary>
    private IEnumerator PulseGlow()
    {
        if (glowLight == null) yield break;
        
        float baseIntensity = glowLight.intensity;
        float pulseSpeed = 2f;
        
        while (!hasBeenCollected)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            glowLight.intensity = baseIntensity * (0.5f + pulse * 0.5f);
            yield return null;
        }
    }
    
    // ===== TESTING METHODS =====
    
    [ContextMenu("Test: Trigger Collection")]
    public void TestTriggerCollection()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("[RedFragmentCollectable] Must be in Play mode to test!");
            return;
        }
        
        bool success;
        Interact(null, out success);
        Debug.Log($"[RedFragmentCollectable] Test interact result: {success}");
    }
    
    [ContextMenu("Test: Overlay Flashback Only")]
    public void TestOverlayFlashback()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("[RedFragmentCollectable] Must be in Play mode to test!");
            return;
        }
        
        if (FlashbackManager.Instance != null)
        {
            FlashbackManager.Instance.StartOverlayFlashback(flashbackDialogue, dialogueLineDuration);
        }
    }
}
