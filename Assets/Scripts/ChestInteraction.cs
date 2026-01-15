using UnityEngine;

/// <summary>
/// Example script for chest interactions that give card rewards
/// Attach this to chest GameObjects in exploration scenes
/// Requires: 
/// - Collider component with "Is Trigger" enabled
/// - ExplorationRewardManager in the scene
/// </summary>
public class ChestInteraction : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Should the chest be destroyed after opening?")]
    [SerializeField] private bool destroyAfterOpening = true;
    
    [Tooltip("Should the chest be deactivated instead of destroyed?")]
    [SerializeField] private bool deactivateInstead = false;
    
    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject openChestSprite; // Optional: Sprite to show when opened
    [SerializeField] private GameObject closedChestSprite; // Optional: Sprite to hide when opened
    [SerializeField] private ParticleSystem openParticles; // Optional: Particles on open
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip openSound; // Optional: Sound effect
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasBeenOpened = false;
    
    /// <summary>
    /// Detect when player walks into chest trigger
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player") && !hasBeenOpened)
        {
            OpenChest();
        }
    }
    
    /// <summary>
    /// Open the chest and show card rewards
    /// </summary>
    public void OpenChest()
    {
        if (hasBeenOpened)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[ChestInteraction] Chest '{gameObject.name}' already opened!");
            return;
        }
        
        hasBeenOpened = true;
        
        if (showDebugLogs)
            Debug.Log($"[ChestInteraction] Opening chest: {gameObject.name}");
        
        // Visual feedback
        UpdateVisuals();
        
        // Audio feedback
        PlayOpenSound();
        
        // Show card reward UI (2 random starter cards)
        if (ExplorationRewardManager.Instance != null)
        {
            ExplorationRewardManager.ShowReward();
        }
        else
        {
            Debug.LogError("[ChestInteraction] ExplorationRewardManager.Instance is null! Make sure there's an ExplorationRewardManager in the scene.");
        }
        
        // Handle chest cleanup after a short delay
        if (destroyAfterOpening || deactivateInstead)
        {
            Invoke(nameof(CleanupChest), 0.5f); // Wait half second
        }
    }
    
    /// <summary>
    /// Update chest visuals after opening
    /// </summary>
    private void UpdateVisuals()
    {
        // Show open sprite, hide closed sprite
        if (openChestSprite != null)
            openChestSprite.SetActive(true);
        
        if (closedChestSprite != null)
            closedChestSprite.SetActive(false);
        
        // Play particles
        if (openParticles != null)
            openParticles.Play();
    }
    
    /// <summary>
    /// Play chest opening sound
    /// </summary>
    private void PlayOpenSound()
    {
        if (openSound != null && AudioManager.Instance != null)
        {
            // AudioManager.Instance.PlaySFX(openSound);
        }
    }
    
    /// <summary>
    /// Destroy or deactivate the chest
    /// </summary>
    private void CleanupChest()
    {
        if (deactivateInstead)
        {
            if (showDebugLogs)
                Debug.Log($"[ChestInteraction] Deactivating chest: {gameObject.name}");
            gameObject.SetActive(false);
        }
        else if (destroyAfterOpening)
        {
            if (showDebugLogs)
                Debug.Log($"[ChestInteraction] Destroying chest: {gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    // Optional: For interaction button prompts
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenOpened)
        {
            // Show interaction prompt (if you have a UI system for this)
            // Example: UIManager.ShowPrompt("Press E to open chest");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide interaction prompt
            // Example: UIManager.HidePrompt();
        }
    }
}
