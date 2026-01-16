using UnityEngine;

/// <summary>
/// Purple fragment collectible dropped by the boss.
/// Collecting this leads to the evil ending.
/// </summary>
public class PurpleFragmentCollectable : MonoBehaviour, IInteractable
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioSource collectSound;
    [SerializeField] private Light glowLight;
    
    [Header("Settings")]
    [SerializeField] private string collectMessage = "[Purple Fragment] Your true power... reclaimed.";
    [SerializeField] private string collectMessageAudio;
    [SerializeField] private float messageDuration = 3f;
    [SerializeField] private string interactionPrompt = "[System] Press E to interact";
    [SerializeField] private string interactionPromptAudio;
    [SerializeField] private bool debugMode = true;
    
    private bool hasBeenCollected = false;
    
    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }
    
    void Start()
    {
        // Don't show if already collected
        if (GameSession.HasCollectedPurpleFragment)
        {
            Destroy(gameObject);
        }
    }
    
    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        
        if (hasBeenCollected) return;
        
        hasBeenCollected = true;
        interactSuccessful = true;
        
        if (debugMode) Debug.Log("[PurpleFragmentCollectable] 💜 Purple Fragment collected - EVIL PATH!");
        
        // Mark as collected
        GameSession.HasCollectedPurpleFragment = true;
        
        // Play effects
        if (collectEffect != null) collectEffect.Play();
        if (collectSound != null) collectSound.Play();
        
        // Show message
        if (CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowMonologue(collectMessage, messageDuration, collectMessageAudio);
        }
        
        // Play ominous sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("ObjectiveComplete");
        }
        
        // Hide visuals
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
        {
            renderer.enabled = false;
        }
        if (glowLight != null) glowLight.enabled = false;
        
        // Destroy collider
        Collider col = GetComponent<Collider>();
        if (col != null) Destroy(col);
        
        OnInteractionComplete?.Invoke(this);
        
        // Destroy all fragment followers (red/blue companions)
        DestroyAllFragmentFollowers();
        
        // Destroy after effect
        Destroy(gameObject, 2f);
    }
    
    private void DestroyAllFragmentFollowers()
    {
        // Find and destroy all CompanionFollowers in scene
        CompanionFollower[] followers = FindObjectsByType<CompanionFollower>(FindObjectsSortMode.None);
        foreach (var follower in followers)
        {
            if (debugMode) Debug.Log($"[PurpleFragmentCollectable] Destroying follower: {follower.gameObject.name}");
            Destroy(follower.gameObject);
        }
        
        if (debugMode) Debug.Log($"[PurpleFragmentCollectable] Destroyed {followers.Length} fragment followers");
    }
    
    public void EndInteraction()
    {
        // Nothing needed
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CaptionManager.Instance != null)
        {
            CaptionManager.Instance.ShowSystemMessage(interactionPrompt, 3f, interactionPromptAudio);
        }
    }
}
