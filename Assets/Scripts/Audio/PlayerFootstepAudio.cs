using UnityEngine;

/// <summary>
/// Handles footstep sounds for player movement
/// Attach to the player GameObject
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Footstep Sound Names")]
    [SerializeField] private string[] footstepSoundNames = { "Footstep1", "Footstep2", "Footstep3" };
    
    [Header("Settings")]
    [SerializeField] private float footstepInterval = 0.5f; // Time between footsteps
    [SerializeField] private float minimumVelocity = 0.1f; // Minimum movement speed to play footsteps
    [SerializeField] private bool debugLogs = false;
    
    private CharacterController characterController;
    private float footstepTimer = 0f;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (characterController == null)
        {
            Debug.LogError("[PlayerFootstepAudio] CharacterController component not found!");
        }
    }
    
    void Update()
    {
        // Check if player is moving
        if (characterController != null && characterController.velocity.magnitude > minimumVelocity)
        {
            footstepTimer += Time.deltaTime;
            
            if (footstepTimer >= footstepInterval)
            {
                PlayFootstepSound();
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset timer when not moving
            footstepTimer = 0f;
        }
    }
    
    private void PlayFootstepSound()
    {
        if (AudioManager.Instance != null)
        {
            // Play a random footstep sound for variation
            AudioManager.Instance.PlayRandom(footstepSoundNames);
            
            if (debugLogs)
            {
                Debug.Log("[PlayerFootstepAudio] Playing footstep sound");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerFootstepAudio] AudioManager instance not found!");
        }
    }
    
    /// <summary>
    /// Manually trigger a footstep sound (e.g., from animation events)
    /// </summary>
    public void TriggerFootstep()
    {
        PlayFootstepSound();
    }
}
