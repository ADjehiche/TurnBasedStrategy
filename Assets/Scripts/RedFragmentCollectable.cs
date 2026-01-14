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
        
        // Hide the collectible visually (but don't destroy yet)
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers) r.enabled = false;
        
        if (glowLight != null) glowLight.enabled = false;
        
        // Disable collider to prevent re-interaction
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Trigger flashback
        if (FlashbackManager.Instance != null)
        {
            if (useSceneFlashback)
            {
                FlashbackManager.Instance.StartRageFlashback(flashbackSceneName, OnFlashbackComplete);
            }
            else
            {
                FlashbackManager.Instance.StartOverlayFlashback(flashbackDialogue, dialogueLineDuration, OnFlashbackComplete);
            }
        }
        else
        {
            Debug.LogError("[RedFragmentCollectable] FlashbackManager not found! Skipping flashback.");
            OnFlashbackComplete();
        }
    }
    
    /// <summary>
    /// Called when flashback sequence completes
    /// </summary>
    private void OnFlashbackComplete()
    {
        if (debugMode) Debug.Log("[RedFragmentCollectable] Flashback complete - spawning follower");
        
        // Mark as collected in GameSession
        GameSession.HasCollectedRedFragment = true;
        
        // Spawn the follower
        SpawnRedFragmentFollower();
        
        // Notify any listening systems (like boss door glyph counter)
        NotifyGlyphSystems();
        
        // Complete interaction
        OnInteractionComplete?.Invoke(this);
        
        // Destroy this collectible
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Spawn the red fragment as a follower companion
    /// </summary>
    private void SpawnRedFragmentFollower()
    {
        if (redFragmentFollowerPrefab == null)
        {
            Debug.LogWarning("[RedFragmentCollectable] Red fragment follower prefab not assigned!");
            return;
        }
        
        Vector3 spawnPos = spawnPosition != null ? spawnPosition.position : transform.position;
        
        // Spawn slightly to the side of the player (opposite side from blue if present)
        spawnPos += Vector3.left * 2f + Vector3.up * 0.5f;
        
        GameObject follower = Instantiate(redFragmentFollowerPrefab, spawnPos, Quaternion.identity);
        
        // Start following if it has the CompanionFollower component
        CompanionFollower followerScript = follower.GetComponent<CompanionFollower>();
        if (followerScript != null)
        {
            followerScript.StartFollowing();
        }
        
        if (debugMode) Debug.Log("[RedFragmentCollectable] Spawned red fragment follower");
    }
    
    /// <summary>
    /// Notify glyph/boss door systems that a fragment has been collected
    /// </summary>
    private void NotifyGlyphSystems()
    {
        // Find boss door or glyph controller and notify
        // This will be implemented when boss door system is in place
        if (debugMode) Debug.Log("[RedFragmentCollectable] Red Fragment registered as glyph for boss door");
        
        // Play objective complete sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("ObjectiveComplete");
        }
        
        // Show system message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage("[Rage Fragment Acquired]", 2f);
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
